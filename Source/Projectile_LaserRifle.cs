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
        new Bullet_LaserRifleDef def {
            get { return base.def as Bullet_LaserRifleDef; }
        }

        int destroyDelay = -1;


        public void SetTextures(GraphicData main, GraphicData caps) {
            if (main == null) {
                materialBeam = def.graphicData.Graphic.MatSingle;
                materialBeamCap = def.graphicDataCap == null ? null : def.graphicDataCap.Graphic.MatSingle;
                return;
            }

            materialBeam = main.Graphic.MatSingle;
            materialBeamCap = caps == null ? null : caps.Graphic.MatSingle;
        }

        public void SetupMatrices()
        {
            if (setupComplete) return;

            QualityCategory quality = QualityCategory.Awful;
            var pawn = this.launcher as Pawn;
            if (pawn != null && pawn.equipment != null)
            {
                foreach (var thing in pawn.equipment.GetDirectlyHeldThings()) {
                    if (thing.def as LaserGunDef == null) continue;
                    if (thing.TryGetQuality(out quality)) break;
                }
            }

            var building = this.launcher as Building_LaserGun;
            if (building != null)
            {
                building.TryGetQuality(out quality);
            }
            
            var defWeapon = equipmentDef as LaserGunDef;

            switch (quality)
            {
                case QualityCategory.Awful: SetTextures(def.graphicAwful, def.graphicAwfulCap); break;
                case QualityCategory.Poor: SetTextures(def.graphicPoor, def.graphicPoorCap); break;
                case QualityCategory.Normal: SetTextures(def.graphicNormal, def.graphicNormalCap); break;
                case QualityCategory.Good: SetTextures(def.graphicGood, def.graphicGoodCap); break;
                case QualityCategory.Excellent: SetTextures(def.graphicExcellent, def.graphicExcellentCap); break;
                case QualityCategory.Masterwork: SetTextures(def.graphicMasterwork, def.graphicMasterworkCap); break;
                case QualityCategory.Legendary: SetTextures(def.graphicLegendary, def.graphicLegendaryCap); break;
            }
            Log.Warning("materialBeam: " + materialBeam + ", materialBeamCap: " + materialBeamCap);

            var capSize = def.capSize * def.beamWidth;
            var capOverlap = def.capOverlap * def.beamWidth;
            if (materialBeamCap == null)
            {
                capSize = 0;
                capOverlap = 0;
            }

            Vector3 rotationAngle = ExactRotation.eulerAngles;
            rotationAngle.x = 0;
            rotationAngle.z = 0;
            Quaternion rotation = Quaternion.Euler(rotationAngle);

            float altitude = def.Altitude;
            Vector3 dest = destination; dest.y = altitude;
            Vector3 orig = origin; orig.y = altitude;

            Vector3 dir = (dest - orig).normalized;
            Vector3 a = origin + dir * (defWeapon == null ? 0.9f : defWeapon.barrelLength);
            Vector3 b = dest;
            float length = (b - a).magnitude;

            Vector3 drawingScale = new Vector3(def.beamWidth, 1f, length - capSize * 2 + capOverlap * 2);
 
            Vector3 drawingPosition = (a + b) / 2;
            drawingPosition.y = altitude;
            drawingMatrix.SetTRS(drawingPosition, rotation, drawingScale);

            if (materialBeamCap != null)
            {
                Vector3 drawingScaleCap = new Vector3(def.beamWidth, 1f, def.beamWidth);

                Vector3 drawingPositionCapB = b - dir * capSize / 2;
                drawingPositionCapB.y = altitude;
                drawingMatrixCapB.SetTRS(drawingPositionCapB, rotation, drawingScaleCap);

                rotationAngle.y += 180;
                Vector3 drawingPositionCapA = a + dir * capSize / 2;
                drawingPositionCapA.y = altitude;
                drawingMatrixCapA.SetTRS(drawingPositionCapA, Quaternion.Euler(rotationAngle), drawingScaleCap);
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

            float opacity = (float) Math.Sin(Math.Pow(1.0 - 1.0 * ticks / def.lifetime, def.impulse) * Math.PI);
            Graphics.DrawMesh(MeshPool.plane10, drawingMatrix, FadedMaterialPool.FadedVersionOf(materialBeam, opacity), 0);

            if (materialBeamCap != null)
            {
                Graphics.DrawMesh(MeshPool.plane10, drawingMatrixCapA, FadedMaterialPool.FadedVersionOf(materialBeamCap, opacity), 0);
                Graphics.DrawMesh(MeshPool.plane10, drawingMatrixCapB, FadedMaterialPool.FadedVersionOf(materialBeamCap, opacity), 0);
            }
        }
    }
}
