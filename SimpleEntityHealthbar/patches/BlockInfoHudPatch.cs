using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace SimpleEntityHealthbar.patches;

[HarmonyPatchCategory("simpleentityhealthbar_blockinfo")]
public class BlockInfoHudPatch
{
    private Harmony _harmony;
	
    public void Patch()
    {
        _harmony = SimpleEntityHealthbarModSystem.NewPatch("Block Hud Info", "simpleentityhealthbar_blockinfo");
    }
	
    public void Unpatch()
    {
        _harmony.UnpatchAll();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GuiDialog), nameof(GuiDialog.OnRenderGUI))]
    public static bool BlockInfoHudPatch_RenderGUI(GuiDialog __instance)
    {
        if (__instance is HudElementBlockAndEntityInfo)
        {
            return !SimpleEntityHealthbarModSystem.IsHealthBarActive();   
        }

        return true;
    }
    
}