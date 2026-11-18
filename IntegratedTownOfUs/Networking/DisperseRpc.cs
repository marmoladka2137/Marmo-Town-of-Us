// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Extensions;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Networking;

[RegisterCustomRpc((uint)TownOfUsRpc.Disperse)]
public sealed class DisperseRpc(TouMarmo.TouMarmoPlugin plugin, uint id)
    : PlayerCustomRpc<TouMarmo.TouMarmoPlugin, Dictionary<byte, Vector2>>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    public override void Write(MessageWriter writer, Dictionary<byte, Vector2>? data)
    {
        if (data == null)
        {
            writer.Write((byte)0);
            return;
        }

        writer.Write((byte)data.Count);
        foreach (var kvp in data)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }
    }

    public override Dictionary<byte, Vector2> Read(MessageReader reader)
    {
        var count = reader.ReadByte();
        var data = new Dictionary<byte, Vector2>(count);
        for (var i = 0; i < count; i++)
        {
            var key = reader.ReadByte();
            var value = reader.ReadVector2();
            data[key] = value;
        }

        return data;
    }

    public override void Handle(PlayerControl innerNetObject, Dictionary<byte, Vector2>? data)
    {
        if (data == null || data.Count == 0)
        {
            return;
        }

        Coroutines.Start(DisperserModifier.CoDisperse(data));
    }
}