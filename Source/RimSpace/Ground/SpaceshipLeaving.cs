using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using RimWorld;

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
				Log.Error("Drop pod left the map, but its destination tile is " + this.destinationTile);
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			Lord lord = TransporterUtility.FindLord(this.groupID, base.Map);
			if (lord != null)
			{
				base.Map.lordManager.RemoveLord(lord);
			}



			WorldObject_Vessel vessel = (WorldObject_Vessel)WorldObjectMaker.MakeWorldObject(this.worldObjectDef ?? WorldObjectDefOf.TravelingTransportPods);
			vessel.Tile = base.Map.Tile;
			vessel.SetFaction(Faction.OfPlayer);
			vessel.destinationTile = this.destinationTile;
			vessel.arrivalAction = this.arrivalAction;
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

	
	}
}
