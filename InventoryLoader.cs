using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpikysLib.DataStructures;
using SpikysLib.Extensions;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace BetterInventory;

public sealed class InventoryLoader : ILoadable {

    public static IEnumerable<ModSubInventory> SubInventories {
        get {
            foreach (var inventory in _special) yield return inventory;
            foreach (var inventory in _classic) yield return inventory;
        }
    }
    public static ReadOnlyCollection<ModSubInventory> Special => _special.AsReadOnly();
    public static ReadOnlyCollection<ModSubInventory> Classic => _classic.AsReadOnly();

    public void Load(Mod mod) { }

    public void Unload() {
        ClearCache();
        static void Clear(IList<ModSubInventory> list) {
            foreach (var inventory in list) ModConfigExtensions.SetInstance(inventory, true);
            list.Clear();
        }
        Clear(_classic);
        Clear(_special);
    }

    internal static void Register(ModSubInventory inventory) {
        ModConfigExtensions.SetInstance(inventory);
        inventory.CanBePrimary = LoaderUtils.HasOverride(inventory, i => i.IsPrimaryFor);
        List<ModSubInventory> list = inventory.CanBePrimary ? _special : _classic;

        int before = list.FindIndex(i => inventory.ComparePositionTo(i) < 0 || i.ComparePositionTo(inventory) > 0);
        if (before != -1) list.Insert(before, inventory);
        else list.Add(inventory);
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

    public static void ClearCache() => s_slotToInv.Clear();

    private static readonly List<ModSubInventory> _classic = [];
    private static readonly List<ModSubInventory> _special = [];

    private static readonly Cache<VanillaSlot, Slot?> s_slotToInv = new(slot => ComputeInventorySlot(Main.LocalPlayer, slot.Inv, slot.Context, slot.Slot)) {
        EstimateValueSize = slot => sizeof(int) + IntPtr.Size
    };
}

public readonly record struct VanillaSlot(Item[] Inv, int Context, int Slot);