using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterInventory.ItemSearch;


public sealed class Bestiary : ILoadable {

    public static bool Enabled => Configs.ClientConfig.Instance.searchDrops;

    public void Load(Mod mod) {
        On_UIBestiaryEntryButton.ctor += HookDarkenEntryButton;
        IL_UIBestiaryEntryIcon.Update += ILIconUpdateFakeUnlock;
        IL_UIBestiaryEntryIcon.DrawSelf += ILIconDrawFakeUnlock;

        IL_UIBestiaryEntryInfoPage.AddInfoToList += IlEntryPageFakeUnlock;
        On_UIBestiaryEntryInfoPage.AddInfoToList += HookDarkenEntryPage;

        IL_Filters.BySearch.FitsFilter += ILSearchFilterFakeUnlock;

        On_Filters.ByUnlockState.GetDisplayNameKey += HookUnlockFilterName;
        On_Filters.ByUnlockState.FitsFilter += HookCustomUnlockFilter;

        IL_UIBestiaryFilteringOptionsGrid.UpdateAvailability += ILFakeUnlockFilters;
        On_UIBestiaryFilteringOptionsGrid.UpdateButtonSelections += HookDarkenFilters;
        
        On_UIBestiaryTest.Recalculate += HookDelaySearch;

        On_UIBestiaryTest.FilterEntries += HookBestiaryUnkownNPCBehaviourFilter;

        On_FewFromOptionsNotScaledWithLuckDropRule.ReportDroprates += HookFixDropRates;
        On_UIBestiaryInfoItemLine.ctor += HookBossBag;
        On_ItemDropBestiaryInfoElement.GetSearchString += HookSearchBossBagText;
    }

    public void Unload() {
        _bossBagSearch.Clear();
    }


    private void HookDarkenEntryButton(On_UIBestiaryEntryButton.orig_ctor orig, UIBestiaryEntryButton self, BestiaryEntry entry, bool isAPrettyPortrait) {
        orig(self, entry, isAPrettyPortrait);
        if(!Enabled || self.Entry.Icon.GetUnlockState(self.Entry.UIInfoProvider.GetEntryUICollectionInfo())) return;
        ((UIImage)self.Children.First().Children.First()).Color.ApplyRGB(IconDark);
        Reflection.UIBestiaryEntryButton._borders.GetValue(self).Color.ApplyRGB(IconDark);
        Reflection.UIBestiaryEntryButton._bordersGlow.GetValue(self).Color.ApplyRGB(IconDark);
    }
    
    private static void ILIconUpdateFakeUnlock(ILContext il) {
        ILCursor cursor = new(il);

        // this._collectionInfo = this._entry.UIInfoProvider.GetEntryUICollectionInfo();
        cursor.GotoNext(MoveType.After, i => i.MatchCallvirt(typeof(IBestiaryUICollectionInfoProvider), nameof(IBestiaryUICollectionInfoProvider.GetEntryUICollectionInfo)));

        // ++ <fakeUnlock> 
        cursor.EmitDelegate((BestiaryUICollectionInfo info) => {
            if (Enabled && Configs.ClientConfig.Instance.unknownBehaviour == Configs.UnknownSearchBehaviour.Known) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
            return info;
        });
        // ...
    }
    private static void ILIconDrawFakeUnlock(ILContext il) {
        ILCursor cursor = new(il);

        // ...
        // bool unlockState = this._entry.Icon.GetUnlockState(this._collectionInfo);
        cursor.GotoNext(MoveType.After, i => i.MatchCallvirt(typeof(IEntryIcon), nameof(IEntryIcon.GetUnlockState)));

        // ++ <changeVisibleState>
        cursor.EmitDelegate((bool unlocked) => unlocked || (Enabled && Configs.ClientConfig.Instance.unknownBehaviour == Configs.UnknownSearchBehaviour.Known));
    }
    

