using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{
	/*
	 *  Fuel will be consumed to refill energy and to take off
	*/

	public class Manager_Fuel : Manager
	{

		public bool FuelDepleted => base.depleted;
		public float MaxFuel => base.maxAmount;
		public float FuelLevel => base.Level;
		public float Fuel { get => base.curAmount; set => base.curAmount = value; }
		public bool Fueling = false;
		public bool Draining = false;
		public Manager_Energy EnergyManager => comp.GetManager(ManagerType.Energy) as Manager_Energy;



		public Manager_Fuel(Pawn vessel) : base(vessel)
		{

		}
		public override void Setup(bool respawningAfterLoad)
		{
			base.Setup(respawningAfterLoad);
		}
		public override void ManagerTimedTick()
		{
			base.ManagerTimedTick();
		}
		public override void ManagerTick()
		{
            if (Fueling)
            {

            }
			if (EnergyManager.Charging)
            {

            }

			base.ManagerTimedTick();
		}
		public override void ExposeData()
		{
			base.ExposeData();
		}

		public float FillFuel(float amount)
        {
			return  base.Fill(amount);
        }
		public float ConsumeFuel(float amount)
        {
			return base.Consume(amount);
        }
 

	}
}