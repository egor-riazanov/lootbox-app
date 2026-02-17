using System.Collections.Generic;
using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using AxGrid.Path;
using UnityEngine;

namespace Lootbox
{
    /// <summary>
    ///     Controls a single slot reel's scrolling behaviour for the lootbox mini-slot mechanic.
    ///     Items scroll downward through a RectMask2D window. The reel accelerates to max speed,
    ///     holds while "spinning", then decelerates and snaps the closest item to centre on stop.
    ///     Communicates with the game FSM via <see cref="Settings.Invoke" />.
    /// </summary>
    public class SlotReelController : MonoBehaviourExtBind
    {
        // ───────────────────────── Inspector fields ─────────────────────────

        [SerializeField] private float _itemHeight = 200f;
        [SerializeField] private float _maxSpeed = 2000f;
        [SerializeField] private float _accelerationTime = 1.5f;
        [SerializeField] private float _decelerationTime = 2.0f;
        [SerializeField] private int _initialItemCount = 5;
        [SerializeField] private SlotItemFactory _factory;
        [SerializeField] private RectTransform _container;

        // ───────────────────────── Runtime state ────────────────────────────

        private readonly List<RectTransform> _items = new();

        /// <summary>
        ///     Y position below which an item is recycled to the top of the strip.
        /// </summary>
        private float _bottomThreshold;
        private bool _isScrolling;
        private float _scrollSpeed;

        /// <summary>
        ///     Y position above which an item is recycled to the bottom of the strip.
        /// </summary>
        private float _topThreshold;

        // ───────────────────────── Initialisation ───────────────────────────

        [OnAwake]
        private void Init()
        {
            // Create initial slot items via factory.
            _items.Clear();

            var minCount = Mathf.Max(
                _initialItemCount,
                Mathf.CeilToInt(_container.rect.height / _itemHeight) + 2
            );

            var variety = _factory.VarietyCount;
            if (variety > 1)
                minCount = Mathf.CeilToInt((float)minCount / variety) * variety;

            for (var i = 0; i < minCount; i++)
                _items.Add(_factory.Create(_container));

            // Thresholds sit half the strip length from centre (adapts to any item count).
            _topThreshold = (_items.Count / 2f) * _itemHeight;
            _bottomThreshold = -(_items.Count / 2f) * _itemHeight;

            // Lay items out vertically, centred around y = 0.
            var h = _itemHeight;
            var half = (_items.Count - 1) / 2f;

            for(var i = 0; i < _items.Count; i++) {
                _items[i].anchoredPosition = new Vector2(
                    _items[i].anchoredPosition.x,
                    (half - i) * h
                );
            }

        }

        // ───────────────────────── FSM binding ──────────────────────────────

        /// <summary>
        ///     Reacts to slot state changes dispatched by the game FSM.
        /// </summary>
        [Bind("OnSlotStateChanged")]
        private void OnSlotStateChanged(string state)
        {
            switch (state) {
                case "spinning":
                    StartSpinning();
                    break;
                case "stopping":
                    StartStopping();
                    break;
                // "idle" and "result" require no special handling.
            }
        }

        // ───────────────────────── Spin lifecycle ───────────────────────────

        /// <summary>
        ///     Begins the reel spin by easing from zero to max speed.
        /// </summary>
        private void StartSpinning()
        {
            _isScrolling = true;
            _scrollSpeed = 0f;

            Path = CPath.Create()
                .EasingCubicEaseIn(_accelerationTime, 0f, _maxSpeed, v => _scrollSpeed = v);
        }

        /// <summary>
        ///     Decelerates the reel from its current speed down to zero, then snaps to centre.
        /// </summary>
        private void StartStopping()
        {
            var startSpeed = _scrollSpeed;

            Path = CPath.Create()
                .EasingCubicEaseOut(_decelerationTime, startSpeed, 0f, v => _scrollSpeed = v)
                .Action(() => {
                    _scrollSpeed = 0f;
                    SnapToCenter();
                });
        }

        /// <summary>
        ///     Smoothly aligns the closest item to the exact centre (y = 0).
        /// </summary>
        private void SnapToCenter()
        {
            RectTransform closest = null;
            var closestDist = float.MaxValue;

            foreach (var item in _items) {
                var dist = Mathf.Abs(item.anchoredPosition.y);

                if (dist < closestDist) {
                    closestDist = dist;
                    closest = item;
                }
            }

            if (closest == null) {
                FinishStopping();
                return;
            }

            var startY = closest.anchoredPosition.y;

            // Already close enough — skip the animation.
            if (Mathf.Abs(startY) < 1f) {
                FinishStopping();
                return;
            }

            // Ease from the current offset to zero, shifting every item by the same delta.
            Path = CPath.Create()
                .EasingQuadEaseOut(0.4f, startY, 0f, targetY => {
                    var delta = targetY - closest.anchoredPosition.y;
                    foreach (var it in _items)
                        it.anchoredPosition += new Vector2(0, delta);
                })
                .Action(() => FinishStopping());
        }

        /// <summary>
        ///     Finalises the stop sequence and notifies the FSM that this reel is done.
        /// </summary>
        private void FinishStopping()
        {
            _isScrolling = false;
            _scrollSpeed = 0f;
            Settings.Invoke("OnSlotStopped");
        }

        // ───────────────────────── Per-frame scroll ─────────────────────────

        /// <summary>
        ///     Moves every item downward each frame and recycles items that leave the strip.
        /// </summary>
        [OnUpdate]
        private void UpdateScroll()
        {
            if (!_isScrolling || _scrollSpeed <= 0f)
                return;

            var displacement = _scrollSpeed * Time.deltaTime;

            for(var i = 0; i < _items.Count; i++) {
                // Move downward (negative y).
                var pos = _items[i].anchoredPosition;
                pos.y -= displacement;
                _items[i].anchoredPosition = pos;
            }

            // Recycle any item that falls below the bottom threshold.
            for(var i = 0; i < _items.Count; i++) {
                if (_items[i].anchoredPosition.y < _bottomThreshold) {
                    var highestY = FindHighestY(i);
                    var pos = _items[i].anchoredPosition;
                    pos.y = highestY + _itemHeight;
                    _items[i].anchoredPosition = pos;

                    _factory.AssignNextSprite(_items[i]);
                }
            }
        }

        // ───────────────────────── Helpers ───────────────────────────────────

        /// <summary>
        ///     Returns the highest y value among all items except the one at <paramref name="excludeIndex" />.
        /// </summary>
        private float FindHighestY(int excludeIndex)
        {
            var highest = float.MinValue;

            for(var i = 0; i < _items.Count; i++) {
                if (i == excludeIndex)
                    continue;
                if (_items[i].anchoredPosition.y > highest)
                    highest = _items[i].anchoredPosition.y;
            }

            return highest;
        }

    }
}