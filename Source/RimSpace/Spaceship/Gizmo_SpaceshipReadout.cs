
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
    [StaticConstructorOnStartup]
    public class Gizmo_ShipPanel : Gizmo
    {
        public int queueZ = 0;
        public int queueX = 0;
        public float GizmoWidth = 160f;
        public float GizmoHeight = 75f;
        public Vector2 Buffer = new Vector2(75f, 75f);
        public Pawn Spaceship;
        public CompSpaceship comp;
        //public AdventureHits shield;
        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.0f, 0.0f, 0.6f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        private static readonly Texture2D FullHealthBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.0f, 0.6f, 0.0f));
        private static readonly Texture2D FullPainBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.6f, 0.0f, 0.0f));
        private static readonly Texture2D FullResourceBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.6f, 0.6f, 0.0f));
        

        public Gizmo_ShipPanel()
        {
            this.Order = -100f;
        }
        public override float GetWidth(float maxWidth)
        {
            return GizmoWidth;
        }

        public void makeGraph(string Label, Rect space, float yMinPercent, float heightPercent, float fillPercent, Texture2D FullBarTex, Texture2D EmptyBarTex)
        {
            Rect graphWindow = space;
            graphWindow.yMin = space.y + space.height * yMinPercent;
            graphWindow.height = space.height * heightPercent;
            Widgets.FillableBar(graphWindow, fillPercent, FullBarTex, EmptyBarTex, false);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(graphWindow, (fillPercent * 100f).ToString("F0") + " / " + 100f.ToString("F0") + " " + Label);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public void makeGraph(string Label, Rect space, float yMinPercent, float heightPercent, float CurAmount, float MaxAmount, Texture2D FullBarTex, Texture2D EmptyBarTex)
        {
            Rect graphWindow = space;
            graphWindow.yMin = space.y + space.height * yMinPercent;
            graphWindow.height = space.height * heightPercent;
            Widgets.FillableBar(graphWindow, CurAmount / MaxAmount, FullBarTex, EmptyBarTex, false);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(graphWindow, (CurAmount * 100f).ToString("F0") + " / " + (MaxAmount * 100f).ToString("F0") + " " + Label);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public void makeNestedGraphs(string Label, Rect space, float yMinPercent, float heightPercent, float fillPercent1, float fillPercent2, Texture2D FullBarTex1, Texture2D FullBarTex2, Texture2D EmptyBarTex)
        {
            Rect graphWindow = space;
            graphWindow.yMin = space.y + space.height * yMinPercent;
            graphWindow.height = space.height * heightPercent;

            Rect subgraphWindow1 = graphWindow;
            subgraphWindow1.yMin = graphWindow.y + graphWindow.height * 0.25f;
            subgraphWindow1.height = graphWindow.height * 0.5f;

            Rect subgraphWindow2 = graphWindow;
            subgraphWindow2.yMin = graphWindow.y;
            subgraphWindow2.height = graphWindow.height * 1f;
            Widgets.FillableBar(subgraphWindow2, fillPercent2, FullBarTex2, EmptyBarTex, false);
            Widgets.FillableBar(subgraphWindow1, fillPercent1, FullBarTex1, EmptyBarTex, false);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(graphWindow, (fillPercent1 * 100f).ToString("F0") + "%|" + (fillPercent2 * 100f).ToString("F0") + "% " + Label);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public void makePanel(Rect space)
        {
            Rect LabelWindow = space;
            LabelWindow.height = space.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(LabelWindow, this.Spaceship.Name.ToStringFull+"| Crew:"+ this.comp.CrewList.Count.ToString());

            //var nt = this.Spaceship.health.hediffSet.hediffs.FindAll(s => s is Hediff_HeVe);
            
            makeNestedGraphs("Shields|Hull", space, 0.20f, 0.20f, 
                this.comp.shields.Level, 
                this.Spaceship.health.summaryHealth.SummaryHealthPercent / 1.0f,
                Gizmo_ShipPanel.FullPainBarTex, Gizmo_ShipPanel.FullHealthBarTex,
                Gizmo_ShipPanel.EmptyShieldBarTex);
            makeGraph("Life Support", space, 0.40f, 0.20f, comp.lifeSupport.Level, Gizmo_ShipPanel.FullShieldBarTex, Gizmo_ShipPanel.EmptyShieldBarTex);
            makeGraph("Energy", space, 0.60f, 0.20f, comp.energy.EnergyLevel, SolidColorMaterials.NewSolidColorTexture(200f/255f,115f/255f,50f/255f,1f), Gizmo_ShipPanel.EmptyShieldBarTex);
            /*
            Vector2 vector2 = new Vector2(space.x,space.y);
            Find.WindowStack.ImmediateWindow(typeof(DebugWindowsOpener).GetHashCode()+1, new Rect(vector2.x, vector2.y, 24f * 3.5f, 24f).Rounded(), WindowLayer.GameUI, delegate
            {

                WidgetRow row = new WidgetRow(0f, 0f, UIDirection.RightThenDown, 99999f, 4f);

                if (row.ButtonIcon(ContentFinder<Texture2D>.Get("GUI/Reset", true), "Restart Rimworld", null, null, null, true))
                {
                    GenCommandLine.Restart();
                }
                if (row.ButtonIcon(ContentFinder<Texture2D>.Get("WorldObjects/SpaceRegion", true), "Make Space Map"))
                {
                    //this.makeSystemMap();
                }
            }, false, false, 0f);*/
        }



    

        public void getPawnBars()
        {
            //health
            //pain
            //vitality
            //resource of equiped 
        }
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {

            topLeft = Buffer + new Vector2(queueX * GizmoWidth, queueZ * GizmoHeight);

            //setup main window
            Rect mainWindow = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 100f);
            Widgets.DrawWindowBackground(mainWindow);
            Rect innerWindow = mainWindow.ContractedBy(6f);
            makePanel(innerWindow);



            return new GizmoResult(GizmoState.Clear);

        }

    }
    
    
    
    
    
    /*
    //Setup Label window 
    Rect LabelWindow = innerWindow;
    LabelWindow.height = mainWindow.height / 2f;
    Text.Font = GameFont.Tiny;
    Widgets.Label(LabelWindow, this.PartyMember.Name.ToStringShort);// this.shield.LabelCap);



    Rect graphWindowHealth = innerWindow;

    if (this.PartyMember.apparel.WornApparel.Find(s => s.def.defName.Equals("AdventurersBackpack")) != null)
    {
        graphWindowHealth.yMin = innerWindow.y + innerWindow.height * 3f / 4f;
        graphWindowHealth.height = innerWindow.height * 1f / 4f;

        Rect graphWindowHits = innerWindow;

        graphWindowHits.yMin = innerWindow.y + innerWindow.height * 2f / 4f;
        graphWindowHits.height = innerWindow.height * 1f / 4f;
        float fillPercent = this.PartyMember.health.summaryHealth.SummaryHealthPercent / 1.0f;



        float fillPercentHits = hits.Energy / hits.EnergyMax;
        Widgets.FillableBar(graphWindowHealth, fillPercent, Gizmo_PartyPanel.FullHealthBarTex, Gizmo_PartyPanel.EmptyShieldBarTex, false);
        Widgets.FillableBar(graphWindowHits, fillPercentHits, Gizmo_PartyPanel.FullShieldBarTex, Gizmo_PartyPanel.EmptyShieldBarTex, false);

        //Text.Font = GameFont.Small;

        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(graphWindowHealth, (this.PartyMember.health.summaryHealth.SummaryHealthPercent * 100f).ToString("F0") + " / " + 100f.ToString("F0") + " Health");
        Widgets.Label(graphWindowHits, (hits.Energy * 100f).ToString("F0") + " / " + (100f * hits.EnergyMax).ToString("F0") + " Vitality");

        Text.Anchor = TextAnchor.UpperLeft;
    }
    else
    {
        graphWindowHealth.yMin = innerWindow.y + innerWindow.height / 2f;
        float fillPercent = this.PartyMember.health.summaryHealth.SummaryHealthPercent / 1.0f;
        Widgets.FillableBar(graphWindowHealth, fillPercent, Gizmo_PartyPanel.FullHealthBarTex, Gizmo_PartyPanel.EmptyShieldBarTex, false);
        //Text.Font = GameFont.Small;

        Text.Anchor = TextAnchor.MiddleCenter;

        Widgets.Label(graphWindowHealth, (this.PartyMember.health.summaryHealth.SummaryHealthPercent * 100f).ToString("F0") + " / " + 100f.ToString("F0") + " Health");

        Text.Anchor = TextAnchor.UpperLeft;
    }

    Rect graphWindowPain = innerWindow;
    graphWindowPain.yMin = innerWindow.y + innerWindow.height * 1f / 4f;
    graphWindowPain.height = innerWindow.height * 1f / 4f;

    //this.PartyMember.def.statBases.Add(new StatModifier() { });

    float fillPercent3 = this.PartyMember.health.hediffSet.PainTotal / 1f;
    Widgets.FillableBar(graphWindowPain, fillPercent3, Gizmo_PartyPanel.FullPainBarTex, Gizmo_PartyPanel.EmptyShieldBarTex, false);
    Text.Anchor = TextAnchor.MiddleCenter;
    Widgets.Label(graphWindowPain, (fillPercent3 * 100f).ToString("F0") + " / " + 100f.ToString("F0") + " Pain");
    Text.Anchor = TextAnchor.UpperLeft;
    */
    
}
