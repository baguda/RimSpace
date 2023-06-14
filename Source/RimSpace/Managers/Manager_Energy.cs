using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{

    public class Manager_Energy : Manager
    {
        public float EnergyPerFuel => 1f/FuelFillCount*100f;

        public float FuelFillCount = 10; 
        public bool EnergyDepleted => base.depleted;
        public float MaxEnergy => energyNeed.MaxLevel;
        public float EnergyLevel => energyNeed.CurLevelPercentage;
        public float Energy { get => energyNeed.CurLevel; set => energyNeed.CurLevel = value; }


        public Manager_Shields Shields => comp.shields;
        public Need_MechEnergy energyNeed => this.comp.pawn.needs.energy;

        public List<Thing> fuel => comp.ContentsList.FindAll(s => s.def.Equals(ThingDefOf.Chemfuel));
        public bool hasFuel => fuel.Any();

        public bool Charging = false;

        public bool Discharging = false;

        public float useFuel(int count = 1)
        {
            if (hasFuel)
            {
                Thing med = this.fuel.First();
                med.stackCount -= count;
                if (med.stackCount <= 0) med.Destroy(DestroyMode.Vanish);
                return this.EnergyPerFuel * count;
            }
            return 0f;
        }



        public Manager_Energy(Pawn vessel) : base(vessel)
        {

        }
        public override void Setup(bool respawningAfterLoad)
        {
            base.Setup(respawningAfterLoad);
        }
        public override void ManagerTimedTick()
        {
            if (comp.RefillPower && hasFuel ) this.Fill(useFuel());
            base.ManagerTimedTick();
        }
        public override void ManagerTick()
        {
            base.ManagerTimedTick();
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }
        public override float Fill(float amount)
        {
            float result = Energy + amount;
            if (result >= maxAmount)
            {
                Energy = MaxEnergy;
                return result - MaxEnergy;
            }
            Energy = result;
            return 0f;
        }

    }
}
