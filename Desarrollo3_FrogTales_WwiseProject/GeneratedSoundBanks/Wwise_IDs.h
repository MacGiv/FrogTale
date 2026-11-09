/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_ENEMY_ALERT_ATTACK = 466760000U;
        static const AkUniqueID PLAY_ENEMY_ATTACK = 2257393085U;
        static const AkUniqueID PLAY_ENEMY_BOOGER_HIT = 1588384297U;
        static const AkUniqueID PLAY_ENEMY_COMBAT_ALERT_FOX = 3812302960U;
        static const AkUniqueID PLAY_ENEMY_COMBAT_ALERT_OWL = 3676654925U;
        static const AkUniqueID PLAY_ENEMY_DAMAGE = 3570856298U;
        static const AkUniqueID PLAY_ENEMY_DEAD = 2311449283U;
        static const AkUniqueID PLAY_ENV_DOORS_OPEN = 4062598144U;
        static const AkUniqueID PLAY_ENV_FLY_HEALTH = 3947027502U;
        static const AkUniqueID PLAY_ENV_ITEMS_PICKUP = 2861822361U;
        static const AkUniqueID PLAY_ENV_PLATFORM = 304580579U;
        static const AkUniqueID PLAY_ENV_PLAYER_PUSHBUTTON = 917757380U;
        static const AkUniqueID PLAY_ENV_ROCK_DROP = 1464352939U;
        static const AkUniqueID PLAY_ENV_ROCK_PUSHBUTTON = 445041756U;
        static const AkUniqueID PLAY_MX = 2447410425U;
        static const AkUniqueID PLAY_PLAYER_ARROW_HIT = 3850900273U;
        static const AkUniqueID PLAY_PLAYER_BOOGER_SHOT = 3262794997U;
        static const AkUniqueID PLAY_PLAYER_BOW_SHOT = 2585949963U;
        static const AkUniqueID PLAY_PLAYER_DAMAGE = 3638125099U;
        static const AkUniqueID PLAY_PLAYER_ENEMY_MASK_REMOVE = 1071460542U;
        static const AkUniqueID PLAY_PLAYER_FALL_WATER = 3772621023U;
        static const AkUniqueID PLAY_PLAYER_FOOTSTEPS = 98439365U;
        static const AkUniqueID PLAY_PLAYER_TONGUE_HIT = 4231385810U;
        static const AkUniqueID PLAY_PLAYER_VOX = 1431043311U;
        static const AkUniqueID PLAY_TEST_SOUND = 3211564518U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace MUSIC_STATES
        {
            static const AkUniqueID GROUP = 1690668539U;

            namespace STATE
            {
                static const AkUniqueID BOSSFIGHT = 580146960U;
                static const AkUniqueID DUNGEON = 608898761U;
                static const AkUniqueID GAMEPLAY = 89505537U;
                static const AkUniqueID MENU = 2607556080U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OVERWORLD = 1562068129U;
                static const AkUniqueID SHOP = 251412225U;
            } // namespace STATE
        } // namespace MUSIC_STATES

    } // namespace STATES

    namespace SWITCHES
    {
        namespace ARROW_HIT
        {
            static const AkUniqueID GROUP = 2004922858U;

            namespace SWITCH
            {
                static const AkUniqueID GRASS = 4248645337U;
                static const AkUniqueID METAL = 2473969246U;
                static const AkUniqueID WOOD = 2058049674U;
            } // namespace SWITCH
        } // namespace ARROW_HIT

        namespace ENEMY_ALERT
        {
            static const AkUniqueID GROUP = 493036092U;

            namespace SWITCH
            {
                static const AkUniqueID BOSS = 1560169506U;
                static const AkUniqueID FOX = 1083137540U;
                static const AkUniqueID OWL = 679047689U;
            } // namespace SWITCH
        } // namespace ENEMY_ALERT

        namespace ENEMY_DEAD
        {
            static const AkUniqueID GROUP = 3805217580U;

            namespace SWITCH
            {
                static const AkUniqueID BOSS = 1560169506U;
                static const AkUniqueID FOX = 1083137540U;
                static const AkUniqueID OWL = 679047689U;
            } // namespace SWITCH
        } // namespace ENEMY_DEAD

        namespace FOOTSTEPS
        {
            static const AkUniqueID GROUP = 2385628198U;

            namespace SWITCH
            {
                static const AkUniqueID GRASS = 4248645337U;
                static const AkUniqueID LARGEGRASS = 2405689736U;
                static const AkUniqueID STONE = 1216965916U;
            } // namespace SWITCH
        } // namespace FOOTSTEPS

    } // namespace SWITCHES

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID MAIN = 3161908922U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID MAIN_AUDIO_BUS = 2246998526U;
        static const AkUniqueID MUSIC = 3991942870U;
        static const AkUniqueID SFX = 393239870U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
