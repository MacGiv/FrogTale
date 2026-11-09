namespace FrogGame.Gameplay
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    using FrogGame.Core;

    /// <summary>
    /// Item that can be grabbed and retrieved via tongue or direct interaction.
    /// Objeto interactivo que puede ser sujetado y arrastrado por la lengua.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GrabbableItem : MonoBehaviour
    {
        public enum ItemType
        {
            Collectible,
            Shield,
            HeavyBlock,
            CarriableBox,
            AbilityBow,
            AbilitySneeze
        }

        [Header("Item Settings")]
        [SerializeField] private ItemType type = ItemType.Collectible;
        [SerializeField] private float pullSpeed = 25f;
        [SerializeField] private float stopDistance = 0.5f;

        [Header("Events")]
        [SerializeField] private UnityEvent onGrabbed;
        [SerializeField] private UnityEvent onCollected;

        private bool isBeingPulled = false;
        private Collider2D itemCollider;

        public ItemType Type => type;
        public bool IsBeingPulled => isBeingPulled;
        public Collider2D ItemCollider => itemCollider;

        private void Awake()
        {
            itemCollider = GetComponent<Collider2D>();
        }

        /// <summary>
        /// Called on tongue tip impact to initiate grab sequence and disable collisions.
        /// </summary>
        public void OnGrabStarted()
        {
            if (isBeingPulled) return;

            isBeingPulled = true;
            onGrabbed?.Invoke();

            if (itemCollider != null && type != ItemType.HeavyBlock)
            {
                itemCollider.enabled = false;
            }

            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
        }

        /// <summary>
        /// Called when the tongue finishes retracting back to the player, executing item collection logic.
        /// </summary>
        public void OnGrabCompleted(Transform pullTarget)
        {
            onCollected?.Invoke();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnvItemsPickup(gameObject);
            }

            if (pullTarget != null && TryGetComponent<IPickupable>(out var pickupable))
            {
                pickupable.Collect(pullTarget.gameObject);
            }

            switch (type)
            {
                case ItemType.Collectible:
                    break;

                case ItemType.Shield:
                    if (gameObject != null)
                    {
                        if (pullTarget != null && pullTarget.TryGetComponent<PlayerHealthSystem>(out var healthSystem))
                        {
                            healthSystem.SetShieldState(true);
                        }
                        Destroy(gameObject);
                    }
                    break;

                case ItemType.HeavyBlock:
                    if (itemCollider != null) itemCollider.enabled = true;
                    isBeingPulled = false;
                    break;

                case ItemType.CarriableBox:
                    if (pullTarget != null && pullTarget.TryGetComponent<PlayerCarryHandler>(out var carryHandler))
                    {
                        carryHandler.AttachBox(this);
                    }
                    isBeingPulled = false;
                    break;

                case ItemType.AbilityBow:
                    if (pullTarget != null && pullTarget.TryGetComponent<PlayerBrain>(out var brainBow))
                    {
                        brainBow.AbilityTracker?.UnlockBow();
                        brainBow.AmmoSystem?.AddArrows(10);
                    }
                    Destroy(gameObject);
                    break;

                case ItemType.AbilitySneeze:
                    if (pullTarget != null && pullTarget.TryGetComponent<PlayerBrain>(out var brainSneeze))
                    {
                        brainSneeze.AbilityTracker?.UnlockSneeze();
                    }
                    Destroy(gameObject);
                    break;
            }
        }

        /// <summary>
        /// Legacy grab wrapper for non-tongue pull logic.
        /// </summary>
        public void Grab(Transform pullTarget)
        {
            if (isBeingPulled) return;

            OnGrabStarted();
            StartCoroutine(LegacyPullRoutine(pullTarget));
        }

        private IEnumerator LegacyPullRoutine(Transform pullTarget)
        {
            while (pullTarget != null && Vector2.Distance(transform.position, pullTarget.position) > stopDistance)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    pullTarget.position,
                    pullSpeed * Time.deltaTime
                );
                yield return null;
            }

            OnGrabCompleted(pullTarget);
        }
    }
}