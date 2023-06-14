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
		public float healAmount = 1.1f;
		public float tendCost = 1.1f;
		public float healCost = 1.1f;
		public float JoyFillCost = 1.1f;
		public float RestFillCost = 1.1f;
		public float FoodFillCost = 1.1f;
		public float energyDraw = 10f;
		public float energyCost = 1f;

		public List<Thing> meds => comp.ContentsList.FindAll(s => s.HasThingCategory(ThingCategoryDefOf.Medicine));
		public int totalMedsCount => meds.Select(s => s.stackCount).Sum();
		public bool hasMeds => meds.Any();
		public List<Thing> herbMeds => meds.FindAll(s => s.def.Equals(ThingDefOf.MedicineHerbal));
		public int totalHerbMedsCount => herbMeds.Select(s => s.stackCount).Sum();
		public bool hasHerbMeds => herbMeds.Any();
		public List<Thing> indMeds => meds.FindAll(s => s.def.Equals(ThingDefOf.MedicineIndustrial));
		public int totalIndMedsCount => indMeds.Select(s => s.stackCount).Sum();
		public bool hasIndMeds => indMeds.Any();
		public List<Thing> ultMeds => meds.FindAll(s => s.def.Equals(ThingDefOf.MedicineUltratech));
		public int totalUltMedsCount => ultMeds.Select(s => s.stackCount).Sum();
		public bool hasUltMeds => ultMeds.Any();


		public bool heal = true;
		public bool tend => this.meds.Any();
		public int netCrewMedicalSkill => Crew.Select(d => d.skills.skills.Find(s => s.def.defName.Equals("Medicine")).levelInt).Sum();
		public int totalMedicalSkill => 20;
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

		public bool isFull => this.Support == maxSupport;
		public float maxSupport { get => base.maxAmount; set => base.maxAmount = value; }
		public float SupportLevel => base.Level;
		public float Support { get => base.curAmount; set => base.curAmount = value; }
		public bool Charging = false;
		public bool Discharging = false;


		public float ConvertEnergy(float amount)
        {
			float diff = this.comp.energy.Consume(amount);
			return this.Fill(energyCost * (amount-diff));
        }


		public Manager_LifeSupport(Pawn vessel) : base(vessel)
		{

		}
		public override void Setup(bool respawningAfterLoad)
		{
			base.Setup(respawningAfterLoad);
		}
		public override void ManagerTimedTick()
		{
			Consume(HealNow());
			Consume(TendNow());
			Consume(GiveNeeds());
			if (comp.PowerToLifeSupport && !isFull) ConvertEnergy(energyDraw);
			base.ManagerTimedTick();
		}
		public override void ManagerTick()
		{
			base.ManagerTick();
		}
		public override string ToPrint()
		{

			return "Life Support Status: " + this.statusString + " at " + this.Level.ToString("F0") + "% (" + curAmount.ToString() + "/" + maxAmount.ToString() + ")";
		}
		public override void ExposeData()
		{
			base.ExposeData();
		}

		public float TendNow()
		{
			if (tend)
			{
				float result = 0;
				comp.CrewList.ForEach(s =>
				{
					result += tendPawn(s);
				});
				return result;
			}
			else return 0f;
		}
		public float HealNow()
		{
			if (heal)
			{
				float result = 0f;
				comp.CrewList.ForEach(s =>
				{

					s.health.hediffSet.hediffs.ForEach(delegate (Hediff hedif)
					{
						if (!hedif.IsPermanent() && hedif.def != HediffDefOf.MechlinkImplant)
                        {
							hedif.Severity -= healAmount;
							result += healCost;
						}
							
					});
				});
				return result;
			}
			else return 0f;
		}
		public float GiveNeeds()
		{
			float result = 0f;
			foreach (Pawn member in Crew)
			{
				if (member.needs.food.CurLevel < 0.3f)
				{
					member.needs.food.CurLevel = 1f;
					result += FoodFillCost;
				}
				if (member.needs.rest.CurLevel < 0.3f)
				{
					member.needs.rest.CurLevel = 1f;
					result += RestFillCost;
				}
				if (member.RaceProps.Humanlike && !(member.IsPrisoner || member.IsSlave) && member.needs.joy.CurLevel < 0.3f)
				{
					member.needs.joy.CurLevel = 1f;
					result += JoyFillCost;
				}
			}
			return result;
		}
		public float tendPawn(Pawn pawn)
		{
			float result = 0f;
			foreach (var hedif in pawn.health.hediffSet.hediffs)
			{
				if (hedif.TendableNow())
				{
					float medShift = useMed(pawn.playerSettings.medCare);
					hedif.Tended(LowerTendQuality, UpperTendQuality);
					result += tendCost;
					
				}
			}
			return result;
		}
		public float useMed(MedicalCareCategory care, int count = 1)
		{
			if (care == MedicalCareCategory.NoMeds)
			{
				return 0f;
			}
			if (care == MedicalCareCategory.HerbalOrWorse)
			{
				if (hasHerbMeds)
                {
					Thing med = this.herbMeds.First();
					med.stackCount -= count;
					if (med.stackCount <= 0) med.Destroy(DestroyMode.Vanish);
					return 0.2f;
				}
				return 0f;

			}
			if (care == MedicalCareCategory.NormalOrWorse)
			{
				if (hasIndMeds)
				{
					Thing med = this.indMeds.First();
					med.stackCount -= count;
					if (med.stackCount <= 0) med.Destroy(DestroyMode.Vanish);
					return 0.40f;
				}
				else if (hasHerbMeds)
				{
					Thing med = this.herbMeds.First();
					med.stackCount -= count;
					if (med.stackCount <= 0) med.Destroy(DestroyMode.Vanish);
					return 0.20f;
				}
				return 0f;

			}
			if (care == MedicalCareCategory.Best)
			{
				if (hasUltMeds)
				{
					Thing med = this.ultMeds.First();
					med.stackCount -= count;
					if (med.stackCount <= 0) med.Destroy(DestroyMode.Vanish);
					return 0.60f;

				}
				else if (hasIndMeds)
				{
					Thing med = this.indMeds.First();
					med.stackCount -= count;
					if (med.stackCount <= 0) med.Destroy(DestroyMode.Vanish);
					return 0.40f;
				}
				else if (hasHerbMeds)
				{
					Thing med = this.herbMeds.First();
					med.stackCount -= count;
					if (med.stackCount <= 0) med.Destroy(DestroyMode.Vanish);
					return 0.20f;
				}
				return 0f;

			}
			return 0;
		
        }

		public List<Thing> meals => comp.ContentsList.FindAll(s => s.HasThingCategory(ThingCategoryDefOf.FoodMeals));



		public void checkFill()
        {
			//if(this.Level < 0.25f) this.Fill(processThing(meds.RandomElement()));
		}
	}
}
