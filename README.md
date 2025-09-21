# vs-simpleentityhealthbar
Vintage Story Mod that displays both on Hud and in world over targeted entities a healthbar

# Manual Configuration via JSON

The Simple Entity Healthbar mod stores its configuration in a JSON file, named `SimpleEntityHealthbar.json` in VintageStory mods config directory.

## Editing the Config File

You can manually edit this file with any text editor (such as Notepad, VS Code, or Sublime Text) to change the mod's behavior. The file will look similar to this:

```json
{
  "ShowHudHealthBar": true,
  "ShowNameplates": true,
  "MaxTargetDistance": 15 // currently caps at 50 
}
```

### Config Options

- **ShowHudHealthBar**: (bool)
  - Enable/Disable the Hud healthbar (shown on top center screen)
  - Example: `true`
  - Default `true`

- **ShowNameplates**: (boolean)
  - Enable/Disable the Entity nameplates containing name and healthbar
  - Example: `true`
  - Default: `false`

- **MaxTargetDistance**: (integer)
  - Max distance at which nameplates are displayed (Hud Healthbar only shows at standard  pick distance 4.5 blocks max)
  - Example: `20`
  - Default: `15`
  - Note: This is capped by the code to 50, since the experience is poor 
  and i would like to avoid any unforseen crashes. Even with proper scaling at 50
  blocks distance the nameplate seems to provide little value

### Tips
- Always save the file after editing.
- Changes will take effect the next time the mod loads the config (usually on game restart or reload).
- If you make a mistake, you can delete the config file and the mod will recreate it with default values.
