using System.Drawing;
using System.Runtime.CompilerServices;
using Vintagestory.API.MathTools;

namespace SimpleEntityHealthbar;

using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

#nullable disable

public class EntityAgentHealthBar : HudElement
{
        public EntityAgent TargetEntityAgent;

        private ICoreClientAPI _clientApi;
        
        public GuiElementStatbar healthbar;
        private GuiElementDynamicText title;
        private long drawTickListenerId;
        private long clearTickListenerId;
        
        public override bool ShouldReceiveKeyboardEvents() => false;
        public override bool ShouldReceiveMouseEvents() => false;
        public override bool Focusable => false;
        public override bool ShouldReceiveRenderEvents() => IsHealthBarActive();

        public EntityAgentHealthBar(ICoreClientAPI clientApi) : base(clientApi)
        {
            drawTickListenerId = clientApi.Event.RegisterGameTickListener(Every15ms, 15);
            _clientApi = clientApi;
        }
        public override string ToggleKeyCombinationCode => null;

        public bool IsHealthBarActive()
        {
            return TargetEntityAgent != null || (TargetEntityAgent == null && clearTickListenerId != 0);
        }

        public override void OnGuiOpened()
        {
            ComposeGuis();
        }

        private void Every15ms(float dt)
        {
            EntityAgent oldTargetEntityAgent = TargetEntityAgent;
            TargetEntityAgent = _clientApi.World.Player?.CurrentEntitySelection?.Entity as EntityAgent;
            if (oldTargetEntityAgent == null && TargetEntityAgent == null) return;
            
            // Lost Target lets schedule removal
            if (oldTargetEntityAgent != null && TargetEntityAgent == null)
            {
                if (clearTickListenerId > 0) return;
                clearTickListenerId = _clientApi.Event.RegisterCallback((timePassed) =>
                {
                    TryClose();
                    clearTickListenerId = 0;
                }, 1000);
                return;
            }

            // We want to draw a new target entity so we clear the current delayed close.
            if (clearTickListenerId > 0)
            {
                _clientApi.Event.UnregisterCallback(clearTickListenerId);
                clearTickListenerId = 0;
                Composers.ClearComposers();
                ComposeGuis();
            }
            
            if (!IsOpened()) TryOpen();
            UpdateHealth();
            
        }

        void UpdateHealth()
        {

            ITreeAttribute healthTree = TargetEntityAgent?.WatchedAttributes.GetTreeAttribute("health");
            if (healthTree == null) return;

            float? health = healthTree.TryGetFloat("currenthealth");
            float? maxHealth = healthTree.TryGetFloat("maxhealth");

            if (health == null || maxHealth == null) return;
            if (healthbar == null) return;

            healthbar.SetLineInterval(1);
            healthbar.SetValues((float)health, 0, (float)maxHealth);

            if (title == null) return;
            title.Text = GetHealthTextValue(health, maxHealth);
            title.RecomposeText();
        }

        public void ComposeGuis()
        {
            if (TargetEntityAgent == null) return;
            float width = _clientApi.Gui.WindowBounds.OuterWidthInt * 0.3f;
            ElementBounds dialogBounds = new ElementBounds()
            {
                Alignment = EnumDialogArea.CenterFixed,
                BothSizing = ElementSizing.Fixed,
                fixedWidth = width,
                fixedHeight = 50,
                fixedY = 10
            }.WithFixedAlignmentOffset(0, 5);

            ElementBounds healthBarBounds = ElementBounds.Fixed(0, 35, width, 14);
            ElementBounds entityNameBounds = ElementBounds.Fixed(0, 0, width*0.8, 30);
            ElementBounds entityHealthBounds = ElementBounds.Fixed(width*0.8, 0, width*0.2, 30);
            entityHealthBounds.fixedMarginX = 5.0;
            entityNameBounds.fixedMarginX = 5.0;
            dialogBounds.fixedMarginX = 5.0;

            
            double[] darkGreen = ColorUtil.ToRGBADoubles(Color.ForestGreen.ToArgb());
            double[] black = ColorUtil.ToRGBADoubles(Color.Black.ToArgb());

            string entityName = TargetEntityAgent.GetName();
            string healthTextValue = GetHealthTextValue(0.0f, 0.0f);

            double[] healthbarColor = GuiStyle.HealthBarColor;
            if (TargetEntityAgent is EntityTrader) healthbarColor = darkGreen;

            CairoFont entityHealthFont = CairoFont.WhiteMediumText().WithStroke(black, 2.0);
            entityHealthFont.Orientation = EnumTextOrientation.Right;
            CairoFont entityNameFont = CairoFont.WhiteMediumText().WithStroke(black, 2.0);
            
            ITreeAttribute healthTree = TargetEntityAgent.WatchedAttributes.GetTreeAttribute("health");
            string key = "entityhealthbar-" + TargetEntityAgent.EntityId;
            Composers["entityhealthbar"] =
                capi.Gui
                .CreateCompo(key, dialogBounds.FlatCopy().FixedGrow(0, 20))
                .BeginChildElements(dialogBounds)
                    .AddIf(healthTree != null)
                        .AddDynamicText(entityName, entityNameFont, entityNameBounds)
                        .AddDynamicText(healthTextValue, entityHealthFont, entityHealthBounds, "healthbarvalue")
                        .AddStatbar(healthBarBounds, healthbarColor, "healthstatbar")
                        .AddRichtext(TargetEntityAgent.GetInfoText(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, 60, width, 20))
                    .EndIf()
                .EndChildElements().Compose();

            title = Composers["entityhealthbar"].GetElement("healthbarvalue") as GuiElementDynamicText;
            healthbar = Composers["entityhealthbar"].GetStatbar("healthstatbar");
        }
        

        private string GetHealthTextValue(float? health, float? maxHealth)
        {
            var healthRnd =  health != null ? MathF.Round((float)health, 1) : 0.0f;
            var maxHealthRnd =  maxHealth != null ? MathF.Round((float)maxHealth, 1) : 0.0f;
            return string.Format("{0} / {1}", healthRnd , maxHealthRnd);
        }


        public override void Dispose()
        {
            if (IsOpened()) TryClose();
            base.Dispose();
            _clientApi.Event.UnregisterGameTickListener(drawTickListenerId);
        }

    }