namespace FrogGame.Gameplay
{
    using UnityEngine;
    using FrogGame.Core;

    public class PlayerShootTongueState : IState
    {
        private readonly PlayerBrain brain;
        private Coroutine tongueCoroutine;
        private bool hasShotTongue;

        public PlayerShootTongueState(PlayerBrain brain) => this.brain = brain;

        public void Enter()
        {
            brain.InputHandler.ConsumeTongueInput();
            brain.MovementHandler.Stop();

            hasShotTongue = false;

            if (brain.CarryHandler != null && brain.CarryHandler.IsCarrying)
            {
                bool rockDrop = brain.CarryHandler.TryDropBox();

                brain.FSM.ChangeState(brain.IdleState);

                if (AudioManager.Instance != null && rockDrop)
                {
                    AudioManager.Instance.PlayEnvRockDrop(brain.gameObject);
                }

                return;
            }

            if (brain.AnimationHandler != null)
            {
                brain.AnimationHandler.OnTongueExtended += StartShootTongue;
            }

            brain.Animator.Play("ShootTongue");
        }

        private void StartShootTongue()
        {
            if (hasShotTongue) return;

            hasShotTongue = true;

            Vector2 direction = brain.MovementHandler.FacingDirection;
            tongueCoroutine = brain.StartCoroutine(brain.TongueController.ShootTongueRoutine(direction, OnTongueComplete));
        }

        private void OnTongueComplete(TongueController.TongueHitResult result, Vector2 targetPos)
        {
            if (result == TongueController.TongueHitResult.Grapple)
            {
                brain.GrappleState.SetTarget(targetPos);
                brain.FSM.ChangeState(brain.GrappleState);
            }
            else
            {
                brain.FSM.ChangeState(brain.IdleState);
            }
        }

        public void LogicUpdate() { }
        public void PhysicsUpdate() { }

        public void Exit()
        {
            if (brain.AnimationHandler != null)
            {
                brain.AnimationHandler.OnTongueExtended -= StartShootTongue;
            }

            if (tongueCoroutine != null)
            {
                brain.StopCoroutine(tongueCoroutine);
                tongueCoroutine = null;
            }
            brain.TongueController.DisableLine();
        }
    }
}