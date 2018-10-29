﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using NR_AutoMachineTool.Utilities;
using static NR_AutoMachineTool.Utilities.Ops;

namespace NR_AutoMachineTool
{

    public interface IRangePowerSupplyMachine
    {
        int MinPowerForSpeed { get; }
        int MaxPowerForSpeed { get; }
        int MinPowerForRange { get; }
        int MaxPowerForRange { get; }

        float SupplyPowerForSpeed { get; set; }
        float SupplyPowerForRange { get; set; }

        bool Glowable { get; }
        bool Glow { get; set; }
        bool SpeedSetting { get; }
    }

    class ITab_RangePowerSupply : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(600f, 380f);

        public ITab_RangePowerSupply()
        {
            this.size = WinSize;
            this.labelKey = "NR_AutoMachineTool.SupplyPowerTab";

            this.descriptionForSpeed = "NR_AutoMachineTool.SupplyPowerForSpeedText".Translate();
            this.descriptionForRange = "NR_AutoMachineTool.SupplyPowerForRangeText".Translate();
        }
        
        private string descriptionForSpeed;

        private string descriptionForRange;

        private IRangePowerSupplyMachine Machine { get => (IRangePowerSupplyMachine)this.SelThing; }

        protected override void FillTab()
        {
            int minPowerSpeed = this.Machine.MinPowerForSpeed;
            int maxPowerSpeed = this.Machine.MaxPowerForSpeed;
            int minPowerRange = this.Machine.MinPowerForRange;
            int maxPowerRange = this.Machine.MaxPowerForRange;

            string valueLabelForSpeed = "NR_AutoMachineTool.SupplyPowerForSpeedValueLabel".Translate() + " (" + minPowerSpeed + " to " + maxPowerSpeed + ") ";
            string valueLabelForRange = "NR_AutoMachineTool.SupplyPowerForRangeValueLabel".Translate() + " (" + minPowerRange + " to " + maxPowerRange + ") ";

            Listing_Standard list = new Listing_Standard();
            Rect inRect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);

            list.Begin(inRect);
            list.Gap();
            var rect = new Rect();

            if (this.Machine.SpeedSetting)
            {
                // for speed
                rect = list.GetRect(50f);
                Widgets.Label(rect, descriptionForSpeed);
                list.Gap();

                rect = list.GetRect(50f);
                var speed = (int)Widgets.HorizontalSlider(rect, (float)this.Machine.SupplyPowerForSpeed, (float)minPowerSpeed, (float)maxPowerSpeed, true, valueLabelForSpeed, minPowerSpeed.ToString(), maxPowerSpeed.ToString(), 100);
                this.Machine.SupplyPowerForSpeed = speed;
                list.Gap();

                rect = list.GetRect(30f);
                string buf = this.Machine.SupplyPowerForSpeed.ToString();
                int power = (int)this.Machine.SupplyPowerForSpeed;
                Widgets.Label(rect.LeftHalf(), valueLabelForSpeed);
                Widgets.TextFieldNumeric<int>(rect.RightHalf(), ref power, ref buf, this.Machine.SupplyPowerForSpeed, this.Machine.SupplyPowerForSpeed);
                list.Gap();
                this.Machine.SupplyPowerForSpeed = power;
            }

            // for range
            rect = list.GetRect(50f);
            Widgets.Label(rect, descriptionForRange);
            list.Gap();

            rect = list.GetRect(50f);
            var range = (int)Widgets.HorizontalSlider(rect, (float)this.Machine.SupplyPowerForRange, (float)minPowerRange, (float)maxPowerRange, true, valueLabelForRange, minPowerRange.ToString(), maxPowerRange.ToString(), 500);
            this.Machine.SupplyPowerForRange = range;
            list.Gap();

            if (this.Machine.Glowable)
            {
                rect = list.GetRect(30f);
                bool glow = this.Machine.Glow;
                Widgets.CheckboxLabeled(rect, "NR_AutoMachineTool.SupplyPowerSunLampText".Translate(), ref glow);
                this.Machine.Glow = glow;
            }
            list.Gap();

            list.End();
        }
    }
}
