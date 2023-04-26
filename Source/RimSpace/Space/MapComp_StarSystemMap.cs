using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimSpace
{
    public class MapComp_SpaceMap : MapComponent
    {
        public bool isSpace => this.map.Parent.def.defName.Equals("OrbitalLocation");

        public MapComp_SpaceMap(Map map) : base(map)
        {
            base.map = map;
        }
        public override void MapComponentTick()
        {
            if (isSpace) this.FilterNonSpacers();
            base.MapComponentTick();
        }
        public void FilterNonSpacers()
        {
            this.map.listerThings.AllThings.FindAll(thing => thing.def.HasComp(typeof(CompSpacer)) == false).ForEach(delegate (Thing NotSpaceWorthyThing) { NotSpaceWorthyThing.Destroy(DestroyMode.Vanish); });
        }

    }
}
