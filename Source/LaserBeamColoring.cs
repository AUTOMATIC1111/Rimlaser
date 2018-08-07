using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Rimlaser
{
    public class LaserColor
    {
        static LaserColor[] colors = {
            new LaserColor { index = 0, name = "RimlaserBeamBrown" },
            new LaserColor { index = 1, name = "RimlaserBeamOrange" },
            new LaserColor { index = 2, name = "RimlaserBeamRed" },
            new LaserColor { index = 3, name = "RimlaserBeamPink" },
            new LaserColor { index = 4, name = "RimlaserBeamBlue" },
            new LaserColor { index = 5, name = "RimlaserBeamTeal" },
            new LaserColor { index = 6, name = "RimlaserBeamRedBlack", allowed = false },
        };

        public int index;
        public string name;
        public bool allowed = true;


        private static Thing FindClosestPrism(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.RimlaserPrism), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
        }

        public static IEnumerable<FloatMenuOption> GetChangeBeamColorFloatMenuOptions(Thing gun, Pawn pawn)
        {
            Thing prism = FindClosestPrism(pawn);
            if (prism == null) yield break;

            foreach (LaserColor color in colors)
            {
                if (!color.allowed) continue;

                string caption = string.Format("RimlaserChangeBeamColor".Translate(), color.name.Translate());

                Action action = delegate ()
                {
                    Job job = new Job(JobDefOf.ChangeLaserColor, gun, prism);
                    job.count = 1;

                    // we are storing color in an unrelated field because I have no idea how to approach this properly
                    job.maxNumMeleeAttacks = color.index;

                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };

                yield return new FloatMenuOption(caption, action);
            }

            yield break;
        }
    };
}
