using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Rimlaser
{
    class Verb_ShootBeam : Verb_Shoot
    {
        /// <summary>
        /// The standard TryCastShot logic with added SetupMatrices after LaserBeam.Launch().
        /// </summary>
        protected override bool TryCastShot()
        {
            bool result;
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
            {
                result = false;
            }
            else
            {
                ThingDef projectileDef = this.Projectile;
                if (projectileDef == null)
                {
                    result = false;
                }
                else
                {
                    ShootLine shootLine;
                    bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
                    if (this.verbProps.stopBurstWithoutLos && !flag)
                    {
                        result = false;
                    }
                    else
                    {
                        if (base.EquipmentSource != null)
                        {
                            CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                            if (comp != null)
                            {
                                comp.Notify_ProjectileLaunched();
                            }
                        }
                        Thing launcher = this.caster;
                        Thing equipment = base.EquipmentSource;
                        CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
                        if (compMannable != null && compMannable.ManningPawn != null)
                        {
                            launcher = compMannable.ManningPawn;
                            equipment = this.caster;
                        }
                        Vector3 drawPos = this.caster.DrawPos;
                        LaserBeam spawnedBeam = (LaserBeam)GenSpawn.Spawn(projectileDef, shootLine.Source, this.caster.Map, WipeMode.Vanish);
                        if (this.verbProps.forcedMissRadius > 0.5f)
                        {
                            float num = VerbUtility.CalculateAdjustedForcedMiss(this.verbProps.forcedMissRadius, this.currentTarget.Cell - this.caster.Position);
                            if (num > 0.5f)
                            {
                                int max = GenRadial.NumCellsInRadius(num);
                                int num2 = Rand.Range(0, max);
                                if (num2 > 0)
                                {
                                    IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num2];
                                    ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                                    if (Rand.Chance(0.5f))
                                    {
                                        projectileHitFlags = ProjectileHitFlags.All;
                                    }
                                    if (!this.canHitNonTargetPawnsNow)
                                    {
                                        projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                                    }
                                    spawnedBeam.Launch(launcher, drawPos, c, this.currentTarget, projectileHitFlags, equipment, null);
                                    spawnedBeam.SetupMatrices();
                                    return true;
                                }
                            }
                        }
                        ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
                        Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                        ThingDef targetCoverDef = (randomCoverToMissInto == null) ? null : randomCoverToMissInto.def;
                        if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
                        {
                            shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                            ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                            if (Rand.Chance(0.5f) && this.canHitNonTargetPawnsNow)
                            {
                                projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                            }
                            spawnedBeam.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags2, equipment, targetCoverDef);
                            spawnedBeam.SetupMatrices();
                            result = true;
                        }
                        else if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
                        {
                            ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                            if (this.canHitNonTargetPawnsNow)
                            {
                                projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                            }
                            spawnedBeam.Launch(launcher, drawPos, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, equipment, targetCoverDef);
                            spawnedBeam.SetupMatrices();
                            result = true;
                        }
                        else
                        {
                            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
                            if (this.canHitNonTargetPawnsNow)
                            {
                                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
                            }
                            if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
                            {
                                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
                            }
                            if (this.currentTarget.Thing != null)
                            {
                                spawnedBeam.Launch(launcher, drawPos, this.currentTarget, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
                                spawnedBeam.SetupMatrices();
                            }
                            else
                            {
                                spawnedBeam.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
                                spawnedBeam.SetupMatrices();
                            }
                            result = true;
                        }
                    }
                }
            }
            return result;
        }
    }
}
