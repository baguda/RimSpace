using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse.AI;
using RimWorld.Planet;

namespace RimSpace
{
    public class CompSpaceship : CompVessel
    {

        private int nextTick;

        public int TickPeriod = 60;
      

        public bool RefillPower = true;
        private bool powerToShields = false;
        private bool powerToLifeSupport = true;
        public Manager_LifeSupport LifeSupport;
        public Manager_Energy Energy;
        public Manager_Shields Shields;
        public HashSet<IntVec3> discoveredCells = new HashSet<IntVec3>();



        public CompProperties_Spaceship Props => this.props as CompProperties_Spaceship;
        public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>() as GameComp_StarSystem;
        public int NextTick { set => nextTick = value; get => nextTick; }
        public bool PowerToShields => powerToShields;
        public bool PowerToLifeSupport => powerToLifeSupport;
        public Manager_LifeSupport lifeSupport => LifeSupport == null ? new Manager_LifeSupport(this.pawn) : LifeSupport;
        public Manager_Energy energy => Energy == null ? new Manager_Energy(this.pawn) : Energy;
        public Manager_Shields shields => Shields == null ? new Manager_Shields(this.pawn) : Shields;
        public int Count => this.innerContainer.Count;
        public bool hasContents => this.innerContainer.Any();
        public List<Pawn> CrewList
        {
            get
            {
                List<Pawn> result = new List<Pawn>();
                foreach (Thing thing in this.innerContainer.ToList().FindAll(s => s is Pawn && (s as Pawn).RaceProps.Humanlike))
                {

                    result.Add(thing as Pawn);
                }
                return result;
            }
        }
        public List<Thing> ContentsList => this.innerContainer.ToList();
        public override int OpenTicks => Props.getReadyTime;
        public Pawn Pilot => CrewList.Find(s => s.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
        public bool HasPilot => Pilot != null;
        public bool inSpace => this.map.Parent.def.defName.Equals("OrbitalLocation");
        public MapComp_SpaceMap SpaceComp => this.map.GetComponent<MapComp_SpaceMap>();
        public Pawn overseer => this.pawn.GetOverseer();
        public bool hasOverseer => overseer != null;
        public bool Overseen => hasOverseer ? this.CrewList.FindAll(s => s.ThingID == this.overseer.ThingID).Any() : false;
        public List<Thing> nearbyThings => map.listerThings.AllThings.FindAll(s=> GenRadial.RadialCellsAround(this.parent.Position, 5f, false).ToList().Contains(s.Position));
        public Building nearbyPlanet => nearbyThings.Find(s => s.def.HasComp(typeof (CompPlanet))) as Building;
        public CompPlanet nearbyPlanetComp => (nearbyPlanet).GetComp<CompPlanet>();




        private bool TimedTick()
        {

            if (Find.TickManager.TicksGame >= NextTick)
            {
                this.NextTick = Find.TickManager.TicksGame + 3600;// this.TickPeriod;

                return true;
            }
            return false;
        }
        public void TogglePower()
        {
            powerToShields = !powerToShields;
            powerToLifeSupport = !powerToLifeSupport;
        }
        public static Building_CryptosleepCasket FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {

            foreach (ThingDef singleDef in from def in DefDatabase<ThingDef>.AllDefs
                                           where def.IsCryptosleepCasket
                                           select def)
            {
                IntVec3 positionHeld = p.PositionHeld;
                Map mapHeld = p.MapHeld;
                ThingRequest thingReq = ThingRequest.ForDef(singleDef);
                PathEndMode peMode = PathEndMode.InteractionCell;
                TraverseParms traverseParams = TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false, false, false);
                float maxDistance = 9999f;
                Predicate<Thing> validator = ((Thing x) => !((Building_CryptosleepCasket)x).HasAnyContents && traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations));

                Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(positionHeld, mapHeld, thingReq, peMode, traverseParams, maxDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                if (building_CryptosleepCasket != null)
                {
                    return building_CryptosleepCasket;
                }
            }
            return null;
        }
        /*
        public override void EjectContents()
        {
            ThingDef filth_Slime = ThingDefOf.Filth_Slime;
            foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    pawn.filth.GainFilth(filth_Slime);
                    if (pawn.RaceProps.IsFlesh)
                    {
                        //pawn.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null, null);
                    }
                }
            }
            if (!parent.Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent.Position, map, false), MaintenanceType.None));
            }
            base.EjectContents();
        }
        */
        public void DrawCommandRadius()
        {
            if (this.pawn.Spawned && this.pawn.Drafted)
            {
                GenDraw.DrawRadiusRing(this.pawn.Position, 24.9f, Color.white, (IntVec3 c) => this.CanCommandTo(c));

            }
        }
        public bool CanCommandTo(LocalTargetInfo target)
        {
            bool result = target.Cell.InBounds(this.pawn.MapHeld) && (float)this.pawn.Position.DistanceToSquared(target.Cell) < 620.01f;
            if (result)
            {
                //SpaceComp.knownSpace.Add(target.Cell);
                if (map.fogGrid.IsFogged(target.Cell)) map.fogGrid.Unfog(target.Cell);
            }
            return result;
        }
        public void homecoming()
        {
            if (!this.parent.Spawned)
            {
                Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
                return;
            }
            //Current.Game.GetComponent<GameComp_StarSystem>().makeStarMap();
            Map map = this.parent.Map;
            int num = (int)GenMath.SphericalDistance(starSystem.LagrangePoint.DrawPos.normalized, Find.WorldGrid.GetTileCenter(this.parent.Tile).normalized);
            float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);

