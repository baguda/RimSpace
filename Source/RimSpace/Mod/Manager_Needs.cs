using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{

	public class Manager_Needs : Manager
	{


		public Manager_Needs(Pawn vessel) : base(vessel)
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
