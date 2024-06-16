using System.ComponentModel;
using BetterInventory.Default.SearchProviders;
using Newtonsoft.Json;
using SpikysLib.Configs.UI;
using SpikysLib.Extensions;
using Terraria.ModLoader.Config;

namespace BetterInventory.Configs;

public sealed class Compatibility : ModConfig {

    [DefaultValue(0), JsonProperty] internal int failedILs = 0;


    [ReloadRequired, DefaultValue(false)] public bool compatibilityMode;

    [JsonIgnore, ShowDespiteJsonIgnore, NullAllowed] public object? DisableAll {
        get => null;
        set {
            FixedUI.Value.fastScroll.Parent = false;
            FixedUI.Value.listScroll = false;
            FixedUI.Value.wrapping = false;
            Crafting.Instance.recipeFilters.Parent = false;
            Crafting.Instance.craftOnList.Parent = false;
            Crafting.Instance.Save();

            SmartConsumption.Value.materials = false;
            SmartConsumption.Value.baits = false;
            SmartPickup.Value.previousSlot.Parent = ItemPickupLevel.None;
            MarksDisplay.Value.fakeItem.Parent = false;
            MarksDisplay.Value.icon.Parent = false;
            SmartPickup.Value.autoEquip = AutoEquipLevel.None;
            SmartPickup.Value.autoUpgrade.Parent = false;
            SmartPickup.Value.hotbarLast = false;
            SmartPickup.Value.fixSlot = false;
            QuickMove.Value.displayHotkeys.Parent = HotkeyDisplayMode.None;
            QuickMove.Value.displayHotkeys.Value.highlightIntensity = 0;
            InventoryManagement.Instance.favoriteInBanks = false;
            InventoryManagement.Instance.shiftRight = false;
            InventoryManagement.Instance.stackTrash = false;
            InventoryManagement.Instance.craftStack.Parent = false;
            InventoryManagement.Instance.Save();

            BetterGuide.Value.moreRecipes = false;
            BetterGuide.Value.craftInMenu = false;
            BetterGuide.Value.unknownDisplay = UnknownDisplay.Vanilla;
            FavoriteRecipes.Value.unfavoriteOnCraft = UnfavoriteOnCraft.None;
            BetterBestiary.Value.unknownDisplay = UnknownDisplay.Vanilla;
            BetterBestiary.Value.displayedUnlock = UnlockLevel.Vanilla;
            QuickSearch.Value.catalogues[new(RecipeList.Instance)] = false;
            ItemSearch.Instance.Save();
        }
    }

    [JsonIgnore, ShowDespiteJsonIgnore, CustomModConfigItem(typeof(HideDefaultElement))] public UnloadedCrafting UnloadedCrafting {get; set;} = new();
    [JsonIgnore, ShowDespiteJsonIgnore, CustomModConfigItem(typeof(HideDefaultElement))] public UnloadedInventoryManagement UnloadedInventoryManagement {get; set;} = new();
    [JsonIgnore, ShowDespiteJsonIgnore, CustomModConfigItem(typeof(HideDefaultElement))] public UnloadedItemSearch UnloadedItemSearch {get; set;} = new();

    public static bool CompatibilityMode => Instance.compatibilityMode;
    public static Compatibility Instance = null!;
    
    public override ConfigScope Mode => ConfigScope.ClientSide;
}

public sealed class UnloadedCrafting {
    public bool fastScroll = false;
    public bool listScroll = false;
    public bool wrapping = false;
    public bool recipeFilters = false;
    public bool craftOnList = false;

    public static UnloadedCrafting Value => Compatibility.Instance.UnloadedCrafting;
}

public sealed class UnloadedInventoryManagement {
    public bool materials = false;
    public bool baits = false;
    public bool previousSlot = false;
    public bool autoEquip = false;
    public bool autoUpgrade = false;
    public bool hotbarLast = false;
    public bool fixSlot = false;
    public bool favoriteInBanks = false;
    public bool shiftRight = false;
    public bool stackTrash = false;
    public bool marksFakeItem = false;
    public bool marksIcon = false;
    public bool quickMoveHotkeys = false;
    public bool quickMoveHighlight = false;
    public bool craftStack = false;
    
    public static UnloadedInventoryManagement Value => Compatibility.Instance.UnloadedInventoryManagement;
}

public sealed class UnloadedItemSearch {
    public bool searchRecipes = false;
    public bool guideMoreRecipes = false;
    public bool guideFavorite = false;
    public bool guideUnfavoriteOnCraft = false;
    public bool guideCraftInMenu = false;
    public bool guideUnknown = false;
    public bool bestiaryUnknown = false;
    public bool bestiaryDisplayedUnlock = false;

    [JsonIgnore] public bool BestiaryUnlock { set { bestiaryUnknown = value; bestiaryDisplayedUnlock = value; } }
    [JsonIgnore] public bool GuideAvailableRecipes { set { guideFavorite = value; guideCraftInMenu = value; guideUnknown = value; } }
    
    public static UnloadedItemSearch Value => Compatibility.Instance.UnloadedItemSearch;
}