            Pilot.mechanitor.UnassignPawnFromAnyControlGroup(pawn);
            Pilot.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, pawn);
            WorldObject_SpaceShuttle vessel = WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("TravelingSpaceShip")) as WorldObject_SpaceShuttle;
            ThingOwner directlyHeldThings = this.GetDirectlyHeldThings();
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
            activeDropPod.Contents = new ActiveDropPodInfo();
            activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
            vessel.Tile = this.parent.Map.Tile;
            vessel.SetFaction(Faction.OfPlayer);
            vessel.toSpace = false;
            //vessel.targetObject = starSystem.LagrangePoint;// this.destinationTile;
            vessel.arrivalAction = null; //this.arrivalAction;

            Find.WorldObjects.Add(vessel);
            vessel.AddPod(activeDropPod.Contents, true);
            this.parent.DeSpawn(DestroyMode.Vanish);

            CameraJumper.TryShowWorld();//.TryJump(new GlobalTargetInfo(starSystem.SystemMap.Center, starSystem.SystemMap));//.TryHideWorld();
        }
        
        public void visitWorld()
        {


            //Current.Game.GetComponent<GameComp_StarSystem>().makeStarMap();
            //Map map = this.parent.Map;
            //int num = (int)GenMath.SphericalDistance(starSystem.LagrangePoint.DrawPos.normalized, Find.WorldGrid.GetTileCenter(this.parent.Tile).normalized);
            //float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
            /*
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
            activeDropPod.Contents = new ActiveDropPodInfo();
            activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
            activeDropPod.Contents.savePawnsWithReferenceMode = false;
            activeDropPod.Contents.parent = null;
            ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();*/




            Log.Message("CompSpaceship.visitWorld: 1");
            if (!this.parent.Spawned)
            {
                Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
                return;
            }


            Pilot.mechanitor.UnassignPawnFromAnyControlGroup(pawn);
            Log.Message("CompSpaceship.visitWorld: 2");
            Pilot.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, pawn);
            Log.Message("CompSpaceship.visitWorld: 3");
            ThingOwner directlyHeldThings = this.GetDirectlyHeldThings();
            Log.Message("CompSpaceship.visitWorld: 4");
            Building landedShip = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("SpaceShuttle")) as Building;
            Log.Message("CompSpaceship.visitWorld: 5");
            landedShip.SetFaction(Faction.OfPlayer);
            Log.Message("CompSpaceship.visitWorld: 6 has ground map= " + nearbyPlanetComp.hasGroundMap.ToString());
            Map groundMap = nearbyPlanetComp.GroundMap;
            Log.Message("CompSpaceship.visitWorld: 7");
            var point = new IntVec3();
            Log.Message("CompSpaceship.visitWorld: 8");
            RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(s => this.CanLandAt(s, groundMap), groundMap, out point);
            Log.Message("CompSpaceship.visitWorld: 9");
            Log.Message("CompSpaceship.visitWorld: 10 " + nearbyPlanetComp.DefName);
        landedShip.GetComp<CompLaunchShip>().curPlanetID = nearbyPlanetComp.planetThingID;
            Log.Message("CompSpaceship.visitWorld: 11" );
            Log.Message("CompSpaceship.visitWorld: 12");
            Thing Ship = GenSpawn.Spawn(landedShip, point, groundMap);
            Log.Message("CompSpaceship.visitWorld: 13");
            directlyHeldThings.TryDropAll(Ship.InteractionCell, groundMap, ThingPlaceMode.Near, null, null, true);
            Log.Message("CompSpaceship.visitWorld: 14");
            CameraJumper.TryJump(point, groundMap);
            parent.DeSpawn(DestroyMode.Vanish);

            //this.CrewList.ForEach(s => Find.WorldPawns.RemovePawn(s));



        }
        public bool CanLandAt(IntVec3 loc, Map map)
        {
            if (loc.CloseToEdge(map, 10))return false;
            if ( !loc.Standable(map)) return false;
            if ( loc.Fogged(map)) return false;
            if ( loc.Roofed(map)) return false;
            return true;
        }
        public void LandOnPlanet()
        {
            Log.Message("CompSpaceship.LandOnPlanet: 1");
             
            if (nearbyPlanetComp.Props.isHome )
            {
                Log.Message("CompSpaceship.LandOnPlanet: 3");
                homecoming();
                Log.Message("CompSpaceship.LandOnPlanet: 4");
            }
            else
            {
                Log.Message("CompSpaceship.LandOnPlanet: 5");
                if (!nearbyPlanetComp.hasGroundMap)
                {
                    Map gmap = starSystem.makeGroundMap(nearbyPlanetComp);
                    nearbyPlanetComp.GroundMap = gmap;
                    // generate ground map from Def
                    // do landing routine on ground map
                    Log.Message("CompSpaceship.LandOnPlanet: 6");
                    visitWorld();
                }
                else
                {
                    // 
                    Log.Message("CompSpaceship.LandOnPlanet: 7");
                    visitWorld();
                }
            }

        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            this.NextTick = Find.TickManager.TicksGame + this.TickPeriod;
            lifeSupport.Setup(respawningAfterLoad);
            energy.Setup(respawningAfterLoad);
            shields.Setup(respawningAfterLoad);
            //this.pawn.mechanitor = new Pawn_SpaceshipTracker();
        }
        public override void CompTick()
        {
            if (this.Overseen)
            {
                overseer.Position = this.parent.Position;
                if (!pawn.Drafted) { pawn.drafter.Drafted = true; }
            }
            lifeSupport.ManagerTick();
            energy.ManagerTick();
            shields.ManagerTick();
            //if (inSpace)
            base.CompTick();

            if (TimedTick())
            {
                lifeSupport.ManagerTimedTick();
                energy.ManagerTimedTick();
                shields.ManagerTimedTick();
            }
            
        }
        public override void Initialize(CompProperties props)
        {
            this.LifeSupport = new Manager_LifeSupport(this.pawn);
            this.Shields = (new Manager_Shields(this.pawn));
            this.Energy = (new Manager_Energy(this.pawn));
            base.Initialize(props);
        }
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            //if (Rand.Chance(Math.Max(0, Math.Min(0.0f, 0.9f - pawn.HealthScale)))) lifeSupport.consoleExplodes(totalDamageDealt, dinfo);
        }
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {

            float trample = shields.DamageShields(dinfo).Amount;
            if (trample > 0)
            {
                absorbed = false;
                return;
            }
            absorbed = true;


        }
        public override string CompInspectStringExtra()
        {
            string result = this.lifeSupport.ToPrint();

            return base.CompInspectStringExtra() + result;
        }
        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
        }
        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(parent.Position, map, false));
                }
                return true;
            }
            return false;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            if (myPawn.IsQuestLodger())
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield return floatMenuOption;
                yield break;
            }
            foreach (FloatMenuOption floatMenuOption2 in base.CompFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption2;
            }
            if (this.innerContainer.Count < this.Props.getCrewSize && this.overseer != null)
            {
                if (!myPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption floatMenuOption3 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield return floatMenuOption3;
                }
                else
                {
                    JobDef jobDef = DefDatabase<JobDef>.GetNamed("EnterCrew");    //JobDefOf.EnterCryptosleepCasket;
                    string label = "Enter Vessel";
                    Action action = delegate ()
                    {
                        if (!ModsConfig.BiotechActive)
                        {
                            myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), new JobTag?(JobTag.Misc), false);
                            return;
                        }
                        Hediff_PsychicBond hediff_PsychicBond = myPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond, false) as Hediff_PsychicBond;
                        if (hediff_PsychicBond == null || !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(myPawn, hediff_PsychicBond))
                        {
                            myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), new JobTag?(JobTag.Misc), false);
                            return;
                        }
                        WindowStack windowStack = Find.WindowStack;
                        TaggedString text = "PsychicBondDistanceWillBeActive_Cryptosleep".Translate(myPawn.Named("PAWN"), ((Pawn)hediff_PsychicBond.target).Named("BOND"));
                        Action confirmedAct = delegate ()
                        {
                            myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), new JobTag?(JobTag.Misc), false);
                        };

                        windowStack.Add(Dialog_MessageBox.CreateConfirmation(text, confirmedAct, true, null, WindowLayer.Dialog));
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), myPawn, parent, "ReservedBy", null);
                }
            }
            yield break;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (true)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = Props.getEjectLabel;
                command_Action.defaultDesc = Props.getEjectDesc; ;

                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("GUI/Land", true);
                command_Action.action = delegate
                {
                    if (!starSystem.HomePortReady)
                    {

                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            new TaggedString(
                                "Warning! The ship landing area is currently inactive. " +
                                "Attempting to land with an inavtive ship landing area may result in a rough or crash landing; which can damage or destroy the space shuttle. " +
                                "Chance of crash landing depends on the weather and visiblity of the landing pad. (Current Chance of crash landing: " +  "%)"),
                            new Action(this.LandOnPlanet), false, null, WindowLayer.Dialog));
                        return;
                    } //
                    LandOnPlanet();
                };

                if (nearbyPlanet == null)
                {
                    command_Action.Disable("No Nearby planet to land on");
                }
                yield return command_Action;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Fill Energy";
                command_Action.defaultDesc = "Fill up energy"; ;

                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("ui/misc/badtexture", true);
                command_Action.action = delegate 
                {
                    this.energy.Energy = this.energy.MaxEnergy;
                };

                yield return command_Action;

                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "Fill Shields";
                command_Action2.defaultDesc = "Fill up shields"; ;
                command_Action2.hotKey = KeyBindingDefOf.Misc8;
                command_Action2.icon = ContentFinder<Texture2D>.Get("ui/misc/badtexture", true);
                command_Action2.action = delegate {this.shields.curAmount = this.shields.maxShield;};

                yield return command_Action2;

                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "Fill Life Support";
                command_Action3.defaultDesc = "Fill up life support"; ;
                command_Action3.hotKey = KeyBindingDefOf.Misc8;
                command_Action3.icon = ContentFinder<Texture2D>.Get("ui/misc/badtexture", true);
                command_Action3.action = delegate { this.lifeSupport.curAmount = this.lifeSupport.maxSupport; };

                yield return command_Action3;
            }

            //----
            /*
             *             if (parent.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && true)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.EjectContents);
                command_Action.defaultLabel = Props.getEjectLabel;
                command_Action.defaultDesc = Props.getEjectDesc; ;
                if (this.innerContainer.Count == 0)
                {
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());
                }
                if ((this.map.GetComponent<MapComp_SpaceMap>() as MapComp_SpaceMap).isSpace)
                {
                    command_Action.Disable("Cannot Eject Crew Into Space...");
                }
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return command_Action;
            }
			if (this.pawn.drafter == null) this.pawn.drafter = new Pawn_DraftController(this.pawn);
			Command_Toggle command_Toggle = new Command_Toggle();
			command_Toggle.hotKey = KeyBindingDefOf.Command_ColonistDraft;
			command_Toggle.isActive = (() => this.pawn.drafter.Drafted);
			command_Toggle.toggleAction = delegate ()
			{
				this.pawn.drafter.Drafted = !this.pawn.drafter.Drafted;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
				if (this.pawn.drafter.Drafted)
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
				}
			};
			command_Toggle.defaultDesc = "CommandToggleDraftDesc".Translate();
			command_Toggle.icon = TexCommand.Draft;
			command_Toggle.turnOnSound = SoundDefOf.DraftOn;
			command_Toggle.turnOffSound = SoundDefOf.DraftOff;
			command_Toggle.groupKeyIgnoreContent = 81729172;
			command_Toggle.defaultLabel = (this.pawn.drafter.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate();
			if (this.pawn.Downed)
			{
				command_Toggle.Disable("IsIncapped".Translate(this.pawn.LabelShort, this.pawn));
			}
			if (this.pawn.Deathresting)
			{
				command_Toggle.Disable("IsDeathresting".Translate(this.pawn.Named("PAWN")));
			}
			if (!Overseen)
			{
				command_Toggle.Disable("Must have overseer inside");
			}
			if (!this.pawn.drafter.Drafted)
			{
				command_Toggle.tutorTag = "Draft";
			}
			else
			{
				command_Toggle.tutorTag = "Undraft";
			}
			yield return command_Toggle;
			*/
            //----
            yield break;
        }
        public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            return Enumerable.Empty<FloatMenuOption>();
        }
        public override void PostDraw()
        {
            if (!shields.depleted)
            {


                float num = Mathf.Lerp(0.5f, 1.5f, 1f);// shields.shields.CurLevel);//this.Props.minDrawSize, this.Props.maxDrawSize, this.energy);
                Vector3 vector = this.pawn.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = 4; // Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    //vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent), 0);
            }
            if (Find.Selector.SelectedPawns.Contains(this.parent)) DrawCommandRadius();




        }
        public override void PostExposeData()
        {

            base.PostExposeData();
            

        }
        //public static Material BubbleMaterial = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
    }


}