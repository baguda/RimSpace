using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace RimSpace
{
    public class MapComp_SpaceMap : MapComponent
    {
        
        public GameComp_StarSystem starSystem => Current.Game.GetComponent<GameComp_StarSystem>();


        
        public HashSet<IntVec3> knownSpace = new HashSet<IntVec3>();
        public List<CompPlanet> planets => this.map.listerThings.AllThings.FindAll(s => s.def.HasComp(typeof(CompPlanet))).Select(f => f.TryGetComp<CompPlanet>()).ToList();
        public List<Pawn> playerShips => map.mapPawns.AllPawns.Where(s=>s.def.HasComp(typeof(CompSpaceship))).ToList();
        public bool isSpace => this.map.Parent.def.defName.Equals("OrbitalLocation");
        public List<string> planetNames => planets.Select(s => s.DefName).ToList();
        public int planetCount => planets.Count();
        public CompPlanet getHomePlanet => planets.Find(s => s.isHome);


        public MapComp_SpaceMap(Map map) : base(map)
        {
            base.map = map;
            
        }

        public void FilterNonSpacers()
        {
            this.map.listerThings.AllThings.FindAll(thing => thing.def.HasComp(typeof(CompSpacer)) == false)
                                            .ForEach(delegate (Thing NotSpaceWorthyThing) 
                                            { 
                                                NotSpaceWorthyThing.Destroy(DestroyMode.Vanish);
                                            });
        }
        public IEnumerable<Gizmo> panel()
        {
            int ind = 0;
            int inx = 0;
            foreach (Pawn pawn in playerShips)
            {
                Gizmo_ShipPanel ppan = new Gizmo_ShipPanel();
                ppan.Spaceship = pawn;
                ppan.comp = pawn.GetComp<CompSpaceship>();
                ppan.queueX = inx;
                ppan.queueZ = ind;

                ind++;
                if (ind % 5 == 0) inx++;
                yield return ppan;
            }
        }
        public CompPlanet getPlanet(string DefName)
        {
            return this.planets.Find(p => p.DefName == DefName);
        }
        public CompPlanet getPlanet(int PlanetID)
        {
            return this.planets.Find(p => p.planetThingID == PlanetID);
        }
        public void addPlanet(CompPlanet data)
        {
            this.planets.Add(data);
        }
        public void addPlanet(Building planet)
        {
            this.addPlanet(planet.TryGetComp<CompPlanet>());
        }
        public void SetAllFogged()
        {
            CellIndices cellIndices = this.map.cellIndices;
            if (map.fogGrid == null)
            {
                map.fogGrid.fogGrid = new bool[cellIndices.NumGridCells];
            }
            foreach (IntVec3 c in this.map.AllCells)
            {
                map.fogGrid.fogGrid[cellIndices.CellToIndex(c)] = true;
            }
            if (Current.ProgramState == ProgramState.Playing)
            {
                this.map.roofGrid.Drawer.SetDirty();
            }
        }
        public override void MapComponentTick()
        {
            if (isSpace)
            {
                this.FilterNonSpacers();
                map.skyManager.ForceSetCurSkyGlow(1f);
                map.skyManager.SkyManagerUpdate();
            }
                
            base.MapComponentTick();
        }
        public override void MapComponentOnGUI()
        {

            GizmoGridDrawer.DrawGizmoGrid(panel(), 1000, out Gizmo ds);
        }

        public override void MapGenerated()
        {

            //if (!isSpace) this.SetAllFogged();
            base.MapGenerated();
           
        }


    }

    public class SpaceFog
    {
        public BoolGrid grid;
        public Map map;

        public SpaceFog(Map map)
        {
            this.map = map;
            this.grid = new BoolGrid(map);
            grid.Clear();
        }

        public void discoverCell(IntVec3 cell)
        {
            grid.Set(cell, true);
            
        }

        public void DrawGrid()
        {

        }
        public void DrawCellFog(Vector3 position)
        {
            Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, MaterialPool.MatFrom("UI/Overlays/FuelingPort", ShaderDatabase.GasRotating), 0);
        }
        public void DrawCellFog(IntVec3 position)
        {
            Graphics.DrawMesh(MeshPool.plane10, position.ToVector3Shifted(), Quaternion.identity, MaterialPool.MatFrom("UI/Overlays/FuelingPort", ShaderDatabase.GasRotating), 0);
        }
        // Token: 0x0400620E RID: 25102
        private static readonly Material FuelingPortCellMaterial = MaterialPool.MatFrom("UI/Overlays/FuelingPort", ShaderDatabase.Transparent);
    }

    
}
