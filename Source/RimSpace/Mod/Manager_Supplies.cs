using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{
	/*
	 *  Supplies will be consumed to refill energy and to take off
	*/

	public class Manager_Supplies : Manager
	{

		public bool SuppliesDepleted => base.depleted;
		public float MaxSupplies => base.maxAmount;
		public float SuppliesLevel => base.Level;
		public float Supplies { get => base.curAmount; set => base.curAmount = value; }
		public bool Supplying = false;
		public bool Draining = false;
		public Manager_LifeSupport LifeSupportManager => comp.GetManager(ManagerType.LifeSupport) as Manager_LifeSupport;



		public Manager_Supplies(Pawn vessel) : base(vessel)
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
			if (Supplying)
			{

			}
			if (LifeSupportManager.Charging)
			{

			}

			base.ManagerTimedTick();
		}
		public override void ExposeData()
		{
			base.ExposeData();
		}

		public float FillSupplies(float amount)
		{
			return base.Fill(amount);
		}
		public float ConsumeSupplies(float amount)
		{
			return base.Consume(amount);
		}


	}
}