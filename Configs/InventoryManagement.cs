using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace BetterInventory.Configs;

public sealed class InventoryManagement : ModConfig {
    [DefaultValue(true)] public bool smartConsumption;
    [DefaultValue(true)] public bool smartAmmo;
    [DefaultValue(SmartPickupLevel.AllItems)] public SmartPickupLevel smartPickup = SmartPickupLevel.AllItems;
    [DefaultValue(false)] public bool autoEquip = false;

    public Toggle<QuickMove> quickMove = new(true);
    [DefaultValue(true)] public bool fastRightClick;
    [DefaultValue(true)] public bool itemRightClick;


    public enum SmartPickupLevel { Off, FavoriteOnly, AllItems }
    public bool SmartPickupEnabled(Item item) => smartPickup switch {
        SmartPickupLevel.AllItems => true,
        SmartPickupLevel.FavoriteOnly => item.favorited,
        SmartPickupLevel.Off or _ => false
    };

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InventoryManagement Instance = null!;

}

public sealed class QuickMove {
    [Range(0, 3600), DefaultValue(60)] public int chainTime = 60;
    [DefaultValue(true)] public bool showTooltip = true;
    [DefaultValue(false)] public bool highlightSlots = false; // TODO impl
    [DefaultValue(true)] public bool returnToSlot = true;
}