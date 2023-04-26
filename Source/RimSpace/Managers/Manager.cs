using System;
using System.Collections.Generic;
using Verse;

namespace RimSpace
{
	public class Manager : IExposable
	{
		public Pawn Vessel;
		public Map map => Vessel.Map;
		public CompSpaceship comp => Vessel.GetComp<CompSpaceship>();
		public List<Pawn> Crew => comp.CrewList;


		private int NextTick = 0;
		public int TickPeriod = 300;
		public ManagerType MgrType;

		private float AmountInt = 0f;
		public float maxAmount = 100;
		public bool depleted => this.AmountInt == 0f;
		public float curAmount { get => this.AmountInt; set => this.AmountInt = value; }
		public float Level => curAmount / maxAmount;


		

		public Manager(Pawn vessel)
		{
			if (vessel.def.HasComp(typeof(CompSpaceship))) this.Vessel = vessel;
		}
		public virtual void Setup(bool respawningAfterLoad)
		{
		}
		public virtual void ManagerTimedTick()
		{
		}
		public virtual void ManagerTick()
		{
			if (TimedTick()) ManagerTimedTick();
		}
		public virtual void ExposeData()
        {
			//Scribe_Values.Look<bool>()

		}
		public virtual float Consume(float amount)
		{
			float result = curAmount - amount;
			if (result <= 0f)
			{
				curAmount = 0;
				return Math.Abs(result);
			}
			curAmount = result;
			return 0f;
		}
		public virtual float Fill(float amount)
		{
			float result = curAmount + amount;
			if (result >= maxAmount)
			{
				curAmount = maxAmount;
				return result - maxAmount;
			}
			curAmount = result;
			return 0f;
		}

		private bool TimedTick()
		{
			if (Find.TickManager.TicksGame >= NextTick)
			{
				NextTick = Find.TickManager.TicksGame + this.TickPeriod;
				return true;
			}
			return false;
		}

	}
	public enum ManagerType
	{
		LifeSupport,
		Energy,
		Shields,
	}

	public class Manager_ : Manager
	{
		public Manager_ (Pawn vessel) : base(vessel)
		{
		}
		public override void Setup(bool respawningAfterLoad)
		{

		}
		public override void ManagerTimedTick()
		{

		}
		public override void ManagerTick()
		{
		}
		public override void ExposeData()
		{
		}
	}

}