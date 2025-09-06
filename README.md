# vs-simpleentityhealthbar
Vintage Story Mod that highlights items dropped on the ground

# Manual Configuration via JSON

The Item Pickup Highlighter mod stores its configuration in a JSON file, named `SimpleEntityHealthbar.json` in VintageStory mods config directory.

## Editing the Config File

You can manually edit this file with any text editor (such as Notepad, VS Code, or Sublime Text) to change the mod's behavior. The file will look similar to this:

```json
{
  "HighlightDistance": 10,
  "HighlightContinousMode": false,
  "HighlightColor": 4294967295
}
```

### Config Options

- **HighlightDistance**: (integer, minimum 2)
  - The maximum distance (in blocks) from the player at which items will be highlighted.
  - Example: `10`

- **HighlightContinousMode**: (boolean)
  - If `true`, items are highlighted continuously without pressing the hotkey.
  - If `false`, you must use the hotkey to highlight items.
  - Example: `true` or `false`

- **HighlightColor**: (integer, ARGB format)
  - The color of the highlight, stored as a 32-bit integer in ARGB format (Alpha, Red, Green, Blue).
  - Example: `4294967295` (which is white, fully opaque: `0xFFFFFFFF`)
  - To use a custom color, convert your desired color to ARGB integer format. You can use online tools or C#'s `Color.ToArgb()` method.

### Tips
- Always save the file after editing.
- Changes will take effect the next time the mod loads the config (usually on game restart or reload).
- If you make a mistake, you can delete the config file and the mod will recreate it with default values.
