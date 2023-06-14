using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimSpace
{


	public class Need_Shields : Need
	{


		public float[] thrsh = { 0.5f, 0.25f, 0.1f, 0.0f };
		public CompSpaceship comp => this.pawn.GetComp<CompSpaceship>();
		public Manager_Shields manager => comp.shields;
		public override float MaxLevel => 1f;
		public override bool ShowOnNeedList => manager.CanHaveShields;
		public override float CurInstantLevel => base.CurInstantLevel;
		public override float CurLevel { get => base.CurLevel; set => base.CurLevel = value; }
		public override int GUIChangeArrow => manager.Charging ? 1 : -1;



		public ShipSupportCategory CurCategory
		{
			get
			{
				if (this.CurLevel == thrsh[3])
				{
					return ShipSupportCategory.Down;
				}
				if (this.CurLevel < thrsh[2])
				{
					return ShipSupportCategory.Critcal;
				}
				if (this.CurLevel < thrsh[1])
				{
					return ShipSupportCategory.Strained;
				}
				if (this.CurLevel < thrsh[0])
				{
					return ShipSupportCategory.Stressed;
				}
				return ShipSupportCategory.Holding;
			}
		}

		public Need_Shields(Pawn pawn) : base(pawn)
		{
			this.threshPercents = new List<float>();
			foreach (float n in thrsh)
			{
				this.threshPercents.Add(n);
			}

			this.pawn.def.HasComp(typeof(CompSpaceship));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			//Scribe_Values.Look<int>(ref this.ticksAtZero, "ticksAtZero", 0, false);
		}

		public override void SetInitialLevel()
		{
			this.CurLevel = 1f;
		}

		public override void NeedInterval()
		{
			if (false)//!this.IsFrozen)
			{
				if (manager.CanHaveShields && manager.Charging)
				{
					if (this.CurCategory == ShipSupportCategory.Holding)
					{
						this.CurLevel += 80f;
					}
					else if (this.CurCategory == ShipSupportCategory.Stressed)
					{
						this.CurLevel += 40f;
					}
					else if (this.CurCategory == ShipSupportCategory.Strained)
					{
						this.CurLevel += 20f;
					}
					else if (this.CurCategory == ShipSupportCategory.Critcal)
					{
						this.CurLevel += 10f;
					}
					else if (this.CurCategory == ShipSupportCategory.Down)
					{
						this.CurLevel += 5f;
					}
				}
			}
		}

		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2, float customMargin = -1, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
		{
			base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
		}

		public override string GetTipString()
		{
			return this.CurCategory.ToString();
		}


	}


	public enum ShipSupportCategory : byte
	{
		Holding = 0,
		Stressed = 1,
		Strained = 2,
		Critcal = 3,
		Down = 4
	}
}

	

