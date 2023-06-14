using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimSpace
{
	public class SpaceshipLandingArea
	{
		private CellRect rect;
		private Map map;
		private Thing firstBlockingThing;
		private bool blockedByRoof;
		public List<CompSpaceportBeacon> beacons = new List<CompSpaceportBeacon>();

		public IntVec3 CenterCell => this.rect.CenterCell;
		public CellRect MyRect => this.rect;
		public bool Clear => this.firstBlockingThing == null && !this.blockedByRoof;
		public bool BlockedByRoof => this.blockedByRoof;
		public Thing FirstBlockingThing => this.firstBlockingThing;
		public bool Active
		{
			get
			{
				for (int i = 0; i < this.beacons.Count; i++)
				{
					if (!this.beacons[i].Active)
					{
						return false;
					}
				}
				return true;
			}
		}
		
		public SpaceshipLandingArea(CellRect rect, Map map)
		{
			this.rect = rect;
			this.map = map;
		}
		public void RecalculateBlockingThing()
		{
			this.blockedByRoof = false;
			foreach (IntVec3 c in this.rect)
			{
				if (c.Roofed(this.map))
				{
					this.blockedByRoof = true;
					break;
				}
				List<Thing> thingList = c.GetThingList(this.map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (!(thingList[i] is Pawn) && (thingList[i].def.Fillage != FillCategory.None || thingList[i].def.IsEdifice() || thingList[i] is Skyfaller))
					{
						this.firstBlockingThing = thingList[i];
						return;
					}
				}
			}
			this.firstBlockingThing = null;
		}

	}

	
}

