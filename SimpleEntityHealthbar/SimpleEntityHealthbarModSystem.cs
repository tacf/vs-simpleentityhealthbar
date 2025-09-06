
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
    private GuiDialog _gameDefaultTooltip;

    public override void Start(ICoreAPI api)
    {
        api.World.Logger.Event("started '" + ModId + "'");   
    }
    

    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        _capi.Event.RegisterGameTickListener(OnGameTick, 100, 0);
        _gameDefaultTooltip = _capi.Gui.LoadedGuis.Find(x => x.DebugName == "HudElementBlockAndEntityInfo");
        
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
                    _mobhealthBar.TryClose();
                    _mobhealthBar.Dispose();
                    _mobhealthBar = null;
                } 
                
                if (targetEntity.Alive && targetEntity.IsInteractable && targetEntity.GetBehavior<EntityBehaviorBoss>() == null)
                {
                    _mobhealthBar = new EntityAgentHealthBar(_capi, targetEntity as EntityAgent);
                    _gameDefaultTooltip?.TryClose();
                    _gameDefaultTooltip?.ClearComposers();
                    _mobhealthBar.ComposeGuis();
                }
            } else if (targetEntity == null)
            {
                if (_mobhealthBar != null)
                {
                    _mobhealthBar.TryClose();
                    _mobhealthBar.Dispose();
                    _gameDefaultTooltip?.TryOpen();
                    _mobhealthBar = null;
                }
            }
            _mobhealthBarEntity = targetEntity;
    }
}
