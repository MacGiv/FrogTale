
namespace FrogGame.Gameplay
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Reads inputs using Unity's New Input System.
    /// Maneja la lectura de entradas con el nuevo sistema de inputs de Unity.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Action References")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference tongueAction;
        [SerializeField] private InputActionReference arrowAction;
        [SerializeField] private InputActionReference stunAction;
        [SerializeField] private InputActionReference pauseAction;

        public Vector2 MoveInput { get; private set; }
        public bool TonguePressed { get; private set; }
        public bool ArrowPressed { get; private set; }
        public bool StunPressed { get; private set; }

        public event Action OnPausePressed;

        private bool isInputEnabled = true;

        private void OnEnable()
        {
            RegisterInputCallbacks();
            EnableInput();

            if (pauseAction != null)
            {
                pauseAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            UnregisterInputCallbacks();
            DisableInput();

            if (pauseAction != null)
            {
                pauseAction.action.Disable();
            }
        }

        private void Update()
        {
            if (isInputEnabled && moveAction != null)
            {
                MoveInput = moveAction.action.ReadValue<Vector2>();
            }
        }

        /// <summary>
        /// Enables input processing and restores action listeners.
        /// Habilita el procesamiento de entradas y restaura los listeners.
        /// </summary>
        public void EnableInput()
        {
            isInputEnabled = true;

            if (moveAction != null) moveAction.action.Enable();
            if (tongueAction != null) tongueAction.action.Enable();
            if (arrowAction != null) arrowAction.action.Enable();
            if (stunAction != null) stunAction.action.Enable();
        }

        /// <summary>
        /// Disables input processing and clears any buffered input values.
        /// Deshabilita las entradas y limpia cualquier valor o flag de entrada retenido.
        /// </summary>
        public void DisableInput()
        {
            isInputEnabled = false;

            if (moveAction != null) moveAction.action.Disable();
            if (tongueAction != null) tongueAction.action.Disable();
            if (arrowAction != null) arrowAction.action.Disable();
            if (stunAction != null) stunAction.action.Disable();

            ResetInputs();
        }

        /// <summary>
        /// Resets all directional and button state values to default.
        /// Reinicia todas las entradas de dirección y botones a cero/falso.
        /// </summary>
        public void ResetInputs()
        {
            MoveInput = Vector2.zero;
            TonguePressed = false;
            ArrowPressed = false;
            StunPressed = false;
        }

        private void RegisterInputCallbacks()
        {
            if (tongueAction != null) tongueAction.action.performed += OnTonguePerformed;
            if (arrowAction != null) arrowAction.action.performed += OnArrowPerformed;
            if (stunAction != null) stunAction.action.performed += OnStunPerformed;
            if (pauseAction != null) pauseAction.action.performed += OnPausePerformed;
        }

        private void UnregisterInputCallbacks()
        {
            if (tongueAction != null) tongueAction.action.performed -= OnTonguePerformed;
            if (arrowAction != null) arrowAction.action.performed -= OnArrowPerformed;
            if (stunAction != null) stunAction.action.performed -= OnStunPerformed;
            if (pauseAction != null) pauseAction.action.performed -= OnPausePerformed;
        }

        private void OnTonguePerformed(InputAction.CallbackContext context)
        {
            if (isInputEnabled) TonguePressed = true;
        }

        private void OnArrowPerformed(InputAction.CallbackContext context)
        {
            if (isInputEnabled) ArrowPressed = true;
        }

        private void OnStunPerformed(InputAction.CallbackContext context)
        {
            if (isInputEnabled) StunPressed = true;
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            OnPausePressed?.Invoke();
        }

        public void ConsumeTongueInput() => TonguePressed = false;
        public void ConsumeArrowInput() => ArrowPressed = false;
        public void ConsumeStunInput() => StunPressed = false;
    }
}