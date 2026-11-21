using UnityEngine;
using AK.Wwise;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Main SoundBank")]
    [SerializeField] private AK.Wwise.Bank mainBank;

    [Header("Enemy Events")]
    [SerializeField] private AK.Wwise.Event playEnemyAttack;
    [SerializeField] private AK.Wwise.Event playEnemyBoogerHit;
    [SerializeField] private AK.Wwise.Event playEnemyCombatAlertFox;
    [SerializeField] private AK.Wwise.Event playEnemyCombatAlertOwl;
    [SerializeField] private AK.Wwise.Event playEnemyDamage;
    [SerializeField] private AK.Wwise.Event playEnemyDead;

    [Header("Environment Events")]
    [SerializeField] private AK.Wwise.Event playEnvDoorsOpen;
    [SerializeField] private AK.Wwise.Event playEnvFlyHealth;
    [SerializeField] private AK.Wwise.Event playEnvItemsPickup;
    [SerializeField] private AK.Wwise.Event playEnvRockDrop;
    [SerializeField] private AK.Wwise.Event playEnvRockPushButton;
    [SerializeField] private AK.Wwise.Event playEnvPlatform;
    [SerializeField] private AK.Wwise.Event stopEnvPlatform;
    [SerializeField] private AK.Wwise.Event playEnvPlatformPlayer;

    [Header("Player Events")]
    [SerializeField] private AK.Wwise.Event playPlayerArrowHit;
    [SerializeField] private AK.Wwise.Event playPlayerBoogerShot;
    [SerializeField] private AK.Wwise.Event playPlayerBowShot;
    [SerializeField] private AK.Wwise.Event playPlayerDamage;
    [SerializeField] private AK.Wwise.Event playPlayerEnemyMaskRemove;
    [SerializeField] private AK.Wwise.Event playPlayerFallWater;
    [SerializeField] private AK.Wwise.Event playPlayerFootsteps;
    [SerializeField] private AK.Wwise.Event playPlayerTongueHit;

    [Header("UI Events")]
    [SerializeField] private AK.Wwise.Event playUIClickButton;
    [SerializeField] private AK.Wwise.Event playUIHoverButton;
    [SerializeField] private AK.Wwise.Event playUIStartMenu;

    [Header("Enemy Switches")]
    [SerializeField] private AK.Wwise.Switch enemyDeadBoss;
    [SerializeField] private AK.Wwise.Switch enemyDeadFox;
    [SerializeField] private AK.Wwise.Switch enemyDeadOwl;

    [Header("Arrow Hit Switches")]
    [SerializeField] private AK.Wwise.Switch arrowHitGrass;
    [SerializeField] private AK.Wwise.Switch arrowHitMetal;
    [SerializeField] private AK.Wwise.Switch arrowHitWood;

    [Header("Player Footsteps Switches")]
    [SerializeField] private AK.Wwise.Switch playerFootstepsConcrete;
    [SerializeField] private AK.Wwise.Switch playerFootstepsGrassLarge;
    [SerializeField] private AK.Wwise.Switch playerFootstepsGrassShort;

    [Header("Player Tongue Hit Switches")]
    [SerializeField] private AK.Wwise.Switch playerTongueHitRandom;
    [SerializeField] private AK.Wwise.Switch playerTongueHitRock;
    [SerializeField] private AK.Wwise.Switch playerTongueShot;

    [Header("Testing")]
    [SerializeField] private AK.Wwise.Event playTestSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (mainBank != null && mainBank.IsValid())
        {
            mainBank.Load();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this && mainBank != null && mainBank.IsValid())
        {
            mainBank.Unload();
        }
    }

    #region Generic Helper
    public void PostEvent(AK.Wwise.Event akEvent, GameObject emitter = null)
    {
        if (akEvent != null && akEvent.IsValid())
        {
            akEvent.Post(emitter != null ? emitter : gameObject);
        }
    }

    public void SetSwitch(AK.Wwise.Switch akSwitch, GameObject emitter)
    {
        if (akSwitch != null && akSwitch.IsValid() && emitter != null)
        {
            akSwitch.SetValue(emitter);
        }
    }
    #endregion

    #region Play Methods
    public void PlayEnemyAttack(GameObject emitter = null) => PostEvent(playEnemyAttack, emitter);
    public void PlayEnemyBoogerHit(GameObject emitter = null) => PostEvent(playEnemyBoogerHit, emitter);
    public void PlayEnemyCombatAlertFox(GameObject emitter = null) => PostEvent(playEnemyCombatAlertFox, emitter);
    public void PlayEnemyCombatAlertOwl(GameObject emitter = null) => PostEvent(playEnemyCombatAlertOwl, emitter);
    public void PlayEnemyDamage(GameObject emitter = null) => PostEvent(playEnemyDamage, emitter);
    public void PlayEnemyDead(GameObject emitter = null) => PostEvent(playEnemyDead, emitter);

    public void PlayEnvDoorsOpen(GameObject emitter = null) => PostEvent(playEnvDoorsOpen, emitter);
    public void PlayEnvFlyHealth(GameObject emitter = null) => PostEvent(playEnvFlyHealth, emitter);
    public void PlayEnvItemsPickup(GameObject emitter = null) => PostEvent(playEnvItemsPickup, emitter);
    public void PlayEnvRockDrop(GameObject emitter = null) => PostEvent(playEnvRockDrop, emitter);
    public void PlayEnvRockPushButton(GameObject emitter = null) => PostEvent(playEnvRockPushButton, emitter);
    public void PlayEnvPlatform(GameObject emitter = null) => PostEvent(playEnvPlatform, emitter);
    public void StopEnvPlatform(GameObject emitter = null) => PostEvent(stopEnvPlatform, emitter);
    public void PlayEnvPlatformPlayer(GameObject emitter = null) => PostEvent(playEnvPlatformPlayer, emitter);

    public void PlayPlayerArrowHit(GameObject emitter = null) => PostEvent(playPlayerArrowHit, emitter);
    public void PlayPlayerBoogerShot(GameObject emitter = null) => PostEvent(playPlayerBoogerShot, emitter);
    public void PlayPlayerBowShot(GameObject emitter = null) => PostEvent(playPlayerBowShot, emitter);
    public void PlayPlayerDamage(GameObject emitter = null) => PostEvent(playPlayerDamage, emitter);
    public void PlayPlayerEnemyMaskRemove(GameObject emitter = null) => PostEvent(playPlayerEnemyMaskRemove, emitter);
    public void PlayPlayerFallWater(GameObject emitter = null) => PostEvent(playPlayerFallWater, emitter);
    public void PlayPlayerFootsteps(GameObject emitter = null) => PostEvent(playPlayerFootsteps, emitter);
    public void PlayPlayerTongueHit(GameObject emitter = null) => PostEvent(playPlayerTongueHit, emitter);

    public void PlayUIClickButton(GameObject emitter = null) => PostEvent(playUIClickButton, emitter);
    public void PlayUIHoverButton(GameObject emitter = null) => PostEvent(playUIHoverButton, emitter);
    public void PlayUIStartMenu(GameObject emitter = null) => PostEvent(playUIStartMenu, emitter);

    public void PlayTestSound(GameObject emitter = null) => PostEvent(playTestSound, emitter);
    #endregion

    #region Switch Methods
    public void SetEnemyDeadBoss(GameObject emitter) => SetSwitch(enemyDeadBoss, emitter);
    public void SetEnemyDeadFox(GameObject emitter) => SetSwitch(enemyDeadFox, emitter);
    public void SetEnemyDeadOwl(GameObject emitter) => SetSwitch(enemyDeadOwl, emitter);

    public void SetArrowHitGrass(GameObject emitter) => SetSwitch(arrowHitGrass, emitter);
    public void SetArrowHitMetal(GameObject emitter) => SetSwitch(arrowHitMetal, emitter);
    public void SetArrowHitWood(GameObject emitter) => SetSwitch(arrowHitWood, emitter);

    public void SetPlayerFootstepsConcrete(GameObject emitter) => SetSwitch(playerFootstepsConcrete, emitter);
    public void SetPlayerFootstepsGrassLarge(GameObject emitter) => SetSwitch(playerFootstepsGrassLarge, emitter);
    public void SetPlayerFootstepsGrassShort(GameObject emitter) => SetSwitch(playerFootstepsGrassShort, emitter);

    public void SetPlayerTongueHitRandom(GameObject emitter) => SetSwitch(playerTongueHitRandom, emitter);
    public void SetPlayerTongueHitRock(GameObject emitter) => SetSwitch(playerTongueHitRock, emitter);
    public void SetPlayerTongueShot(GameObject emitter) => SetSwitch(playerTongueShot, emitter);
    #endregion
}