    private static void IlEntryPageFakeUnlock(ILContext il) {
        ILCursor cursor = new(il);

        // BestiaryUICollectionInfo uICollectionInfo = this.GetUICollectionInfo(entry, extraInfo);
        cursor.GotoNext(MoveType.After, i => i.MatchCall(typeof(UIBestiaryEntryInfoPage), "GetUICollectionInfo"));

        // ++ <fakeUnlock> 
        cursor.EmitDelegate((BestiaryUICollectionInfo info) => {
            if(!Enabled) return info;
            if (Configs.ClientConfig.Instance.unknownBehaviour == Configs.UnknownSearchBehaviour.Known) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
            else if(BestiaryEntryUnlockState.NotKnownAtAll_0 < info.UnlockState && info.UnlockState < BestiaryEntryUnlockState.CanShowDropsWithoutDropRates_3) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithoutDropRates_3;
            return info;
        });
        // ...
    }

    private static void HookDarkenEntryPage(On_UIBestiaryEntryInfoPage.orig_AddInfoToList orig, UIBestiaryEntryInfoPage self, BestiaryEntry entry, ExtraBestiaryInfoPageInformation extraInfo) {
        orig(self, entry, extraInfo);
        if(!Enabled) return;
        if(s_darkPage != (entry.UIInfoProvider.GetEntryUICollectionInfo().UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0)) {
            DarkenElement(self, s_darkPage ? (1 / PageDark) : PageDark, 1);
            s_darkPage = !s_darkPage;
        }
        if (s_darkPage) DarkenElement(Reflection.UIBestiaryEntryInfoPage._list.GetValue(self), PageDark);
    }


    private void HookBestiaryUnkownNPCBehaviourFilter(On_UIBestiaryTest.orig_FilterEntries orig, UIBestiaryTest self) {
        orig(self);
        if (!Enabled || Configs.ClientConfig.Instance.unknownBehaviour != Configs.UnknownSearchBehaviour.Hidden) return;
        List<BestiaryEntry> entries = Reflection.UIBestiaryTest._workingSetEntries.GetValue(Main.BestiaryUI);
        for (int i = entries.Count - 1; i >= 0; i--) {
            if (entries[i].UIInfoProvider.GetEntryUICollectionInfo().UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0) entries.RemoveAt(i);
        }
    }

    private static void ILSearchFilterFakeUnlock(ILContext il) {
        ILCursor cursor = new(il);

        // ...
        // BestiaryUICollectionInfo info = entry.UIInfoProvider.GetEntryUICollectionInfo();
        cursor.GotoNext(MoveType.After, i => i.MatchCallvirt(typeof(IBestiaryUICollectionInfoProvider), nameof(IBestiaryUICollectionInfoProvider.GetEntryUICollectionInfo)));

        // ++ <fakeUnlock> 
        cursor.EmitDelegate((BestiaryUICollectionInfo info) => {
            if (Enabled) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
            return info;
        });
        // ...
    }

    private static string HookUnlockFilterName(On_Filters.ByUnlockState.orig_GetDisplayNameKey orig, Filters.ByUnlockState self) => !Enabled ? orig(self) : "Mods.BetterInventory.UI.FullUnlock";
    private static bool HookCustomUnlockFilter(On_Filters.ByUnlockState.orig_FitsFilter orig, Filters.ByUnlockState self, BestiaryEntry entry) => !Enabled ? orig(self, entry) : entry.UIInfoProvider.GetEntryUICollectionInfo().UnlockState != BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;


    private void ILFakeUnlockFilters(ILContext il) {
        ILCursor cursor = new(il);

        // ...
        // for (<filter>) {
        //     ...
        //     bool b = this.GetIsFilterAvailableForEntries(bestiaryEntryFilter, entries);
        cursor.GotoNext(MoveType.After, i => i.MatchCall(typeof(UIBestiaryFilteringOptionsGrid), "GetIsFilterAvailableForEntries"));

        //     ++ <fakeUnlock> 
        cursor.EmitLdloc(13);
        cursor.EmitLdloc(14);
        cursor.EmitDelegate((bool on, IBestiaryEntryFilter filter, List<BestiaryEntry> entries) => {
            if(filter.ForcedDisplay.HasValue) return on;
            if (Enabled && Configs.ClientConfig.Instance.unknownBehaviour == Configs.UnknownSearchBehaviour.Known) {
                for (int i = 0; i < entries.Count; i++) if (filter.FitsFilter(entries[i])) return true;
            }
            return on;
        });

    }

