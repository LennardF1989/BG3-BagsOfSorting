# Bags of Sorting
A command-line tool to generate custom inventory bags for Baldur's Gate 3.

## Installation
> **NOTE!** Currently, the tool will only work on Windows!

Download the latest release from [GitHub](https://github.com/LennardF1989/BG3-BagsOfSorting/releases) and extract it somewhere easy to find.

You will also need the [Pouch of Wonders](https://www.nexusmods.com/baldursgate3/mods/1368/)-mod (recommended) or another method to grant yourself items in-game.

## Usage
> **NOTE!** Currently, the tool supports both a GUI and a command-line interface. Less tech-savvy users should just use the GUI. The GUI is a verbatim implementation of the instructions below, so please read them to understand what everything does.

The following commands are available:
- `--export-atlas-icons` will parse all .LSX files it can find under `Content\Stock`, and try to extract icons from the .DDS files to `Output\Icons`.
- `--generate-pak` will read the `Content\Bags.json` file and use that information to generate a `BagsOfSorting.pak` in `Output`.
- `--add-bag` will create a new `Content\Bags.json` with 1 empty bag, or will open an existing `Content\Bags.json` and add 1 empty bag to it. Added for convenience, as it removes the need of having to manually generate GUIDs.

### First-time use
Run the command `--add-bag` to generate an empty `Content\Bags.json`. Open that file in your text editor and update the `IndexPaths` to point to Data-folder of the game. Next, run `--export-atlas-icons` to export all icons from the game assets. You're all set!

### Adding bags
Run the command `--add-bag` to add an additional bag to your `Content\Bags.json`.

Open the JSON-file in an editor (for example Visual Studio code, but even Notepad would be fine), and adjust everything accordingly as per example below.

```json
{
    "MapKey": "98f71782-01f7-4e12-b142-b82d30ec98fd",
    "DisplayName": "Story Pouch",
    "Description": "Story Description",
    "TechnicalDescription": "Story Technical Description",
    "AutoPickupCondition": null,
    "ItemIcon": {
        "Name": "Item_LOOT_SCROLL_FireBolt",
        "Custom": false,
        "Generate": true
    },
    "TooltipIcon": {
        "Name": "Item_LOOT_SCROLL_FireBolt",
        "Custom": false,
        "Generate": true
    },
    "Color": "Orange",
    "Amount": 1
}
```

`MapKey` is required and should always be a unique GUID.

`DisplayName`, `Description` and `TechnicalDescription` are optional. When left empty, the game will show the description of a normal Pouch.

`AutoPickupCondition` allows you to script a way to automatically add items to a particular bag. The game uses this for camp supplies, alchemy items, etcetera. I couldn't really find a useful way to make it work for scrolls (for example), without resorting to manually adding tags to all those (base) items.

`ItemIcon` is the icon shown in the inventory. 
- `Name` can be set to any filename (without the extension) found under `Output\Icons`. 
- When `Custom` is set to `true`, the path `Content\Custom` is used instead and a 64x64 PNG-file is expected to be found. 
- When `Generate` is set to `true`, the icon set under `Name` will shrunk to two-thirds of its size and be combined with the icon of a normal pouch.

`TooltipIcon` works exactly the same as `ItemIcon`. However, it is recommended to use a 380x380 PNG-file when `Custom` is set to `true` and `Generate` is set to `false`. This will give the best results in-game.

`Color` determines how the item is presented in-game. The following values are available:
- None
- Green
- Blue
- Pink
- Gold
- Orange

`Amount` can be set to any value and determines how many bags will be available in the Pouch of Wonders.

## Generating the PAK
Run the command `--generate-pak` and the resulting `BagsOfSorting.pak` can be found in `Output`. The PAK-file will contain a copy of your `Bags.json`, so you are always able the regenerate the exact same file again. This is important, because your save-file can get corrupted if any used bag is no longer available.

## Installing the PAK
The PAK-file has to be dropped directly into `Baldurs Gate 3\Data` and will just work, even on existing playthoughs.

## Getting access to your custom bags
As mentioned earlier, the easiest method would be to use the [Pouch of Wonders]([Pouch of Wonders](https://www.nexusmods.com/baldursgate3/mods/1368/)), as that was purposely created for this tool as a replacement for a lot of similar mods. 

## Additional functionality: Add more items to the Pouch of Wonders
It's possible to include other items in the Pouch of Wonders than just the custom bags using this tool.

Inside your `Content\Bags.json` update the `AdditionalTreasures` accordingly, for example:
```json
"AdditionalTreasures": {
    "T_TUT_Chest_Potions": 1,
    "I_OBJ_Tool_Shovel": 2
}
```

This will give you access to all items in the Tutorial Chest (*) and 2 shovels.

\* This is which is a method a lot of mods are using now. Do notice the `T_`-prefix to reference a specific `TreasureTable` instead of an `Item`, which uses the `I_`-prefix instead.

## Additional functionality: Generate a standalone mod
If you do not want to use Pouch of Wonders, because you are already using something else, the GUI now has the possibility to generate a standalone mod to work with the game. 

Simply changes the `Folder`-value in the `Treasure Table`-tab to something other than `PouchOfWonders`. Set the `Name`-value to your desired TreasureTable, for example `TUT_Chest_Potions` for the Tutorial Chest.

## Additional functionality: Flip the generated icons
It is possible to flip the generated icons from left to right by setting `AlignGeneratedItemIconsRight` to `true` in the `Content\Bags.json`. It is however not recommended to do this when you use generated icons. This is because in-game the amount of items in a pouch is shown the upper-right corner, and hide the visual cue to Item icons are meant to provide.

## Additional functionaly: Search for items
It's possible to search for items in all of the game files and your mods. Simply press the "Index PAKs"-button and wait for it to be done. 

Next head over the the "Search PAKs"-tab, and you can search for items by a part of their name. Keep in mind that the query is searched as a whole, best results are probably achieved by searching for single words.

For your convenience, after clicking a Game Object, you can press the "Add"-button to immediately add it to your Treasure Table.

## License
License is purposely set to LGPL-2.1 to encourage pull requests with useful changes, while still allowing the tool to be used in other solutions that don't share the same license. Just be sure to give credits where credits are due.

## Dependencies
- [.NET 6 version](https://github.com/LennardF1989/lslib/tree/dotnet6) of [LSLib](https://github.com/Norbyte/lslib) to create LXS-, LOCA- and PAK-files.
- [Pfim](https://github.com/nickbabcock/Pfim) to read from DDS-files.
- [Cross-platform version](https://github.com/matyalatte/Texconv-Custom-DLL) of [Texconv](https://github.com/Microsoft/DirectXTex/) to convert PNG-files to DDS-files.
- [ImageSharp](https://github.com/SixLabors/ImageSharp) to manipulate the images.
