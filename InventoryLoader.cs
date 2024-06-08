using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpikysLib.DataStructures;
using SpikysLib.Extensions;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace BetterInventory;

public enum SubInventoryType {
    Classic,
    WithCondition,
    Equipement,
    RightClickTarget,
}

public sealed class InventoryLoader : ILoadable {

    public static IEnumerable<ModSubInventory> SubInventories {
        get {
            foreach (var inventory in _equipement) yield return inventory;
            foreach (var inventory in _condition) yield return inventory;
            foreach (var inventory in _classic) yield return inventory;
        }
    }
    public static ReadOnlyCollection<ModSubInventory> Dedicated => _equipement.AsReadOnly();
    public static ReadOnlyCollection<ModSubInventory> Special => _condition.AsReadOnly();
    public static ReadOnlyCollection<ModSubInventory> Classic => _classic.AsReadOnly();

    public void Load(Mod mod) { }

    public void Unload() {
        ClearCache();
        static void Clear(IList<ModSubInventory> list) {
            foreach (var inventory in list) ModConfigExtensions.SetInstance(inventory, true);
            list.Clear();
        }
        Clear(_equipement);
        Clear(_condition);
        Clear(_classic);
    }

    internal static void Register(ModSubInventory inventory) {
        ModConfigExtensions.SetInstance(inventory);
        if (LoaderUtils.HasOverride(inventory, i => i.IsRightClickTarget)) inventory.Type = SubInventoryType.Equipement;
        else if (LoaderUtils.HasOverride(inventory, i => i.Accepts)) inventory.Type = SubInventoryType.WithCondition;
        else inventory.Type = SubInventoryType.Classic;
        List<ModSubInventory> list = inventory.Type switch {
            SubInventoryType.Equipement => _equipement,
            SubInventoryType.WithCondition => _condition,
            SubInventoryType.Classic or _ => _classic,
        };

        for (int i = 0; i < list.Count; i++) {
            if (inventory.ComparePositionTo(list[i]) >= 0 && list[i].ComparePositionTo(inventory) <= 0) continue;
            list.Insert(i, inventory);
            return;
        }
        list.Add(inventory);
    }

    public static Slot? FindItem(Player player, Predicate<Item> predicate) {
        foreach (ModSubInventory slots in SubInventories) {
            int slot = slots.Items(player).FindIndex(predicate);
            if (slot != -1) return new(slots, slot);
        }
        return null;
    }

    public static bool IsInventorySlot(Player player, Item[] inv, int context, int slot, out Slot itemSlot) {
        Slot? s = GetInventorySlot(player, inv, context, slot);
        itemSlot = s ?? default;
        return s.HasValue;
    }
    public static Slot? GetInventorySlot(Player player, Item[] inventory, int context, int slot) => player == Main.LocalPlayer ? s_slotToInv.GetOrAdd(new(inventory, context, slot)) : ComputeInventorySlot(player, inventory, context, slot);
    private static Slot? ComputeInventorySlot(Player player, Item[] inventory, int context, int slot) {
        foreach (ModSubInventory slots in SubInventories) {
            int slotOffset = 0;
            foreach (ListIndices<Item> items in slots.Items(player).Lists) {
                int index = items.FromInnerIndex(slot);
                if (items.List == inventory && index != -1) return new(slots, index + slotOffset);
                slotOffset += items.Count;
            }
        }
        return null;
    }

    public static IEnumerable<ModSubInventory> GetSubInventories(Item item, SubInventoryType level) {
        List<ModSubInventory> equipement = new();
        foreach (var inv in _equipement) {
            if (!inv.Accepts(item)) continue;
            if (inv.IsRightClickTarget(item)) yield return inv;
            else if (level <= SubInventoryType.Equipement) equipement.Add(inv);
        }
        foreach (var inv in equipement) yield return inv;

        if (level > SubInventoryType.WithCondition) yield break;
        foreach (var inv in _condition) {
            if (inv.Accepts(item)) yield return inv;
        }

        if (level > SubInventoryType.Classic) yield break;
        foreach (var inv in _classic) yield return inv;
    }

    public static void ClearCache() => s_slotToInv.Clear();

    private static readonly List<ModSubInventory> _equipement = [];
    private static readonly List<ModSubInventory> _condition = [];
    private static readonly List<ModSubInventory> _classic = [];

    private static readonly Cache<VanillaSlot, Slot?> s_slotToInv = new(slot => ComputeInventorySlot(Main.LocalPlayer, slot.Inv, slot.Context, slot.Slot)) {
        EstimateValueSize = slot => sizeof(int) + IntPtr.Size
    };
}

public readonly record struct VanillaSlot(Item[] Inv, int Context, int Slot);