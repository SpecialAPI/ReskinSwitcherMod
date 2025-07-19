# Reskin Switcher
Allows you to switch reskins easily while the game is running. Only works with reskins made with for this mod. Reskins added by mods not made for this mod will always replace the "default" reskin when no other reskin is selected.

# How to make your reskin mod work with this mod
 1. Take your reskin spritesheets/folders out of the `sprites` folder. If a spritesheet/folder is in the `sprites` folder, it will be used as a reskin by Mod the Gungeon API and not this mod.
 2. In the new location with your spritesheets/folders, make a new file named `{your reskin name}-resprite.spapi`. The file should look like this:
```
# GroupName
{What you are trying to reskin (Example: Blasphemy, The Robot, BSG)}

# RespriteName
{The name of your reskin}

# RespriteFiles
{The names of your reskin spritesheets/folders go here. Spritesheet names must have the .png at the end.}
```
To switch reskins in game, in the pause menu go to `Mod Configs -> Reskin Switcher`. There you will find arrow boxes for all found reskin groups.

# Advanced Mode
To enable advanced mode for your reskin, you need to add these lines to your `-resprite.spapi` file:
```
# AdvancedMode
true
```
If advanced mode is on, making an individual frame reskin with a sprite bigger than the original will change that sprite's dimensions instead of stretching it to fit the original dimensions.