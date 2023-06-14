using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimSpace
{
	public class PlaceWorker_NeedsLandingZone : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var zones = ShipLandingBeaconUtility.GetLandingZones(map);
			if (zones != null)
			{
				foreach (var zone in zones)
				{
					if (center == zone.CenterCell) return true;
				}
			}
			return false;
		}

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{

		}
	}
}
