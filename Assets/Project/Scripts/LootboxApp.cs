using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using Lootbox.FSM;
using UnityEngine;

namespace Lootbox
{
    public class LootboxApp : MonoBehaviourExt
    {
        [OnAwake]
        private void OnAwake()
        {
            Settings.Fsm = new AxGrid.FSM.FSM();
            Settings.Fsm.Add(
                new IdleState(),
                new SpinningState(),
                new StoppingState(),
                new ShowResultState()
            );
        }

        [OnStart]
        private void OnStart()
        {
            Settings.Fsm.Start("Idle");
        }

        [OnUpdate]
        private void OnUpdate()
        {
            Settings.Fsm.Update(Time.deltaTime);
        }
    }
}
