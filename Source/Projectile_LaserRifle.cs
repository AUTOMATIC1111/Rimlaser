using System;

using UnityEngine;
using RimWorld;
using Verse;

namespace Rimlaser
{
    public class Bullet_LaserRifle : Bullet
    {
        int ticks = 0;

        bool setupComplete = false;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Matrix4x4 drawingMatrixCapA = default(Matrix4x4);
        public Matrix4x4 drawingMatrixCapB = default(Matrix4x4);
        Material materialBeam;
        Material materialBeamCap;
        new Bullet_LaserRifleDef def
        {
            get { return base.def as Bullet_LaserRifleDef; }
        }

        int destroyDelay = -1;


        public void SetTextures(GraphicData main, GraphicData caps)
        {
            if (main == null)
            {
                materialBeam = def.graphicData.Graphic.MatSingle;
                materialBeamCap = def.graphicDataCap == null ? null : def.graphicDataCap.Graphic.MatSingle;
                return;
            }

            materialBeam = main.Graphic.MatSingle;
            materialBeamCap = caps == null ? null : caps.Graphic.MatSingle;
        }

        void SetColor() {
            QualityCategory quality = QualityCategory.Awful;
            IBeamColorThing gun = null;
            var pawn = this.launcher as Pawn;
            if (pawn != null && pawn.equipment != null)
            {
                foreach (var thing in pawn.equipment.GetDirectlyHeldThings())
                {
                    if (gun == null) gun = thing as IBeamColorThing;
                    if (thing.def as LaserGunDef == null) continue;
                    if (thing.TryGetQuality(out quality)) break;

                }
            }

            var building = this.launcher as Building_LaserGun;
            if (building != null)
            {
                if (gun == null) gun = building as IBeamColorThing;
                building.TryGetQuality(out quality);
            }

            int colorIndex = -1;
            if (gun != null && gun.BeamColor != -1)
            {
                colorIndex = gun.BeamColor;
            }
            if (colorIndex == -1)
            {
                switch (quality)
                {
                    case QualityCategory.Awful: colorIndex = 0; break;
                    case QualityCategory.Poor: colorIndex = 1; break;
                    case QualityCategory.Normal: colorIndex = 2; break;
                    case QualityCategory.Good: colorIndex = 3; break;
                    case QualityCategory.Excellent: colorIndex = 4; break;
                    case QualityCategory.Masterwork: colorIndex = 5; break;
                    case QualityCategory.Legendary: colorIndex = 6; break;
                }
            }
            if (colorIndex == -1)
            {
                colorIndex = 0;
            }


            switch (colorIndex)
            {
                case 0: SetTextures(def.graphicAwful, def.graphicAwfulCap); break;
                case 1: SetTextures(def.graphicPoor, def.graphicPoorCap); break;
                case 2: SetTextures(def.graphicNormal, def.graphicNormalCap); break;
                case 3: SetTextures(def.graphicGood, def.graphicGoodCap); break;
                case 4: SetTextures(def.graphicExcellent, def.graphicExcellentCap); break;
                case 5: SetTextures(def.graphicMasterwork, def.graphicMasterworkCap); break;
                case 6: SetTextures(def.graphicLegendary, def.graphicLegendaryCap); break;
            }
        }

        public void SetupMatrices()
        {
            if (setupComplete) return;

            SetColor();

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

            setupComplete = true;
        }

        public override void Destroy(DestroyMode mode)
        {
            if (destroyDelay == -1)
            {
                destroyDelay = def.lifetime - ticks;
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

        public override void Draw()
        {
            SetupMatrices();

            float opacity = (float)Math.Sin(Math.Pow(1.0 - 1.0 * ticks / def.lifetime, def.impulse) * Math.PI);
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
