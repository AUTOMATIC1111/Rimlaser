using RimWorld;
using Verse;

namespace Rimlaser
{
    public class Building_LaserGun : Building_TurretGun
    {
        CompPowerTrader power;
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

            power = GetComp<CompPowerTrader>();
        }
        
        public override void Tick()
        {
            if (burstCooldownTicksLeft > previousBurstCooldownTicksLeft) {
                isCharged = false;
            }
            previousBurstCooldownTicksLeft = burstCooldownTicksLeft;


            if (!isCharged)
            {
                if (Drain((def as Building_LaserGunDef).beamPowerConsumption))
                {
                    isCharged = true;
                }
            }

            if (isCharged || burstCooldownTicksLeft>1)
            {
                base.Tick();
            }
        }

        public float AvailablePower()
        {
            if (power.PowerNet == null) return 0;

            float availablePower = 0;
            foreach (var battery in power.PowerNet.batteryComps)
            {
                availablePower += battery.StoredEnergy;
            }
            return availablePower;
        }
        public bool Drain(float amount)
        {
            if (amount <= 0) return true;
            if (AvailablePower() < amount) return false;

            foreach (var battery in power.PowerNet.batteryComps)
            {
                var drain = battery.StoredEnergy > amount ? amount : battery.StoredEnergy;
                battery.DrawPower(drain);
                amount -= drain;

                if (amount <= 0) break;
            }

            return true;
        }

        public override string GetInspectString()
        {
            string result = base.GetInspectString();

            if (!isCharged)
            {
                result += "\n";
                result += "LaserTurretNotCharged".Translate();
            }

            return result;
        }

    }
}
