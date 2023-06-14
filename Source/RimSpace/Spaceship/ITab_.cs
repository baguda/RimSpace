using System.Collections.Generic;

using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;


namespace RimSpace
{

    public class ITab_ContentsCrew : ITab_ContentsBase
    {
        public override IList<Thing> container
        {
            get
            {
                var Vessel = (base.SelThing as ThingWithComps).GetComp<CompSpaceship>();
                this.listInt.Clear();
                if (Vessel != null && Vessel.ContainedThing != null)
                {
                    this.listInt.Add(Vessel.ContainedThing);
                }
                return this.listInt;
            }
        }
        public ITab_ContentsCrew()
        {
            this.labelKey = "TabCasketContents";
            this.containedItemsKey = "ContainedItems";
            this.canRemoveThings = true;
        }
        private List<Thing> listInt = new List<Thing>();
    }


    public class ITab_ContentsCrewLarge : ITab_ContentsBase
    {
        public List<Contents> data = new List<Contents>();
        private List<Thing> listInt = new List<Thing>();
        public CompSpaceship comp => (this.SelObject as Pawn).GetComp<CompSpaceship>();
        public override IList<Thing> container
        {
            get
            {
                var Vessel = (base.SelThing as ThingWithComps).GetComp<CompSpaceship>();
                this.listInt.Clear();
                if (Vessel != null && Vessel.ContainedThing != null)
                {
                    this.listInt.Add(Vessel.ContainedThing);
                }
                return this.listInt;
            }
        }
        public override bool UseDiscardMessage => false;
        //public CompSpaceship Transporter => base.SelThing.TryGetComp<CompSpaceship>();
        public override bool IsVisible => (base.SelThing.Faction == null || base.SelThing.Faction == Faction.OfPlayer)/* && this.SelPawn.GetComp<Comp_CrewHolder>().hasContents */;
        public override IntVec3 DropOffset => base.DropOffset;
        public ITab_ContentsCrewLarge()
        {
            this.labelKey = "Crew";
            this.containedItemsKey = "ContainedItems";

        }
        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            float a = 0f;
            Rect rect = new Rect(0f, curY, (inRect.width - 10f) / 1f, inRect.height);
            Text.Font = GameFont.Small;
            bool flag = false;
            Widgets.BeginGroup(rect);
            string Status = "Crew: " + comp.CrewList.Count.ToString() + "/" + this.comp.Props.getCrewSize.ToString();// +
               // " : Medical Level: " + ((comp.managers.Find(m => m.MgrType == ManagerType.LifeSupport) as Manager_LifeSupport).CrewMedicalLevel * 100f).ToString() + "% | Captain: " +
               // this.comp.overseer.Name.ToStringShort;

            Widgets.ListSeparator(ref a, rect.width, Status);
            if (comp.hasContents)
            {
                foreach (Pawn pawn in comp.CrewList)
                {
                    TransferableOneWay t = new TransferableOneWay();
                    t.things = new List<Thing>();
                    t.things.Add(pawn as Thing);
                    flag = true;
                    base.DoThingRow(base.SelPawn.def, 1, t.things, rect.width, ref a, delegate (int x)
                    {
                        
                        //this.OnDropToLoadThing(t, x);
                    });
                }

                foreach (Thing item in comp.ContentsList.Where(s => !(s is Pawn)))
                {
                    TransferableOneWay t = new TransferableOneWay();
                    t.things = new List<Thing>() ;
                    t.things.Add(item);
                    base.DoThingRow(item.def, item.stackCount , t.things, rect.width, ref a, delegate (int x)
                    {
                       // this.comp.lifeSupport.processThing(t);
                    });
                }

            }
            if (!flag)
            {
                Widgets.NoneLabel(ref a, rect.width, null);
            }
            Widgets.EndGroup();

            Rect inRect2 = new Rect(0f, curY, (inRect.width - 10f) / 1f, inRect.height);
            float b = 0f;
            Text.Font = GameFont.Small;
            Widgets.BeginGroup(inRect2);
            //Widgets.DrawTextureFitted(rect, ContentFinder<Texture2D>.Get("Terrain/Space12", true), 1f);

