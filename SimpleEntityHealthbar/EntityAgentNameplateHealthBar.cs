using System.Drawing;
using System.Runtime.CompilerServices;
using SimpleEntityHealthbar;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace Simpleentityhealthbarnameplate;

using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

#nullable disable

public class EntityAgentNameplateHealthBar : HudElement
{
    private EntityAgent _targetEntityAgent;
    private EntityAgent _lastTargetEntityAgent;

    private GuiElementStatbar _healthbar;
    private GuiElementDynamicText _title;
    private long _drawTickListenerId;
    private long _clearTickListenerId;

    public override bool ShouldReceiveKeyboardEvents() => false;
    public override bool ShouldReceiveMouseEvents() => false;
    public override bool Focusable => false;
    public override bool ShouldReceiveRenderEvents() => IsHealthBarActive();

    public EntityAgentNameplateHealthBar(ICoreClientAPI clientApi) : base(clientApi)
    {
        _drawTickListenerId = clientApi.Event.RegisterGameTickListener(Every15ms, 100);
    }

    public override string ToggleKeyCombinationCode => null;

    public bool IsHealthBarActive()
    {
        return (_targetEntityAgent != null && _healthbar != null) || (_targetEntityAgent == null && _clearTickListenerId != 0 && _healthbar != null);
    }

    public override void OnGuiOpened()
    {
        ComposeGuis();
    }

    private bool TryGetDistantTarget()
    {
        // Helps reduce the number of ray trace calls
        // If player is targeting a block we skip
        if (capi.World.Player.CurrentBlockSelection != null) return false;
        
        IClientWorldAccessor world = capi.World;
        ClientMain game = (world as ClientMain);
        EntityPlayer player = game?.EntityPlayer;
        if (player == null)  return false;
        
        EntityFilter efilter = (Entity e) => e.IsInteractable && e.EntityId != player.EntityId;
        Vec3d fromPos = player.Pos.XYZ.Add(player.LocalEyePos);
        BlockSelection blockSelection = new (); // make a dummy block selection object to prevent changing player targeting capabilities
        game.RayTraceForSelection(fromPos, game.player.Entity.SidedPos.Pitch, game.player.Entity.SidedPos.Yaw, float.Max(ModConfig.Instance.MaxTargetDistance, 50), ref blockSelection, ref player.EntitySelection, null, efilter);
        return player.EntitySelection != null;
    }

    private void Every15ms(float dt)
    {
        EntityAgent oldTargetEntityAgent = _targetEntityAgent;
        
        _targetEntityAgent = capi.World.Player?.CurrentEntitySelection?.Entity as EntityAgent;

        if (_targetEntityAgent == null)
        {
            TryGetDistantTarget();
            _targetEntityAgent = capi.World.Player?.CurrentEntitySelection?.Entity as EntityAgent;
        }

        
        if  (oldTargetEntityAgent == null && _targetEntityAgent == null) return;
        

        // Lost Target lets schedule removal
        if (oldTargetEntityAgent != null && _targetEntityAgent == null)
        {
            if (_clearTickListenerId > 0) return;
            _clearTickListenerId = capi.Event.RegisterCallback((timePassed) =>
            {
                TryClose();
                _clearTickListenerId = 0;
                Logger.Debug("Scheduling healthbar removal");
                _lastTargetEntityAgent = oldTargetEntityAgent;
            }, 5000);
            return;
        }

        // We want to draw a new target entity so we clear the current delayed close.
        if (_clearTickListenerId > 0)
        {
            capi.Event.UnregisterCallback(_clearTickListenerId);
            _clearTickListenerId = 0;
            Composers.ClearComposers();
            ComposeGuis();
        }

        if (!IsOpened()) TryOpen();
        _lastTargetEntityAgent  = _targetEntityAgent;
        UpdateHealth();
    }

    private void UpdateHealth()
    {
        ITreeAttribute healthTree = _targetEntityAgent?.WatchedAttributes.GetTreeAttribute("health");
        if (healthTree == null) return;

        float? health = healthTree.TryGetFloat("currenthealth");
        float? maxHealth = healthTree.TryGetFloat("maxhealth");

        if (health == null || maxHealth == null) return;
        if (_healthbar == null) return;

        _healthbar.SetLineInterval(1);
        _healthbar.SetValues((float)health, 0, (float)maxHealth);

        if (_title == null) return;
        _title.Text = GetHealthTextValue(health, maxHealth);
        _title.RecomposeText();
    }

