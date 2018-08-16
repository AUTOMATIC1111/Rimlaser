using System;

using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Rimlaser
{
    public class LaserBeam : Bullet
    {
        new LaserBeamDef def => base.def as LaserBeamDef;

        public override void Draw()
        {

        }

        void TriggerEffect(EffecterDef effect, Vector3 position)
        {
            TriggerEffect(effect, IntVec3.FromVector3(position));
        }

        void TriggerEffect(EffecterDef effect, IntVec3 dest)
        {
            if (effect == null) return;

            var targetInfo = new TargetInfo(dest, Map, false);

            Effecter effecter = effect.Spawn();
            effecter.Trigger(targetInfo, targetInfo);
            effecter.Cleanup();
        }

        protected override void Impact(Thing hitThing)
        {
            if (hitThing == null)
            {
                TriggerEffect(def.explosionEffect, destination);
            }
            else
            {
                if (hitThing is Pawn)
                {
                    Pawn hitPawn = hitThing as Pawn;
                    if (hitPawn.IsShielded())
                    {
                        weaponDamageMultiplier *= def.shieldDamageMultiplier;
                    }
                }

                TriggerEffect(def.explosionEffect, ExactPosition);
            }

            LaserBeamGraphic graphic = ThingMaker.MakeThing(def.beamGraphic, null) as LaserBeamGraphic;
            if (graphic != null)
            {
                graphic.Spawn(def, launcher, hitThing, origin, destination, intendedTarget.CenterVector3, equipmentDef);
                GenSpawn.Spawn(graphic, origin.ToIntVec3(), Map, WipeMode.Vanish);
            }

            base.Impact(hitThing);
        }
    }
}
