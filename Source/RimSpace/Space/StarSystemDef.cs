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

		public IntVec3 mapSize = new IntVec3(301, 1, 301);
		public IntVec3 getMapSize => mapSize;


	}
}
