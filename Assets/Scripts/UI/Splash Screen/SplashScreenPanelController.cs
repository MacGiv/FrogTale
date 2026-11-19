namespace FrogGame.UI
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.InputSystem;
    using FrogGame.Core;

    public class SplashScreenPanelController : MonoBehaviour
    {
        [Header("Input System Setup")]
        [SerializeField] private InputActionReference submitAction;

        private void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnEnable()
        {
            if (submitAction != null)
            {
                submitAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (submitAction != null)
            {
                submitAction.action.Disable();
            }
        }

        private void Update()
        {
            if (submitAction != null && submitAction.action.WasPressedThisFrame())
            {
                AudioManager.Instance.PlayUIStartMenu();

                LoadNextScene();
            }
        }

        private void LoadNextScene() => SceneManager.LoadScene(SceneNames.MAIN_MENU);
    }
}