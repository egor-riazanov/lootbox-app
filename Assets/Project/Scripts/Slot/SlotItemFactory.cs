using UnityEngine;
using UnityEngine.UI;

namespace Lootbox
{
    /// <summary>
    ///     Creates slot items from a prefab and manages their sprite assignment.
    /// </summary>
    public class SlotItemFactory : MonoBehaviour, ISlotItemFactory
    {
        [SerializeField] private GameObject _slotItemPrefab;
        [SerializeField] private Sprite[] _sprites;

        private int _spriteIndex;

        /// <inheritdoc />
        public int VarietyCount => _sprites != null ? _sprites.Length : 1;

        /// <inheritdoc />
        public RectTransform Create(RectTransform parent)
        {
            var instance = Instantiate(_slotItemPrefab, parent);
            var rt = instance.GetComponent<RectTransform>();
            AssignNextSprite(rt);
            return rt;
        }

        /// <inheritdoc />
        public void AssignNextSprite(RectTransform item)
        {
            if (_sprites == null || _sprites.Length == 0)
                return;

            var image = item.GetComponent<Image>();
            if (image == null)
                return;

            image.sprite = _sprites[_spriteIndex % _sprites.Length];
            _spriteIndex++;
        }
    }
}
