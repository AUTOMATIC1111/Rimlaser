using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Rimlaser.Util
{
    public static class ThingExtensions
    {
        public static bool IsShielded(this Thing myThing)
        {
            if (myThing == null) return false;
            return (myThing as Pawn).IsShielded();
        }

        public static bool HasBlood(this Thing myThing, string type = null)
        {
            if (myThing == null) return false;
            return (myThing as Pawn).HasBlood(type);
        }

        public static bool IsShielded(this Pawn myPawn)
        {
            if (myPawn == null || myPawn.apparel == null) return false;
            DamageInfo damageTest = new DamageInfo(DamageDefOf.Bomb, 0f, 0f, -1, null);
            //Check for equipped items that could obstruct the beam
            for (int i = 0; i < myPawn.apparel.WornApparel.Count; i++)
            {
                if (myPawn.apparel.WornApparel.ElementAt<Apparel>(i).CheckPreAbsorbDamage(damageTest))
                    return true;
            }
            return false;
        }

        public static bool HasBlood(this Pawn myPawn, string type = null)
        {
            if (myPawn == null || myPawn.RaceProps == null) return false;
            if (!type.NullOrEmpty())
                return (myPawn.RaceProps.BloodDef != null && myPawn.RaceProps.BloodDef.defName == type);
            return myPawn.RaceProps.BloodDef != null;
        }
    }
}
