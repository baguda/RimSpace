using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using RimWorld;
using System.Linq;

namespace RimSpace
{
	[StaticConstructorOnStartup]
	public abstract class WorldObject_VesselX : WorldObject, IThingHolder
	{
		public const float TravelSpeed = 0.00015f;
		public IntVec3 destinationCell = IntVec3.Invalid;
		public int destinationTile = -1;
		public int initialTile = -1;
		public float traveledPct;
		protected ThingOwner innerContainer;
		public TransportPodsArrivalAction arrivalAction;
		private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();
		public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>();

		public WorldObject_VesselX() { }


		public Vector3 Start => Find.WorldGrid.GetTileCenter(this.initialTile);
		public Vector3 End => starSystem.SpaceStation.DrawPos;
		public override Vector3 DrawPos => Vector3.Slerp(this.Start, this.End, this.traveledPct);
		public ThingOwner GetDirectlyHeldThings() => this.innerContainer;
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
		public float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = this.Start;
				Vector3 end = this.End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return TravelSpeed / num;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Values.Look<IntVec3>(ref this.destinationCell, "destinationCell", default(IntVec3), false);
			Scribe_Values.Look<int>(ref this.initialTile, "initialTile", 0, false);
			Scribe_Values.Look<float>(ref this.traveledPct, "traveledPct", 0f, false);
		}

		public override void PostAdd()
		{
			base.PostAdd();
			this.initialTile = base.Tile;
		}

		public override void Tick()
		{
			base.Tick();
			this.innerContainer.ThingOwnerTick(true);
			this.traveledPct += this.TraveledPctStepPerTick;
			if (this.traveledPct >= 1f)
			{
				this.traveledPct = 1f;
				this.Arrived();
			}
		}

		public virtual void Arrived()
		{
		}


		public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
		{
			contents.parent = this;
			this.pods.Add(contents);
			ThingOwner innerContainer = contents.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Pawn pawn = innerContainer[i] as Pawn;
				if (pawn != null && !pawn.IsWorldPawn())
				{
					if (!base.Spawned)
					{
						Log.Warning("Passing pawn " + pawn + " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
					}
					if (justLeftTheMap)
					{
						pawn.ExitMap(false, Rot4.Invalid);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					}
				}
			}
			contents.savePawnsWithReferenceMode = true;
		}

	}

}


namespace RimSpace
{
	public class WorldObject_Vessel : WorldObject, IThingHolder
	{

		public Action arrivalAction;
		public List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();
		private bool arrived;
		private int initialTile = -1;
		private float traveledPct;
		public float TravelSpeed = 0.00025f;
		public  WorldObject targetObject;
		

		public virtual Vector3 targetDestination => targetObject.DrawPos;

