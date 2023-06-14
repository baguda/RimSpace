using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWorld;
using MapToolBag;
using System.Diagnostics;

namespace RimSpace
{
	[StaticConstructorOnStartup]
	public class CompLaunchShip : ThingComp
	{
		private CompTransporter cachedCompTransporter;
		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);
		public static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);
		public int curPlanetID;
		private const float FuelPerTile = 2.25f;
		public CompProperties_LaunchShip Props => (CompProperties_LaunchShip)this.props;
		public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>();
		public CompSpaceship Spaceship => parent.GetComp<CompSpaceship>();
		private static List<Thing> tmpActiveDropPods = new List<Thing>();
		public ShipLandingArea homePort => ShipLandingBeaconUtility.GetLandingZones(parent.Map).Find(s=>s.MyRect.Contains(parent.Position));

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			if (this.Transporter.LoadingInProgressOrReadyToLaunch)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandLaunchGroup".Translate();
				command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
				command_Action.icon = CompLaunchShip.LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;
				command_Action.action = delegate ()
				{
					if (this.Transporter.AnyInGroupHasAnythingLeftToLoad)
					{

						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate
							(this.Transporter.FirstThingLeftToLoadInGroup.LabelCapNoCount, this.Transporter.FirstThingLeftToLoadInGroup), 
							new Action(this.TryLaunch), false, null, WindowLayer.Dialog));
						return;
					}
					this.TryLaunch();
				};
				bool flag = true;
				foreach(var item in this.Transporter.innerContainer.ToList().FindAll(s => s is Pawn))
                {
					if ((item as Pawn).health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant)) flag = false;
                }


				if (flag) { command_Action.Disable("Needs Pilot with Mechlink Implant"); }
				//if (!starSystem.hasStation && !starSystem.hasLagrange) { command_Action.Disable("No Space Station to Target"); }
				yield return command_Action;
			}

			if (this.Transporter.LoadingInProgressOrReadyToLaunch)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandLaunchGroup".Translate();
				command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
				command_Action.icon = CompLaunchShip.LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;
				command_Action.action = delegate ()
				{
					if (this.Transporter.AnyInGroupHasAnythingLeftToLoad)
					{

						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate
							(this.Transporter.FirstThingLeftToLoadInGroup.LabelCapNoCount, this.Transporter.FirstThingLeftToLoadInGroup),
							new Action(this.TryLaunch), false, null, WindowLayer.Dialog));
						return;
					}
					this.TryLaunch();
				};
				bool flag = true;
				foreach (var item in this.Transporter.innerContainer.ToList().FindAll(s => s is Pawn))
				{
					if ((item as Pawn).health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant)) flag = false;
				}


				if (flag) { command_Action.Disable("Needs Pilot with Mechlink Implant"); }
				if (!starSystem.hasStation) { command_Action.Disable("No Space Station to Target"); }
				yield return command_Action;
			}

			yield break;
		}
		/*

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{

			{



				if (!this.parent.Map.GetComponent<MapComp_SpaceMap>().isSpace)
				{
					var CA3 = new Command_Action

					{
						defaultLabel = "testLaunch",
						icon = ContentFinder<Texture2D>.Get("Ships/source/D2", true),
						defaultDesc = "Test for Spacer Comp",
						action = new Action(delegate ()
						{
							var GC = Current.Game.GetComponent<GameComp_StarSystem>();

							Map newSpaceMap = GC.makeSystemMap();

							var thingInd = this.parent;
							thingInd.DeSpawn(DestroyMode.Vanish);
							GenSpawn.Spawn(thingInd, (Current.Game.GetComponent<GameComp_StarSystem>() as GameComp_StarSystem).getHomePlanet.Location, newSpaceMap, WipeMode.Vanish);


						})
					};
					yield return CA3;
				}
				else
				{
					var CA3 = new Command_Action

					{
						defaultLabel = "testLand",
						icon = ContentFinder<Texture2D>.Get("Ships/source/D1", true),
						defaultDesc = "Test for Spacer Comp",
						action = new Action(delegate ()
						{

							var WO = Find.WorldObjects.AllWorldObjects.Find(m => (m is MapParent) && (m as MapParent).HasMap && m.Faction.IsPlayer) as MapParent;
							Map HomeMap = WO.Map;

							var thingInd = this.parent;
							thingInd.DeSpawn(DestroyMode.Vanish);
							GenSpawn.Spawn(thingInd, HomeMap.Center, HomeMap, WipeMode.Vanish);


						})
					};
					yield return CA3;
				}






				//this.pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, this.pawn);

			}
			yield break;
		}
		*/
		public override string CompInspectStringExtra()
		{
			return base.CompInspectStringExtra();

		}
		public CompTransporter Transporter
		{
			get
			{
				if (this.cachedCompTransporter == null)
				{
					this.cachedCompTransporter = this.parent.GetComp<CompTransporter>();
				}
				return this.cachedCompTransporter;
			}
		}

		public bool AllInGroupConnectedToFuelingPort
		{
			get
			{
				
				List<CompTransporter> transportersInGroup = this.Transporter.TransportersInGroup(this.parent.Map);
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					//if (!transportersInGroup[i].Launchable.ConnectedToFuelingPort)
					{
						//return false;
					}
				}
				return true;
			}
		}

		public bool AllFuelingPortSourcesInGroupHaveAnyFuel
		{
			get
			{
				List<CompTransporter> transportersInGroup = this.Transporter.TransportersInGroup(parent.Map);
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					//if (!transportersInGroup[i].Launchable.FuelingPortSourceHasAnyFuel)
					{
						//return false;
					}
				}
				return true;
			}
		}

        public override void PostExposeData()
        {
			Scribe_Values.Look<int>(ref curPlanetID, "curPlanetID");
            base.PostExposeData();
        }



        public void TryLaunch()
		{
			if (Prefs.DevMode) Log.Message(this.ToString() + "." + (new StackTrace().GetFrame(0).GetMethod().Name) + ": 0");
			Log.Message("CompLaunchShip.TryLaunch: 1");
			if (!this.parent.Spawned)
			{
				Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
				return;
			}
			Log.Message("CompLaunchShip.TryLaunch: 2");
			List<CompTransporter> transportersInGroup = this.Transporter.TransportersInGroup(this.parent.Map);
			Log.Message("CompLaunchShip.TryLaunch: 3");
			if (transportersInGroup == null)
			{
				Log.Error("Tried to launch " + this.parent + ", but it's not in any group.");
				return;
			}
			Log.Message("CompLaunchShip.TryLaunch: 4");
			Map map = this.parent.Map;
			Log.Message("CompLaunchShip.TryLaunch: 4.1");
			starSystem.GenLagrangePoint("LagrangePoint", TileHandlerUtility.getWorldEdgeTiles().RandomElement(), Faction.OfPlayer, "L1");

			Log.Message("CompLaunchShip.TryLaunch: 4.2");
			starSystem.LocalMap = starSystem.makeSpaceMap(starSystem.LagrangePoint);
			//int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);
			Log.Message("CompLaunchShip.TryLaunch: 5");
			this.Transporter.TryRemoveLord(map);
			Log.Message("CompLaunchShip.TryLaunch: 6");
			int groupID = this.Transporter.groupID;
			Log.Message("CompLaunchShip.TryLaunch: 7");
			//float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				Log.Message("CompLaunchShip.TryLaunch: 8");
				CompTransporter compTransporter = transportersInGroup[i];
				Log.Message("CompLaunchShip.TryLaunch: 9");
				//Building fuelingPortSource = compTransporter.Launchable.FuelingPortSource;
				Log.Message("CompLaunchShip.TryLaunch: 10");
				//if (fuelingPortSource != null)
				{
					//fuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
				}
				Log.Message("CompLaunchShip.TryLaunch: 11");
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				Log.Message("CompLaunchShip.TryLaunch: 12");
				ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
				Log.Message("CompLaunchShip.TryLaunch: 13");
				activeDropPod.Contents = new ActiveDropPodInfo();
				Log.Message("CompLaunchShip.TryLaunch: 14");
				activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				Log.Message("CompLaunchShip.TryLaunch: 15");
				SpaceshipLeaving flyShipLeaving = (SpaceshipLeaving)SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
				Log.Message("CompLaunchShip.TryLaunch: 16");
				flyShipLeaving.groupID = groupID;

				try
				{
					Log.Message("CompLaunchShip.TryLaunch: 16.1 " + this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).DefName + ": " 
								+ this.starSystem.LocapMapComp.getPlanet(curPlanetID).isHome.ToString());
					flyShipLeaving.curPlanetID = this.curPlanetID;   
				}
				catch
				{

					this.curPlanetID = starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getHomePlanet.planetThingID;
					Log.Message("CompLaunchShip.TryLaunch: 16.12 " + this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).DefName + ": " 
								+ this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).isHome.ToString());
					flyShipLeaving.curPlanetID = this.curPlanetID;
				}
				Log.Message("CompLaunchShip.TryLaunch: 16.2 " + this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(flyShipLeaving.curPlanetID).DefName + ": " 
											+ this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(flyShipLeaving.curPlanetID).isHome.ToString());
				Log.Message("CompLaunchShip.TryLaunch: 17");
				//flyShipLeaving.destinationTile = destinationTile;
				//flyShipLeaving.arrivalAction = arrivalAction;
				flyShipLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
				Log.Message("CompLaunchShip.TryLaunch: 18");
				compTransporter.CleanUpLoadingVars(map);
				if (this.homePort != null)
				{
					//starSystem.homePortRect = this.homePort.MyRect;
					starSystem.homePortMap = this.parent.Map;
					starSystem.homePortPoint = this.homePort.CenterCell;
				}
				Log.Message("CompLaunchShip.TryLaunch: 19");
				compTransporter.parent.Destroy(DestroyMode.Vanish);
				Log.Message("CompLaunchShip.TryLaunch: 20");
				GenSpawn.Spawn(flyShipLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);

			}
			CameraJumper.TryHideWorld();
			/*
			if (false)
			{
				if (!this.parent.Spawned)
				{
					Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
					return;
				}
				List<CompTransporter> transportersInGroup = this.Transporter.TransportersInGroup(this.parent.Map);
				if (transportersInGroup == null)
				{
					Log.Error("Tried to launch " + this.parent + ", but it's not in any group.");
					return;
				}
				if (!this.Transporter.LoadingInProgressOrReadyToLaunch || !this.AllInGroupConnectedToFuelingPort || !this.AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					return;
				}
				//Current.Game.GetComponent<GameComp_StarSystem>().makeStarMap();
				Map map = this.parent.Map;
				starSystem.GenLagrangePoint("LagrangePoint", TileHandlerUtility.getWorldEdgeTiles().RandomElement(), Faction.OfPlayer, "L1");
				int num = (int)GenMath.SphericalDistance(Find.WorldGrid.GetTileCenter(this.parent.Tile).normalized, starSystem.LagrangePoint.DrawPos.normalized);
				this.Transporter.TryRemoveLord(map);
				int groupID = this.Transporter.groupID;
				float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					CompTransporter compTransporter = transportersInGroup[i];
					WorldObject_SpaceShuttle vessel =
						WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.
						GetNamed("TravelingSpaceShip")) as WorldObject_SpaceShuttle;
					ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
					ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
					activeDropPod.Contents = new ActiveDropPodInfo();
					activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
					vessel.Tile = this.parent.Map.Tile;
					vessel.SetFaction(Faction.OfPlayer);
					vessel.toSpace = true;
					vessel.targetObject = starSystem.LagrangePoint;// this.destinationTile;
					vessel.arrivalAction = null; //this.arrivalAction;
					starSystem.homePortRect = this.homePort.MyRect;
					starSystem.homePortMap = map;
					starSystem.homePortPoint = this.homePort.CenterCell;
					Find.WorldObjects.Add(vessel);
					vessel.AddPod(activeDropPod.Contents, true);
					this.parent.DeSpawn(DestroyMode.Vanish);
				}
				CameraJumper.TryShowWorld();//.TryJump(new GlobalTargetInfo(starSystem.SystemMap.Center, starSystem.SystemMap));//.TryHideWorld();
			}
            else if (true)
            {
				if (Prefs.DevMode) Log.Message(this.ToString() + "." + (new StackTrace().GetFrame(0).GetMethod().Name) + ": 0");
				Log.Message("CompLaunchShip.TryLaunch: 1");
				if (!this.parent.Spawned)
				{
					Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
					return;
				}
				Log.Message("CompLaunchShip.TryLaunch: 2");
				List<CompTransporter> transportersInGroup = this.Transporter.TransportersInGroup(this.parent.Map);
				Log.Message("CompLaunchShip.TryLaunch: 3");
				if (transportersInGroup == null)
				{
					Log.Error("Tried to launch " + this.parent + ", but it's not in any group.");
					return;
				}
				Log.Message("CompLaunchShip.TryLaunch: 4");
				Map map = this.parent.Map;
				Log.Message("CompLaunchShip.TryLaunch: 4.1");
				starSystem.GenLagrangePoint("LagrangePoint", TileHandlerUtility.getWorldEdgeTiles().RandomElement(), Faction.OfPlayer, "L1");

				Log.Message("CompLaunchShip.TryLaunch: 4.2");
				starSystem.LocalMap = starSystem.makeSpaceMap(starSystem.LagrangePoint);
				//int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);
				Log.Message("CompLaunchShip.TryLaunch: 5");
				this.Transporter.TryRemoveLord(map);
				Log.Message("CompLaunchShip.TryLaunch: 6");
				int groupID = this.Transporter.groupID;
				Log.Message("CompLaunchShip.TryLaunch: 7");
				//float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					Log.Message("CompLaunchShip.TryLaunch: 8");
					CompTransporter compTransporter = transportersInGroup[i];
					Log.Message("CompLaunchShip.TryLaunch: 9");
					//Building fuelingPortSource = compTransporter.Launchable.FuelingPortSource;
					Log.Message("CompLaunchShip.TryLaunch: 10");
					//if (fuelingPortSource != null)
					{
						//fuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
					}
					Log.Message("CompLaunchShip.TryLaunch: 11");
					ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
					Log.Message("CompLaunchShip.TryLaunch: 12");
					ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
					Log.Message("CompLaunchShip.TryLaunch: 13");
					activeDropPod.Contents = new ActiveDropPodInfo();
					Log.Message("CompLaunchShip.TryLaunch: 14");
					activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
					Log.Message("CompLaunchShip.TryLaunch: 15");
					SpaceshipLeaving flyShipLeaving = (SpaceshipLeaving)SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
					Log.Message("CompLaunchShip.TryLaunch: 16");
					flyShipLeaving.groupID = groupID;
					
                    try
                    {
						Log.Message("CompLaunchShip.TryLaunch: 16.1 " + this.curPlanet.DefName + ": " + this.curPlanet.isHome.ToString());
						flyShipLeaving.curPlanet = this.curPlanet;
                    }
                    catch
                    {

						this.curPlanet = starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getHomePlanet;
						Log.Message("CompLaunchShip.TryLaunch: 16.12 " + this.curPlanet.DefName + ": " + this.curPlanet.isHome.ToString());
						flyShipLeaving.curPlanet = this.curPlanet;
					}
					Log.Message("CompLaunchShip.TryLaunch: 16.2 " + flyShipLeaving.curPlanet.DefName + ": " + flyShipLeaving.curPlanet.isHome.ToString());
					Log.Message("CompLaunchShip.TryLaunch: 17");
					//flyShipLeaving.destinationTile = destinationTile;
					//flyShipLeaving.arrivalAction = arrivalAction;
					flyShipLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
					Log.Message("CompLaunchShip.TryLaunch: 18");
					compTransporter.CleanUpLoadingVars(map);
					if(this.homePort != null)
                    {
						starSystem.homePortRect = this.homePort.MyRect;
						starSystem.homePortMap = this.parent.Map;
						starSystem.homePortPoint = this.homePort.CenterCell;
                    }
					Log.Message("CompLaunchShip.TryLaunch: 19");
					compTransporter.parent.Destroy(DestroyMode.Vanish);
					Log.Message("CompLaunchShip.TryLaunch: 20");
					GenSpawn.Spawn(flyShipLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
				
				}
				CameraJumper.TryHideWorld();
			}
            else
			{
				if (Prefs.DevMode) Log.Message(this.ToString() + "." + (new StackTrace().GetFrame(0).GetMethod().Name) + ": 0");
				Log.Message("CompLaunchShip.TryLaunch: 1");
				if (!this.parent.Spawned)
				{
					Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
					return;
				}
				Log.Message("CompLaunchShip.TryLaunch: 2");
				List<CompTransporter> transportersInGroup = this.Transporter.TransportersInGroup(this.parent.Map);
				Log.Message("CompLaunchShip.TryLaunch: 3");
				if (transportersInGroup == null)
				{
					Log.Error("Tried to launch " + this.parent + ", but it's not in any group.");
					return;
				}
				Log.Message("CompLaunchShip.TryLaunch: 4");
				Map map = this.parent.Map;
				Log.Message("CompLaunchShip.TryLaunch: 4.1");
				starSystem.GenLagrangePoint("LagrangePoint", TileHandlerUtility.getWorldEdgeTiles().RandomElement(), Faction.OfPlayer, "L1");

				//int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);
				Log.Message("CompLaunchShip.TryLaunch: 5");
				this.Transporter.TryRemoveLord(map);
				Log.Message("CompLaunchShip.TryLaunch: 6");
				int groupID = this.Transporter.groupID;
				Log.Message("CompLaunchShip.TryLaunch: 7");
				//float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					Log.Message("CompLaunchShip.TryLaunch: 8");
					CompTransporter compTransporter = transportersInGroup[i];
					Log.Message("CompLaunchShip.TryLaunch: 9");
					//Building fuelingPortSource = compTransporter.Launchable.FuelingPortSource;
					Log.Message("CompLaunchShip.TryLaunch: 10");
					//if (fuelingPortSource != null)
					{
						//fuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
					}
					Log.Message("CompLaunchShip.TryLaunch: 11");
					ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
					Log.Message("CompLaunchShip.TryLaunch: 12");
					ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
					Log.Message("CompLaunchShip.TryLaunch: 13");
					activeDropPod.Contents = new ActiveDropPodInfo();
					Log.Message("CompLaunchShip.TryLaunch: 14");
					activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
					Log.Message("CompLaunchShip.TryLaunch: 15");
					SpaceshipLeaving flyShipLeaving = (SpaceshipLeaving)SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
					Log.Message("CompLaunchShip.TryLaunch: 16");
					flyShipLeaving.groupID = groupID;
					flyShipLeaving.curPlanet = curPlanet;
					Log.Message("CompLaunchShip.TryLaunch: 17");
					//flyShipLeaving.destinationTile = destinationTile;
					//flyShipLeaving.arrivalAction = arrivalAction;
					flyShipLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
					Log.Message("CompLaunchShip.TryLaunch: 18");
					compTransporter.CleanUpLoadingVars(map);

					starSystem.homePortRect = this.homePort.MyRect;
					starSystem.homePortMap = this.parent.Map;
					starSystem.homePortPoint = this.homePort.CenterCell;
					Log.Message("CompLaunchShip.TryLaunch: 19");
					compTransporter.parent.Destroy(DestroyMode.Vanish);
					Log.Message("CompLaunchShip.TryLaunch: 20");
					GenSpawn.Spawn(flyShipLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);

				}
				CameraJumper.TryHideWorld();

			}
		
			*/
		}



	}
}
