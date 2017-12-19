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
    class ITab_PullerFilter : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(300f, 500f);

        public ITab_PullerFilter()
        {
            this.size = WinSize;
            this.labelKey = "NR_AutoMachineTool_Puller.OutputItemFilterTab";

            this.description = "NR_AutoMachineTool_Puller.OutputItemFilterText".Translate();
        }

        private string description;

        private Building_ItemPuller Puller
        {
            get => (Building_ItemPuller)this.SelThing;
        }

        public override bool IsVisible => Puller.Filter != null;
        
        private Vector2 scrollPosition;

        protected override void FillTab()
        {
            Listing_Standard list = new Listing_Standard();
            Rect inRect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);

            list.Begin(inRect);
            list.Gap();

            var rect = list.GetRect(40f);
            Widgets.Label(rect, this.description);
            list.Gap();

            list.End();
            var height = list.CurHeight;

            ThingFilterUI.DoThingFilterConfigWindow(inRect.BottomPartPixels(inRect.height - height), ref this.scrollPosition, this.Puller.Filter);
        }
    }
}
