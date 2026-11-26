namespace TouMarmo.Modules.LobbyMimic;

public static class MimicCommands
{
    public static bool Handle(ChatController chat,string text)
    {
        var parts=text.Trim().Split(' ',2,StringSplitOptions.RemoveEmptyEntries);
        if(parts.Length==0 || !parts[0].Equals("/mimic",StringComparison.OrdinalIgnoreCase) &&
            !parts[0].Equals("/unmimic",StringComparison.OrdinalIgnoreCase)) return false;
        string message;
        if(!MimicRuntime.CanCommand) message="Only the lobby host can use Mimic commands before the game starts.";
        else if(parts[0].Equals("/unmimic",StringComparison.OrdinalIgnoreCase))
        {
            if(parts.Length!=1) message="Usage: /unmimic";
            else { MimicRuntime.RpcStop(PlayerControl.LocalPlayer,MimicRuntime.Session.Version);message="Mimic disabled. Original names and cosmetics restored."; }
        }
        else if(parts.Length!=2 || string.IsNullOrWhiteSpace(parts[1])) message="Usage: /mimic <player name>";
        else
        {
            var name=parts[1].Trim();
            var matches=PlayerControl.AllPlayerControls.ToArray().Where(p=>p && p.Data && !p.Data.Disconnected &&
                p.Data.PlayerName.Equals(name,StringComparison.OrdinalIgnoreCase)).ToArray();
            if(matches.Length==0) message="Player not found. Use their full original lobby name.";
            else if(matches.Length>1) message="More than one player has that name. Rename one before using Mimic.";
            else {MimicRuntime.Begin(matches[0]);message="Mimic enabled until the end of the next game. Use /unmimic in the lobby to cancel.";}
        }
        MiscUtils.AddSystemChat(PlayerControl.LocalPlayer.Data,"<color=#8BFDFD>System</color>",message);
        chat.freeChatField.Clear();chat.quickChatMenu.Clear();chat.quickChatField.Clear();chat.UpdateChatMode();
        return true;
    }
}
