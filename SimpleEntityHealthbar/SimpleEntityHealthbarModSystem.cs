
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace SimpleEntityHealthbar;

public class SimpleEntityHealthbarModSystem : ModSystem
{
    private ICoreClientAPI _capi;
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
    public static string ModId = "SimpleEntityHealthbar";
    private Entity _mobhealthBarEntity;
    private EntityAgentHealthBar _mobhealthBar;
    private bool _needUpdateBlockInfoHudVisibility = ClientSettings.ShowBlockInfoHud; 

    public override void Start(ICoreAPI api)
    {
        api.World.Logger.Event("started '" + ModId + "'");   
    }
    

    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        _capi.Event.RegisterGameTickListener(OnGameTick, 100, 0);
        _capi.Settings.AddWatcher<bool>("showBlockInfoHud", OnShowBlockInfoHud);
        
        base.StartClientSide(_capi);
    }

    private void OnShowBlockInfoHud(bool newValue)
    {
        // We prevent from showing the hud if enabled during HB display
        // we flag to presentation after bar disappears
        if (newValue && _mobhealthBar != null)
        {
            ClientSettings.ShowBlockInfoHud = false;
            _needUpdateBlockInfoHudVisibility = true;
        }
    }


    // Handle hiding BlockInfoHud during Healthbar presentation
    private void HandleBlockInfoHudVisibility(bool hide = false)
    {
        var showingBlockInfoHud = ClientSettings.ShowBlockInfoHud;
        
        // We're going to display a healtbar and the hud is present (we want to hide it)
        if (showingBlockInfoHud && hide)
        {
            ClientSettings.ShowBlockInfoHud = false;
            _needUpdateBlockInfoHudVisibility = true;
        } 
        else if (!showingBlockInfoHud && !hide && _needUpdateBlockInfoHudVisibility)
        {
            // We want to enable Block info hud, and we previously have changed it
            ClientSettings.ShowBlockInfoHud = true;
            _needUpdateBlockInfoHudVisibility = false;
        }
    }

    private void OnGameTick(float obj)
    {
            var targetEntity = _capi.World.Player.CurrentEntitySelection?.Entity;
            switch (targetEntity)
            {
                case EntityAgent when _mobhealthBarEntity == targetEntity && _mobhealthBarEntity != null:
                    return;
                case EntityAgent agent:
                {
                    if (_mobhealthBar != null && _mobhealthBarEntity != targetEntity)
                    {
                        _mobhealthBar.TryClose();
                        _mobhealthBar.Dispose();
                        _mobhealthBar = null;
                    } 
                
                    if (agent.Alive && agent.IsInteractable && agent.GetBehavior<EntityBehaviorBoss>() == null)
                    {
                        _mobhealthBar = new EntityAgentHealthBar(_capi, agent);
                        HandleBlockInfoHudVisibility(hide: true);
                        _mobhealthBar.ComposeGuis();
                    }

                    break;
                }
                case null:
                {
                    if (_mobhealthBar != null)
                    {
                        _mobhealthBar.TryClose();
                        _mobhealthBar.Dispose();
                        _mobhealthBar = null;
                        HandleBlockInfoHudVisibility(hide: false); // has to be last
                    }

                    break;
                }
            }
            _mobhealthBarEntity = targetEntity;
    }
}
