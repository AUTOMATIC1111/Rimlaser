using RimWorld;
using Verse;

namespace Rimlaser
{
    class CompPowerTraderExtended : CompPowerTrader
    {
   
        public override string CompInspectStringExtra()
        {
            string result = base.CompInspectStringExtra();
            
            var laserGun = parent as Building_LaserGun;
            if (!laserGun.isCharged)
            {
                result += "\n";
                result += "LaserTurretNotCharged".Translate();
            }

            return result;
        }

        public float AvailablePower()
        {
            float availablePower = 0;
            foreach (var battery in PowerNet.batteryComps)
            {
                availablePower += battery.StoredEnergy;
            }
            return availablePower;
        }
        public void Drain(float amount)
        {
            foreach (var battery in PowerNet.batteryComps)
            {
                var drain = battery.StoredEnergy > amount ? amount : battery.StoredEnergy;
                battery.DrawPower(drain);
                amount -= drain;

                if (amount <= 0) break;
            }
        }
    }

}