    public void ComposeGuis()
    {
        if (_targetEntityAgent == null) return;
        float width = 100f;
        ElementBounds dialogBounds = new ElementBounds()
        {
            Alignment = EnumDialogArea.CenterFixed,
            BothSizing = ElementSizing.Fixed,
            fixedWidth = width*4,
            fixedHeight = 50,
            fixedY = 10
        }.WithFixedAlignmentOffset(0, 5);

        ElementBounds healthBarBounds = ElementBounds.Fixed(width*3/2, 35, width, 14);
        ElementBounds entityNameBounds = ElementBounds.Fixed(0, 0, width*4, 30);


        double[] darkGreen = ColorUtil.ToRGBADoubles(Color.ForestGreen.ToArgb());
        double[] black = ColorUtil.ToRGBADoubles(Color.Black.ToArgb());

        string entityName = _targetEntityAgent.GetName();

        double[] healthbarColor = GuiStyle.HealthBarColor;
        if (_targetEntityAgent is EntityTrader)
        {
            healthbarColor = darkGreen;
            // Hide name on nameplate because Entity already has a nametag
            if (_targetEntityAgent.HasBehavior<EntityBehaviorNameTag>()) entityName = "";
        }
        
        CairoFont entityNameFont = CairoFont.WhiteSmallishText().WithStroke(black, 2.0);
        entityNameFont.Orientation = EnumTextOrientation.Center;

        ITreeAttribute healthTree = _targetEntityAgent.WatchedAttributes.GetTreeAttribute("health");
        string key = "entityhealthbarnameplatenameplate-" + _targetEntityAgent.EntityId;
        Composers["entityhealthbarnameplate"] =
            capi.Gui
                .CreateCompo(key, dialogBounds.FlatCopy().FixedGrow(0, 20))
                .BeginChildElements(dialogBounds)
                .AddIf(healthTree != null)
                .AddDynamicText(entityName, entityNameFont, entityNameBounds)
                .AddStatbar(healthBarBounds, healthbarColor, "healthstatbar")
                .EndIf()
                .EndChildElements();

        _title = Composers["entityhealthbarnameplate"].GetElement("healthbarvalue") as GuiElementDynamicText;
        _healthbar = Composers["entityhealthbarnameplate"].GetStatbar("healthstatbar");
    }

    public override void OnRenderGUI(float deltaTime)
    {
        if (!IsHealthBarActive()) return;
        base.OnRenderGUI(deltaTime);
        EntityAgent target = _targetEntityAgent;
        if (_targetEntityAgent == null)
        {
            if (_lastTargetEntityAgent == null)
            {
                Logger.Error("Expected LastTargetEntityAgent to not be null");
                return;
            }
            target = _lastTargetEntityAgent;
        }
        var pos = target.Pos;
        // We either calculate based on the Entity eye position or we use 1.8 because the poor traders have their eyes on their feet
        Vec3d vec3d = MatrixToolsd.Project(new Vec3d((double)pos.X, (double)pos.Y + double.Max(target.LocalEyePos.Y*1.5, 1.8), (double)pos.Z),
            capi.Render.PerspectiveProjectionMat, capi.Render.PerspectiveViewMat, capi.Render.FrameWidth,
            capi.Render.FrameHeight);
        if (vec3d.Z < 0.0) return;
        

        // Project to world
        ElementBounds cB = Composers["entityhealthbarnameplate"].Bounds;
        cB.Alignment = EnumDialogArea.None;
        cB.fixedOffsetX = 0.0;
        cB.fixedOffsetY = 0.0;
        cB.absFixedX = vec3d.X - Composers["entityhealthbarnameplate"].Bounds.OuterWidth / 2.0;
        cB.absFixedY = (double)capi.Render.FrameHeight - vec3d.Y - Composers["entityhealthbarnameplate"].Bounds.OuterHeight * 0.75;
        cB.absMarginX = 0.0;
        cB.absMarginY = 0.0;
        Composers["entityhealthbarnameplate"].Compose();
    }


    private string GetHealthTextValue(float? health, float? maxHealth)
    {
        var healthRnd = health != null ? MathF.Round((float)health, 1) : 0.0f;
        var maxHealthRnd = maxHealth != null ? MathF.Round((float)maxHealth, 1) : 0.0f;
        return string.Format("{0} / {1}", healthRnd, maxHealthRnd);
    }


    public override void Dispose()
    {
        if (IsOpened()) TryClose();
        base.Dispose();
        capi.Event.UnregisterGameTickListener(_drawTickListenerId);
    }
}