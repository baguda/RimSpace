using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimSpace
{
	public class StationProjectDef : Def
	{


		public List<ThingDefCountClass> costList;
		public float TotalLabor = 100000f;
		public float MaxLaborRatePerTick = 1;
		public int MaxWorkers = 100;
		public bool Repeatable = true;

	}
}