    private void HookDarkenFilters(On_UIBestiaryFilteringOptionsGrid.orig_UpdateButtonSelections orig, UIBestiaryFilteringOptionsGrid self) {
        orig(self);
        if (!Enabled) return;
        EntryFilterer<BestiaryEntry, IBestiaryEntryFilter> filterer = Reflection.UIBestiaryFilteringOptionsGrid._filterer.GetValue(self);
        List<GroupOptionButton<int>> filters = Reflection.UIBestiaryFilteringOptionsGrid._filterButtons.GetValue(self);
        for (int i = 0; i < filterer.AvailableFilters.Count; i++){
            if (Reflection.UIBestiaryFilteringOptionsGrid.GetIsFilterAvailableForEntries.Invoke(self, filterer.AvailableFilters[i], Reflection.UIBestiaryFilteringOptionsGrid._filterAvailabilityTests.GetValue(self)[i])) continue;
            Color color = Reflection.GroupOptionButton<int>._color.GetValue(filters[i]);
            color.ApplyRGB(IconDark);
            Reflection.GroupOptionButton<int>._color.SetValue(filters[i], color);
        }
    }


    private void HookFixDropRates(On_FewFromOptionsNotScaledWithLuckDropRule.orig_ReportDroprates orig, FewFromOptionsNotScaledWithLuckDropRule self, List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
        float pesonalDroprate = System.Math.Min(1, self.chanceNumerator / (float)self.chanceDenominator);
        float globalDroprate = pesonalDroprate * ratesInfo.parentDroprateChance;
        float dropRate = 1f / self.dropIds.Length * self.amount * globalDroprate;
        for (int i = 0; i < self.dropIds.Length; i++) drops.Add(new DropRateInfo(self.dropIds[i], 1, 1, dropRate, ratesInfo.conditions));
        Chains.ReportDroprates(self.ChainedRules, pesonalDroprate, drops, ratesInfo);
    }

    private void HookBossBag(On_UIBestiaryInfoItemLine.orig_ctor orig, UIBestiaryInfoItemLine self, DropRateInfo info, BestiaryUICollectionInfo uiinfo, float textScale) {
        orig(self, info, uiinfo, textScale);
        if (Enabled && ItemID.Sets.BossBag[info.itemId]) {
            UIList uIList = new() {
                Left = StyleDimension.FromPixelsAndPercent(-1, 0f),
                Width = StyleDimension.FromPixelsAndPercent(0, 1f),
                Height = StyleDimension.FromPixelsAndPercent(0, 1f),
            };
            uIList.SetPadding(0);
            uIList.PaddingBottom = 4;
            uIList.PaddingBottom = 4;
            uIList.ListPadding = 4;
            uIList.Top.Set(self.Height.Pixels + uIList.PaddingTop, 0);
            self.Append(uIList);

            List<DropRateInfo> drops = new();
            DropRateInfoChainFeed ratesInfo = new(1f);
            foreach (IItemDropRule itemDropRule in Main.ItemDropsDB.GetRulesForItemID(info.itemId)) itemDropRule.ReportDroprates(drops, ratesInfo);
            foreach (DropRateInfo drop in drops) {
                if (drop.itemId.InRange(ItemID.CopperCoin, ItemID.PlatinumCoin)) continue;
                ItemDropBestiaryInfoElement element = new(drop);
                UIElement? dropLine = element.ProvideUIElement(uiinfo);
                if (dropLine is null) continue;
                dropLine.Left.Set(0, 0);
                dropLine.Width.Set(0, 1);
                dropLine.PaddingLeft /= 2;
                dropLine.PaddingRight /= 2;
                uIList.Add(dropLine);
            }
            uIList.Recalculate();
            self.Height.Pixels += uIList.GetTotalHeight() + uIList.PaddingBottom;
        }
    }

    private string HookSearchBossBagText(On_ItemDropBestiaryInfoElement.orig_GetSearchString orig, ItemDropBestiaryInfoElement self, ref BestiaryUICollectionInfo info) {
        string s = orig(self, ref info);
        if(!Enabled) return s;
        DropRateInfo dropRateInfo = Reflection.ItemDropBestiaryInfoElement._droprateInfo.GetValue(self);
        if (!ItemID.Sets.BossBag[dropRateInfo.itemId]) return s;
        return $"{s}|{GetBossBagSearch(dropRateInfo)}";
    }


