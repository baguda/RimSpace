using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimSpace
{
	public class StarSystemDef : Def
	{
		public List<string> SpaceObjectDefNames = new List<string>();
		public List<string> getSpaceObjectDefNames => (List<string>)this.SpaceObjectDefNames;

		public string CentralObjectDefName; 

		public IntVec3 mapSize = new IntVec3(301, 1, 301);
		public IntVec3 getMapSize => mapSize;

		public bool UseAllSpaceObjectDefs = false;
		public int SpaceObjectCount = 4;

		public string SpaceBiomeDefName = "SpaceBiome";


	}
	public class GalacticDef : Def
	{
		public List<string> StarSystemDefNames = new List<string>();
		public List<string> getStarSystemDefNames => (List<string>)this.StarSystemDefNames;

		public bool UseAllStarSystemDefs = false;

		public int StarSystemCount = 4;

		public string WormholeWorldObjectDefName = "Wormhole";




	}
}
