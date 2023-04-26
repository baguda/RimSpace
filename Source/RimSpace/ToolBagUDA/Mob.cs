using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

using System.Linq;
using Verse.AI.Group;

namespace MobileObjects
{
    public class Mob : ThingWithComps
    {
        public TargetInfo CurrentTarget => MobAggro.CurrentTarget;
        public MobDef Def => def as MobDef;
        public Mob_AggroWorker MobAggro;
        public Mob_Mover MobMover;
        public Vector3 Location;
        public Rot4 Heading;

        public Mob()
        {
            MobAggro = new Mob_AggroWorker(this);
            MobMover = new Mob_Mover(this);
        }
        public override Vector3 DrawPos => Location;

        public override void Draw()
        {
            //Graphics.DrawMesh(MeshPool.GridPlane(this.def.graphicData.drawSize), Location, MobMover.ExactRotation, this.def.DrawMatSingle, 0);
            //base.Draw();
        }
        public override void Tick()
        {

            MobAggro.TickVision();
            MobAggro.TickTargeting();
            MobMover.TickMover();
            MobAggro.TickLeash();
            Location = MobMover.ExactLocation;
            base.Position = this.Location.ToIntVec3();
            base.Tick();
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            this.Location = base.Position.ToVector3Shifted();
            base.SpawnSetup(map, respawningAfterLoad);
        }

    }


    [StaticConstructorOnStartup]
    public static class MobGenerator
    {
        public static Mob MakeMob(string MobDefName, IntVec3 Location, Map map, Faction faction)
        {
            Thing thing = ThingMaker.MakeThing(DefDatabase<MobDef>.GetNamed(MobDefName));
            thing.SetFaction(faction);
            GenSpawn.Spawn(thing, Location, map);
            return thing as Mob;
        }




        [DebugAction("Spawning", "Spawn Mob", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnMob()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (MobDef localDef in from kd in DefDatabase<MobDef>.AllDefs
                                        orderby kd.defName
                                        select kd)
            {
                list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Tool, delegate ()
                {
                    Faction faction = Faction.OfAncientsHostile;
                    Mob newMob = MobGenerator.MakeMob(localDef.defName, UI.MouseCell(), Find.CurrentMap, faction);


                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

    }
    [StaticConstructorOnStartup]
    public static class DB
    {

        public static void Msg(string text, bool reset = false)
        {
            if (Prefs.DevMode)
            {
                Verse.Log.Message(text, reset);
            }
        }
    }

}
