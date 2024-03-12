## Better Inventory

This mods adds loads of small features designed to improve the player Inventory and all the systems around it.
I STRONGLY ADVISE you to read the mod configs to learn about all the features and enable/disable them.
Feel free to suggest new features I you think of any! 

Keep in mind, this mod is still in development. If you find a bug, please report it here or on the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=3074374647).

Visit the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=3074374647) if you wish to download and use this mod.


## Features

### Inventory Management
- `Smart Item consumption`: Consume items from a different stack to reduce changes in the selected item
- `Smart Item Pickup`: Items go their previous slot in your inventory when picked up
- `Auto Equip Items`:  Automatically equip items such as Armors, Accessories or Equipment
- `Quick Move`: Quickly move items between special slots in the inventory
- `Favorite Items in Personal Storages`: Favorite items in the Piggy Bank, Safe, ...
- `Craft Stack`: Left Click to craft or buy items in stacks
- `Shift Right Click`: Shift or Control Right Click to transfer one item
- `Stack Trashed and Sold Items` Items sold or trashed multiple times will stack

### Item Actions
- `Fast Container Opening`: Hold rick click to rapidly open containers
- `Fast Extractinator`: Much Faster Extractination
- `Extra Item Right Click`: Item that can be right clicked in the inventory can be right clicked when held
- `Favorited Quick Buff`: Quick buff using favorited potions only
- `Builder Accessories`: Toggle builder accessories with a keybind

### Crafting
- `Fixed UI`: Fixes inconsistencies with the crafting UI, such as the scrolling and material display
- `Recipe Filters`: Filter recipes by their created item
- `Craft on Recipe List`: Craft items directly from the recipe list
- `Held Material`: Adds the held item to the available materials

### Item Search
- `Favorite Recipes`: Allows you to favorite or blacklist recipes
- `Craft in Guide Menu`: Allow you to craft items when talking to the guide and toggle between "All Recipes" and "Available Recipes"
- `Guide Tile`: Adds a second slot next to the guide item to filter by crafting station or condition
- `More recipes`: Adds recipes crafting the item to the guide results
- `Craft Tiles and Conditions`: Overhauls the way crafting station and conditions of recipes are displayed
- `Show Treasure Bag Content`: Displays items dropped by Treasure Bags in the Bestiary
- `Minimum Displayed Info`: Force the minimal displayed information for Bestiary entries
- `Override Unlock Filter`: Replace the Bestiary filter "If Unlocked" with "Not Full Unlocked"
- `Search Item` Quickly open the recipe list or the Bestiary, search recipes using a material or NPCs dropping an item with a simple keybind
- `Search History`: Search the previously searched entry
- `Unknown Entity Display`: Change how unknown items, recipes or NPCs are displayed

## Changelog

### v0.5
- Added the Mod Compatibility Config
- Added an option to sensitive parts of the mod from loading at all
- Added a Compatibility layer to disable part of the mods when they fail loading
- Added Unfavorite favorite recipes on craft
- Added smart consumption for materials
- Added a option for max amount of crafted or bought items with left click
- Auto generation of Builder Accessories keybinds
- Reorganized some options in the configs
- Added full feature list in README.md
- Replaced string with TileDefinition when possible
- Fixed favorite items in banks not been saved
- Fixed crash on load with Better Game UI
- Fixed bugs with shops

### v0.4.1
- Optimized Better Guide and Recipe Filtering
- Optimized Quick Move display
- Clear marks when changing worlds
- Fixed Unknown items able to be favorited
- Fixed Quick move changing selected slot when it should not

### v0.4
- Replaced chat message with notification
- Added Chat message to Version Config
- Displays Smart pickup ghosts
- Allows items to have ghosts in multiple slots
- Improved SmartPickup and AutoEquip pickup order
- Removes Smart Pickup on Shift Click
- Fast Container Opening now applies to the Extractinator
- Improved recipe filters display
- Fixed recipe scroll SFX
- Added Shift Right click override
- Added click overrides for shops
- Added options to invert Crafting override clicks
- Added item stacking for shops and trash
- Added More Better Bestiary and Guide settings
- Added Search History for Guide and Bestiary
- Fixed guideTile applied when disabled
- Fixed Guide Item removed when searching the same item
- Added Different Quick Move hotkey layouts
- Displays the hotkey and highlights the slots where an item can be quick moved to
- Fixed Quick Moving accessories keeping the effect of some items
- Fixed Quick Move and Smart Pickup losing modded item data
- Fixed Quick move changing the favorite state of items
- Fixed others bugs

### v0.3
- Added Smart Pickup
- Small Config label / tooltip changes
- Searching guideItem now properly swaps with guideTile if possible
- Fixed bugged drops in multiplayer by recoding ModsItems
- Fixed displayed requiredTiles when no recipes are available

### v0.2.1.1
- No Auto Equips when transferring items from inside your inventory (e.g. Accessories -> Inventory)
- Fixed modded accessory slots causing a free when picking up items
- Fixed modded accessory slots Quick Move

### v0.2.1
- Added Version config
- Added on-update chat message
- Fixed a possible endless loop when picking up an item
- Fixed an error when guideTile is a tile and not an item			
- Fixed a null reference error when trying to open personal containers
- Fixed Recipe filters doing nothing
- Fixed some items not been counted as known
- Fixed Quick Move chain not ending when closing the inventory
- Fixed recipes sometimes not been updated when guideTile changes
- Fixed guideItem duplication when saving configs
- Fixed weird behavior with Quick Move

### v0.2
- Auto Equip items
- Favorite items in Personal Storages
- Fixed Quick Move losing item prefix

### v0.1
- Initial Pre-Release