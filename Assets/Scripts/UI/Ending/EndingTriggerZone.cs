namespace FrogGame.World
{
    using UnityEngine;
    using FrogGame.UI;

    /// <summary>
    /// Trigger zone placed at the end of the Mangrove level to launch the ending screen sequence.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EndingTriggerZone : MonoBehaviour
    {
        [SerializeField] private FullScreenEndingPlayer endingPlayer;
        [SerializeField] private string playerTag = "Player";

        private bool hasTriggered;

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasTriggered) return;

            if (collision.CompareTag(playerTag))
            {
                hasTriggered = true;

                if (endingPlayer == null)
                {
                    endingPlayer = FindFirstObjectByType<FullScreenEndingPlayer>();
                }

                if (endingPlayer != null)
                {
                    endingPlayer.PlayEnding();
                }
                else
                {
                    Debug.LogWarning("[EndingTriggerZone] FullScreenEndingPlayer reference is missing in scene!");
                }
            }
        }
    }
}