using RimWorld;
using Verse;

namespace Rimlaser
{
    public class Building_LaserGun : Building_TurretGun
    {
        CompPowerTraderExtended power;
        public bool isCharged = false;
        public int previousBurstCooldownTicksLeft = 0;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref isCharged, "isCharged", false, false);
            Scribe_Values.Look<int>(ref previousBurstCooldownTicksLeft, "previousBurstCooldownTicksLeft", 0, false);
            
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            power = GetComp<CompPowerTraderExtended>();
        }
        
        public override void Tick()
        {
            if (burstCooldownTicksLeft > previousBurstCooldownTicksLeft) {
                isCharged = false;
            }
            previousBurstCooldownTicksLeft = burstCooldownTicksLeft;


            if (!isCharged)
            {
                float powerRequired = (def as Building_LaserGunDef).beamPowerConsumption;

                if (power.AvailablePower() >= powerRequired)
                {
                    power.Drain(powerRequired);
                    isCharged = true;
                }
            }

            if (isCharged || burstCooldownTicksLeft>1)
            {
                base.Tick();
            }
        }
    }
}
