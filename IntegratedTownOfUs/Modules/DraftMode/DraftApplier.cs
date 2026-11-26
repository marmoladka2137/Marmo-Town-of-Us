using AmongUs.GameOptions;


namespace TownOfUs.Modules.DraftMode
{
    public static class DraftApplier
    {
        public static Action<byte, string, ushort> OnAssignRoleRequested = DefaultAssignmentHandler;

        internal static List<DraftSlotState> PendingDraftStates = new();

        public static void StorePendingDraftStates(IReadOnlyList<DraftSlotState> slotStates)
        {
            if (slotStates == null) return;
            PendingDraftStates = slotStates.Where(s => s.HasPicked).Select(s => new DraftSlotState { PlayerId=s.PlayerId, SlotNumber=s.SlotNumber, ChosenRoleId=s.ChosenRoleId, HasPicked=true, ForcedRoleName=s.ForcedRoleName }).ToList();
        }

        public static void ApplyDraftResults(IReadOnlyList<DraftSlotState> slotStates)
        {
            if (slotStates == null) return;

            foreach (var s in slotStates)
            {
                var roleName = s.ForcedRoleName;
                if (string.IsNullOrEmpty(roleName))
                    roleName = DraftRolePool.GetRoleNameFromId(s.ChosenRoleId);

                try
                {
                    OnAssignRoleRequested?.Invoke(s.PlayerId, roleName, s.ChosenRoleId);
                }
                catch (Exception e)
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"DraftApplier: assignment handler threw: {e}");
                }
            }
        }

        private static void DefaultAssignmentHandler(byte playerId, string roleName, ushort roleId)
        {
            if (!AmongUsClient.Instance.AmHost)
                return;

            var player = MiscUtils.PlayerById(playerId);

            if (player == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"DraftApplier: no PlayerControl found for player {playerId}, cannot assign role {roleName}");
                return;
            }


            player.RpcSetRole((RoleTypes)roleId);
        }
    }
}