using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{

	public class Manager_Energy : Manager
	{
		public bool EnergyDepleted => base.depleted;
		public float MaxEnergy => base.maxAmount;
		public float EnergyLevel => base.Level;
		public float Energy { get => base.curAmount; set => base.curAmount = value; }
		
		public Manager_Shields Shields => comp.GetManager(ManagerType.Shields) as Manager_Shields;


		public bool Charging = false;

		public bool Discharging = false;

		public Manager_Energy(Pawn vessel) : base(vessel)
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
			base.ManagerTimedTick();
		}
		public override void ExposeData()
		{
			base.ExposeData();
		}

	}
}
