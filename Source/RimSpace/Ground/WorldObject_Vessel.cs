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
		public Vector3 End => starSystem.StarSystem.DrawPos;
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
		public int destinationTile = -1;
		public TransportPodsArrivalAction arrivalAction;
		private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();
		private bool arrived;
		private int initialTile = -1;
		private float traveledPct;
		public float TravelSpeed = 0.00025f;


		public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>();
		public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;
		private Vector3 Start => Find.WorldGrid.GetTileCenter(this.initialTile);
		private Vector3 End => starSystem.StarSystem.DrawPos;
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
			Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Deep.Look<TransportPodsArrivalAction>(ref this.arrivalAction, "arrivalAction", Array.Empty<object>());
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
		private void Arrived()
		{
			if (this.arrived)
			{
				return;
			}
			this.arrived = true;
			/*
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
			*/
			this.DoArrivalAction();
		}
		private void DoArrivalAction()
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				this.pods[i].savePawnsWithReferenceMode = false;
				this.pods[i].parent = null;
			}

			Pawn pawn = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("AstroMech_Fighter"));
			if(pawn.GetComps<CompSpaceship>() == null)
            {
				Log.Error("DoArrivalAction: pawn generated didn't have CompSpaceship");
				return;
            }
			pawn.SetFaction(Faction.OfPlayer);

			Pawn Pilot = this.Pawns.ToList().Find(s => s.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
			Pawns.ToList().ForEach(s => Find.WorldPawns.RemovePawn(Pilot)); 



			foreach (var pod in this.pods)
            {

				GenSpawn.Spawn(pawn, starSystem.getHomePlanet.Location, starSystem.SystemMap);
				ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
				pawn.GetComp<CompSpaceship>().innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				
			}

			if (Pilot != null)
			{

				//f ( Pilot.IsWorldPawn()) Find.WorldPawns.RemovePawn(Pilot);
				Pilot.mechanitor.AssignPawnControlGroup(pawn);

				Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
				
				

			}
			else
			{
				Pilot = this.Pawns.First();
				//if (Pilot != null && Pilot.IsWorldPawn()) Find.WorldPawns.RemovePawn(Pilot);
				
				Pilot.health.AddHediff(HediffDefOf.MechlinkImplant);
				Pilot.mechanitor.AssignPawnControlGroup(pawn);
				Pilot.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
			}








			/*
			if (this.arrivalAction != null)
			{
				try
				{
					this.arrivalAction.Arrived(this.pods, this.destinationTile);
				}
				catch (Exception arg)
				{
					Log.Error("Exception in transport pods arrival action: " + arg);
				}
				this.arrivalAction = null;
			}
			
			else
			{
				for (int j = 0; j < this.pods.Count; j++)
				{
					for (int k = 0; k < this.pods[j].innerContainer.Count; k++)
					{
						Pawn pawn = this.pods[j].innerContainer[k] as Pawn;
						if (pawn != null && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer))
						{
							PawnBanishUtility.Banish(pawn, this.destinationTile);
						}
					}
				}
				bool flag = true;
				if (ModsConfig.BiotechActive)
				{
					flag = false;
					int num = 0;
					while (num < this.pods.Count && !flag)
					{
						for (int l = 0; l < this.pods[num].innerContainer.Count; l++)
						{
							if (this.pods[num].innerContainer[l].def != ThingDefOf.Wastepack)
							{
								flag = true;
								break;
							}
						}
						num++;
					}
				}
				for (int m = 0; m < this.pods.Count; m++)
				{
					for (int n = 0; n < this.pods[m].innerContainer.Count; n++)
					{
						this.pods[m].innerContainer[n].Notify_AbandonedAtTile(this.destinationTile);
					}
				}
				for (int num2 = 0; num2 < this.pods.Count; num2++)
				{
					this.pods[num2].innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
				}
				if (flag)
				{
					string key = "MessageTransportPodsArrivedAndLost";
					if (this.def == WorldObjectDefOf.TravelingShuttle)
					{
						key = "MessageShuttleArrivedContentsLost";
					}
					Messages.Message(key.Translate(), new GlobalTargetInfo(this.destinationTile), MessageTypeDefOf.NegativeEvent, true);
				}
			}
			*/


			this.pods.Clear();
			this.Destroy();
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


	}
}
