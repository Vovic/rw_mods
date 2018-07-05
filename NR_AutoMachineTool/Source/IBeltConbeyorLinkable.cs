﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace NR_AutoMachineTool
{
    interface IBeltConbeyorLinkable
    {
        void Link(IBeltConbeyorLinkable linkable);
        void Unlink(IBeltConbeyorLinkable linkable);
        Rot4 Rotation { get; }
        IntVec3 Position { get; }
        bool ReceivableNow(bool underground, Thing thing);
        bool ReceiveThing(bool underground, Thing thing);
        bool IsUnderground { get; }
        IEnumerable<Rot4> OutputRots { get; }
    }
}
