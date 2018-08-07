using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Rimlaser
{

    class LaserGun :ThingWithComps, IBeamColorThing
    {
        new public LaserGunDef def => base.def as LaserGunDef ?? LaserGunDef.defaultObj;

        public int BeamColor
        {
            get { return beamColorIndex; }
            set { beamColorIndex = value; }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref beamColorIndex, "beamColorIndex", -1, false);
        }


        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(pawn))
            {
                if (o != null) yield return o;
            }

            if (!def.supportsColors) yield break;

            foreach (FloatMenuOption o in LaserColor.GetChangeBeamColorFloatMenuOptions(this, pawn))
            {
                if (o != null) yield return o;
            }
            
            yield break;
        }

        private int beamColorIndex = -1;
    }
}
