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

        int destroyDelay = -1;


        internal Bullet_LaserRifleDef Def() {
            return def as Bullet_LaserRifleDef;
        }

        public void SetupMatrices()
        {
            if (setupComplete) return;

            QualityCategory quality = QualityCategory.Awful;
            var pawn = this.launcher as Pawn;
            if (pawn != null && pawn.equipment != null)
            {
                foreach (var thing in pawn.equipment.GetDirectlyHeldThings()) {
                    Log.Warning(""+thing);
                    if (thing.def as LaserGunDef == null) continue;
                    if (! thing.TryGetQuality(out quality)) continue;
                }
            }

            var building = this.launcher as Building_LaserGun;
            if (building != null)
            {
                building.TryGetQuality(out quality);
            }

            var def = Def();
            var defWeapon = equipmentDef as LaserGunDef;

            switch (quality) {
                case QualityCategory.Awful:
                    materialBeam = def.graphicAwful.Graphic.MatSingle;
                    materialBeamCap = def.graphicAwfulCap.Graphic.MatSingle;
                    break;
                case QualityCategory.Poor:
                    materialBeam = def.graphicPoor.Graphic.MatSingle;
                    materialBeamCap = def.graphicPoorCap.Graphic.MatSingle;
                    break;
                case QualityCategory.Normal:
                    materialBeam = def.graphicNormal.Graphic.MatSingle;
                    materialBeamCap = def.graphicNormalCap.Graphic.MatSingle;
                    break;
                case QualityCategory.Good:
                    materialBeam = def.graphicGood.Graphic.MatSingle;
                    materialBeamCap = def.graphicGoodCap.Graphic.MatSingle;
                    break;
                case QualityCategory.Excellent:
                    materialBeam = def.graphicExcellent.Graphic.MatSingle;
                    materialBeamCap = def.graphicExcellentCap.Graphic.MatSingle;
                    break;
                case QualityCategory.Masterwork:
                    materialBeam = def.graphicMasterwork.Graphic.MatSingle;
                    materialBeamCap = def.graphicMasterworkCap.Graphic.MatSingle;
                    break;
                case QualityCategory.Legendary:
                    materialBeam = def.graphicLegendary.Graphic.MatSingle;
                    materialBeamCap = def.graphicLegendaryCap.Graphic.MatSingle;
                    break;
            }

            var capSize = def.capSize * def.beamWidth;

            Vector3 dir = destination - origin;
            Vector3 a = origin + dir.normalized * (defWeapon == null ? 0.9f : defWeapon.barrelLength);
            Vector3 b = destination;
            float altitude = def.Altitude;
            float length = (b - a).magnitude;

            Vector3 drawingScale = new Vector3(def.beamWidth, 1f, length - capSize * 2 + def.capOverlap * def.beamWidth * 2);
            Vector3 drawingScaleCap = new Vector3(def.beamWidth, 1f, def.beamWidth);

            Vector3 drawingPosition = (a + b) / 2;
            drawingPosition.y = altitude;
            drawingMatrix.SetTRS(drawingPosition, ExactRotation, drawingScale);

            Vector3 drawingPositionCapB = b - dir.normalized * capSize / 2;
            drawingPositionCapB.y = altitude;
            drawingMatrixCapB.SetTRS(drawingPositionCapB, ExactRotation, drawingScaleCap);

            Vector3 angle = ExactRotation.eulerAngles; angle.y += 180;
            Vector3 drawingPositionCapA = a + dir.normalized * capSize / 2;
            drawingPositionCapA.y = altitude;
            drawingMatrixCapA.SetTRS(drawingPositionCapA, Quaternion.Euler(angle), drawingScaleCap);

            setupComplete = true;
        }
        
        public override void Destroy(DestroyMode mode)
        {
            if (destroyDelay == -1)
            {
                destroyDelay = 30;
            }
            else if (destroyDelay == 0)
            {
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            ticks++;

            base.Tick();

            if (destroyDelay > 0) {
                destroyDelay--;
            }

            if (destroyDelay == 0)
            {
                Destroy(DestroyMode.Vanish);
            }
        }

        /// <summary>
        /// Draws the laser ray.
        /// </summary>
        public override void Draw()
        {
            SetupMatrices();

            var def = Def();

            float opacity = (float) Math.Sin(Math.Pow(1.0 - 1.0 * ticks / def.lifetime, def.impulse) * Math.PI);
            Graphics.DrawMesh(MeshPool.plane10, drawingMatrix, FadedMaterialPool.FadedVersionOf(materialBeam, opacity), 0);
            Graphics.DrawMesh(MeshPool.plane10, drawingMatrixCapA, FadedMaterialPool.FadedVersionOf(materialBeamCap, opacity), 0);
            Graphics.DrawMesh(MeshPool.plane10, drawingMatrixCapB, FadedMaterialPool.FadedVersionOf(materialBeamCap, opacity), 0);
        }
    }
}
