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
        [Tooltip("Sprites for the health bar hit overlay indexed by current health (0 = 0 HP, 1 = 1 HP, 2 = 2 HP, 3 = 3 HP).")]
        [SerializeField] private Sprite[] healthBarHitSprites;
        [Min(1)]
        [SerializeField] private int hitFlashCount = 3;
        [Min(0.01f)]
        [SerializeField] private float hitFlashInterval = 0.08f;

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

            if (healthHitCoroutine != null)
            {
                StopCoroutine(healthHitCoroutine);
                healthHitCoroutine = null;
            }

            SetHitOverlaysActive(false);
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
        /// Updates health slider bounds, syncs the health bar hit sprite with current health, and triggers the hit blink effect.
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

            UpdateHealthBarHitSprite(currentHealth);

            if (tookDamage)
            {
                TriggerHealthHitEffect();
            }
        }

        private void UpdateHealthBarHitSprite(int currentHealth)
        {
            if (healthBarHitImage == null || healthBarHitSprites == null || healthBarHitSprites.Length == 0) return;

            int spriteIndex = Mathf.Clamp(currentHealth, 0, healthBarHitSprites.Length - 1);
            if (healthBarHitSprites[spriteIndex] != null)
            {
                healthBarHitImage.sprite = healthBarHitSprites[spriteIndex];
            }
        }

        private void TriggerHealthHitEffect()
        {
            if (healthHitCoroutine != null)
            {
                StopCoroutine(healthHitCoroutine);
                healthHitCoroutine = null;
            }

            SetHitOverlaysActive(false);
            healthHitCoroutine = StartCoroutine(HealthHitEffectRoutine());
        }

        private IEnumerator HealthHitEffectRoutine()
        {
            int flashes = Mathf.Max(1, hitFlashCount);
            float interval = Mathf.Max(0.01f, hitFlashInterval);

            for (int i = 0; i < flashes; i++)
            {
                SetHitOverlaysActive(true);
                yield return new WaitForSeconds(interval);

                SetHitOverlaysActive(false);

                if (i < flashes - 1)
                {
                    yield return new WaitForSeconds(interval);
                }
            }

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