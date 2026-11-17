namespace FrogGame.Gameplay
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Manages player arrow inventory and ammo consumption.
    /// </summary>
    public class PlayerAmmoSystem : MonoBehaviour
    {
        [Header("Ammo Settings")]
        [SerializeField] private int currentArrows = 10;
        [SerializeField] private int maxArrows = 25;

        public int CurrentArrows => currentArrows;
        public bool CanShoot => currentArrows > 0;

        public static event Action<int> OnArrowCountChanged;

        private void Start()
        {
            OnArrowCountChanged?.Invoke(currentArrows);
        }

        /// <summary>
        /// Tries to consume one arrow if available.
        /// </summary>
        public bool TryConsumeArrow()
        {
            if (!CanShoot) return false;

            currentArrows--;
            OnArrowCountChanged?.Invoke(currentArrows);

            return true;
        }

        /// <summary>
        /// Adds arrows to the player's inventory up to maxArrows.
        /// </summary>
        public void AddArrows(int amount)
        {
            if (amount <= 0 || currentArrows >= maxArrows) return;

            currentArrows = Mathf.Min(maxArrows, currentArrows + amount);
            OnArrowCountChanged?.Invoke(currentArrows);
        }
    }
}