using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWorld;

namespace RimSpace
{
	[StaticConstructorOnStartup]
	public class CompLaunchShip : ThingComp
	{
		private CompTransporter cachedCompTransporter;
		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);
		public static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);
		private const float FuelPerTile = 2.25f;
		public CompProperties_LaunchShip Props => (CompProperties_LaunchShip)this.props;
		public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>();
		public CompSpaceship Spaceship => parent.GetComp<CompSpaceship>();
		private static List<Thing> tmpActiveDropPods = new List<Thing>();

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
					Log.Message("CompLaunchShip.CompGetGizmosExtra: 3");
					this.TryLaunch();
				};
				bool flag = true;
				foreach(var item in this.Transporter.innerContainer.ToList().FindAll(s => s is Pawn))
                {
					if ((item as Pawn).health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant)) flag = false;
                }


				if (flag) { command_Action.Disable("Needs Pilot with Mechlink Implant"); }
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




		
		public void TryLaunch()
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
			
			Current.Game.GetComponent<GameComp_StarSystem>().makeSystemMap();
			Map map = this.parent.Map;
			int num = (int)GenMath.SphericalDistance(Find.WorldGrid.GetTileCenter(this.parent.Tile).normalized, Current.Game.GetComponent<GameComp_StarSystem>().StarSystem.DrawPos.normalized);
			
			this.Transporter.TryRemoveLord(map);
			int groupID = this.Transporter.groupID;
			float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
			

			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				
				Log.Message("TryLaunch 0: ");
				CompTransporter compTransporter = transportersInGroup[i];
				Log.Message("TryLaunch 1: ");
				WorldObject_Vessel vessel = WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("TravelingSpaceShip"))as WorldObject_Vessel;
				Log.Message("TryLaunch 2: "+ vessel.def.defName);
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				Log.Message("TryLaunch 3: ");
				ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
				activeDropPod.Contents = new ActiveDropPodInfo();
				activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				Log.Message("TryLaunch 4: " + this.parent.Map.Tile.ToString());

				vessel.Tile = this.parent.Map.Tile;

				Log.Message("TryLaunch 5: ");
				vessel.SetFaction(Faction.OfPlayer);
				Log.Message("TryLaunch 6: ");
				vessel.destinationTile = 1;// this.destinationTile;
				Log.Message("TryLaunch 7: ");
				vessel.arrivalAction = null; //this.arrivalAction;
				Log.Message("TryLaunch 8: ");
				Find.WorldObjects.Add(vessel);
				Log.Message("TryLaunch 9: ");
				vessel.AddPod(activeDropPod.Contents, true);
				Log.Message("TryLaunch 10: ");
				this.parent.DeSpawn(DestroyMode.Vanish);





				/*
				CompLaunchShip.tmpActiveDropPods.Clear();
				CompLaunchShip.tmpActiveDropPods.AddRange(parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));
				for (int ind = 0; ind < CompLaunchShip.tmpActiveDropPods.Count; ind++)
				{
					SpaceshipLeaving spaceshipLeaving = CompLaunchShip.tmpActiveDropPods[ind] as SpaceshipLeaving;

					if (spaceshipLeaving != null && spaceshipLeaving.groupID == groupID)
					{
						spaceshipLeaving.alreadyLeft = true;
						vessel.AddPod(spaceshipLeaving.Contents, true);
						spaceshipLeaving.Contents = null;
						spaceshipLeaving.Destroy(DestroyMode.Vanish);
					}
				}
				*/
			}
			CameraJumper.TryShowWorld();//.TryJump(new GlobalTargetInfo(starSystem.SystemMap.Center, starSystem.SystemMap));//.TryHideWorld();



		}


		/*
				CompTransporter compTransporter = transportersInGroup[i];
				
				//Building fuelingPortSource = compTransporter.Launchable.FuelingPortSource;
				
				//if (fuelingPortSource != null)
				{
					//fuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(amount); Log.Message("TryLaunch 6");SpaceshipLeaving
				}
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				
				ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
				activeDropPod.Contents = new ActiveDropPodInfo();
				activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);

				Log.Message("TryLaunch 0: ");

				//SpaceshipLeaving sf = SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod) as SpaceshipLeaving;
				// + sf.ToString());
				SpaceshipLeaving ShipLeaving = ThingMaker.MakeThing(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, null) as SpaceshipLeaving;
				Log.Message("TryLaunch 1");
				//Log.Message("TryLaunch 6" + ShipLeavingA.ToString());
				//SpaceshipLeaving ShipLeaving = ShipLeavingA as SpaceshipLeaving;
				//if (ShipLeaving == null) { ShipLeaving = new SpaceshipLeaving(); Log.Message("TryLaunch 2"); }

				if (activeDropPod != null && !ShipLeaving.innerContainer.TryAdd(activeDropPod, true))
				{
					Log.Error("Could not add " + activeDropPod.ToStringSafe<Thing>() + " to a skyfaller.");
					activeDropPod.Destroy(DestroyMode.Vanish);
				}
				Log.Message("TryLaunch 2");




				
				

				ShipLeaving.groupID = groupID;
								Log.Message("TryLaunch 3 ");
				//ShipLeaving.destinationTile = 2;
				//ShipLeaving.arrivalAction = null;
				ShipLeaving.worldObjectDef = DefDatabase<WorldObjectDef>.GetNamed("TravelingSpaceShip");// WorldObjectDefOf.TravelingTransportPods;
				Log.Message("TryLaunch 4 ");
				compTransporter.CleanUpLoadingVars(map);
				Log.Message("TryLaunch 5 ");
				compTransporter.parent.Destroy(DestroyMode.Vanish);
				Log.Message("TryLaunch 6 ");
				GenSpawn.Spawn(ShipLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
				*/

		/*
		if (!this.parent.Spawned)
		{
			Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
			return;
		}

		Map map = this.parent.Map;
		int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);

		this.Transporter.TryRemoveLord(map);
		int groupID = this.Transporter.groupID;
		float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
		for (int i = 0; i < transportersInGroup.Count; i++)
		{
			CompTransporter compTransporter = transportersInGroup[i];
			Building fuelingPortSource = compTransporter.Launchable.FuelingPortSource;
			if (fuelingPortSource != null)
			{
				fuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
			}
			ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
			ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
			activeDropPod.Contents = new ActiveDropPodInfo();
			activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
			FlyShipLeaving flyShipLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
			flyShipLeaving.groupID = groupID;
			flyShipLeaving.destinationTile = destinationTile;
			flyShipLeaving.arrivalAction = arrivalAction;
			flyShipLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
			compTransporter.CleanUpLoadingVars(map);
			compTransporter.parent.Destroy(DestroyMode.Vanish);
			GenSpawn.Spawn(flyShipLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
		}
		CameraJumper.TryHideWorld();
		*/

	}
}
