using System.Runtime.InteropServices;
using UnityEngine.Networking;


[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class GeneratedNetworkCode
{
    public static void _ReadStructSyncListItemInfo_Inventory(NetworkReader reader, Inventory.SyncListItemInfo instance)
    {
        ushort num = reader.ReadUInt16();
        instance.Clear();
        for (ushort num2 = 0; num2 < num; num2++)
        {
            instance.AddInternal(new Inventory.SyncItemInfo
            {
                id = (int)reader.ReadPackedUInt32(),
                durability = reader.ReadSingle(),
                uniq = (int)reader.ReadPackedUInt32(),
                modSight = (int)reader.ReadPackedUInt32(),
                modBarrel = (int)reader.ReadPackedUInt32(),
                modOther = (int)reader.ReadPackedUInt32()
            });
        }
    }

    public static void _WriteStructSyncListItemInfo_Inventory(NetworkWriter writer, Inventory.SyncListItemInfo value)
    {
        ushort count = value.Count;
        writer.Write(count);
        for (ushort num = 0; num < count; num++)
        {
            Inventory.SyncItemInfo item = value.GetItem(num);
            writer.WritePackedUInt32((uint)item.id);
            writer.Write(item.durability);
            writer.WritePackedUInt32((uint)item.uniq);
            writer.WritePackedUInt32((uint)item.modSight);
            writer.WritePackedUInt32((uint)item.modBarrel);
            writer.WritePackedUInt32((uint)item.modOther);
        }
    }

    public static void _WriteOffset_None(NetworkWriter writer, Offset value)
    {
        writer.Write(value.position);
        writer.Write(value.rotation);
        writer.Write(value.scale);
    }

    public static Offset _ReadOffset_None(NetworkReader reader)
    {
        return new Offset
        {
            position = reader.ReadVector3(),
            rotation = reader.ReadVector3(),
            scale = reader.ReadVector3()
        };
    }

    public static PlayerStats.HitInfo _ReadHitInfo_PlayerStats(NetworkReader reader)
    {
        return new PlayerStats.HitInfo
        {
            amount = reader.ReadSingle(),
            tool = (int)reader.ReadPackedUInt32(),
            time = (int)reader.ReadPackedUInt32(),
            attacker = reader.ReadString(),
            plyID = (int)reader.ReadPackedUInt32()
        };
    }

    public static void _WriteHitInfo_PlayerStats(NetworkWriter writer, PlayerStats.HitInfo value)
    {
        writer.Write(value.amount);
        writer.WritePackedUInt32((uint)value.tool);
        writer.WritePackedUInt32((uint)value.time);
        writer.Write(value.attacker);
        writer.WritePackedUInt32((uint)value.plyID);
    }

    public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
    {
        if (value == null)
        {
            writer.Write((ushort)0);
            return;
        }

        ushort value2 = (ushort)value.Length;
        writer.Write(value2);
        for (ushort num = 0; num < value.Length; num++)
        {
            writer.WritePackedUInt32((uint)value[num]);
        }
    }

    public static void _WritePickupInfo_Pickup(NetworkWriter writer, Pickup.PickupInfo value)
    {
        writer.Write(value.position);
        writer.Write(value.rotation);
        writer.WritePackedUInt32((uint)value.itemId);
        writer.Write(value.durability);
        writer.WritePackedUInt32((uint)value.ownerPlayerID);
        _WriteArrayInt32_None(writer, value.weaponMods);
    }

    public static int[] _ReadArrayInt32_None(NetworkReader reader)
    {
        int num = reader.ReadUInt16();
        if (num == 0)
        {
            return new int[0];
        }

        int[] array = new int[num];
        for (int i = 0; i < num; i++)
        {
            array[i] = (int)reader.ReadPackedUInt32();
        }

        return array;
    }

    public static Pickup.PickupInfo _ReadPickupInfo_Pickup(NetworkReader reader)
    {
        return new Pickup.PickupInfo
        {
            position = reader.ReadVector3(),
            rotation = reader.ReadQuaternion(),
            itemId = (int)reader.ReadPackedUInt32(),
            durability = reader.ReadSingle(),
            ownerPlayerID = (int)reader.ReadPackedUInt32(),
            weaponMods = _ReadArrayInt32_None(reader)
        };
    }

    public static PlayerPositionData _ReadPlayerPositionData_None(NetworkReader reader)
    {
        return new PlayerPositionData
        {
            position = reader.ReadVector3(),
            rotation = reader.ReadSingle(),
            playerID = (int)reader.ReadPackedUInt32()
        };
    }

    public static PlayerPositionData[] _ReadArrayPlayerPositionData_None(NetworkReader reader)
    {
        int num = reader.ReadUInt16();
        if (num == 0)
        {
            return new PlayerPositionData[0];
        }

        PlayerPositionData[] array = new PlayerPositionData[num];
        for (int i = 0; i < num; i++)
        {
            array[i] = _ReadPlayerPositionData_None(reader);
        }

        return array;
    }

    public static void _WritePlayerPositionData_None(NetworkWriter writer, PlayerPositionData value)
    {
        writer.Write(value.position);
        writer.Write(value.rotation);
        writer.WritePackedUInt32((uint)value.playerID);
    }

    public static void _WriteArrayPlayerPositionData_None(NetworkWriter writer, PlayerPositionData[] value)
    {
        if (value == null)
        {
            writer.Write((ushort)0);
            return;
        }

        ushort value2 = (ushort)value.Length;
        writer.Write(value2);
        for (ushort num = 0; num < value.Length; num++)
        {
            _WritePlayerPositionData_None(writer, value[num]);
        }
    }

    public static void _WriteInfo_Ragdoll(NetworkWriter writer, Ragdoll.Info value)
    {
        writer.Write(value.ownerHLAPI_id);
        writer.Write(value.steamClientName);
        _WriteHitInfo_PlayerStats(writer, value.deathCause);
        writer.WritePackedUInt32((uint)value.charclass);
        writer.WritePackedUInt32((uint)value.PlayerId);
    }

    public static Ragdoll.Info _ReadInfo_Ragdoll(NetworkReader reader)
    {
        return new Ragdoll.Info
        {
            ownerHLAPI_id = reader.ReadString(),
            steamClientName = reader.ReadString(),
            deathCause = _ReadHitInfo_PlayerStats(reader),
            charclass = (int)reader.ReadPackedUInt32(),
            PlayerId = (int)reader.ReadPackedUInt32()
        };
    }

    public static RoundSummary.SumInfo_ClassList _ReadSumInfo_ClassList_RoundSummary(NetworkReader reader)
    {
        return new RoundSummary.SumInfo_ClassList
        {
            class_ds = (int)reader.ReadPackedUInt32(),
            scientists = (int)reader.ReadPackedUInt32(),
            chaos_insurgents = (int)reader.ReadPackedUInt32(),
            mtf_and_guards = (int)reader.ReadPackedUInt32(),
            scps_except_zombies = (int)reader.ReadPackedUInt32(),
            zombies = (int)reader.ReadPackedUInt32(),
            warhead_kills = (int)reader.ReadPackedUInt32(),
            time = (int)reader.ReadPackedUInt32()
        };
    }

    public static void _WriteSumInfo_ClassList_RoundSummary(NetworkWriter writer, RoundSummary.SumInfo_ClassList value)
    {
        writer.WritePackedUInt32((uint)value.class_ds);
        writer.WritePackedUInt32((uint)value.scientists);
        writer.WritePackedUInt32((uint)value.chaos_insurgents);
        writer.WritePackedUInt32((uint)value.mtf_and_guards);
        writer.WritePackedUInt32((uint)value.scps_except_zombies);
        writer.WritePackedUInt32((uint)value.zombies);
        writer.WritePackedUInt32((uint)value.warhead_kills);
        writer.WritePackedUInt32((uint)value.time);
    }

    public static void _WriteBreakableWindowStatus_BreakableWindow(NetworkWriter writer,
        BreakableWindow.BreakableWindowStatus value)
    {
        writer.Write(value.position);
        writer.Write(value.rotation);
        writer.Write(value.broken);
    }

    public static BreakableWindow.BreakableWindowStatus _ReadBreakableWindowStatus_BreakableWindow(NetworkReader reader)
    {
        return new BreakableWindow.BreakableWindowStatus
        {
            position = reader.ReadVector3(),
            rotation = reader.ReadQuaternion(),
            broken = reader.ReadBoolean()
        };
    }
}