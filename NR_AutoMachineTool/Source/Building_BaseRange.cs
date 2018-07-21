﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using NR_AutoMachineTool.Utilities;
using static NR_AutoMachineTool.Utilities.Ops;
using System.Collections;

namespace NR_AutoMachineTool
{
    public interface IRange
    {
        int GetRange();
        IntVec3 Position { get; }
        Rot4 Rotation { get; }
        IEnumerable<IntVec3> GetAllTargetCells();
    }

    public abstract class Building_BaseRange<T> : Building_BaseLimitation<T>, IRange, IRangePowerSupplyMachine where T : Thing
    {
        public int MinPowerForRange => this.Extension.TargetCellResolver.MinPowerForRange;
        public int MaxPowerForRange => this.Extension.TargetCellResolver.MaxPowerForRange;

        public virtual bool Glowable { get => false; }

        private bool glow = false;
        public virtual bool Glow
        {
            get => this.glow;
            set
            {
                if (this.glow != value)
                {
                    this.glow = value;
                    this.ChangeGlow();
                }
            }
        }

        protected ModExtension_AutoMachineTool Extension => this.def.GetModExtension<ModExtension_AutoMachineTool>();

        private float supplyPowerForRange;

        public float SupplyPowerForRange
        {
            get => this.supplyPowerForRange;
            set
            {
                if (this.supplyPowerForRange != value)
                {
                    this.supplyPowerForRange = value;
                    this.ChangeGlow();
                    this.allTargetCellsCache = null;
                }
                this.SetPower();
            }
        }

        [Unsaved]
        protected int targetEnumrationCount = 200;

        [Unsaved]
        private bool nextTargetCells = false;

        [Unsaved]
        private HashSet<IntVec3> allTargetCellsCache;

        [Unsaved]
        private List<List<IntVec3>> splittedTargetCells;

        [Unsaved]
        private int splittedTargetCellsIndex = 0;

        private const int CACHE_CLEAR_INTERVAL_TICKS = 180;

        public IEnumerable<IntVec3> GetAllTargetCells()
        {
            this.CacheTargetCells();
            return allTargetCellsCache;
        }

        private void CacheTargetCells()
        {
            if (this.allTargetCellsCache == null)
            {
                this.allTargetCellsCache = this.Extension.TargetCellResolver.GetRangeCells(this.Position, this.Map, this.Rotation, this.GetRange()).ToHashSet();
                if (this.targetEnumrationCount > 0)
                {
                    this.splittedTargetCells = this.allTargetCellsCache.ToList().Grouped(this.targetEnumrationCount);
                }
            }
        }

        private List<IntVec3> GetCurrentSplittedTargetCells()
        {
            this.CacheTargetCells();
            if (this.splittedTargetCellsIndex >= this.splittedTargetCells.Count)
            {
                this.splittedTargetCellsIndex = 0;
            }
            return this.splittedTargetCells[this.splittedTargetCellsIndex];
        }

        private void NextSplittedTargetCells()
        {
            this.splittedTargetCellsIndex++;
            if (this.splittedTargetCellsIndex >= this.splittedTargetCells.Count)
            {
                this.splittedTargetCellsIndex = 0;
            }
        }

        private void ClearAllTargetCellCache()
        {
            if (this.IsActive())
            {
                this.allTargetCellsCache = null;
                if (this.Extension.TargetCellResolver.NeedClearingCache)
                {
                    MapManager.AfterAction(CACHE_CLEAR_INTERVAL_TICKS, this.ClearAllTargetCellCache);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.supplyPowerForRange, "supplyPowerForRange", this.MinPowerForRange);
            Scribe_Values.Look<bool>(ref this.glow, "glow", false);
        }

        protected override void ReloadSettings(object sender, EventArgs e)
        {
            if (this.SupplyPowerForRange < this.MinPowerForRange)
            {
                this.SupplyPowerForRange = this.MinPowerForRange;
            }
            if (this.SupplyPowerForRange > this.MaxPowerForRange)
            {
                this.SupplyPowerForRange = this.MaxPowerForRange;
            }
        }

        protected override void SetPower()
        {
            if (-this.SupplyPowerForRange - this.SupplyPowerForSpeed - (this.Glowable && this.Glow ? 2000 : 0) != this.TryGetComp<CompPowerTrader>().PowerOutput)
            {
                this.powerComp.PowerOutput = -this.SupplyPowerForRange - this.SupplyPowerForSpeed - (this.Glowable && this.Glow ? 2000 : 0);
            }
        }

        private void ChangeGlow()
        {
            Option(this.TryGetComp<CompGlower>()).ForEach(glower =>
            {
                var tmp = this.TryGetComp<CompFlickable>().SwitchIsOn;
                glower.Props.glowRadius = this.Glow ? (this.GetRange() + 2f) * 2f : 0;
                glower.Props.overlightRadius = this.Glow ? (this.GetRange() + 2.1f) : 0;
                this.TryGetComp<CompFlickable>().SwitchIsOn = !tmp;
                this.TryGetComp<CompPowerTrader>().PowerOn = !tmp;
                glower.UpdateLit(this.Map);
                this.TryGetComp<CompFlickable>().SwitchIsOn = tmp;
                this.TryGetComp<CompPowerTrader>().PowerOn = tmp;
                glower.UpdateLit(this.Map);
            });
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                this.SupplyPowerForRange = this.MinPowerForRange;
            }
            Option(this.TryGetComp<CompGlower>()).ForEach(g =>
            {
                CompProperties_Glower newProp = new CompProperties_Glower();
                newProp.compClass = g.Props.compClass;
                newProp.glowColor = g.Props.glowColor;
                newProp.glowRadius = g.Props.glowRadius;
                newProp.overlightRadius = g.Props.overlightRadius;
                g.props = newProp;
            });
            this.ChangeGlow();
            if (this.Extension.TargetCellResolver.NeedClearingCache)
            {
                MapManager.AfterAction(CACHE_CLEAR_INTERVAL_TICKS, this.ClearAllTargetCellCache);
            }
        }

        public int GetRange()
        {
            return this.Extension.TargetCellResolver.GetRange(this.SupplyPowerForRange);
        }

        protected virtual IEnumerable<IntVec3> GetTargetCells()
        {
            if (SplitTargetCells)
            {
                this.nextTargetCells = true;
                return this.GetCurrentSplittedTargetCells(); ;
            }
            else
            {
                return this.GetAllTargetCells();
            }
        }

        /*
        public override void Draw()
        {
            base.Draw();

            if (Find.Selector.FirstSelectedObject == this && this.SplitTargetCells)
            {
                GenDraw.DrawFieldEdges(this.GetCurrentSplittedTargetCells(), Color.red);
            }
        }
        */

        protected override void Ready()
        {
            base.Ready();
            if (this.State == WorkingState.Ready && SplitTargetCells && this.nextTargetCells)
            { 
                this.NextSplittedTargetCells();
                this.nextTargetCells = false;
            }
        }

        private bool SplitTargetCells => this.targetEnumrationCount > 0 && this.GetAllTargetCells().Count() > this.targetEnumrationCount;
    }
}
