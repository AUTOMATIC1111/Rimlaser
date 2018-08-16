using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Rimlaser
{
    class LaserBeamGraphic :Thing
    {
        new LaserBeamDef def;

        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        Material materialBeam;
        Vector3 calculatedOrigin;
        Vector3 calculatedDestination;
        Mesh mesh;
        int ticks;

        public float Opacity => (float)Math.Sin(Math.Pow(1.0 - 1.0 * ticks / def.lifetime, def.impulse) * Math.PI);

        public override void Tick()
        {
            if (def==null || ticks++ > def.lifetime)
            {
                Destroy(DestroyMode.Vanish);
            }
        }

        public override void Draw()
        {
            if (mesh == null) return;

            float opacity = Opacity;
            Graphics.DrawMesh(mesh, drawingMatrix, FadedMaterialPool.FadedVersionOf(materialBeam, opacity), 0);
        }

        public void SetTextures(int index)
        {
            materialBeam = def.GetBeamMaterial(index);

            if (materialBeam == null)
                materialBeam = def.graphicData.Graphic.MatSingle;
        }

        void SetColor(Thing launcher, out IDrawnWeaponWithRotation drawnWeaponWithRotation)
        {
            IBeamColorThing gun = null;
            drawnWeaponWithRotation = null;

            Pawn pawn = launcher as Pawn;
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

        public void Spawn(LaserBeamDef beamDef, Thing launcher, Thing hitThing, Vector3 origin, Vector3 destination, Vector3 intendedTarget, Def equipmentDef)
        {
            def = beamDef;

            IDrawnWeaponWithRotation weapon;
            SetColor(launcher, out weapon);

            float beamWidth = def.beamWidth;

            float altitude = def.Altitude;
            Vector3 dest = destination; dest.y = altitude;
            Vector3 orig = origin; orig.y = altitude;
            Quaternion rotation = Quaternion.LookRotation(dest - orig);

            var defWeapon = equipmentDef as LaserGunDef;
            Vector3 dir = (dest - orig).normalized;
            Vector3 a = orig + dir * (defWeapon == null ? 0.9f : defWeapon.barrelLength);
            Vector3 b = dest - dir * ((hitThing.IsShielded() && def.IsWeakToShields) ? 0.45f : 0.01f);
            float length = (b - a).magnitude;

            Vector3 drawingScale = new Vector3(beamWidth, 1f, length);

            Vector3 drawingPosition = (a + b) / 2;
            drawingMatrix.SetTRS(drawingPosition, rotation, drawingScale);

            if (weapon != null)
            {
                float angle = (destination - origin).AngleFlat() - (intendedTarget - origin).AngleFlat();
                weapon.RotationOffset = (angle + 180) % 360 - 180;
            }

            calculatedOrigin = a;
            calculatedDestination = b;

            float textureRatio = 1.0f * materialBeam.mainTexture.width / materialBeam.mainTexture.height;
            float seamTexture = def.seam < 0 ? textureRatio : def.seam;
            float capLength = beamWidth / textureRatio / 2f * seamTexture;
            float seamGeometry = length <= capLength * 2 ? 0.5f : capLength * 2 / length;

            mesh = MeshMakerLaser.Mesh(seamTexture, seamGeometry);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (def==null || def.decorations == null) return;

            foreach (var decoration in def.decorations)
            {
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
                    GenSpawn.Spawn(moteThrown, calculatedOrigin.ToIntVec3(), map, WipeMode.Vanish);

                    position += offset;
                    length -= spacing;
                    i++;
                }
            }
        }
    }
}
