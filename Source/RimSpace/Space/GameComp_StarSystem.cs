using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;

namespace RimSpace
{
    public class GameComp_StarSystem : GameComponent
    {

        public GameComp_StarSystem(Game game) : base()
        {
        }


        public StarSystemDef def => DefDatabase<StarSystemDef>.GetNamed("SystemCore");
        //WorldObject_Orbital
        //


        // StarSystem
        public WorldObject_Orbital StarSystem;
        public Map SystemMap => hasSystem ? StarSystem.Map : null;
        public bool hasSystem => this.StarSystem != null;
        public bool hasMap => this.hasSystem ? this.SystemMap != null : false;

        public Map makeSystemMap()
        {
            if (!this.hasMap)
            {
                Log.Message("Space... The Final Frontier");

                this.tryGenOrbital();
                Log.Message("makeSpaceMap: " + StarSystem.Label);
                Map map = MapGenerator.GenerateMap(def.getMapSize, StarSystem, StarSystem.MapGeneratorDef, StarSystem.ExtraGenStepDefs, null);
                try
                {
                    if (DefDatabase<WeatherDef>.AllDefs.ToList<WeatherDef>().FindAll(s => s.defName.Equals("OuterSpaceWeather")).Any())
                    {
                        Log.Message("set weather");
                        map.weatherManager.curWeather = WeatherDef.Named("OuterSpaceWeather"); //TODO ADD WEATHER DEF FOR SPACE WEATHER 
                    }
                    else Log.Message("no weather");
                }
                catch { if (Prefs.DevMode) Log.Message("No space weather catch"); }
                Find.World.WorldUpdate();
                return map;
            }
            else return this.SystemMap;
        }
        public bool tryGenOrbital()
        {
            if (!this.hasSystem)
            {
                int tile = Find.World.grid.TilesCount - 1; // TODO: Find Ocean Tile
                WorldObjectDef WODef = DefDatabase<WorldObjectDef>.GetNamed("OrbitalLocation", true);
                WorldObject_Orbital newOrbital = (WorldObject_Orbital)WorldObjectMaker.MakeWorldObject(WODef);
                newOrbital.Tile = tile;
                this.applySpaceSurface(tile);
                newOrbital.realTile = getTile(tile);
                newOrbital.SetFaction(Faction.OfAncientsHostile);
                Find.WorldObjects.Add(newOrbital);
                //newOrbital.Name = "Outer orbital position";
                this.StarSystem = newOrbital;

                return true;
            }
            return false;


        }
        public bool applySpaceSurface(int tileNum)
        {
            try
            {
                List<int> neighbors = new List<int>();
                Find.World.grid.GetTileNeighbors(tileNum, neighbors);
                foreach (int tile in neighbors)
                {
                    Find.World.grid.tiles.ElementAt(tile).biome = DefDatabase<BiomeDef>.GetNamed("SpaceBiome");
                }
                Find.World.grid.tiles.ElementAt(tileNum).elevation = 00f;
                Find.World.grid.tiles.ElementAt(tileNum).hilliness = Hilliness.Flat;
                Find.World.grid.tiles.ElementAt(tileNum).rainfall = 0f;
                Find.World.grid.tiles.ElementAt(tileNum).swampiness = 0f;
                Find.World.grid.tiles.ElementAt(tileNum).temperature = 0f;
                Find.World.grid.tiles.ElementAt(tileNum).biome = DefDatabase<BiomeDef>.GetNamed("SpaceBiome");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public Tile getTile(int tileNum) => Find.World.grid.tiles.ElementAt<Tile>(tileNum);


        // Planets
        public List<PlanetData> planets = new List<PlanetData>();
        public List<string> planetNames => planets.Select(s => s.DefName).ToList();
        public int planetCount => planets.Count();
        public PlanetData getPlanet(string DefName)
        {
            return this.planets.Find(p => p.DefName == DefName);
        }
        public void addPlanet(PlanetData data)
        {
            this.planets.Add(data);
        }
        public void addPlanet(string Name, IntVec3 location, bool isHome = false, Map map = null)
        {
            this.addPlanet(new PlanetData(Name, location, isHome, map));
        }
        public PlanetData getHomePlanet => planets.Find(s => s.isHome);





        public override void GameComponentOnGUI()
        {

            base.GameComponentOnGUI();

            if (Prefs.DevMode)
            {


                Vector2 vector2 = new Vector2((float)UI.screenWidth * 0.5f - (24f * 9), 3f);
                Find.WindowStack.ImmediateWindow(typeof(DebugWindowsOpener).GetHashCode(), new Rect(vector2.x, vector2.y, 24f * 3.5f, 24f).Rounded(), WindowLayer.GameUI, delegate
                {

                    WidgetRow row = new WidgetRow(24f * 0f, 0f, UIDirection.RightThenDown, 99999f, 4f);

                    if (row.ButtonIcon(ContentFinder<Texture2D>.Get("GUI/Reset", true), "Restart Rimworld", null, null, null, true))
                    {
                        GenCommandLine.Restart();
                    }
                    if (row.ButtonIcon(ContentFinder<Texture2D>.Get("WorldObjects/SpaceRegion", true), "Make Space Map"))
                    {
                        this.makeSystemMap();
                    }
                }, false, false, 0f);
            }


        }


    }




    public struct PlanetData
    {
        public Map map;
        public IntVec3 Location;
        public bool isHome;
        public string DefName;

        public bool hasMap => this.map != null;
        public string label => DefDatabase<ThingDef>.GetNamed(DefName).label;

        public PlanetData(string Name, IntVec3 location, bool isHome = false, Map map = null)
        {
            this.DefName = Name;
            this.map = map;
            this.Location = location;
            this.isHome = isHome;

        }


    }
}
