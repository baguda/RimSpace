using Verse;
using RimWorld;

namespace RimSpace

{
    public class CompProperties_LaunchShip : CompProperties
    {

        public CompProperties_LaunchShip()
        {
            this.compClass = typeof(CompLaunchShip);
        }
		public bool requireFuel = true;
		public int fixedLaunchDistanceMax = -1;
        public ThingDef skyfallerLeaving;
	}
}
