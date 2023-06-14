using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using MapToolBag;

namespace RimSpace
{
    public class GameComp_StarSystem : GameComponent
    {
        //public WorldObject_Orbital StarSystem;
        //public Map SystemMap => hasSystem ? StarSystem.Map : null;
        //public bool hasMap => this.hasSystem ? this.SystemMap != null : false;
        public List<WorldObject_Wormhole> Wormholes = new List<WorldObject_Wormhole>();
        public Map LocalMap 
        {
            get
            {
                if (this.LocalMapIndex >= 0)
                {
                    return Find.Maps[(int)this.LocalMapIndex];
                }
                return null;
            }
            set
            {
                this.LocalMapIndex = (value != null) ? (sbyte)value.Index : (sbyte)-1;
            }
        }
        //public ShipLandingArea homePort => ShipLandingBeaconUtility.GetLandingZones(this.homePortMap).Find(s=> s.CenterCell.Equals(this.homePortPoint));
        public Map homePortMap
        {
            get
            {
                if (this.homePortMapIndex >= 0)
                {
                    return Find.Maps[(int)this.homePortMapIndex];
                }
                return null;
            }
            set
            {
                this.homePortMapIndex = (value != null) ? (sbyte)value.Index : (sbyte)-1;
            }
        }
        public MapComp_SpaceMap LocapMapComp => LocalMap.GetComponent<MapComp_SpaceMap>();
        public IntVec3 homePortPoint;
        //public CellRect homePortRect ;
        public bool hasStation = false;
        public bool hasLagrange = false;
        private sbyte homePortMapIndex = -1;
        private sbyte LocalMapIndex = -1;


        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref hasStation, "hasStation");
            Scribe_Values.Look<bool>(ref hasLagrange, "hasLagrange");
            Scribe_Values.Look<sbyte>(ref this.homePortMapIndex, "homePortMapIndex", -1, false);
            Scribe_Values.Look<sbyte>(ref this.LocalMapIndex, "LocalMapIndex", -1, false);
            Scribe_Values.Look<IntVec3>(ref this.homePortPoint, "pos", IntVec3.Invalid, false);
            base.ExposeData();
        }


        public bool HomePortReady => this.homePortPoint != null;// && homePort.Active;
        
        public StarSystemDef def => DefDatabase<StarSystemDef>.GetNamed("SystemCore");
        //public TargetInfo SpaceShuttlePort = new TargetInfo(Current.Game.AnyPlayerHomeMap.Center, Current.Game.AnyPlayerHomeMap);
        public WorldObject_SpaceStation SpaceStation => Find.WorldObjects.AllWorldObjects.Find(s=> s.def.defName.Equals("SpaceStation"))as WorldObject_SpaceStation;
        public WorldObject_Orbital LagrangePoint => Find.WorldObjects.AllWorldObjects.Find(s => s.def.defName.Equals("LagrangePoint")) as WorldObject_Orbital;
        public List<MapComp_SpaceMap> spaceComps => StarMaps.Select(s => s.GetComponent<MapComp_SpaceMap>()).ToList();
        public List<Map> StarMaps => this.Wormholes.Select(s => s.Map).ToList();


        public GameComp_StarSystem(Game game) : base()
        {
        }
        public float CalcCrashLanding()
        {
            float result = 0;
            if (homePortMap.weatherManager.curWeather.Equals(WeatherDefOf.Clear)) result += 0.2f;
            if (homePortMap.glowGrid.GameGlowAt(homePortPoint) >= 0.3f) result += 0.2f;
           // result += (homePortRect.Area) / 1250f;
            result += Rand.Range(0.05f, 0.20f);
            if (Prefs.DevMode) Log.Message("WorldObject_SpaceShuttle.CalcCrashLanding: CrashChance = " + (result * 100f).ToString() + "%");
            return result;

        }
        public float CalcCrashLanding(Map map, IntVec3 point)
        {
            float result = 0;
            if (map.weatherManager.curWeather.Equals(WeatherDefOf.Clear)) result += 0.2f;
            if (map.glowGrid.GameGlowAt(point) >= 0.3f) result += 0.2f;
            result += ((float)ShipLandingBeaconUtility.GetLandingZones(map).Find(s => s.MyRect.Contains(point)).MyRect.Area) / 1250f;
            result += Rand.Range(0.05f, 0.20f);
            if (Prefs.DevMode) Log.Message("WorldObject_SpaceShuttle.CalcCrashLanding: CrashChance = " + (result * 100f).ToString() + "%");
            return result;

        }
        public bool WormholeHasMap(string name)
        {
            return getWormholeNamed(name).HasMap;
        }
        public WorldObject_Wormhole getWormholeNamed(string name)
        {
           return  Wormholes.Find(s => s.Name == name);
        }
        public WorldObject_Orbital GenOrbital(string WorldObjectDefName, int tile, Faction faction, string name = null, bool Surface = true)
        {
                WorldObjectDef WODef = DefDatabase<WorldObjectDef>.GetNamed(WorldObjectDefName, true);
                WorldObject_Orbital newOrbital = (WorldObject_Orbital)WorldObjectMaker.MakeWorldObject(WODef);
                newOrbital.Tile = tile;
                MapToolBag.TileHandlerUtility.ConditionTile(tile,def.SpaceBiomeDefName);
                MapToolBag.TileHandlerUtility.applyNeighbors(tile, delegate (Tile t, int s) { MapToolBag.TileHandlerUtility.ConditionTile(s, def.SpaceBiomeDefName); });
                newOrbital.realTile = TileHandlerUtility.getTile(tile);
                newOrbital.SetFaction(faction);
                Find.WorldObjects.Add(newOrbital);
                if(name != null) newOrbital.Name = name;
            return newOrbital;
        }
        public WorldObject_Orbital GenLagrangePoint(string WorldObjectDefName, int tile, Faction faction, string name = null, bool Surface = true)
        {
            if (!hasLagrange)
            {
                
                hasLagrange = true;
                GenOrbital(WorldObjectDefName, tile, faction, name, Surface);
                
                
            }
            return this.LagrangePoint;
        }
        public WorldObject_Wormhole GenWormhole(string WorldObjectDefName, int tile, Faction faction, string name = null, bool Surface = true)
        {
            var result = GenOrbital(WorldObjectDefName, tile, faction, name, Surface) as WorldObject_Wormhole;
            this.Wormholes.Add(result);
            return result;

        }
        public WorldObject_Wormhole GenWormhole(int tile, Faction faction, string name = null, bool Surface = true)
        {
            var result = GenOrbital("Wormhole", tile, faction, name, Surface) as WorldObject_Wormhole;
            this.Wormholes.Add(result);
            return result;

        }
        public WorldObject_SpaceStation GenSpaceStation(string WorldObjectDefName, int tile, Faction faction, string name = null, bool Surface = true)
        {
            if(!hasStation)
            {
                  GenOrbital(WorldObjectDefName, tile, faction, name, Surface);
                hasStation = true;
            }
             return this.SpaceStation;
        }
        public WorldObject_SpaceStation GenSpaceStation(int tile, Faction faction, string name = null, bool Surface = true)
        {
            if (!hasStation)
            {
                GenOrbital("SpaceStation", tile, faction, name, Surface);
                hasStation = true;
            }
            return this.SpaceStation;
        }
        public Map makeStarMap(WorldObject_Wormhole wormhole)
        {
            if (!wormhole.HasMap)
            {
                Map map = MapGenerator.GenerateMap(def.getMapSize, wormhole, wormhole.MapGeneratorDef, wormhole.ExtraGenStepDefs, null);
                Find.World.WorldUpdate();
                return map;
            }
            else return wormhole.Map;
        }
        public Map makeSpaceMap(WorldObject_Orbital Orbital)
        {
            if (!Orbital.HasMap)
            {
                Map map = MapGenerator.GenerateMap(def.getMapSize, Orbital, Orbital.MapGeneratorDef, Orbital.ExtraGenStepDefs, null);
                Find.World.WorldUpdate();
                return map;
            }
            else return Orbital.Map;
        }
        public Map makeGroundMap(CompPlanet planet)
        {
            if (!planet.hasGroundMap)
            {
                    Log.Message("GameComp_StarSystem.makeGroundMap: 1");
                WorldObjectDef WODef = DefDatabase<WorldObjectDef>.GetNamed(planet.Props.GroundMapWorldObjectDefName, true);

                    Log.Message("GameComp_StarSystem.makeGroundMap: 2");
                WorldObject_HiddenPlanet newOrbital = (WorldObject_HiddenPlanet)WorldObjectMaker.MakeWorldObject(WODef);


                    Log.Message("GameComp_StarSystem.makeGroundMap: 3");
                int tile = TileHandlerUtility.getWorldEdgeTiles().RandomElement();

                    Log.Message("GameComp_StarSystem.makeGroundMap: 4");
                newOrbital.Tile = tile;

                    Log.Message("GameComp_StarSystem.makeGroundMap: 5");
                MapToolBag.TileHandlerUtility.ConditionTile(tile, planet.Props.BiomeDefName, planet.Props.Elevation,
                    planet.Props.Rainfall, planet.Props.Swampiness, planet.Props.Tempureture, planet.Props.Hilliness);

                    Log.Message("GameComp_StarSystem.makeGroundMap: 6");
                MapToolBag.TileHandlerUtility.applyNeighbors(tile, delegate (Tile t, int s) { MapToolBag.TileHandlerUtility.ConditionTile(s, def.SpaceBiomeDefName); });

                    Log.Message("GameComp_StarSystem.makeGroundMap: 7");
                newOrbital.realTile = TileHandlerUtility.getTile(tile);

                    Log.Message("GameComp_StarSystem.makeGroundMap: 8");
                newOrbital.SetFaction(Faction.OfEmpire);

                    Log.Message("GameComp_StarSystem.makeGroundMap: 9");
                Find.WorldObjects.Add(newOrbital);

                    Log.Message("GameComp_StarSystem.makeGroundMap: 10");
                Map map = MapGenerator.GenerateMap(def.getMapSize, newOrbital,
                    (planet.Props.GroundMapGeneratorDefName != "") ? DefDatabase<MapGeneratorDef>.
                    GetNamed( planet.Props.GroundMapGeneratorDefName) : newOrbital.MapGeneratorDef, 
                    newOrbital.ExtraGenStepDefs, null);

                    Log.Message("GameComp_StarSystem.makeGroundMap: 11");
                planet.GroundMap = map;

                    Log.Message("GameComp_StarSystem.makeGroundMap: 12");
                Find.World.WorldUpdate();

                    Log.Message("GameComp_StarSystem.makeGroundMap: 13");
                return map;
            }
            else return planet.GroundMap;
        }

        private Pawn GenerateTrader(Faction faction, Ideo ideo, int tile, PawnGroupMaker groupMaker, TraderKindDef traderKind)
        {
            PawnKindDef kind = groupMaker.traders.RandomElementByWeight((PawnGenOption x) => x.selectionWeight).kind;
            
            PawnGenerationContext context = PawnGenerationContext.NonPlayer;
            
            Pawn pawn = PawnGenerator.GeneratePawn(
                new PawnGenerationRequest(kind, faction, context, tile, 
                false, false, false, true, false, 1f, false, true, false, true, true,
                true, false, false, false, 0f, 0f, null, 1f, null, null,
                null, null, null, null, null, null, null, null, null, ideo, false, 
                false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult,
                null, null, null, false));
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            pawn.trader.traderKind = traderKind;
            return pawn;
        }

        public void MakeStationMap() { }
        public override void GameComponentOnGUI()
        {

            base.GameComponentOnGUI();

            if (Prefs.DevMode)
            {


                Vector2 vector2 = new Vector2((float)UI.screenWidth * 0.5f - (24f * 9), 3f);
                Find.WindowStack.ImmediateWindow(typeof(DebugWindowsOpener).GetHashCode(), new Rect(vector2.x, vector2.y, 24f * 4.5f, 24f).Rounded(), WindowLayer.GameUI, delegate
                {

                    WidgetRow row = new WidgetRow(24f * 0f, 0f, UIDirection.RightThenDown, 99999f, 4f);

                    if (row.ButtonIcon(ContentFinder<Texture2D>.Get("GUI/Reset", true), "Restart Rimworld", null, null, null, true))
                    {
                        GenCommandLine.Restart();
                    }
                    if (row.ButtonIcon(ContentFinder<Texture2D>.Get("WorldObjects/SpaceStation", true), "Make Space Station"))
                    {
                        
                        this.GenSpaceStation("SpaceStation", getWorldEdgeTiles().RandomElement(),Faction.OfPlayer,"Horizon Station");
                    }
                    if (row.ButtonIcon(ContentFinder<Texture2D>.Get("WorldObjects/Wormhole", true), "Make Wormhole"))
                    {
                        
                        this.GenWormhole("Wormhole", getWorldEdgeTiles().RandomElement(), Faction.OfPlayer, "Wormhole");
                    }
                    if (row.ButtonIcon(ContentFinder<Texture2D>.Get("WorldObjects/SpaceRegion", true), "Make StarMap in Wormhole"))
                    {
                        var hole = this.Wormholes.Find(s => !s.HasMap);
                        if(hole != null)
                        {
                            this.makeStarMap(hole);

                        }
                        else  this.GenWormhole("Wormhole", getWorldEdgeTiles().RandomElement(), Faction.OfPlayer, "Wormhole");
                        
                       
                    }
                }, false, false, 0f);
            }


        }
        public List<int> getWorldEdgeTiles()
        {
            if(Find.World.info.planetCoverage < 1f)
            {
                List<int> result = new List<int>();
                for (int ind = 1; ind >= Find.WorldGrid.TilesCount; ind++)
                {
                    if (Find.WorldGrid.IsOnEdge(ind)) result.Add(ind);
                }
                return result;
            }
            return null;




        }



        /*
        public bool tryGenOrbital(string WorldObjectDefName = "OrbitalLocation")
        {
            if (!this.hasSystem)
            {
                int tile = Find.World.grid.TilesCount - 1; // TODO: Find Ocean Tile
                WorldObjectDef WODef = DefDatabase<WorldObjectDef>.GetNamed(WorldObjectDefName, true);
                WorldObject_Orbital newOrbital = (WorldObject_Orbital)WorldObjectMaker.MakeWorldObject(WODef);
                newOrbital.Tile = tile;
                TileHandlerUtility.ConditionTile(tile, "SpaceBiome");
                newOrbital.realTile = TileHandlerUtility.getTile(tile);
                newOrbital.SetFaction(Faction.OfAncientsHostile);
                Find.WorldObjects.Add(newOrbital);
                //newOrbital.Name = "Outer orbital position";
                this.StarSystem = newOrbital;

                return true;
            }
            return false;


        }
        */

        /*
         public Map makeStarMap()
         {
             if (!this.hasMap)
             {
                 Log.Message("Space... The Final Frontier");

                 this.GenOrbital("OrbitalLocation",Find.World.grid.TilesCount - 1,Faction.OfPlayer);
                 Map map = MapGenerator.GenerateMap(def.getMapSize, StarSystem, StarSystem.MapGeneratorDef, StarSystem.ExtraGenStepDefs, null);
                 try
                 {
                     if (DefDatabase<WeatherDef>.AllDefs.ToList<WeatherDef>().FindAll(s => s.defName.Equals("OuterSpaceWeather")).Any())
                     {
                         map.weatherManager.curWeather = WeatherDef.Named("OuterSpaceWeather"); //TODO ADD WEATHER DEF FOR SPACE WEATHER 
                     }
                     else Log.Message("no weather");
                 }
                 catch { if (Prefs.DevMode) Log.Message("No space weather catch"); }
                 Find.World.WorldUpdate();
                 return map;
             }
             else return this.SystemMap;
         }*/


        /*
        try
        {
            if (DefDatabase<WeatherDef>.AllDefs.ToList<WeatherDef>().FindAll(s => s.defName.Equals("OuterSpaceWeather")).Any())
            {
                map.weatherManager.curWeather = WeatherDef.Named("OuterSpaceWeather"); //TODO ADD WEATHER DEF FOR SPACE WEATHER 
            }
            else Log.Message("no weather");
        }
        catch { if (Prefs.DevMode) Log.Message("No space weather catch"); }
        */



        public void test()
        {
           var grid =  Find.WorldGrid;

            grid.viewAngle = Find.World.PlanetCoverage * 180f;
            grid.viewCenter = Vector3.back;
            float angle = 45f;
            if (grid.viewAngle > 45f)
            {
                angle = Mathf.Max(90f - grid.viewAngle, 0f);
            }
            grid.viewCenter = Quaternion.AngleAxis(angle, Vector3.right) * grid.viewCenter;
            
            PlanetShapeGenerator.Generate(3, out grid.verts, out grid.tileIDToVerts_offsets, out grid.tileIDToNeighbors_offsets, out grid.tileIDToNeighbors_values, 100f, grid.viewCenter, grid.viewAngle);

            int tilesCount = grid.TilesCount;
            double num = 0.0;
            int num2 = 0;
            for (int i = 0; i < tilesCount; i++)
            {
                Vector3 tileCenter = grid.GetTileCenter(i);
                int num3 = (i + 1 < grid.tileIDToNeighbors_offsets.Count) ? grid.tileIDToNeighbors_offsets[i + 1] : grid.tileIDToNeighbors_values.Count;
                for (int j = grid.tileIDToNeighbors_offsets[i]; j < num3; j++)
                {
                    int tileID = grid.tileIDToNeighbors_values[j];
                    Vector3 tileCenter2 = grid.GetTileCenter(tileID);
                    num += (double)Vector3.Distance(tileCenter, tileCenter2);
                    num2++;
                }
            }
            grid.averageTileSize = (float)(num / (double)num2);

        }
    
    
    
    
    
    
    
    }


   


}
