using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace RimSpace
{
	// Token: 0x0200147B RID: 5243
	public class CompSpaceportBeacon : ThingComp
	{
		private List<SpaceshipLandingArea> landingAreas = new List<SpaceshipLandingArea>();
		private Color fieldColor = Color.white;
		public CompProperties_SpaceportBeacon Props => (CompProperties_SpaceportBeacon)this.props;
		public List<SpaceshipLandingArea> LandingAreas
		{
			get
			{
				return this.landingAreas;
			}
		}
		public bool Active
		{
			get
			{
				CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
				return comp == null || comp.PowerOn;
			}
		}
		private bool CanLinkTo(CompSpaceportBeacon other)
		{
			return other != this && CompSpaceportBeacon.CanLinkTo(this.parent.Position, other);
		}
		public void EstablishConnections()
		{
			if (!this.parent.Spawned)
			{
				return;
			}
			List<CompSpaceportBeacon> list = new List<CompSpaceportBeacon>();
			List<CompSpaceportBeacon> list2 = new List<CompSpaceportBeacon>();
			List<Thing> list3 = this.parent.Map.listerThings.ThingsOfDef(ThingDefOf.ShipLandingBeacon);
			foreach (Thing thing in list3)
			{
				CompSpaceportBeacon compShipLandingBeacon = thing.TryGetComp<CompSpaceportBeacon>();
				if (compShipLandingBeacon != null && this.CanLinkTo(compShipLandingBeacon))
				{
					if (this.parent.Position.x == compShipLandingBeacon.parent.Position.x)
					{
						list2.Add(compShipLandingBeacon);
					}
					else if (this.parent.Position.z == compShipLandingBeacon.parent.Position.z)
					{
						list.Add(compShipLandingBeacon);
					}
				}
			}
			using (List<CompSpaceportBeacon>.Enumerator enumerator2 = list.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					CompSpaceportBeacon h = enumerator2.Current;
					using (List<CompSpaceportBeacon>.Enumerator enumerator3 = list2.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							CompSpaceportBeacon v = enumerator3.Current;
							Thing thing2 = list3.FirstOrDefault((Thing x) => x.Position.x == h.parent.Position.x && x.Position.z == v.parent.Position.z);
							if (thing2 != null)
							{
								this.TryAddArea(new SpaceshipLandingArea(CellRect.FromLimits(thing2.Position, this.parent.Position).ContractedBy(1), this.parent.Map)
								{
									beacons = new List<CompSpaceportBeacon>
									{
										this,
										thing2.TryGetComp<CompSpaceportBeacon>(),
										v,
										h
									}
								});
							}
						}
					}
				}
			}
			for (int i = this.landingAreas.Count - 1; i >= 0; i--)
			{
				using (List<CompSpaceportBeacon>.Enumerator enumerator2 = this.landingAreas[i].beacons.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (!enumerator2.Current.TryAddArea(this.landingAreas[i]))
						{
							this.RemoveArea(this.landingAreas[i]);
							break;
						}
					}
				}
			}
		}
		private void RemoveArea(SpaceshipLandingArea area)
		{
			foreach (CompSpaceportBeacon compShipLandingBeacon in area.beacons)
			{
				if (compShipLandingBeacon.landingAreas.Contains(area))
				{
					compShipLandingBeacon.landingAreas.Remove(area);
				}
			}
			this.landingAreas.Remove(area);
		}
		public bool TryAddArea(SpaceshipLandingArea newArea)
		{
			if (!this.landingAreas.Contains(newArea))
			{
				for (int i = this.landingAreas.Count - 1; i >= 0; i--)
				{
					if (this.landingAreas[i].MyRect.Overlaps(newArea.MyRect) && this.landingAreas[i].MyRect != newArea.MyRect)
					{
						if (this.landingAreas[i].MyRect.Area <= newArea.MyRect.Area)
						{
							return false;
						}
						this.RemoveArea(this.landingAreas[i]);
					}
				}
				this.landingAreas.Add(newArea);
			}
			return true;
		}
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			CompGlower compGlower = this.parent.TryGetComp<CompGlower>();
			if (compGlower != null)
			{
				this.fieldColor = compGlower.GlowColor.ToColor.ToOpaque();
			}
			this.EstablishConnections();
			foreach (SpaceshipLandingArea shipLandingArea in this.landingAreas)
			{
				shipLandingArea.RecalculateBlockingThing();
			}
		}
		public override void PostDeSpawn(Map map)
		{
			for (int i = this.landingAreas.Count - 1; i >= 0; i--)
			{
				this.RemoveArea(this.landingAreas[i]);
			}
			foreach (Thing thing in map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("SpacePortBeacon")))
			{
				CompSpaceportBeacon compShipLandingBeacon = thing.TryGetComp<CompSpaceportBeacon>();
				if (compShipLandingBeacon != null)
				{
					compShipLandingBeacon.EstablishConnections();
				}
			}
		}
		public override void CompTickRare()
		{
			foreach (SpaceshipLandingArea shipLandingArea in this.landingAreas)
			{
				shipLandingArea.RecalculateBlockingThing();
			}
		}
		public override void PostDrawExtraSelectionOverlays()
		{
			foreach (SpaceshipLandingArea shipLandingArea in this.landingAreas)
			{
				if (shipLandingArea.Active)
				{
					Color color = shipLandingArea.Clear ? this.fieldColor : Color.red;
					color.a = Pulser.PulseBrightness(1f, 0.6f);
					GenDraw.DrawFieldEdges(shipLandingArea.MyRect.ToList<IntVec3>(), color, null);
				}
				foreach (CompSpaceportBeacon compShipLandingBeacon in shipLandingArea.beacons)
				{
					if (this.CanLinkTo(compShipLandingBeacon))
					{
						GenDraw.DrawLineBetween(this.parent.TrueCenter(), compShipLandingBeacon.parent.TrueCenter(), SimpleColor.White, 0.2f);
					}
				}
			}
		}
		public override string CompInspectStringExtra()
		{
			if (!this.parent.Spawned)
			{
				return null;
			}
			string text = "";
			if (!this.Active)
			{
				text += "NotUsable".Translate() + ": " + "Unpowered".Translate().CapitalizeFirst();
			}
			int i = 0;
			while (i < this.landingAreas.Count)
			{
				if (!this.landingAreas[i].Clear)
				{
					if (!text.NullOrEmpty())
					{
						text += "\n";
					}
					text += "NotUsable".Translate() + ": ";
					if (this.landingAreas[i].BlockedByRoof)
					{
						text += "BlockedByRoof".Translate().CapitalizeFirst();
						break;
					}
					text += "BlockedBy".Translate(this.landingAreas[i].FirstBlockingThing).CapitalizeFirst();
					break;
				}
				else
				{
					i++;
				}
			}
			foreach (Thing thing in this.parent.Map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("SpacePortBeacon")))
			{
				if (thing != this.parent && ShipLandingBeaconUtility.AlignedDistanceTooShort(this.parent.Position, thing.Position, this.Props.edgeLengthRange.min - 1f))
				{
					if (!text.NullOrEmpty())
					{
						text += "\n";
					}
					text += "NotUsable".Translate() + ": " + "TooCloseToOtherBeacon".Translate().CapitalizeFirst();
					break;
				}
			}
			return text;
		}

		public static bool CanLinkTo(IntVec3 position, CompSpaceportBeacon other)
		{
			if (position.x == other.parent.Position.x)
			{
				return other.parent.def.displayNumbersBetweenSameDefDistRange.Includes((float)(Mathf.Abs(position.z - other.parent.Position.z) + 1));
			}
			return position.z == other.parent.Position.z && other.parent.def.displayNumbersBetweenSameDefDistRange.Includes((float)(Mathf.Abs(position.x - other.parent.Position.x) + 1));
		}


	}


	
		public class CompProperties_SpaceportBeacon : CompProperties
		{
			public CompProperties_SpaceportBeacon()
			{
				this.compClass = typeof(CompSpaceportBeacon);
			}

			public FloatRange edgeLengthRange;
		}
	
}
