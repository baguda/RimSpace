using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using RimWorld;
using System.Linq;

namespace RimSpace
{
    public class SpaceshipLeaving : Skyfaller, IActiveDropPod, IThingHolder
    {
        public int groupID = -1;
        public int destinationTile = -1;
        public TransportPodsArrivalAction arrivalAction;
        public bool createWorldObject = true;
        public WorldObjectDef worldObjectDef;
        public bool alreadyLeft;
        public int curPlanetID;

        public SpaceshipLeaving() { }
        private static List<Thing> tmpActiveDropPods = new List<Thing>();
        public ActiveDropPodInfo Contents
        {
            get
            {
                return ((ActiveDropPod)this.innerContainer[0]).Contents;
            }
            set
            {
                ((ActiveDropPod)this.innerContainer[0]).Contents = value;
            }
        }
        public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>();

        public IEnumerable<Pawn> Pawns
        {
            get
            {
                foreach (ActiveDropPod pod in this.innerContainer)
                {
                    foreach (Thing thing in pod.Contents.GetDirectlyHeldThings())
                    {
                        Log.Message("SpaceshipLeaving.Pawns: -" + thing.ToString());
                        if (thing is Pawn)
                        {
                            Log.Message("SpaceshipLeaving.Pawns: " + thing.ToString());
                            yield return thing as Pawn;
                        }
                    }

                }
                yield break;
                /*
                int num;
                for (int i = 0; i < this.innerContainer.Count; i = num + 1)
                {
                    ThingOwner things = this.innerContainer;
                    for (int j = 0; j < things.Count; j = num + 1)
                    {
                        Pawn pawn = things[j] as Pawn;
                        if (pawn != null)
                        {
                            yield return pawn;
                        }
                        num = j;
                    }
                    things = null;
                    num = i;
                }
                yield break;*/
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
            Scribe_Deep.Look<TransportPodsArrivalAction>(ref this.arrivalAction, "arrivalAction", Array.Empty<object>());
            Scribe_Values.Look<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
            Scribe_Values.Look<bool>(ref this.createWorldObject, "createWorldObject", true, false);
            Scribe_Defs.Look<WorldObjectDef>(ref this.worldObjectDef, "worldObjectDef");
        }
        protected override void LeaveMap()
        {
            /*
			//Current.Game.GetComponent<GameComp_StarSystem>().makeStarMap();
			Map map = this.Map;
			starSystem.GenLagrangePoint("LagrangePoint", TileHandlerUtility.getWorldEdgeTiles().RandomElement(), Faction.OfPlayer, "L1");
			int num = (int)GenMath.SphericalDistance(Find.WorldGrid.GetTileCenter(this.parent.Tile).normalized, starSystem.LagrangePoint.DrawPos.normalized);
			//this.Transporter.TryRemoveLord(map);
			//int groupID = this.Transporter.groupID;
			//float amount = Mathf.Max(CompLaunchable.FuelNeededToLaunchAtDist((float)num), 1f);
			//for (int i = 0; i < transportersInGroup.Count; i++)
			{
				CompTransporter compTransporter = transportersInGroup[i];
				WorldObject_SpaceShuttle vessel = WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("TravelingSpaceShip")) as WorldObject_SpaceShuttle;
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
				this.DeSpawn(DestroyMode.Vanish);
			}
			CameraJumper.TryShowWorld();//.TryJump(new GlobalTargetInfo(starSystem.SystemMap.Center, starSystem.SystemMap));//.TryHideWorld();
			*/


            Log.Message("SpaceshipLeaving.LeaveMap: " + this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).DefName + ": " +
                                this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).isHome.ToString());
            if (this.alreadyLeft || !this.createWorldObject)
            {
                if (this.Contents != null)
                {
                    using (IEnumerator<Thing> enumerator = ((IEnumerable<Thing>)this.Contents.innerContainer).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Pawn pawn;
                            if ((pawn = (enumerator.Current as Pawn)) != null)
                            {
                                pawn.ExitMap(false, Rot4.Invalid);
                            }
                        }
                    }
                    this.Contents.innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.QuestLogic);
                }
                base.LeaveMap();
                return;
            }
            if (this.groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + this.groupID);
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            if (this.destinationTile < 0)
            {
                //Log.Error("Drop pod left the map, but its destination tile is " + this.destinationTile);
                //this.Destroy(DestroyMode.Vanish);
                //return;
            }
            Lord lord = TransporterUtility.FindLord(this.groupID, base.Map);
            if (lord != null)
            {
                base.Map.lordManager.RemoveLord(lord);
            }




            if (this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).isHome)
            {

                Log.Message("SpaceshipLeaving.LeaveMap: curPlanet.isHome");
                WorldObject_SpaceShuttle vessel =
                            WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.
                            GetNamed("TravelingSpaceShip")) as WorldObject_SpaceShuttle;
                vessel.Tile = base.Map.Tile;
                vessel.SetFaction(Faction.OfPlayer);

                vessel.toSpace = true;
                vessel.arrivalAction = null;
                Find.WorldObjects.Add(vessel);
                SpaceshipLeaving.tmpActiveDropPods.Clear();
                SpaceshipLeaving.tmpActiveDropPods.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));
                for (int i = 0; i < SpaceshipLeaving.tmpActiveDropPods.Count; i++)
                {
                    SpaceshipLeaving spaceshipLeaving = SpaceshipLeaving.tmpActiveDropPods[i] as SpaceshipLeaving;
                    if (spaceshipLeaving != null && spaceshipLeaving.groupID == this.groupID)
                    {
                        spaceshipLeaving.alreadyLeft = true;
                        vessel.AddPod(spaceshipLeaving.Contents, true);
                        spaceshipLeaving.Contents = null;
                        spaceshipLeaving.Destroy(DestroyMode.Vanish);
                    }
                }
            }
            else
            {

                Log.Message("SpaceshipLeaving.LeaveMap: !curPlanet.isHome");
                Pawn pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("AstroMech_SpaceShuttle"));

                if (pawn.GetComps<CompSpaceship>() == null)
                {
                    Log.Error("DoArrivalAction: pawn generated didn't have CompSpaceship");
                    return;
                }
                Log.Message("SpaceshipLeaving.LeaveMap: 1");
                pawn.SetFaction(Faction.OfPlayer);
                Log.Message("SpaceshipLeaving.LeaveMap: 2");
                Pawn Pilot = this.Pawns.ToList().Find(s => s.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
                Log.Message("SpaceshipLeaving.LeaveMap: 3");
                //this.Pawns.ToList().ForEach(s => Find.WorldPawns.RemovePawn(s));
                Log.Message("SpaceshipLeaving.LeaveMap: 4");
                GenSpawn.Spawn(pawn, this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).Location, starSystem.LocalMap);
                Log.Message("SpaceshipLeaving.LeaveMap: 5");
                CameraJumper.TryJump(this.starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getPlanet(curPlanetID).Location, starSystem.LocalMap);
                Log.Message("SpaceshipLeaving.LeaveMap: 6");
                foreach (ActiveDropPod pod in this.innerContainer)
                {

                    Log.Message("SpaceshipLeaving.LeaveMap: 7");
                    ThingOwner directlyHeldThings = pod.Contents.GetDirectlyHeldThings();
                    pawn.GetComp<CompSpaceship>().innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
                }
                if (Pilot != null)
                {

                    Log.Message("SpaceshipLeaving.LeaveMap: 8");
                    Pilot.mechanitor.AssignPawnControlGroup(pawn);
                    Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
                }
                else
                {

                    Log.Message("SpaceshipLeaving.LeaveMap: 9");
                    Pilot = this.Pawns.First();
                    Pilot.health.AddHediff(HediffDefOf.MechlinkImplant);
                    Pilot.mechanitor.AssignPawnControlGroup(pawn);
                    Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
                }
                //this.pods.Clear();
                this.Destroy();
            }

        }


    }
}
