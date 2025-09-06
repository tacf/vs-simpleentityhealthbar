using Vintagestory.API.Common.Entities;
using Vintagestory.Client.NoObf;

namespace SimpleEntityHealthbar;

// Class Highly based on https://github.com/anegostudios/vssurvivalmod/blob/master/Gui/HudBosshealthBars.cs

using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

#nullable disable

public class EntityAgentHealthBar : HudElement
    {
        float lastHealth;
        float lastMaxHealth;
        public EntityAgent TargetEntityAgent;

        GuiElementStatbar healthbar;
        GuiElementDynamicText title;
        long listenerId;

        public override double InputOrder { get { return 1; } }

        public EntityAgentHealthBar(ICoreClientAPI capi, EntityAgent entityAgent) : base(capi)
        {
            this.TargetEntityAgent = entityAgent;
            listenerId = capi.Event.RegisterGameTickListener(this.OnGameTick, 20);
            

            ComposeGuis();
        }
        public override string ToggleKeyCombinationCode { get { return null; } }

        private void OnGameTick(float dt)
        {
            UpdateHealth();
        }
      

        void UpdateHealth()
        {
            ITreeAttribute healthTree = TargetEntityAgent.WatchedAttributes.GetTreeAttribute("health");
            if (healthTree == null) return;

            float? health = healthTree.TryGetFloat("currenthealth");
            float? maxHealth = healthTree.TryGetFloat("maxhealth");

            if (health == null || maxHealth == null) return;
            if (lastHealth == health && lastMaxHealth == maxHealth) return;
            if (healthbar == null) return;

            healthbar.SetLineInterval(1);
            healthbar.SetValues((float)health, 0, (float)maxHealth);

            lastHealth = (float)health;
            lastMaxHealth = (float)maxHealth;

            if (title != null)
            {
                title.Text = GetHealthTextValue(health, maxHealth);
                title.RecomposeText();
            }
        }

        public void ComposeGuis()
        {
            float width = 850;
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
            CairoFont entityHealthFont = CairoFont.WhiteMediumText();
            entityHealthFont.Orientation = EnumTextOrientation.Right;
            

            string description = GetHealthTextValue(0.0f, 0.0f);

            ITreeAttribute healthTree = TargetEntityAgent.WatchedAttributes.GetTreeAttribute("health");
            string key = "entityhealthbar-" + TargetEntityAgent.EntityId;
            Composers["entityhealthbar"] =
                capi.Gui
                .CreateCompo(key, dialogBounds.FlatCopy().FixedGrow(0, 20))
                .BeginChildElements(dialogBounds)
                    .AddIf(healthTree != null)
                        .AddDynamicText(TargetEntityAgent.GetName(), CairoFont.WhiteMediumText(), entityNameBounds)
                        .AddDynamicText(description, entityHealthFont, entityHealthBounds, "healthbarvalue")
                        .AddStatbar(healthBarBounds, GuiStyle.HealthBarColor, "healthstatbar")
                        .AddRichtext(TargetEntityAgent.GetInfoText(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, 60, width, 20))
                    .EndIf()
                .EndChildElements()
                .Compose()
            ;

            title = Composers["entityhealthbar"].GetElement("healthbarvalue") as GuiElementDynamicText;
            healthbar = Composers["entityhealthbar"].GetStatbar("healthstatbar");
            TryOpen();
        }

        private string GetHealthTextValue(float? health, float? maxHealth)
        {
            var healthRnd =  health != null ? MathF.Round((float)health, 1) : 0.0f;
            var maxHealthRnd =  maxHealth != null ? MathF.Round((float)maxHealth, 1) : 0.0f;
            return string.Format("({0}/{1})", healthRnd , maxHealthRnd);
        }

        // Can't be closed
        public override bool TryClose()
        {
            return base.TryClose();
        }

        public override bool ShouldReceiveKeyboardEvents()
        {
            return false;
        }
        

        public override void OnRenderGUI(float deltaTime)
        {   
            base.OnRenderGUI(deltaTime);
        }
        
        
        // Can't be focused
        public override bool Focusable => false;

        // Can't be focused
        protected override void OnFocusChanged(bool on)
        {

        }

        public override void OnMouseDown(MouseEvent args)
        {
            // Can't be clicked
        }

        public override void Dispose()
        {
            base.Dispose();

            capi.Event.UnregisterGameTickListener(listenerId);
        }

    }