		public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>();
		public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;
		public virtual Vector3 Start => Find.WorldGrid.GetTileCenter(this.initialTile);
		public virtual Vector3 End => targetDestination;
		public override Vector3 DrawPos => Vector3.Slerp(this.Start, this.End, this.traveledPct);
		public override bool ExpandingIconFlipHorizontal => GenWorldUI.WorldToUIPosition(this.Start).x > GenWorldUI.WorldToUIPosition(this.End).x;
		public override float ExpandingIconRotation
		{
			get
			{
				if (!this.def.rotateGraphicWhenTraveling)
				{
					return base.ExpandingIconRotation;
				}
				Vector2 vector = GenWorldUI.WorldToUIPosition(this.Start);
				Vector2 vector2 = GenWorldUI.WorldToUIPosition(this.End);
				float num = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x) * 57.29578f;
				if (num > 180f)
				{
					num -= 180f;
				}
				return num + 90f;
			}
		}
		private float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = this.Start;
				Vector3 end = this.End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return TravelSpeed / num;
			}
		}
		private bool PodsHaveAnyPotentialCaravanOwner
		{
			get
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					ThingOwner innerContainer = this.pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && CaravanUtility.IsOwner(pawn, base.Faction))
						{
							return true;
						}
					}
				}
				return false;
			}
		}
		public bool PodsHaveAnyFreeColonist
		{
			get
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					ThingOwner innerContainer = this.pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && pawn.IsColonist && pawn.HostFaction == null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}
		public IEnumerable<Pawn> Pawns
		{
			get
			{
				int num;
				for (int i = 0; i < this.pods.Count; i = num + 1)
				{
					ThingOwner things = this.pods[i].innerContainer;
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
				yield break;
			}
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look<ActiveDropPodInfo>(ref this.pods, "pods", LookMode.Deep, Array.Empty<object>());
			//Scribe_Deep.Look<TransportPodsArrivalAction>(ref this.arrivalAction, "arrivalAction", Array.Empty<object>());
			Scribe_Values.Look<bool>(ref this.arrived, "arrived", false, false);
			Scribe_Values.Look<int>(ref this.initialTile, "initialTile", 0, false);
			Scribe_Values.Look<float>(ref this.traveledPct, "traveledPct", 0f, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					this.pods[i].parent = this;
				}
			}
		}
		public override void PostAdd()
		{
			base.PostAdd();
			this.initialTile = base.Tile;
		}
		public override void Tick()
		{
			base.Tick();
			this.traveledPct += this.TraveledPctStepPerTick;
			if (this.traveledPct >= 1f)
			{
				this.traveledPct = 1f;
				this.Arrived();
			}
		}
		public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
		{
			contents.parent = this;
			this.pods.Add(contents);
			ThingOwner innerContainer = contents.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Pawn pawn = innerContainer[i] as Pawn;
				if (pawn != null && !pawn.IsWorldPawn())
				{
					if (!base.Spawned)
					{
						Log.Warning("Passing pawn " + pawn + " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
					}
					if (justLeftTheMap)
					{
						pawn.ExitMap(false, Rot4.Invalid);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					}
				}
			}
			contents.savePawnsWithReferenceMode = true;
		}
		public bool ContainsPawn(Pawn p)
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				if (this.pods[i].innerContainer.Contains(p))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void Arrived()
		{
			if (this.arrived)
			{
				return;
			}
			this.arrived = true;
			if (arrivalAction != null) DoArrivalAction(arrivalAction);
		}
		/*
		private void DoArrivalAction()
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				this.pods[i].savePawnsWithReferenceMode = false;
				this.pods[i].parent = null;
			}
			Pawn pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("AstroMech_Hulk"));
			if(pawn.GetComps<CompSpaceship>() == null)
            {
				Log.Error("DoArrivalAction: pawn generated didn't have CompSpaceship");
				return;
            }
			pawn.SetFaction(Faction.OfPlayer);
			Pawn Pilot = this.Pawns.ToList().Find(s => s.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
			this.Pawns.ToList().ForEach(s => Find.WorldPawns.RemovePawn(s));
			GenSpawn.Spawn(pawn, starSystem.getHomePlanet.Location, starSystem.SystemMap);
			CameraJumper.TryJump(starSystem.getHomePlanet.Location, starSystem.SystemMap);
			foreach (var pod in this.pods)
            {
				ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
				pawn.GetComp<CompSpaceship>().innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
			}
			if (Pilot != null)
			{
				Pilot.mechanitor.AssignPawnControlGroup(pawn);
				Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
			}
			else
			{
				Pilot = this.Pawns.First();
				Pilot.health.AddHediff(HediffDefOf.MechlinkImplant);
				Pilot.mechanitor.AssignPawnControlGroup(pawn);
				Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
			}
			this.pods.Clear();
			this.Destroy();
		}
		*/
		/*
		public virtual void DoArrivalAction(PawnKindDef pawnKindDef, IntVec3 location, Map map, bool camJump = true)
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				this.pods[i].savePawnsWithReferenceMode = false;
				this.pods[i].parent = null;
			}
			Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef);
			if (pawn.GetComps<CompSpaceship>() == null)
			{
				Log.Error("DoArrivalAction: pawn generated did not have CompSpaceship");
				return;
			}
			pawn.SetFaction(Faction.OfPlayer);

			Pawn Pilot = this.Pawns.ToList().Find(s => s.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
			this.Pawns.ToList().ForEach(s => Find.WorldPawns.RemovePawn(s));

			GenSpawn.Spawn(pawn, location, map);
			if(camJump)CameraJumper.TryJump(location, map);

			foreach (var pod in this.pods)
			{
				ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
				pawn.GetComp<CompSpaceship>().innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
			}
			if (Pilot != null)
			{
				Pilot.mechanitor.AssignPawnControlGroup(pawn);
				Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
			}
			else
			{
				Pilot = this.Pawns.First();
				Pilot.health.AddHediff(HediffDefOf.MechlinkImplant);
				Pilot.mechanitor.AssignPawnControlGroup(pawn);
				Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
			}
			this.pods.Clear();
			this.Destroy();
		}
		*/
		public virtual void DoArrivalAction(Action ArrivalAction = null)
		{
			if (arrivalAction != null) DoArrivalAction(arrivalAction);
		}

			public ThingOwner GetDirectlyHeldThings() => null;
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
			for (int i = 0; i < this.pods.Count; i++)
			{
				outChildren.Add(this.pods[i]);
			}
		}
        public override void SpawnSetup()
        {
			base.SpawnSetup();
        }
        /*
                private void Arrived()
                {
                    if (this.arrived)
                    {
                        return;
                    }
                    this.arrived = true;

        if (this.arrivalAction == null || !this.arrivalAction.StillValid(this.pods.Cast<IThingHolder>(), this.destinationTile))
        {
            this.arrivalAction = null;
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].Tile == this.destinationTile)
                {
                    this.arrivalAction = new TransportPodsArrivalAction_LandInSpecificCell(maps[i].Parent, DropCellFinder.RandomDropSpot(maps[i], true));
                    break;
                }
            }
            if (this.arrivalAction == null)
            {
                if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(this.pods.Cast<IThingHolder>(), this.destinationTile))
                {
                    this.arrivalAction = new TransportPodsArrivalAction_FormCaravan();
                }
                else
                {
                    List<Caravan> caravans = Find.WorldObjects.Caravans;
                    for (int j = 0; j < caravans.Count; j++)
                    {
                        if (caravans[j].Tile == this.destinationTile && TransportPodsArrivalAction_GiveToCaravan.CanGiveTo(this.pods.Cast<IThingHolder>(), caravans[j]))
                        {
                            this.arrivalAction = new TransportPodsArrivalAction_GiveToCaravan(caravans[j]);
                            break;
                        }
                    }
                }
            }
        }

        if (this.arrivalAction != null && this.arrivalAction.ShouldUseLongEvent(this.pods, this.destinationTile))
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                this.DoArrivalAction();
            }, "GeneratingMapForNewEncounter", false, null, true);
            return;
        }

                    this.DoArrivalAction();
                }*/

    }

	public class WorldObject_SpaceShuttle : WorldObject_Vessel
    {
		public bool toSpace = true;
        public override void SpawnSetup()
        {
			if (toSpace) this.targetObject = starSystem.LagrangePoint;

			//this.targetObject = starSystem.SpaceStation;
			else
				this.targetObject = starSystem.homePortMap.Parent;

				base.SpawnSetup();

        }
        public override Vector3 Start => this.toSpace ? base.Start : starSystem.LagrangePoint.DrawOrbitPos;
		//public override Vector3 End => this.toSpace ? base.End : starSystem.LagrangePoint.DrawOrbitPos;
		public override void Arrived()
		{
			base.Arrived();
			this.DoArrivalAction();
		}
        public override void DoArrivalAction(Action ArrivalAction = null)
        {
            if (toSpace)
            {

				for (int i = 0; i < this.pods.Count; i++)
				{
					this.pods[i].savePawnsWithReferenceMode = false;
					this.pods[i].parent = null;
				}
				Pawn pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("AstroMech_SpaceShuttle"));
				if (pawn.GetComps<CompSpaceship>() == null)
				{
					Log.Error("DoArrivalAction: pawn generated didn't have CompSpaceship");
					return;
				}

				pawn.SetFaction(Faction.OfPlayer);
				Pawn Pilot = this.Pawns.ToList().Find(s => s.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
				this.Pawns.ToList().ForEach(s => Find.WorldPawns.RemovePawn(s));
				GenSpawn.Spawn(pawn, starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getHomePlanet.Location, starSystem.LocalMap);
				CameraJumper.TryJump(starSystem.LocalMap.GetComponent<MapComp_SpaceMap>().getHomePlanet.Location, starSystem.LocalMap);
				foreach (var pod in this.pods)
				{
					ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
					pawn.GetComp<CompSpaceship>().innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				}
				if (Pilot != null)
				{
					Pilot.mechanitor.AssignPawnControlGroup(pawn);
					Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
				}
				else
				{
					Pilot = this.Pawns.First();
					Pilot.health.AddHediff(HediffDefOf.MechlinkImplant);
					Pilot.mechanitor.AssignPawnControlGroup(pawn);
					Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
				}
				this.pods.Clear();
				this.Destroy();

			}
            else
            {

				for (int i = 0; i < this.pods.Count; i++)
				{
					this.pods[i].savePawnsWithReferenceMode = false;
					this.pods[i].parent = null;
				}
				Building landedShip = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("SpaceShuttle")) as Building;


				landedShip.SetFaction(Faction.OfPlayer);
				this.Pawns.ToList().ForEach(s => Find.WorldPawns.RemovePawn(s));

				if (starSystem.HomePortReady)
                {
					Thing Ship = GenSpawn.Spawn(landedShip, starSystem.homePortPoint, starSystem.homePortMap);
					CameraJumper.TryJump(starSystem.homePortPoint, starSystem.homePortMap);
					foreach (var pod in this.pods)
					{
						ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
						directlyHeldThings.TryDropAll(Ship.InteractionCell, starSystem.homePortMap, ThingPlaceMode.Near, null, null, true);

					}
				}
                else
                {
					
					var r = starSystem.homePortPoint + new IntVec3((int)Rand.Range(-4f, 4f), 0, (int)Rand.Range(-4f, 4f));
					if (Rand.Chance(starSystem.CalcCrashLanding(starSystem.homePortMap, r)))
                    {
						
						
						Thing Ship = GenSpawn.Spawn(landedShip, r , starSystem.homePortMap);
						CameraJumper.TryJump(r, starSystem.homePortMap);
						foreach (var pod in this.pods)
						{
							ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
							directlyHeldThings.TryDropAll(Ship.InteractionCell, starSystem.homePortMap, ThingPlaceMode.Near, null, null, true);

						}
					}
                    else
                    {
						//var rr = starSystem.homePortMap.listerThings.AllThings.FindAll(s => s.def.Equals(ThingDefOf.ShipLandingBeacon)).Select(x => x.Position);

						 r = r + new IntVec3((int)Rand.Range(-6f, 6f), 0, (int)Rand.Range(-6f, 6f));
						GenExplosion.DoExplosion(r, starSystem.homePortMap, 10, DamageDefOf.Bomb, landedShip, (int)Rand.Range(10f, 60f), (int)Rand.Range(10f, 60f), SoundDefOf.PlanetkillerImpact, null, null, null,
							ThingDefOf.ShipChunk,0.01f, 1, GasType.BlindSmoke, true, ThingDefOf.Filth_Fuel, 1, 10, 0.4f, false, null, null, null, true, 1, 0, true, null, 2f);
						Thing Ship = GenSpawn.Spawn(landedShip, r, starSystem.homePortMap);
						
						CameraJumper.TryJump(starSystem.homePortPoint, starSystem.homePortMap);
						foreach (var pod in this.pods)
						{
							ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
							directlyHeldThings.TryDropAll(Ship.InteractionCell, starSystem.homePortMap, ThingPlaceMode.Near, null, null, true);

						}
					}
                }


				this.pods.Clear();
				this.Destroy();


			}
			//base.DoArrivalAction();
		}
        public override void ExposeData()
        {
			Scribe_Values.Look<bool>(ref toSpace, "toSpace", true);
            base.ExposeData();
        }


    }
	public class WorldObject_SpaceFleet : WorldObject_Vessel
	{
		public bool toStation = true;
		public WorldObject_Wormhole targetWormhole;
		public List<Pawn> SpaceShips = new List<Pawn>();

		public override void SpawnSetup()
		{
			if (toStation)
				this.targetObject = starSystem.SpaceStation;
			else
				this.targetObject = targetWormhole;

			base.SpawnSetup();

		}
		public override void Arrived()
		{
			base.Arrived();
		}
		public override void DoArrivalAction(Action arrivalAction)
		{
			if (toStation)
			{

			}
			else
			{

			}
			base.DoArrivalAction();
		}



	}

}