            Widgets.EndGroup();

            //base.DoItemsLists(inRect2, ref b);
            curY += Mathf.Max(a, b);

            
        }
        protected override void OnDropThing(Thing t, int count)
        {
            base.OnDropThing(t, count);
            Pawn pawn;
            if ((pawn = (t as Pawn)) != null)
            {
                this.RemovePawnFromLoadLord(pawn);
            }
        }
        private void RemovePawnFromLoadLord(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            if (lord != null && lord.LordJob is LordJob_LoadAndEnterTransporters)
            {
                lord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily, null);
            }
        }
        private void OnDropToLoadThing(TransferableOneWay t, int count)
        {
            t.ForceTo(t.CountToTransfer - count);
            this.EndJobForEveryoneHauling(t);
            foreach (Thing thing in t.things)
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    this.RemovePawnFromLoadLord(pawn);
                }
            }
        }
        private void EndJobForEveryoneHauling(TransferableOneWay t)
        {
            List<Pawn> allPawnsSpawned = base.SelThing.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                if (allPawnsSpawned[i].CurJobDef == DefDatabase<JobDef>.GetNamed("EnterCrew"))// JobDefOf.HaulToTransporter)
                {
                    JobDriver_EnterCrew jobDriver_EnterCrew = (JobDriver_EnterCrew)allPawnsSpawned[i].jobs.curDriver;
                    //if (jobDriver_EnterCrew.Transporter == this.Transporter && jobDriver_EnterCrew.ThingToCarry != null && jobDriver_EnterCrew.ThingToCarry.def == t.ThingDef)
                    {
                        allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                    }
                }
            }
        }
    }


    public struct Contents
    {
        public string defName;

        public List<Thing> things;

        public Contents(Thing item)
        {
            this.defName = item.def.defName;
            this.things = new List<Thing>() { item };

        }
        public void addOne(Thing item)
        {
            this.things.Add(item);
        }

        public ThingDef thingDef => DefDatabase<ThingDef>.GetNamed(this.defName);
        public string ToPrint => this.defName + ": x" + count.ToString();
        public int count => things.Count();
    }






    /*

    [StaticConstructorOnStartup]
    public class Gizmo_PartyPanel : Gizmo
    {
        public int queueZ = 0;
        public int queueX = 0;
        public float GizmoWidth = 160f;
        public float GizmoHeight = 75f;
        public Vector2 Buffer = new Vector2(75f, 75f);
        public Pawn PartyMember;
        //public AdventureHits shield;
        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.0f, 0.0f, 0.6f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        private static readonly Texture2D FullHealthBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.0f, 0.6f, 0.0f));
        private static readonly Texture2D FullPainBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.6f, 0.0f, 0.0f));
        private static readonly Texture2D FullResourceBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.6f, 0.6f, 0.0f));
        AdventureHits hits => this.PartyMember.apparel.WornApparel.Find(s => s.def.defName.Equals("AdventurersBackpack")) as AdventureHits;

        public Gizmo_PartyPanel()
        {
            this.order = -100f;
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
            var nt = this.PartyMember.health.hediffSet.hediffs.FindAll(s => s is Hediff_HeVe);

            Rect LabelWindow = space;
            LabelWindow.height = space.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(LabelWindow, this.PartyMember.Name.ToStringShort);// this.shield.LabelCap);
            makeNestedGraphs("Pain|Health", space, 0.25f, 0.25f, this.PartyMember.health.hediffSet.PainTotal / 1f, this.PartyMember.health.summaryHealth.SummaryHealthPercent / 1.0f, Gizmo_PartyPanel.FullPainBarTex, Gizmo_PartyPanel.FullHealthBarTex, Gizmo_PartyPanel.EmptyShieldBarTex);
            if (this.PartyMember.apparel.WornApparel.Find(s => s.def.defName.Equals("AdventurersBackpack")) != null)
            {
                makeGraph("Vitality", space, 0.5f, 0.25f, hits.Energy, hits.EnergyMax / 1f, Gizmo_PartyPanel.FullShieldBarTex, Gizmo_PartyPanel.EmptyShieldBarTex);
            }
            var cnt = nt != null ? nt.Count : 0;
            if (cnt > 0)
            {
                foreach (Hediff_HeVe hediff in nt)
                {
                    if (hediff.isActive)
                    {
                        makeGraph(hediff.Def.ResourceLabel, space, 0.75f, 0.25f, hediff.CurrentAmount / 100, hediff.MaxResource / 100f, SolidColorMaterials.NewSolidColorTexture(hediff.LabelColor), Gizmo_PartyPanel.EmptyShieldBarTex);

                    }
                }
            }

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
            Rect mainWindow = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(mainWindow);
            Rect innerWindow = mainWindow.ContractedBy(6f);
            makePanel(innerWindow);



            return new GizmoResult(GizmoState.Clear);

        }

    }
    */
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


