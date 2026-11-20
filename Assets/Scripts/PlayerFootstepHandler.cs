namespace FrogGame.Gameplay
{
    using UnityEngine;
    using FrogGame.Core;

    /// <summary>
    /// Listens to footstep animation events, detects the ground surface under the player feet, and triggers Wwise audio.
    /// </summary>
    public class PlayerFootstepHandler : MonoBehaviour
    {
        [Header("Feet References")]
        [SerializeField] private Transform feetPoint;

        private PlayerBrain brain;

        private void Awake() => brain = GetComponent<PlayerBrain>();

        private void Start()
        {
            if (brain != null && brain.AnimationHandler != null)
            {
                brain.AnimationHandler.OnFootstep += HandleFootstep;
            }
        }

        private void OnDestroy()
        {
            if (brain != null && brain.AnimationHandler != null)
            {
                brain.AnimationHandler.OnFootstep -= HandleFootstep;
            }
        }

        private void HandleFootstep()
        {
            if (AudioManager.Instance == null || SurfaceDetectorManager.Instance == null) return;

            Vector2 samplePos = feetPoint != null ? feetPoint.position : transform.position;
            SurfaceType surface = SurfaceDetectorManager.Instance.GetGroundSurfaceAtPosition(samplePos);

            switch (surface)
            {
                case SurfaceType.Concrete:

                    AudioManager.Instance.SetPlayerFootstepsConcrete(gameObject);

                    break;
                case SurfaceType.GrassLarge:

                    AudioManager.Instance.SetPlayerFootstepsGrassLarge(gameObject);

                    break;
                case SurfaceType.GrassShort:

                    AudioManager.Instance.SetPlayerFootstepsGrassShort(gameObject);

                    break;
            }

            AudioManager.Instance.PlayPlayerFootsteps(gameObject);
        }
    }
}