namespace FrogGame.UI
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using FrogGame.Gameplay;

    /// <summary>
    /// Plays a full-screen frame-by-frame sprite sequence for the ending cutscene.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class FullScreenEndingPlayer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image displayImage;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        [Header("Animation Frames")]
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 12f;
        [SerializeField] private bool loopLastFrame = true;

        [Header("Transition & Flow")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float holdAfterFinishDuration = 1.5f;
        [SerializeField] private UnityEvent onSequenceCompleted;

        private CanvasGroup canvasGroup;
        private bool isPlaying;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (displayImage != null)
            {
                displayImage.enabled = false;
            }
        }

        /// <summary>
        /// Starts the full-screen ending animation sequence.
        /// </summary>
        public void PlayEnding()
        {
            if (isPlaying || frames == null || frames.Length == 0) return;
            StartCoroutine(PlaySequenceRoutine());
        }

        private IEnumerator PlaySequenceRoutine()
        {
            isPlaying = true;

            // Disable player input so the character cannot move or fall during the ending
            PlayerInputHandler inputHandler = FindFirstObjectByType<PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.enabled = false;
            }

            if (aspectRatioFitter != null)
            {
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                aspectRatioFitter.aspectRatio = 2560f / 1440f;
            }

            if (displayImage != null)
            {
                displayImage.sprite = frames[0];
                displayImage.enabled = true;
            }

            canvasGroup.blocksRaycasts = true;

            // Smooth fade-in to full screen
            if (fadeInDuration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                    yield return null;
                }
            }
            canvasGroup.alpha = 1f;

            // Play all frames sequentially
            float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
            for (int i = 0; i < frames.Length; i++)
            {
                if (displayImage != null && frames[i] != null)
                {
                    displayImage.sprite = frames[i];
                }

                yield return new WaitForSecondsRealtime(frameDuration);
            }

            if (!loopLastFrame && displayImage != null)
            {
                displayImage.enabled = false;
            }

            if (holdAfterFinishDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdAfterFinishDuration);
            }

            onSequenceCompleted?.Invoke();
        }
    }
}