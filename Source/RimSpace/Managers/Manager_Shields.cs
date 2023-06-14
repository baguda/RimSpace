using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{
	public class Manager_Shields : Manager
	{

		public int lastDamageTick = -999;
 

		public float maxShield => base.maxAmount;
		public float ShieldLevel => base.Level;
		public float Shield { get => base.curAmount; set => base.curAmount = value; }
		public bool Charging => Find.TickManager.TicksGame > this.lastDamageTick + 120;
		public Manager_Shields(Pawn vessel) : base(vessel)
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
		public bool CanHaveShields
		{
			get
			{
				bool result = true;
				if (!this.Vessel.def.HasComp(typeof(CompSpaceship)))
				{
					result = false;
				}
				if (!this.Vessel.def.HasComp(typeof(CompSpaceship)))
				{
					result = false;
				}
				return result;
			}
		}

		public float boostShields(float amount)
        {
			return Fill(amount);
        }

		public DamageInfo DamageShields(DamageInfo damageInfo)
        {

			return new DamageInfo(damageInfo.Def, Consume(damageInfo.Amount), damageInfo.ArmorPenetrationInt,
											   damageInfo.Angle, damageInfo.Instigator, damageInfo.HitPart,
											   damageInfo.Weapon, damageInfo.Category, damageInfo.IntendedTarget,
											   damageInfo.InstigatorGuilty, false);


		}

	}
}
