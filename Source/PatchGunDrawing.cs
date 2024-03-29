using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Rimlaser
{

    [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming", new Type[] { typeof(Thing), typeof(Vector3), typeof(float) }), StaticConstructorOnStartup]
    public static class PatchGunDrawing
    {
        static FieldInfo pawnField;

        static PatchGunDrawing()
        {
            pawnField = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static void Prefix(ref Thing eq, ref Vector3 drawLoc, ref float aimAngle, PawnRenderer __instance)
        {
            IDrawnWeaponWithRotation gun = eq as IDrawnWeaponWithRotation;
            if (gun == null) return;

            drawLoc -= new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
            aimAngle = (aimAngle + gun.RotationOffset ) % 360;
            drawLoc += new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
        }
    }
}
