using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace MapToolBag
{
    /*
    public static class MapMaker
    {
        public static Map mapBeingGenerated;
        private static Dictionary<string, object> data = new Dictionary<string, object>();
        private static IntVec3 playerStartSpotInt = IntVec3.Invalid;
        public static List<IntVec3> rootsToUnfog = new List<IntVec3>();
        private static List<GenStepWithParams> tmpGenSteps = new List<GenStepWithParams>();
        public const string ElevationName = "Elevation";
        public const string FertilityName = "Fertility";
        public const string CavesName = "Caves";
        public const string RectOfInterestName = "RectOfInterest";
        public const string UsedRectsName = "UsedRects";
        public const string RectOfInterestTurretsGenStepsCount = "RectOfInterestTurretsGenStepsCount";
        public static MapGenFloatGrid Elevation => MapMaker.FloatGridNamed("Elevation");
        public static MapGenFloatGrid Fertility => MapMaker.FloatGridNamed("Fertility");
        public static MapGenFloatGrid Caves => MapMaker.FloatGridNamed("Caves");
        public static IntVec3 PlayerStartSpot
        {
            get
            {
                if (!MapMaker.playerStartSpotInt.IsValid)
                {
                    Log.Error("Accessing player start spot before setting it.", false);
                    return IntVec3.Zero;
                }
                return MapMaker.playerStartSpotInt;
            }
            set
            {
                MapMaker.playerStartSpotInt = value;
            }
        }
        public static Map MakeMap(IntVec3 mapSize, MapParent parent, CellSetMapDef cellSetMapDef, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {

            Log.Message("MapMaker.MakeMap: MapMaker Init");
            ProgramState programState = Current.ProgramState;
            Current.ProgramState = ProgramState.MapInitializing;
            MapMaker.playerStartSpotInt = IntVec3.Invalid;
            MapMaker.rootsToUnfog.Clear();
            MapMaker.data.Clear();
            MapMaker.mapBeingGenerated = null;
            DeepProfiler.Start("InitNewGeneratedMap");
            Rand.PushState();
            int seed = Gen.HashCombineInt(Find.World.info.Seed, parent.Tile);
            Rand.Seed = seed;
            Map result;

            Log.Message("MapMaker.MakeMap: starting map gen process");
            try
            {
                if (parent != null && parent.HasMap)
                {
                    Log.Error("Tried to generate a new map and set " + parent + " as its parent, but this world object already has a map. One world object can't have more than 1 map.", false);
                    parent = null;
                }
                DeepProfiler.Start("Set up map");
                Map map = new Map();
                map.uniqueID = Find.UniqueIDsManager.GetNextMapID();
                map.generationTick = GenTicks.TicksGame;
                MapMaker.mapBeingGenerated = map;
                map.info.Size = mapSize;
                map.info.parent = parent;
                map.ConstructComponents();

                DeepProfiler.End();
                Current.Game.AddMap(map);
                Log.Message("MapMaker.MakeMap: Filling map...");
                if (extraInitBeforeContentGen != null)
                {
                    extraInitBeforeContentGen(map);
                }

                map.areaManager.AddStartingAreas();

                //map.weatherDecider.StartInitialWeather();

                map.weatherManager.curWeather = WeatherDefOf.Clear;
               // map.weatherDecider.curWeatherDuration = 10000;
                map.weatherManager.curWeatherAge = 0;
                Log.Message("MapMaker.MakeMap: Generate contents into map");
                DeepProfiler.Start("Generate contents into map");

                MapMaker.GenerateContentsIntoMap(cellSetMapDef, map, seed);

                DeepProfiler.End();
                Log.Message("MapMaker.MakeMap: finalizing map Init...");

                Find.Scenario.PostMapGenerate(map);
                DeepProfiler.Start("Finalize map init");
                map.FinalizeInit();
                DeepProfiler.End();
                DeepProfiler.Start("MapComponent.MapGenerated()");
                MapComponentUtility.MapGenerated(map);
                DeepProfiler.End();
                if (parent != null)
                {
                    parent.PostMapGenerate();
                }
                result = map;
            }
            finally
            {
                DeepProfiler.End();
                MapMaker.mapBeingGenerated = null;
                Current.ProgramState = programState;
                Rand.PopState();
                Log.Message("MapMaker.MakeMap: finalizing map...");
            }
            return result;
        }

        public static void GenerateContentsIntoMap(CellSetMapDef cellSetMapDef, Map map, int seed)
        {
            MapMaker.data.Clear();
            Rand.PushState();
            try
            {
                Rand.Seed = seed;
                Log.Message("MapMaker.GenerateContentsIntoMap: Making Map From Def...");
                DeepProfiler.Start("Making Map From Def...");
                try
                {
                    cellSetMapDef.MapDatabase.genIntoMap(map);
                }
                catch (Exception arg)
                {
                    Log.Error("Error While Generating CellSetMap: " + arg, false);
                }
                finally
                {
                    DeepProfiler.End();
                }

            }
            finally
            {
                Rand.PopState();
                RockNoises.Reset();
                MapMaker.data.Clear();
            }


            /* MapMaker.data.Clear();
             Rand.PushState();
             try
             {
                 Rand.Seed = seed;
                 RockNoises.Init(map);
                 MapMaker.tmpGenSteps.Clear();
                 MapMaker.tmpGenSteps.AddRange(from x in genStepDefs
                                               orderby x.def.order, x.def.index
                                               select x);
                 for (int i = 0; i < MapMaker.tmpGenSteps.Count; i++)
                 {
                     DeepProfiler.Start("GenStep - " + MapMaker.tmpGenSteps[i].def);
                     try
                     {
                         Rand.Seed = Gen.HashCombineInt(seed, MapMaker.GetSeedPart(MapMaker.tmpGenSteps, i));
                         MapMaker.tmpGenSteps[i].def.genStep.Generate(map, MapMaker.tmpGenSteps[i].parms);
                     }
                     catch (Exception arg)
                     {
                         Log.Error("Error in GenStep: " + arg, false);
                     }
                     finally
                     {
                         DeepProfiler.End();
                     }
                 }
             }
             finally
             {
                 Rand.PopState();
                 RockNoises.Reset();
                 MapMaker.data.Clear();
             }*/
        
/*}
        public static T GetVar<T>(string name)
        {
            object obj;
            if (MapMaker.data.TryGetValue(name, out obj))
            {
                return (T)((object)obj);
            }
            return default(T);
        }

        public static bool TryGetVar<T>(string name, out T var)
        {
            object obj;
            if (MapMaker.data.TryGetValue(name, out obj))
            {
                var = (T)((object)obj);
                return true;
            }
            var = default(T);
            return false;
        }

        public static void SetVar<T>(string name, T var)
        {
            MapMaker.data[name] = var;
        }

        public static MapGenFloatGrid FloatGridNamed(string name)
        {
            MapGenFloatGrid var = MapMaker.GetVar<MapGenFloatGrid>(name);
            if (var != null)
            {
                return var;
            }
            MapGenFloatGrid mapGenFloatGrid = new MapGenFloatGrid(MapMaker.mapBeingGenerated);
            MapMaker.SetVar<MapGenFloatGrid>(name, mapGenFloatGrid);
            return mapGenFloatGrid;
        }

        private static int GetSeedPart(List<GenStepWithParams> genSteps, int index)
        {
            int seedPart = genSteps[index].def.genStep.SeedPart;
            int num = 0;
            for (int i = 0; i < index; i++)
            {
                if (MapMaker.tmpGenSteps[i].def.genStep.SeedPart == seedPart)
                {
                    num++;
                }
            }
            return seedPart + num;
        }


    }
    */
    public class GenStepSetDef : GenStepDef
    {
        public override void PostLoad()
        {
            base.PostLoad();
            this.genStep.def = (this);
        }

        public string CellSetMapName;
    }

    public class GenStep_CellSets : GenStep
    {
        public override int SeedPart => 1597836;

        public override void Generate(Map map, GenStepParams parms)
        {



        }
    }


}
