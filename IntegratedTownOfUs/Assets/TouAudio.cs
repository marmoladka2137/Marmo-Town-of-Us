using AmongUs.GameOptions;
using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAudio
{
    // THIS FILE SHOULD ONLY HOLD AUDIO
    public static LoadableAsset<AudioClip> ImpostorIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Impostor)!;
    public static LoadableAsset<AudioClip> CrewmateIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Crewmate)!;
    public static LoadableAsset<AudioClip> ScientistIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Scientist)!;
    public static LoadableAsset<AudioClip> EngineerIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Engineer)!;
    public static LoadableAsset<AudioClip> ShapeshifterIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Shapeshifter)!;
    public static LoadableAsset<AudioClip> NoisemakerIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Noisemaker)!;
    public static LoadableAsset<AudioClip> PhantomIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Phantom)!;
    public static LoadableAsset<AudioClip> TrackerIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Tracker)!;
    public static LoadableAsset<AudioClip> DetectiveIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Detective)!;
    public static LoadableAsset<AudioClip> JudgeIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Judge)!;
    public static LoadableAsset<AudioClip> ViperIntroSound =>
        CustomRoleUtils.GetIntroSound(RoleTypes.Viper)!;
    public static LoadableAsset<AudioClip> HexBombDetonateSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("HexBomb");
    public static LoadableAsset<AudioClip> HexBombAlarmSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("HexBombAlarm");
    public static LoadableAsset<AudioClip> NoisemakerDeathSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("NoisemakerDeath");

    public static LoadableAsset<AudioClip> TrackerActivateSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("TrackerActivate");

    public static LoadableAsset<AudioClip> TrackerDeactivateSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("TrackerDeactivate");

    public static LoadableAsset<AudioClip> TrapperPlaceSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("TrapperPlace");

    public static LoadableAsset<AudioClip> QuestionSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("QuestionSound");

    public static LoadableAsset<AudioClip> ToppatIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ToppatIntro");

    public static LoadableAsset<AudioClip> DeputyIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("DeputyIntroNew");

    public static LoadableAsset<AudioClip> DeputyIntroSoundOld { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("DeputyIntro");

    public static LoadableAsset<AudioClip> VampIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("VampireIntro");

    public static LoadableAsset<AudioClip> ProsIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ProsIntro");

    public static LoadableAsset<AudioClip> ProsecuteSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ProsecuteAudio");

    public static LoadableAsset<AudioClip> SpyIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SpyIntro");

    public static LoadableAsset<AudioClip> OtherIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("OtherIntro");

    public static LoadableAsset<AudioClip> SheriffIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SheriffIntro");

    public static LoadableAsset<AudioClip> VigiIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("VigilanteIntro");

    public static LoadableAsset<AudioClip> AltruistReviveSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("AltruistIntro");

    public static LoadableAsset<AudioClip> JailSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("JailSound");

    public static LoadableAsset<AudioClip> MayorRevealSound { get; set; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MayorRevealSound");

    public static LoadableAsset<AudioClip> PoliticianIntroSound { get; set; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MayorRevealSound");

    public static LoadableAsset<AudioClip> MediumIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MediumIntro");

    public static LoadableAsset<AudioClip> TimeLordIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("TimeLordIntro");

    public static LoadableAsset<AudioClip> EngiFix1 { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("Fix1");

    public static LoadableAsset<AudioClip> EngiFix2 { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("Fix2");

    public static LoadableAsset<AudioClip> EngiFix3 { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("Fix3");

    public static LoadableAsset<AudioClip> GuardianAngelSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("GuardianAngelProtect");

    public static LoadableAsset<AudioClip> DiscoveredSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("Discovered");

    public static LoadableAsset<AudioClip> ChefSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ChefRing");

    public static LoadableAsset<AudioClip> GlitchSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("GlitchIntro");

    public static LoadableAsset<AudioClip> HackedSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("HackedSound");

    public static LoadableAsset<AudioClip> UnhackedSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("UnhackedSound");

    public static LoadableAsset<AudioClip> MimicSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MimicSound");

    public static LoadableAsset<AudioClip> UnmimicSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("UnmimicSound");

    public static LoadableAsset<AudioClip> ArsoIgniteSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ArsonistIgnite");

    public static LoadableAsset<AudioClip> WerewolfRampageSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("WerewolfRampage");

    public static LoadableAsset<AudioClip> GrenadeSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("GrenadeThrow");

    public static LoadableAsset<AudioClip> WarlockIntroSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("WarlockIntro");

    public static LoadableAsset<AudioClip> SwooperActivateSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SwooperActivate");

    public static LoadableAsset<AudioClip> SwooperDeactivateSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SwooperDeactivate");

    public static LoadableAsset<AudioClip> MineSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MineSound");

    public static LoadableAsset<AudioClip> EscapistMarkSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("EscapistMark");

    public static LoadableAsset<AudioClip> EscapistRecallSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("EscapistRecall");

    public static LoadableAsset<AudioClip> JanitorCleanSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("JanitorClean");

    public static LoadableAsset<AudioClip> TribunalSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MarshalTribunal");

    public static LoadableAsset<AudioClip> LaserKillSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ShootLaserMeeting");
    public static LoadableAsset<AudioClip> SentryPlaceSound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SentryPlace");

    public static LoadableAsset<AudioClip> DenySound { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("Deny");

    public static LoadableAsset<AudioClip> SentryIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SentryIntro");

    public static LoadableAsset<AudioClip> SinisterIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SinisterIntro");

    public static LoadableAsset<AudioClip> PotionIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("PotionIntro");

    public static LoadableAsset<AudioClip> ScreamIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ScreamIntro");

    public static LoadableAsset<AudioClip> ExplodeIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("ExplodeIntro");

    public static LoadableAsset<AudioClip> SuspenseIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SuspenseIntro");

    public static LoadableAsset<AudioClip> SneakyIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("SneakyIntro");

    public static LoadableAsset<AudioClip> MirrorcasterIntro { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MirrorcasterIntro");

    public static LoadableAsset<AudioClip> MirrorcasterShatter { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("MirrorcasterShatter");

    public static LoadableAsset<AudioClip> BombExplode { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("BombExplode");

    public static LoadableAsset<AudioClip> DeputyReveal { get; } =
        new TouMarmo.Modules.OriginalMotion.OriginalAudioAsset("DeputyReveal");

    public static void PlaySound(LoadableAsset<AudioClip> clip, float vol = 1f)
    {
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(clip.LoadAsset(), false, vol);
        }
    }
}
