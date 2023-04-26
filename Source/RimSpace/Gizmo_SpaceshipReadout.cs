﻿
using System;
using UnityEngine;
using Verse;
using RimWorld;


namespace RimSpace
{
	[StaticConstructorOnStartup]
	public class Gizmo_SpaceshipReadout : Gizmo
	{
		public Gizmo_SpaceshipReadout()
		{
			this.Order = -100f;
		}
		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}
		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Rect rect3 = rect2;
			rect3.height = rect.height / 2f;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect3, this.shield.IsApparel ? this.shield.parent.LabelCap : "ShieldInbuilt".Translate().Resolve());
			Rect rect4 = rect2;
			rect4.yMin = rect2.y + rect2.height / 2f;
			float fillPercent = this.shield.Energy / Mathf.Max(1f, this.shield.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true, -1));
			Widgets.FillableBar(rect4, fillPercent, Gizmo_SpaceshipReadout.FullShieldBarTex, Gizmo_SpaceshipReadout.EmptyShieldBarTex, false);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect4, (this.shield.Energy * 100f).ToString("F0") + " / " + (this.shield.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true, -1) * 100f).ToString("F0"));
			Text.Anchor = TextAnchor.UpperLeft;
			TooltipHandler.TipRegion(rect2, "ShieldPersonalTip".Translate());
			return new GizmoResult(GizmoState.Clear);
		}
		public CompShield shield;
		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
	}
}