namespace RimSpace
{
    public class ITab_ContentsTransporter : ITab_ContentsBase
    {
        public override IList<Thing> container => this.Transporter.innerContainer;

        public override bool UseDiscardMessage => false;
        public CompSpaceship Transporter => base.SelThing.TryGetComp<CompSpaceship>();
        public override bool IsVisible => (base.SelThing.Faction == null || base.SelThing.Faction == Faction.OfPlayer) 
            && this.Transporter != null &&  this.Transporter.innerContainer.Any;
        public override IntVec3 DropOffset => ShipJob_Unload.DropoffSpotOffset;
        public ITab_ContentsTransporter()
        {
            this.labelKey = "TabTransporterContents";
            this.containedItemsKey = "ContainedItems";
        }
        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            CompSpaceship transporter = this.Transporter;
            Rect rect = new Rect(0f, curY, (inRect.width - 10f) / 2f, inRect.height);
            Text.Font = GameFont.Small;
            bool flag = false;
            float a = 0f;
            Widgets.BeginGroup(rect);
            Widgets.ListSeparator(ref a, rect.width, "Crew".Translate());

                for (int i = 0; i < transporter.CrewList.Count; i++)
                {
                TransferableOneWay t = new TransferableOneWay(); //transporter.leftToLoad[i];
                t.things.Add(transporter.CrewList[i]);
                    if (t.CountToTransfer > 0 && t.HasAnyThing)
                    {
                        flag = true;
                        base.DoThingRow(t.ThingDef, t.CountToTransfer, t.things, rect.width, ref a, delegate (int x)
                        {
                            this.OnDropToLoadThing(t, x);
                        });
                    }
                }
            
            if (!flag)
            {
                Widgets.NoneLabel(ref a, rect.width, null);
            }
            Widgets.EndGroup();
            Rect inRect2 = new Rect((inRect.width + 10f) / 2f, curY, (inRect.width - 10f) / 2f, inRect.height);
            float b = 0f;
            base.DoItemsLists(inRect2, ref b);
            curY += Mathf.Max(a, b);
        }
        protected override void OnDropThing(Thing t, int count)
        {
            base.OnDropThing(t, count);
            Pawn pawn;
            if ((pawn = (t as Pawn)) != null)
            {
                this.RemovePawnFromLoadLord(pawn);
            }
        }
        private void RemovePawnFromLoadLord(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            if (lord != null && lord.LordJob is LordJob_LoadAndEnterTransporters)
            {
                lord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily, null);
            }
        }
        private void OnDropToLoadThing(TransferableOneWay t, int count)
        {
            t.ForceTo(t.CountToTransfer - count);
            this.EndJobForEveryoneHauling(t);
            foreach (Thing thing in t.things)
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    this.RemovePawnFromLoadLord(pawn);
                }
            }
        }
        private void EndJobForEveryoneHauling(TransferableOneWay t)
        {
            List<Pawn> allPawnsSpawned = base.SelThing.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
                {
                    JobDriver_HaulToTransporter jobDriver_HaulToTransporter = (JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver;
                    //if (jobDriver_HaulToTransporter.Transporter == this.Transporter && jobDriver_HaulToTransporter.ThingToCarry != null && jobDriver_HaulToTransporter.ThingToCarry.def == t.ThingDef)
                    {
                        allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                    }
                }
            }
        }
    }
}
