using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Rimlaser
{
    [HarmonyPatch(typeof(TurretTop), "DrawTurret", new Type[] { }), StaticConstructorOnStartup]
    class PatchTuretTopDrawTurret
    {
        static FieldInfo parentTurretField;
        static FieldInfo curRotationIntField;

        static PatchTuretTopDrawTurret()
        {
            parentTurretField = typeof(TurretTop).GetField("parentTurret", BindingFlags.NonPublic | BindingFlags.Instance);
            curRotationIntField = typeof(TurretTop).GetField("curRotationInt", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static bool Prefix(TurretTop __instance)
        {
            Building_LaserGun turret = parentTurretField.GetValue(__instance) as Building_LaserGun;
            if (turret == null) return true;

            float rotation = (float)curRotationIntField.GetValue(__instance);
            if (turret.TargetCurrentlyAimingAt.HasThing)
            {
                rotation = (turret.TargetCurrentlyAimingAt.CenterVector3 - turret.TrueCenter()).AngleFlat();
            }

            IDrawnWeaponWithRotation gunRotation = turret.gun as IDrawnWeaponWithRotation;
            if (gunRotation != null) rotation += gunRotation.RotationOffset;

            Material material = turret.def.building.turretTopMat;
            SpinningLaserGunTurret spinningGun = turret.gun as SpinningLaserGunTurret;
            if (spinningGun != null)
            {
                spinningGun.turret = turret;
                material = spinningGun.Graphic.MatSingle;
            }
            
            Vector3 b = new Vector3(turret.def.building.turretTopOffset.x, 0f, turret.def.building.turretTopOffset.y);
            float turretTopDrawSize = turret.def.building.turretTopDrawSize;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(turret.DrawPos + Altitudes.AltIncVect + b, rotation.ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
            Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);

            return false;
        }
    }
}
