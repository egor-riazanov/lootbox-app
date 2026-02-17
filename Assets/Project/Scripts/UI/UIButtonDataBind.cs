using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Lootbox
{
    /// <summary>
    /// Reusable button component that binds a Unity UI Button
    /// to the AxGrid FSM event system.
    /// Click invokes an FSM event; interactability is driven by a model field.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonDataBind : MonoBehaviourExtBind
    {
        [SerializeField] private string _eventName;
        [SerializeField] private string _enabledFieldName;

        private Button _button;

        [OnAwake]
        private void OnAwake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => Settings.Invoke(_eventName));
        }

        [Bind("On{_enabledFieldName}Changed")]
        private void OnEnabledChanged(bool value)
        {
            _button.interactable = value;
        }
    }
}
