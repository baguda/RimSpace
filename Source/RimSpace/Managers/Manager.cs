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
		public float[] thrsh = { 0.5f, 0.25f, 0.1f, 0.0f };


		public ManagerType MgrType;

		private float AmountInt = 50f;
		public float maxAmount = 100;
		public bool depleted => this.AmountInt == 0f;
		public float curAmount { get => this.AmountInt; set => this.AmountInt = value; }
		public float Level => curAmount / maxAmount;
		public SystemStatus status
        {
			get
			{
				if (this.Level == thrsh[3])
				{
					return SystemStatus.Down;
				}
				if (this.Level < thrsh[2])
				{
					return SystemStatus.Critcal;
				}
				if (this.Level < thrsh[1])
				{
					return SystemStatus.Strained;
				}
				if (this.Level < thrsh[0])
				{
					return SystemStatus.Stressed;
				}
				return SystemStatus.Holding;
			}
		}

		

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
		public virtual string ToPrint()
        {
			string result = "";
			return result;
        }
		
		public virtual string statusString
        {
            get
            {
				if (status.Equals(SystemStatus.Holding)) return "Holding";
				if (status.Equals(SystemStatus.Stressed)) return "Stressed";
				if (status.Equals(SystemStatus.Strained)) return "Strained";
				if (status.Equals(SystemStatus.Critcal)) return "Critcal";
				 return "Down";
			}
        }

	}
	public enum ManagerType 
	{
		LifeSupport,
		Energy,
		Shields,
	}
	public enum SystemStatus : byte
	{
		Holding = 0,
		Stressed = 1,
		Strained = 2,
		Critcal = 3,
		Down = 4
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