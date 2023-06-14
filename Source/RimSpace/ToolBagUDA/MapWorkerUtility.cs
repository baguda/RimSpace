using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace MapToolBag

{
    [StaticConstructorOnStartup]
    public static class MapWorkerUtility
    {

        public static Thing tryGenObjectX(Map map, IntVec3 location, string thingDefName, string stuffDefName,bool PlayerProof = false, bool eraser = false, int rotNum = 0)
        {//
         // GenPlace.TryPlaceThing(thing2, intVec2, orGenerateMap, ThingPlaceMode.Near, null, null, default(Rot4));

            //List<Thing> thingList = Limit.GetThingList(this.map);
            Thing thing = null;
            stuffDefName = setStuffDefs(thingDefName, stuffDefName);
            ThingDef thingDef = DefDatabase<ThingDef>.GetNamed(thingDefName, false);
            if (PlayerProof)
            {
                
            }
            if (stuffDefName == null)
            {
                try
                {
                    thing = GenSpawn.Spawn(
                       ThingMaker.MakeThing(thingDef),
                       location, map, new Rot4(rotNum), WipeMode.Vanish);
                    if (eraser) thing.Destroy(DestroyMode.Vanish);
                }
                catch
                {
                    try
                    {
                        stuffDefName = setStuffDefs(thingDefName, stuffDefName);
                        thing = GenSpawn.Spawn(
                           ThingMaker.MakeThing(thingDef, DefDatabase<ThingDef>.GetNamed(thingDefName, false)),
                           location, map, new Rot4(rotNum), WipeMode.Vanish);
                        if (eraser) thing.Destroy(DestroyMode.Vanish);
                    }
                    catch
                    {
                        //Prim.Log2("Could not Spawn, " + thingDefName);
                    }
                }
            }
            else
            {
                //Prim.Log2("TGO, " + location.ToString() + ": " + thingDefName + " , " + stuffDefName); Log.ResetMessageCount();

                try
                {
                    thing = GenSpawn.Spawn(
                       ThingMaker.MakeThing(thingDef, DefDatabase<ThingDef>.GetNamed(stuffDefName, false)),
                       location, map, new Rot4(rotNum), WipeMode.Vanish);
                    if (eraser) thing.Destroy(DestroyMode.Vanish);
                }
                catch
                {
                    //Prim.Log2("Could not Spawn, " + thingDefName);
                }
            }
            return thing;
        }

        public static Thing tryGenObject(Map map, IntVec3 location, string thingDefName, string stuffDefName, bool eraser = false, int rotNum = 0)
        {//
         // GenPlace.TryPlaceThing(thing2, intVec2, orGenerateMap, ThingPlaceMode.Near, null, null, default(Rot4));

            //List<Thing> thingList = Limit.GetThingList(this.map);
            Thing thing = null;
            stuffDefName = setStuffDefs(thingDefName, stuffDefName);
            if (stuffDefName == null)
            {
                try
                {
                     thing = GenSpawn.Spawn(
                        ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(thingDefName, false)),
                        location, map, new Rot4(rotNum), WipeMode.Vanish);
                    if (eraser) thing.Destroy(DestroyMode.Vanish);
                }
                catch
                {
                    try
                    {
                        stuffDefName = setStuffDefs(thingDefName, stuffDefName);
                         thing = GenSpawn.Spawn(
                            ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(thingDefName, false), DefDatabase<ThingDef>.GetNamed(stuffDefName, false)),
                            location, map, new Rot4(rotNum), WipeMode.Vanish);
                        if (eraser) thing.Destroy(DestroyMode.Vanish);
                    }
                    catch
                    {
                       
                    }
                }
            }
            else
            {
                //Prim.Log2("TGO, " + location.ToString() + ": " + thingDefName + " , " + stuffDefName); Log.ResetMessageCount();

                try
                {
                     thing = GenSpawn.Spawn(
                        ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(thingDefName, false), DefDatabase<ThingDef>.GetNamed(stuffDefName, false)),
                        location, map, new Rot4(rotNum), WipeMode.Vanish);
                    if (eraser) thing.Destroy(DestroyMode.Vanish);
                }
                catch
                {
                    //Prim.Log2("Could not Spawn, " + thingDefName);
                }
            }
            return thing;
        }

        public static string setStuffDefsX(string thingDefName, string stuffDefName)
        {

            var tdef = DefDatabase<ThingDef>.GetNamed(thingDefName);
            if (!tdef.MadeFromStuff) return null;
            if (stuffDefName != null)
            {
                var sdef = DefDatabase<ThingDef>.GetNamed(stuffDefName);
                if (sdef.IsStuff)
                {
                    return stuffDefName;
                }


            }
            //return DefDatabase<ThingDef>.AllDefs.ToList().Find(d => d.stuffCategories.Contains(tdef.stuffCategories.RandomElement()) && d.IsStuff).defName;

            var td = DefDatabase<ThingDef>.AllDefs.ToList().Find(d => d.stuffProps.categories.Contains(tdef.stuffCategories.First()) && d.IsStuff);

            return td.defName != null? td.defName : stuffDefName;

        }

        public static string setStuffDefs(string thingDefName, string stuffDefName)
        {

            var tdef = DefDatabase<ThingDef>.GetNamed(thingDefName);
            if (!tdef.MadeFromStuff) return null;
            if (stuffDefName != null)
            {
                var sdef = DefDatabase<ThingDef>.GetNamed(stuffDefName);
                if (sdef.IsStuff && sdef.stuffProps.categories.Any(s=>tdef.stuffCategories.Contains(s)))
                {
                    return stuffDefName;
                }


            }
            //return DefDatabase<ThingDef>.AllDefs.ToList().Find(d => d.stuffCategories.Contains(tdef.stuffCategories.RandomElement()) && d.IsStuff).defName;
            var td = GenStuff.RandomStuffFor(tdef);
            //var td = DefDatabase<ThingDef>.AllDefs.ToList().Find(d => d.stuffProps.categories.Contains(tdef.stuffCategories.First()) && d.IsStuff);
            return td.defName != null ? td.defName : stuffDefName;
        }


        public static bool getIfRoof(string DefName)
        {
            if (DefDatabase<RoofDef>.AllDefs.Select(c => c.defName).Contains(DefName)) return true;
            else return false;
        }
        public static bool getIfTerrain(string DefName)
        {
            if (DefDatabase<TerrainDef>.AllDefs.Select(c => c.defName).Contains(DefName)) return true;
            else return false;
        }
        public static bool getIfThing(string DefName)
        {
            if (DefDatabase<ThingDef>.AllDefs.Select(c => c.defName).Contains(DefName)) return true;

            else return false;
        }
        public static bool getIfItemDrop(string DefName)
        {
            if (!DefDatabase<ThingDef>.AllDefs.Select(c => c.defName).Contains(DefName)) return false;
            var tmpdef = DefDatabase<ThingDef>.GetNamed(DefName);
            if (tmpdef.IsWithinCategory(ThingCategoryDefOf.Items)) return true;
            else return false;
        }

        public static void genTerrain(IntVec3 location, string thingDefName, Map map, bool isRoofed, bool isCleared = false)
        {
            if (isCleared) tryGenObject(map, location, "Wall", "WoodLog", true);
            map.terrainGrid.SetTerrain(location, DefDatabase<TerrainDef>.GetNamed(thingDefName));
            if (isRoofed) map.roofGrid.SetRoof(location, RoofDefOf.RoofConstructed);
        }
        public static void genTerrain(IntVec3 location, string thingDefName, Map map, bool isCleared = false)
        {
            if (isCleared) location.GetThingList(map).ForEach(delegate (Thing t) { t.Destroy(DestroyMode.Vanish); });// tryGenObject(map, location, "Wall", "WoodLog", true);
            map.terrainGrid.SetTerrain(location, DefDatabase<TerrainDef>.GetNamed(thingDefName));
            
        }
        public static void genRoof(IntVec3 location, Map map, string roofDefName)
        {
            map.roofGrid.SetRoof(location, DefDatabase<RoofDef>.GetNamed(roofDefName));
        }
        public static void AdaptiveGenX(Map map, IntVec3 location, string DefName, string StuffName = null, int rotNum = 0, bool eraser = false, bool isRoofed = false, bool isCleared = false)
        {
            if (DefDatabase<ThingDef>.AllDefs.Select(s => s.defName).Contains(DefName))
            {
                tryGenObject(map, location, DefName, StuffName, eraser, rotNum);
            }
            else if (DefDatabase<TerrainDef>.AllDefs.Select(s => s.defName).Contains(DefName))
            {
                genTerrain(location, DefName, map, isRoofed, isCleared);
            }
            else
            {
                Log.Error("DefName not matched to spawnable Thing or terrain: " + DefName.ToString());

            }
        }
        /*public static void AdaptiveGen(Map map, IntVec3 location, string DefName, string StuffName = null, int rotNum = 0, bool eraser = false, bool isRoofed = false, bool isCleared = false)
        {
            if (getIfItemDrop(DefName))
            {
                tryGenObject(map, location, DefName, StuffName, eraser, rotNum); //PlaceThing(map, location, DefName, StuffName, rotNum);
            }
            else if (getIfTerrain(DefName))
            {
                genTerrain(location, DefName, map, isRoofed, isCleared);
            }
            else if (getIfThing(DefName))
            {
                tryGenObject(map, location, DefName, StuffName, eraser, rotNum);
            }
            else
            {
                Log.Error("DefName not matched to spawnable Item, Thing, or terrain: " + DefName.ToString());
            }
        }*/
        public static Thing AdaptiveGen(Map map, IntVec3 location, string DefName, string StuffName = null, int rotNum = 0, bool eraser = false, bool isRoofed = false, bool isCleared = false)
        {
            if (getIfItemDrop(DefName))
            {
               return tryGenObject(map, location, DefName, StuffName, eraser, rotNum); //PlaceThing(map, location, DefName, StuffName, rotNum);
            }
            else if (getIfTerrain(DefName))
            {
                genTerrain(location, DefName, map, isRoofed, isCleared);
                return null;
            }
            else if (getIfThing(DefName))
            {
               return tryGenObject(map, location, DefName, StuffName, eraser, rotNum);
            }
            else
            {
                Log.Error("DefName not matched to spawnable Item, Thing, or terrain: " + DefName.ToString());
                return null;
            }
        }
        public static void AdaptiveSetGen(Map map, IntVec3 location, string DefName, string StuffDefName = null, int rotNum = 0, bool eraser = false, bool isRoofed = false, bool isCleared = false)
        {
            if (getIfItemDrop(DefName))
            {
                PlaceThing(map, location, DefName, StuffDefName, rotNum,location.y); //PlaceThing(map, location, DefName, StuffName, rotNum);
            }
            else if (getIfTerrain(DefName))
            {

                genTerrain(location, DefName, map, isRoofed, isCleared);
            }
            else if (getIfThing(DefName))
            {
                tryGenObject(map, location, DefName, StuffDefName, eraser, rotNum);
            }
            else if (getIfRoof(DefName))
            {
                

                genRoof(location , map, DefName);
            }


        }
        public static void PlaceThing(Map map, IntVec3 Location, string ThingDefName, string StuffDefName = null, int RotNum = 0)
        {
            StuffDefName = setStuffDefs(ThingDefName, StuffDefName);
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(ThingDefName)), Location, map, ThingPlaceMode.Near, null, null, new Rot4(RotNum));


        }
        public static void PlaceThing(Map map, IntVec3 c,string ThingDefName, string StuffDefName = null, int RotNum = 0, int stackCount = -1,QualityCategory quality = QualityCategory.Normal, bool direct = false)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamed(ThingDefName);
            if (stackCount <= 4)
            {
                stackCount = def.stackLimit;
            }
            StuffDefName = setStuffDefs(ThingDefName, StuffDefName);
            ThingDef stuff = DefDatabase<ThingDef>.GetNamed(StuffDefName, false);
            Thing thing = ThingMaker.MakeThing(def, stuff);
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(quality, ArtGenerationContext.Colony);
            }
            if (thing.def.Minifiable)
            {
                thing = thing.MakeMinified();
            }
            thing.stackCount = stackCount;
            if (direct)
            {
                GenPlace.TryPlaceThing(thing, c, map, ThingPlaceMode.Direct, null, null,  new Rot4(RotNum <= 4 ? 0:RotNum));
                return;
            }
            GenPlace.TryPlaceThing(thing, c, map, ThingPlaceMode.Near, null, null, new Rot4(RotNum <= 4 ? 0 : RotNum));
        }
    }
}
