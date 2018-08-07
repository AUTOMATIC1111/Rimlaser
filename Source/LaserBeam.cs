using System;

using UnityEngine;
using RimWorld;
using Verse;

namespace Rimlaser
{
    public class LaserBeam : Bullet
    {
        int ticks = 0;

        bool setupComplete = false;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Matrix4x4 drawingMatrixCapA = default(Matrix4x4);
        public Matrix4x4 drawingMatrixCapB = default(Matrix4x4);
        Material materialBeam;
        Material materialBeamCap;
        Vector3 calculatedOrigin;
        Vector3 calculatedDestination;

        new LaserBeamDef def
        {
            get { return base.def as LaserBeamDef; }
        }

        int destroyDelay = -1;


        public void SetTextures(int index)
        {
            def.GetMaterials(index, out materialBeam, out materialBeamCap);

            if (materialBeam == null)
            {
                materialBeam = def.graphicData.Graphic.MatSingle;
                materialBeamCap = null;
            }
        }

        void SetColor(out IDrawnWeaponWithRotation drawnWeaponWithRotation) {
            IBeamColorThing gun = null;
            drawnWeaponWithRotation = null;

            Pawn pawn = this.launcher as Pawn;
            if (pawn != null && pawn.equipment != null)
            {
                if (gun == null) gun = pawn.equipment.Primary as IBeamColorThing;
                if (drawnWeaponWithRotation == null) drawnWeaponWithRotation = pawn.equipment.Primary as IDrawnWeaponWithRotation;
            }

            if (gun == null) gun = launcher as IBeamColorThing;
            if (drawnWeaponWithRotation == null) drawnWeaponWithRotation = launcher as IDrawnWeaponWithRotation;

            int colorIndex = -1;
            if (gun != null && gun.BeamColor != -1)
            {
                colorIndex = gun.BeamColor;
            }
            if (colorIndex == -1)
            {
                colorIndex = 2;
            }

            SetTextures(colorIndex);
        }

        public void SetupMatrices()
        {
            if (setupComplete) return;

            IDrawnWeaponWithRotation weapon;
            SetColor(out weapon);

            var capSize = def.capSize * def.beamWidth;
            var capOverlap = def.capOverlap * def.beamWidth;
            if (materialBeamCap == null)
            {
                capSize = 0;
                capOverlap = 0;
            }

            float altitude = def.Altitude;
            Vector3 dest = destination; dest.y = altitude;
            Vector3 orig = origin; orig.y = altitude;
            Quaternion rotation = Quaternion.LookRotation(dest - orig);

            var defWeapon = equipmentDef as LaserGunDef;
            Vector3 dir = (dest - orig).normalized;
            Vector3 a = orig + dir * (defWeapon == null ? 0.9f : defWeapon.barrelLength);
            Vector3 b = dest;
            float length = (b - a).magnitude;

            Vector3 drawingScale = new Vector3(def.beamWidth, 1f, length - capSize * 2 + capOverlap * 2);

            Vector3 drawingPosition = (a + b) / 2;
            drawingMatrix.SetTRS(drawingPosition, rotation, drawingScale);

            if (materialBeamCap != null)
            {
                Vector3 drawingScaleCap = new Vector3(def.beamWidth, 1f, def.beamWidth);

                Vector3 drawingPositionCapB = b - dir * capSize / 2;
                drawingMatrixCapB.SetTRS(drawingPositionCapB, rotation, drawingScaleCap);

                Vector3 drawingPositionCapA = a + dir * capSize / 2;
                drawingMatrixCapA.SetTRS(drawingPositionCapA, Quaternion.LookRotation(orig - dest), drawingScaleCap);
            }

            if (weapon != null)
            {
                float angle = (destination - origin).AngleFlat() - (intendedTarget.CenterVector3 - origin).AngleFlat();
                weapon.RotationOffset = (angle + 180) % 360 - 180;
            }

            calculatedOrigin = a;
            calculatedDestination = b;

            setupComplete = true;
        }

        void SpawnDecorations()
        {
            SetupMatrices();

            if (def.decorations == null) return;

            foreach (var decoration in def.decorations) {
                float spacing = decoration.spacing * def.beamWidth;
                float initalOffset = decoration.initialOffset * def.beamWidth;

                Vector3 dir = (calculatedDestination - calculatedOrigin).normalized;
                float angle = (calculatedDestination - calculatedOrigin).AngleFlat();
                Vector3 offset = dir * spacing;
                Vector3 position = calculatedOrigin + offset * 0.5f + dir * initalOffset;
                float length = (calculatedDestination - calculatedOrigin).magnitude - spacing;

                int i = 0;
                while (length > 0)
                {
                    MoteLaserDectoration moteThrown = (MoteLaserDectoration)ThingMaker.MakeThing(decoration.mote, null);
                    if (moteThrown == null) break;

                    moteThrown.beam = this;
                    moteThrown.airTimeLeft = def.lifetime;
                    moteThrown.Scale = def.beamWidth;
                    moteThrown.exactRotation = angle;
                    moteThrown.exactPosition = position;
                    moteThrown.SetVelocity(angle, decoration.speed);
                    moteThrown.baseSpeed = decoration.speed;
                    moteThrown.speedJitter = decoration.speedJitter;
                    moteThrown.speedJitterOffset = decoration.speedJitterOffset * i;
                    GenSpawn.Spawn(moteThrown, origin.ToIntVec3(), Map, WipeMode.Vanish);

                    position += offset;
                    length -= spacing;
                    i++;
                }
            }
        }

        public override void Destroy(DestroyMode mode)
        {
            if (destroyDelay == -1)
            {
                destroyDelay = def.lifetime - ticks;
                SpawnDecorations();
            }

            if (destroyDelay <= 0)
            {
                destroyDelay = 0;
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            ticks++;

            if (destroyDelay > 0)
            {
                destroyDelay--;

                if (destroyDelay == 0)
                {
                    Destroy(DestroyMode.Vanish);
                }
            }
            else
            {
                base.Tick();
            }

        }

        public float Opacity => (float)Math.Sin(Math.Pow(1.0 - 1.0 * ticks / def.lifetime, def.impulse) * Math.PI);

        public override void Draw()
        {
            SetupMatrices();

            float opacity = Opacity;
            Graphics.DrawMesh(MeshPool.plane10, drawingMatrix, FadedMaterialPool.FadedVersionOf(materialBeam, opacity), 0);

            if (materialBeamCap != null)
            {
                Graphics.DrawMesh(MeshPool.plane10, drawingMatrixCapA, FadedMaterialPool.FadedVersionOf(materialBeamCap, opacity), 0);
                Graphics.DrawMesh(MeshPool.plane10, drawingMatrixCapB, FadedMaterialPool.FadedVersionOf(materialBeamCap, opacity), 0);
            }
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
            if (destroyDelay != -1) return;
            Destroy(DestroyMode.Vanish);

            if (hitThing == null)
            {
                TriggerEffect(def.explosionEffect, destination);
            }
            else
            {
                if (hitThing is Pawn && (hitThing as Pawn).RaceProps.meatDef != null)
                {
                    TriggerEffect(def.hitLivingEffect, hitThing.Position);
                }
                TriggerEffect(def.explosionEffect, ExactPosition);
            }

            base.Impact(hitThing);
        }
    }
}