    private static void HookDelaySearch(On_UIBestiaryTest.orig_Recalculate orig, UIBestiaryTest self) {
        orig(self);
        if (s_bestiaryDelayedType == ItemID.None) return;
        SetBestiaryItem(s_bestiaryDelayedType);
        s_bestiaryDelayedType = ItemID.None;
    }


    public static void DarkenElement(UIElement element, float dark, int depth = -1){
        if (element is UIHorizontalSeparator sep) sep.Color.ApplyRGB(dark);
        else if (element is UIBestiaryNPCEntryPortrait portrait) ((UIImage)portrait.Children.Last()).Color.ApplyRGB(dark);
        else if (element is UIPanel panel) {
            panel.BorderColor.ApplyRGB(dark);
            panel.BackgroundColor.ApplyRGB(dark);
        }
        if (element is UIBestiaryInfoItemLine item) {
            item.OnMouseOver += (_, _) => item.BorderColor.ApplyRGB(dark);
            item.OnMouseOut += (_, _) => item.BorderColor.ApplyRGB(dark);
        }

        if (depth != 0) {
            depth--;
            if (element is UIList list) foreach (UIElement e in list) DarkenElement(e, dark, depth);
            else foreach(UIElement e in element.Children) DarkenElement(e, dark, depth);
        }
    }


    public static void ToggleBestiary(bool? enabled = null) {
        if (Main.InGameUI.CurrentState == Main.BestiaryUI) {
            if (enabled == true) return;
            IngameFancyUI.Close();
        } else {
            if (enabled == false) return;
            Main.LocalPlayer.SetTalkNPC(-1, false);
            Main.npcChatCornerItem = 0;
            Main.npcChatText = "";
            Main.mouseLeftRelease = false;
            IngameFancyUI.OpenUIState(Main.BestiaryUI);
            Main.BestiaryUI.OnOpenPage();
        }
    }
    public static void SetBestiaryItem(int type, bool delayed = false) {
        if (delayed) {
            s_bestiaryDelayedType = type;
            return;
        }
        static void PlayNoise(string content) => SoundEngine.PlaySound(SoundID.Grab);
        UISearchBar searchBar = Reflection.UIBestiaryTest._searchBar.GetValue(Main.BestiaryUI);
        BestiaryEntry? oldEntry = Reflection.UIBestiaryTest._selectedEntryButton.GetValue(Main.BestiaryUI)?.Entry;
        searchBar.OnContentsChanged += PlayNoise;
        searchBar.SetContents(Lang.GetItemNameValue(type), true);
        if (searchBar.IsWritingText) searchBar.ToggleTakingText();
        searchBar.OnContentsChanged -= PlayNoise;
        UIBestiaryEntryGrid grid = Reflection.UIBestiaryTest._entryGrid.GetValue(Main.BestiaryUI);
        if (oldEntry is not null) {
            foreach (UIElement element in grid.Children) {
                if (element is not UIBestiaryEntryButton button || button.Entry != oldEntry) continue;
                Reflection.UIBestiaryTest.SelectEntryButton.Invoke(Main.BestiaryUI, button);
                return;
            }
        }
        foreach (UIElement element in grid.Children) {
            if (element is not UIBestiaryEntryButton button) continue;
            Reflection.UIBestiaryTest.SelectEntryButton.Invoke(Main.BestiaryUI, button);
            break;
        }
    }


    public static string GetBossBagSearch(DropRateInfo bossbag){
        if(_bossBagSearch.TryGetValue(bossbag.itemId, out string? s)) return s;
        List<DropRateInfo> drops = new();
        DropRateInfoChainFeed ratesInfo = new(1f);
        List<string> names = new();
        foreach (IItemDropRule itemDropRule in Main.ItemDropsDB.GetRulesForItemID(bossbag.itemId)) itemDropRule.ReportDroprates(drops, ratesInfo);
        foreach (DropRateInfo drop in drops) {
            if (!drop.itemId.InRange(ItemID.CopperCoin, ItemID.PlatinumCoin)) names.Add(Lang.GetItemNameValue(drop.itemId));
        }
        return _bossBagSearch[bossbag.itemId] = string.Join('|', names);
    }


    public const float PageDark = 0.7f;
    public const float IconDark = 0.5f;

    private static int s_bestiaryDelayedType;

    private static bool s_darkPage = false;

    private static readonly Dictionary<int, string> _bossBagSearch = new();
}