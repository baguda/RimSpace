using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimSpace
{

    public class CompPlanet : ThingComp
    {

        public CompProperties_Planet Props => this.props as CompProperties_Planet;
        public float proxyDistance = 5f;
        private PlanetCategory planetCategoryInt;
        public int planetThingID => this.parent.thingIDNumber;
        private sbyte groundMapIndex;
        public IntVec3 GroundLandingPoint;

        public Pawn ProxSpaceship => this.parent.Map.GetComponent<MapComp_SpaceMap>().playerShips.Find(s => GenRadial.RadialCellsAround(this.parent.Position, proxyDistance, false).ToList().Contains(s.Position));

        public override void Initialize(CompProperties props)
        {
            // this.planetCategoryInt = Props.getPlanetCategory;
            base.Initialize(props);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (ProxSpaceship != null)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Land On Planet";
                command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
                command_Action.icon = CompLaunchShip.LaunchCommandTex;
                command_Action.alsoClickIfOtherInGroupClicked = false;
                command_Action.action = delegate ()
                {

                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Land",
                        delegate
                        {


                        }, false, null, WindowLayer.Dialog));
                    return;

                };
                bool flag = true;

                if (flag) { command_Action.Disable("Needs Pilot with Mechlink Implant"); }
                //if (!starSystem.hasStation && !starSystem.hasLagrange) { command_Action.Disable("No Space Station to Target"); }
                yield return command_Action;
            }



            yield break;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            return base.CompFloatMenuOptions(selPawn);
        }

        public override void PostExposeData()
        {

            Scribe_Values.Look<sbyte>(ref groundMapIndex, "groundMapIndex");
            base.PostExposeData();
        }


        public Building Planet => this.parent as Building;
        public Map GroundMap
        {
            get
            {
                if (this.groundMapIndex >= 0)
                {
                    return Find.Maps[(int)this.groundMapIndex];
                }
                return null;
            }
            set
            {
                this.groundMapIndex = (value != null) ? (sbyte)value.Index : (sbyte)-1;
                
            }
        }
        public bool isWormholeBlg => this.DefName.Equals("RimWormhole");
        public bool isStarBlg => this.DefName.Equals("RimSun");
        public Map StarMap => Planet.Map;
        public IntVec3 Location => Planet.Position;
        public string DefName => Planet.def.defName;
        public CompPlanet Comp => Planet.GetComp<CompPlanet>();
        public List<IntVec3> localSpace => GenRadial.RadialCellsAround(this.Location, 5f, false).ToList();
        public WorldObject_Wormhole wormhole => StarMap.Parent as WorldObject_Wormhole;
        public bool hasGroundMap => this.GroundMap != null;
        public string label => DefDatabase<ThingDef>.GetNamed(DefName).label;
        public bool isHome => Comp.Props.isHome ;

    }












    public class CompPlanetX : ThingComp 
    {

        public CompProperties_Planet Props => this.props as CompProperties_Planet;
        public float proxyDistance = 5f;
        private PlanetCategory planetCategoryInt;
        public PlanetDataX data;
        public IntVec3 GroundLandingPoint;

        public Pawn ProxSpaceship => this.parent.Map.GetComponent<MapComp_SpaceMap>().playerShips.Find(s => GenRadial.RadialCellsAround(this.parent.Position, proxyDistance, false).ToList().Contains(s.Position));

        public override void Initialize(CompProperties props)
        {
           // this.planetCategoryInt = Props.getPlanetCategory;
            base.Initialize(props);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (ProxSpaceship != null)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Land On Planet";
                command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
                command_Action.icon = CompLaunchShip.LaunchCommandTex;
                command_Action.alsoClickIfOtherInGroupClicked = false;
                command_Action.action = delegate ()
                {
                    
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Land",
                            delegate 
                            {
                                
                            
                            }, false, null, WindowLayer.Dialog));
                        return;

                };
                bool flag = true;

                if (flag) { command_Action.Disable("Needs Pilot with Mechlink Implant"); }
                //if (!starSystem.hasStation && !starSystem.hasLagrange) { command_Action.Disable("No Space Station to Target"); }
                yield return command_Action;
            }

          

            yield break;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            return base.CompFloatMenuOptions(selPawn);
        }

        public override void PostExposeData()
        {
            Scribe_Deep.Look<PlanetDataX>(ref data, "data");
            base.PostExposeData();
        }
    

    
    }

    public enum PlanetCategory : byte
    {
        //No interaction
        Deadworld = 0,
        JunkWorld = 1,
        OceanicWorld = 2,
        ToxicWorld = 3,
        Iceworld = 4,
        Glassworld = 5,

        //Trade Option
        IndustrialWorld = 6,
        Midworld = 7,
        Glitterworld = 8,
        FarmingWorld = 9,
        MineralWorld = 10, 

        //Trade and Mission option 
        AnimalWorld = 11,
        MedievalWorld = 12,
        Rimworld = 13,

        //Special 
        TranscendentWorld = 14,


    }

    public struct PlanetDataX : IExposable
    {

        public Building Planet => PlanetThing as Building;
        private int planetThingID;
        private sbyte groundMapIndex;
        public Thing PlanetThing
        {
            get
            {
                if (this.planetThingID >= 0)
                {
                    foreach(var thing in Current.Game.GetComponent<GameComp_StarSystem>().LocalMap.listerThings.AllThings)
                    {
                        if (thing.thingIDNumber == planetThingID) return thing;
                    }
                }
                return null;
            }
            set
            {
                this.planetThingID = (value != null) ? (sbyte)value.thingIDNumber : (sbyte)-1;

            }
        }
        public Map GroundMap
        {
            get
            {
                if (this.groundMapIndex >= 0)
                {
                    return Find.Maps[(int)this.groundMapIndex];
                }
                return null;
            }
            set
            {
                this.groundMapIndex = (value != null) ? (sbyte)value.Index :  (sbyte)-1;

            }
        }
        public bool isWormholeBlg => this.DefName.Equals("RimWormhole");
        public bool isStarBlg => this.DefName.Equals("RimSun"); 
        public Map StarMap => Planet.Map;
        public IntVec3 Location => Planet.Position;
        public string DefName => Planet.def.defName;
        public CompPlanet Comp => Planet.GetComp<CompPlanet>();
        public List<IntVec3> localSpace => GenRadial.RadialCellsAround(this.Location, 5f, false).ToList();
        public WorldObject_Wormhole wormhole => StarMap.Parent as WorldObject_Wormhole;
        public bool hasGroundMap => this.GroundMap != null;
        public string label => DefDatabase<ThingDef>.GetNamed(DefName).label;
        public bool isHome => Comp.Props.isHome;

        public void ExposeData() 
        {
            Scribe_Values.Look<sbyte>(ref groundMapIndex, "groundMapIndex");
            Scribe_Values.Look<int>(ref planetThingID, "planetThingID");
        }

        public PlanetDataX(Building planet)
        {
            this.planetThingID = planet.thingIDNumber;
            this.groundMapIndex = (sbyte)-1;

        }
        public PlanetDataX(int thingIDNumber, int groundMapIndex = -1)
        {
            this.planetThingID = thingIDNumber;
            this.groundMapIndex = (sbyte)groundMapIndex;

        }

    }

}












/*
        
        
        Deadworld, Nothing 
        AnimalWorld, mission
        MedievalWorld, mission trade
        Steamworld, mission trade
        Midworld, mission trade
        Urbworld, mission trade
        Glitterworld, trade
        Rimworld, mission trade
        ToxicWorld, Nothing
        Glassworld, Nothing
        TranscendentWorld, Nothing, for now...

        IndustrialWorld, trade
        FarmingWorld, trade
        JunkWorld, mission trade
        Iceworld, nothing
        MineralWorld, 
        OceanicWorld
 
 
using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
 */
