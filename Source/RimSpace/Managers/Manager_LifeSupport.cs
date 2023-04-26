using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{
	public class Manager_LifeSupport : Manager
	{
		public float LowerTendQuality = 0.5f;
		public float UpperTendQuality = 1f;
		public float healAmount = 0.1f;
		public bool tend = true;
		public bool heal = true;
		public int netCrewMedicalSkill => Crew.Select(d => d.skills.skills.Find(s => s.def.defName.Equals("Medicine")).levelInt).Sum();
		public int totalMedicalSkill =>  20;

		public int CrewMedicalSkill => Crew.Select(d => d.skills.skills.Find(s => s.def.defName.Equals("Medicine")).levelInt).Max();
		public float CrewMedicalLevel => (float)netCrewMedicalSkill / (float)totalMedicalSkill;
		public float lowerTendLimit => lowerTendFunc(CrewMedicalLevel);
		public float upperTendLimit => upperTendFunc(CrewMedicalLevel);
		public float healFactor => healFunc(CrewMedicalLevel) * 0.1f;


		public Func<float, float> lowerTendFunc = (float medLvl) => LinearFunc(medLvl, 0.85f, 0.05f);
		public Func<float, float> upperTendFunc = (float medLvl) => LinearFunc(medLvl, 1.5f, 0.1f);
		public Func<float, float> healFunc = (float medLvl) => parabolicFunc(medLvl, -1.8f, 1.8f, 0.1f);

		public static Func<float, float, float, float> LinearFunc = (float indVar, float slope, float yInt) => slope * indVar + yInt;

		public static Func<float, float, float, float, float> parabolicFunc = (float indVar, float accel, float speed, float pos) => 0.5f * accel * indVar * indVar + speed * indVar + pos;

		public float maxSupport => base.maxAmount;
		public float SupportLevel => base.Level;
		public float Support { get => base.curAmount; set => base.curAmount = value; }
		public bool Charging = false;
		public bool Discharging = false;




		public Manager_LifeSupport(Pawn vessel) : base(vessel)
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
