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

        void SpawnBeam(Vector3 a, Vector3 b)
        {
            LaserBeamGraphic graphic = ThingMaker.MakeThing(def.beamGraphic, null) as LaserBeamGraphic;
            if (graphic == null) return;

            graphic.projDef = def;
            graphic.Setup(launcher, a, b);
            GenSpawn.Spawn(graphic, origin.ToIntVec3(), Map, WipeMode.Vanish);
        }

        void SpawnBeamReflections(Vector3 a, Vector3 b, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = (b - a).normalized;
                Vector3 c = b - dir.RotatedBy(Rand.Range(-22.5f,22.5f)) * Rand.Range(1f,4f);

                SpawnBeam(b, c);
            }
        }

        protected override void Impact(Thing hitThing)
        {
            bool shielded = hitThing.IsShielded() && def.IsWeakToShields;

            LaserGunDef defWeapon = equipmentDef as LaserGunDef;
            Vector3 dir = (destination - origin).normalized;
            dir.y = 0;

            Vector3 a = origin + dir * (defWeapon == null ? 0.9f : defWeapon.barrelLength);
            Vector3 b;
            if (hitThing == null)
            {
                b = destination;
            }
            else if (shielded)
            {
                b = hitThing.TrueCenter() - dir.RotatedBy(Rand.Range(-22.5f, 22.5f)) * 0.8f;
            }
            else if ((destination - hitThing.TrueCenter()).magnitude < 1)
            {
                b = destination;
            }
            else
            {
                b = hitThing.TrueCenter();
                b.x += Rand.Range(-0.5f, 0.5f);
                b.z += Rand.Range(-0.5f, 0.5f);
            }

            a.y = b.y = def.Altitude;

            SpawnBeam(a, b);


            Pawn pawn = launcher as Pawn;
            IDrawnWeaponWithRotation weapon = null;
            if (pawn != null && pawn.equipment != null) weapon = pawn.equipment.Primary as IDrawnWeaponWithRotation;
            if (weapon == null) {
                Building_LaserGun turret = launcher as Building_LaserGun;
                if (turret != null) {
                    weapon = turret.gun as IDrawnWeaponWithRotation;
                }
            }
            if (weapon != null)
            {
                float angle = (b - a).AngleFlat() - (intendedTarget.CenterVector3 - a).AngleFlat();
                weapon.RotationOffset = (angle + 180) % 360 - 180;
            }

            if (hitThing == null)
            {
                TriggerEffect(def.explosionEffect, destination);
            }
            else
            {
                if (hitThing is Pawn)
                {
                    Pawn hitPawn = hitThing as Pawn;
                    if (shielded)
                    {
                        weaponDamageMultiplier *= def.shieldDamageMultiplier;

                        SpawnBeamReflections(a, b, 5);
                    }
                }

                TriggerEffect(def.explosionEffect, ExactPosition);
            }

            base.Impact(hitThing);
        }

    }
}
