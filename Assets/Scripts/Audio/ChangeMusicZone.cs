namespace FrogGame.Gameplay
{
    using UnityEngine;

    [RequireComponent(typeof(Collider2D))]
    public class ChangeMusicZone : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private LayerMask playerLayer;

        [Header("Entry Behavior Settings")]
        [SerializeField] private MusicManager.MusicOption enterMusic;

        [Header("Exit Behavior Settings")]
        [SerializeField] private bool revertOnExit = false;
        [SerializeField] private MusicManager.MusicOption exitMusic;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayer(other) && MusicManager.Instance != null)
            {
                MusicManager.Instance.SetMusic(enterMusic);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (revertOnExit && IsPlayer(other) && MusicManager.Instance != null)
            {
                MusicManager.Instance.SetMusic(exitMusic);
            }
        }

        private bool IsPlayer(Collider2D other)
        {
            int colLayer = 1 << other.gameObject.layer;

            return (colLayer & playerLayer) != 0;
        }
    }
}