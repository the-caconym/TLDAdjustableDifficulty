# TLDAdjustableDifficulty

This is a fan-made, experimental mod for the game [The Long Dark](http://hinterlandgames.com/) allowing to adjust the difficulty settings of the game.

### Disclaimer

* **DO NOT** report bugs to Hinterland Studios if you use this mod; any bug you encounter may be due to the mod.

* This is completely fan-made and I have **no affiliation** with Hinterland Studios whatsoever.

* This mod is **experimental**; it should work but there is no guarantee it won't screw up your save somehow (though corrupting a save is highly unlikely). It's also very rough in terms of use (you need to edit an XML file).

* The game was tweaked by Hinterland to provide the four difficulty settings available in the unmodded game; by changing the difficulty settings using this mod, you will likely change the game experience and possibly make it less enjoyable / less balanced.

### How to install

* Install [zeobviouslyfakeacc's](https://github.com/zeobviouslyfakeacc) awesome [Mod Loader for TLD](https://github.com/zeobviouslyfakeacc/ModLoaderInstaller).

* The mod loader will create a directory 'mods' at the root of your game directory; it will also add a first mod: AddModdedToVersionString.dll. Leave it; it will make sure your game appears as loaded in the main menu.

* Move the AdjustableDifficulty.dll file into this 'mods' directory.

* Move the adjustable_difficulty.xml file into your game root directory (that is, the directory that contains the "mods" folder).

* Tweak the values in adjustable_difficulty.xml as you wish.

* The next time you start the game, those values should be taken into account.

### How to use

In order to adjust the difficulty settings, you'll need to edit the adjustable_difficulty.xml XML file. The file is heavily commented and explains what each of the values do. You can adjust the settings for each of the four original difficulty settings: Pilgrim, Voyageur, Stalker, and Interloper; you cannot create new difficulty settings.

Due to how the game works, most of the settings have to be changed using scale factors (multipliers); the default values in the XML file for all four difficulty modes are the ones in the base game, which should help you get an idea of a coherent range for each of the values.

For now, you can adjust:

* How fast the various needs (freezing, calorie burn, etc.) decrease.
* Progressive temperature drop settings.
* Condition recovery settings (while sleeping or awake).
* Chances of blizzard, and how often the weather tends to change.
* Rate of decay for objects.
* Chances of loot in containers.
* Damage to player and clothes when attacked by wildlife.
* How easy it is to fight a wolf struggle.
* The distance at which predator can smell food you carry.
* The rough wildlife amount for each of the four animals (wolf, bear, rabbit, dear), and how often they respawn (roughly).
* The delay before an hypothermia is cured (no other diseases yet).
* The average time before a fish is catched when fishing; can be set to progressively get worse.

You **cannot** adjust (for now):

* Global stuff related to difficulty settings; that is, wolves will always flee the player in Pilgrim, and diseases limitations (instestinal parasites, for example) in Voyageur and Pilgrim cannot be changed.
* Diseases parameters (other than time before hypothermia is cured).
* The amount of loot outside of containers.

### How to make sure it works

Look at your TLD Player.log file (after starting the game at least once upon installing the mod); the mod outputs lots of stuff in the log file that should help you make sure it works. You should for example see lines saying:

```Read XML configuration file 'adjustable_difficulty.xml' successfully.```

The mod will also show every value it read from the file and show when it changes stuff in the game; for example "Adjusted wolf spawn region" messages and so on.

Alternatively, if you want to make sure it works, you can temporarily set a setting very high; for example set player\_needs\_cold\_scale to "30.0" and you should see your coldness bar disappear in a few seconds upon creating or loading a game (make sure you change the proper difficulty mode, IE Pilgrim, Voyageur, etc.).

### Remarks

* The mod has been tested as compatible with version **1.12** of the game; it should however work on not-too-old previous versions (and possibly future ones) as well.

* For now at least, the mod will **only** be active in survival (sandbox) mode.

* The mod should work either on a new game or on an existing save; note that using it on an existing save won't change what already happened (for example, if you suddenly decrease wolves' spawns, you may need to kill the existing ones - they won't disappear by themselves).

* You need to restart the game to take changes in the XML into account.

* Any syntax error in the XML (for example missing tag, or using a float value in an int parameters) will result in the mod not doing any changes; it will output an error in the log file explaining the syntax error.

### How to uninstall

To uninstall the mod, simply remove the .dll file from the mods/ folder.

If you want to completely remove modding capabilities from your game, use [zeobviouslyfakeacc's Mod Loader for TLD](https://github.com/zeobviouslyfakeacc/ModLoaderInstaller) again; you can disable the mod loading patch from the tool.

### License

The mod is licensed under the [WTFPL](http://www.wtfpl.net/) license (Do What the Fuck You Want to Public License); you can basically do whatever you want with the code/the binaries without any attribution required.
