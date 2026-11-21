namespace FrogGame.UI
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using FrogGame.Gameplay;

    /// <summary>
    /// Displays health, split hit-flash overlays (frog portrait + health bar), arrow ammo, unlocked abilities, and LoL-style radial skill cooldowns on the HUD UI.
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Health UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthBarHitImage;
        [SerializeField] private Image frogFaceHitImage;
        [SerializeField] private float hitEffectDuration = 0.2f;

        [Header("Shield UI References")]
        [SerializeField] private Image shieldIcon;

        [Header("Arrow UI")]
        [SerializeField] private GameObject arrowHudContainer;
        [SerializeField] private TextMeshProUGUI arrowText;

        [Header("Stun / Sneeze UI")]
        [SerializeField] private GameObject stunHudContainer;
        [SerializeField] private Image stunCooldownOverlay;
        [SerializeField] private TextMeshProUGUI stunCooldownText;

        private float stunCooldownRemaining = 0f;
        private float stunTotalCooldown = 1f;
        private int lastKnownHealth = -1;
        private Coroutine healthHitCoroutine;

        private void Awake()
        {
            InitializeHitOverlay(healthBarHitImage);
            InitializeHitOverlay(frogFaceHitImage);
        }

        private void OnEnable()
        {
            PlayerHealthSystem.OnHealthChanged += UpdateHealthUI;
            PlayerHealthSystem.OnShieldChanged += UpdateShieldUI;
            PlayerAmmoSystem.OnArrowCountChanged += UpdateArrowUI;
            PlayerAbilityTracker.OnBowUnlocked += UpdateBowVisibility;
            PlayerAbilityTracker.OnSneezeUnlocked += UpdateSneezeVisibility;
            PlayerSneezeHandler.OnSneezeTriggered += HandleStunTriggered;
        }

        private void OnDisable()
        {
            PlayerHealthSystem.OnHealthChanged -= UpdateHealthUI;
            PlayerHealthSystem.OnShieldChanged -= UpdateShieldUI;
            PlayerAmmoSystem.OnArrowCountChanged -= UpdateArrowUI;
            PlayerAbilityTracker.OnBowUnlocked -= UpdateBowVisibility;
            PlayerAbilityTracker.OnSneezeUnlocked -= UpdateSneezeVisibility;
            PlayerSneezeHandler.OnSneezeTriggered -= HandleStunTriggered;
        }

        private void Start()
        {
            PlayerAbilityTracker tracker = FindFirstObjectByType<PlayerAbilityTracker>();
            UpdateBowVisibility(tracker != null && tracker.HasBow);
            UpdateSneezeVisibility(tracker != null && tracker.HasSneeze);
            ResetStunCooldownVisuals();
        }

        private void Update()
        {
            if (stunCooldownRemaining > 0f)
            {
                stunCooldownRemaining -= Time.deltaTime;

                if (stunCooldownRemaining <= 0f)
                {
                    ResetStunCooldownVisuals();
                }
                else
                {
                    if (stunCooldownOverlay != null)
                    {
                        stunCooldownOverlay.fillAmount = Mathf.Clamp01(stunCooldownRemaining / stunTotalCooldown);
                    }

                    if (stunCooldownText != null)
                    {
                        stunCooldownText.gameObject.SetActive(true);
                        stunCooldownText.text = stunCooldownRemaining.ToString("F1");
                    }
                }
            }
        }

        private void InitializeHitOverlay(Image overlayImage)
        {
            if (overlayImage == null) return;

            if (!overlayImage.gameObject.activeSelf)
            {
                overlayImage.gameObject.SetActive(true);
            }

            overlayImage.enabled = false;
        }

        private void SetHitOverlaysActive(bool isActive)
        {
            if (healthBarHitImage != null)
            {
                healthBarHitImage.enabled = isActive;
            }

            if (frogFaceHitImage != null)
            {
                frogFaceHitImage.enabled = isActive;
            }
        }

        /// <summary>
        /// Updates health slider bounds, current value, and triggers the hit visual effect when damage is taken.
        /// </summary>
        private void UpdateHealthUI(int currentHealth, int maxHealth)
        {
            bool tookDamage = lastKnownHealth != -1 && currentHealth < lastKnownHealth;
            lastKnownHealth = currentHealth;

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            if (tookDamage)
            {
                TriggerHealthHitEffect();
            }
        }

        private void TriggerHealthHitEffect()
        {
            if (healthHitCoroutine != null)
            {
                StopCoroutine(healthHitCoroutine);
            }

            healthHitCoroutine = StartCoroutine(HealthHitEffectRoutine());
        }

        private IEnumerator HealthHitEffectRoutine()
        {
            SetHitOverlaysActive(true);

            yield return new WaitForSeconds(hitEffectDuration);

            SetHitOverlaysActive(false);
            healthHitCoroutine = null;
        }

        /// <summary>
        /// Toggles the visibility of the helmet icon on the HUD based on equipped state.
        /// </summary>
        private void UpdateShieldUI(bool isShieldOn)
        {
            if (shieldIcon != null)
            {
                shieldIcon.enabled = isShieldOn;
            }
        }

        /// <summary>
        /// Updates the arrow text counter.
        /// </summary>
        private void UpdateArrowUI(int currentArrows)
        {
            if (arrowText != null)
            {
                arrowText.text = currentArrows.ToString();
            }
        }

        private void UpdateBowVisibility(bool isUnlocked)
        {
            if (arrowHudContainer != null)
            {
                arrowHudContainer.SetActive(isUnlocked);
            }
        }

        private void UpdateSneezeVisibility(bool isUnlocked)
        {
            if (stunHudContainer != null)
            {
                stunHudContainer.SetActive(isUnlocked);
            }

            if (isUnlocked)
            {
                ResetStunCooldownVisuals();
            }
        }

        private void HandleStunTriggered(float remaining, float total)
        {
            stunTotalCooldown = Mathf.Max(0.01f, total);
            stunCooldownRemaining = remaining;

            if (stunCooldownOverlay != null)
            {
                stunCooldownOverlay.fillAmount = 1f;
            }
        }

        private void ResetStunCooldownVisuals()
        {
            stunCooldownRemaining = 0f;

            if (stunCooldownOverlay != null)
            {
                stunCooldownOverlay.fillAmount = 0f;
            }

            if (stunCooldownText != null)
            {
                stunCooldownText.gameObject.SetActive(false);
            }
        }
    }
}