using UnityEngine;
using UnityEngine.SceneManagement;
using FrogGame.Core;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Event")]
    [SerializeField] private AK.Wwise.Event playMX;

    [Header("Location / Context States")]
    [SerializeField] private AK.Wwise.State bossFightState;
    [SerializeField] private AK.Wwise.State dungeonState;
    [SerializeField] private AK.Wwise.State mangroveState;
    [SerializeField] private AK.Wwise.State menuState;
    [SerializeField] private AK.Wwise.State plateauState;
    [SerializeField] private AK.Wwise.State shopState;

    private uint musicPlayingId = 0u;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case SceneNames.MAIN_MENU:

                SetMenu();
                PlayMusic();

                break;
            case SceneNames.PLATEAU:

                SetPlateau();
                PlayMusic();

                break;
            case SceneNames.MANGROVE:

                SetMangrove();
                PlayMusic();

                break;
        }
    }

    #region Play / Stop
    public void PlayMusic()
    {
        if (musicPlayingId != 0u) return;

        if (playMX != null && playMX.IsValid())
        {
            musicPlayingId = playMX.Post(gameObject);
        }
    }

    public void StopMusic()
    {
        if (playMX != null && playMX.IsValid())
        {
            playMX.Stop(gameObject);
            musicPlayingId = 0u;
        }
    }
    #endregion

    #region State Changes
    public void SetState(AK.Wwise.State targetState)
    {
        if (targetState != null && targetState.IsValid())
        {
            targetState.SetValue();
        }
    }

    public void SetBossFight() => SetState(bossFightState);
    public void SetDungeon() => SetState(dungeonState);
    public void SetMangrove() => SetState(mangroveState);
    public void SetMenu() => SetState(menuState);
    public void SetPlateau() => SetState(plateauState);
    public void SetShop() => SetState(shopState);
    #endregion
}