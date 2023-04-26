using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{
	public class Manager_Health : Manager
	{


		public Manager_Health(Pawn vessel) : base(vessel)
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
		public void TreatAll(bool tend = true, bool heal = true)
		{

			foreach (Pawn member in Crew)
			{

				member.health.hediffSet.HasTendableHediff();
				member.health.hediffSet.hediffs.ForEach(delegate (Hediff hedif)
				{

					if (hedif.TendableNow() && tend) hedif.Tended(lowerTendLimit, upperTendLimit);
					//if (!hedif.IsPermanent() && heal) hedif.Severity -= healFactor;

				});


			}

		}
	}
}
