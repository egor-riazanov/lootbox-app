using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;

namespace Lootbox.FSM
{
    [State("Idle")]
    public class IdleState : FSMState
    {
        [Enter]
        private void Enter()
        {
            Model.Set("BtnStartEnabled", true);
            Model.Set("BtnStopEnabled", false);
            Model.Set("SlotState", "idle");
        }

        [Bind("BtnStart")]
        private void OnBtnStart()
        {
            Parent.Change("Spinning");
        }
    }

    [State("Spinning")]
    public class SpinningState : FSMState
    {
        private bool _canStop;

        [Enter]
        private void Enter()
        {
            _canStop = false;
            Model.Set("BtnStartEnabled", false);
            Model.Set("BtnStopEnabled", false);
            Model.Set("SlotState", "spinning");
        }

        [One(3.0f)]
        private void EnableStop()
        {
            _canStop = true;
            Model.Set("BtnStopEnabled", true);
        }

        [Bind("BtnStop")]
        private void OnBtnStop()
        {
            if (!_canStop) return;
            Parent.Change("Stopping");
        }
    }

    [State("Stopping")]
    public class StoppingState : FSMState
    {
        [Enter]
        private void Enter()
        {
            Model.Set("BtnStartEnabled", false);
            Model.Set("BtnStopEnabled", false);
            Model.Set("SlotState", "stopping");
        }

        [Bind("OnSlotStopped")]
        private void OnSlotStopped()
        {
            Parent.Change("ShowResult");
        }
    }

    [State("ShowResult")]
    public class ShowResultState : FSMState
    {
        [Enter]
        private void Enter()
        {
            Model.Set("BtnStartEnabled", true);
            Model.Set("BtnStopEnabled", false);
            Model.Set("SlotState", "result");
            Invoke("PlayResultParticles");
        }

        [Bind("BtnStart")]
        private void OnBtnStart()
        {
            Parent.Change("Spinning");
        }
    }
}
