
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace SimpleEntityHealthbar;

public class SimpleEntityHealthbarModSystem : ModSystem
{
    private ICoreClientAPI _capi;
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
    public static string ModId = "SimpleEntityHealthbar";
    private Entity _mobhealthBarEntity;
    private EntityAgentHealthBar _mobhealthBar;

    public override void Start(ICoreAPI api)
    {
        api.World.Logger.Event("started '" + ModId + "'");   
    }
    

    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        _capi.Event.RegisterGameTickListener(OnGameTick, 100, 0);
        
        base.StartClientSide(_capi);
    }
    
        
    private void OnGameTick(float obj)
    {
            var targetEntity = _capi.World.Player.CurrentEntitySelection?.Entity;
            if (targetEntity is EntityAgent)
            {
                if (_mobhealthBarEntity == targetEntity && _mobhealthBarEntity != null) return;
                if (_mobhealthBar != null && _mobhealthBarEntity != targetEntity)
                {
                    _mobhealthBarEntity = null;
                    _mobhealthBar.TryClose();
                    _mobhealthBar.Dispose();
                    _mobhealthBar = null;
                } 
                
                if (targetEntity.Alive && targetEntity.IsInteractable && targetEntity.GetBehavior<EntityBehaviorBoss>() == null)
                {
                    _mobhealthBarEntity = targetEntity;
                    _mobhealthBar = new EntityAgentHealthBar(_capi, _mobhealthBarEntity as EntityAgent);
                    var targetTooltip = _capi.Gui.LoadedGuis.Find(x => x.DebugName == "HudElementBlockAndEntityInfo");
                    if (targetTooltip != null)
                    {
                        targetTooltip.TryClose();
                    }
                    _mobhealthBar.ComposeGuis();
                }
            } else if (targetEntity == null)
            {
                _mobhealthBarEntity = null;
                if (_mobhealthBar != null)
                {
                    _mobhealthBar.TryClose();
                    _mobhealthBar.Dispose();
                }
            }
    }
}
