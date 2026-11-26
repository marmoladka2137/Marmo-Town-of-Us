namespace TouMarmo.Modules;

public static class FeedbackRules
{
    public static bool ShieldFlash(bool attacker, bool provider) => attacker || provider;
    public static bool GamblerSharedWin(bool confirmedNeutralWinner, bool cancelled) => confirmedNeutralWinner && !cancelled;
    public static int CorpseKills(int current, bool pending, bool succeeded) => pending && succeeded ? current + 1 : current;
    public static string[] UniqueRoles(IEnumerable<string?> names) => names
        .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}
