namespace TouMarmo.Modules;
public static class NewRoleRules
{
    public static bool ExecutionerChoiceOpen(bool firstMeetingStarted,bool used) => !firstMeetingStarted && !used;
    public static bool DoctorReady(int livingCrew, int markedCrew, float percent) => livingCrew > 0 &&
        markedCrew >= (int)Math.Ceiling(livingCrew * Math.Clamp(percent,25f,100f) / 100f);
    public static bool CanSteal(int used, int maximum, bool usedThisRound, bool active) => !active && !usedThisRound && (maximum == 0 || used < maximum);
    public static bool TripleShotUsed(int protectedMistakes) => protectedMistakes >= 2;
}
