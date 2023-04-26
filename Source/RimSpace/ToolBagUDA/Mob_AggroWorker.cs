
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
namespace MobileObjects
{
    public class Mob_AggroWorker
    {

        public Mob_AggroProps props => mob.Def.Aggro;
        public Thing CurrentTarget;
        public IntVec3 AnchorPoint;
        public AggroTable table;
        public bool canAggro = true;
        public Mob mob;
        public HashSet<IntVec3> AggroArea => GenRadial.RadialCellsAround(AnchorPoint, props.LeashRadius, true).ToHashSet();
        public Thing getTopAggroTarget => this.mob.Map.mapPawns.AllPawnsSpawned.Find(s => s.ThingID == table.top);
        public bool HasAggro => table.unit.Any();
        private IntVec3 CurTargetPos;

        public IntVec3 CurTargetLoc
        {
            get
            {
                return CurTargetPos;
            }
            set
            {
                this.CurTargetPos = value;
            }
        }
        public IntVec3 getNewTargetLoc
        {
            get
            {
                if (HasAggro)
                {
                    if (props.AggroBehavior == AggroBehavior.Charge)
                    {
                        CurTargetPos = CurrentTarget.Position;
                    }
                    else if (props.AggroBehavior == AggroBehavior.Hide)
                    {
                        CurTargetPos = CurrentTarget.Position;
                    }
                    else if (props.AggroBehavior == AggroBehavior.Range)
                    {
                        CurTargetPos = CurrentTarget.Position;
                    }
                    else if (props.AggroBehavior == AggroBehavior.Strafe)
                    {
                        CurTargetPos = CurrentTarget.Position;
                    }
                }
                else
                {
                    if (props.NoAggroBehavior == NonAggroBehavior.Idle)
                    {
                        CurTargetPos = mob.Position;
                    }
                    else if (props.NoAggroBehavior == NonAggroBehavior.Post)
                    {
                        CurTargetPos = mob.Position;
                    }
                    else if (props.NoAggroBehavior == NonAggroBehavior.Wander)
                    {
                        CurTargetPos= mob.Position + new IntVec3((mob.Position.x + (Rand.Bool ? 1 : -1)), 0, (mob.Position.z + (Rand.Bool ? 1 : -1))); 
                    }
                }
                return CurTargetPos;
            }

        }


        public Mob_AggroWorker(Mob mob)
        {
            this.mob = mob;
            this.table = new AggroTable();
        }
        //targeting

        public void TickTargeting()
        {
            if (!HasAggro) CurrentTarget = null;
            else
            {
                if (CurrentTarget != getTopAggroTarget)
                {
                    CurrentTarget = getTopAggroTarget;
                }
            }
        }


        //Aggro 
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!HasAggro && canAggro) DropAnchor();
            this.table.AddAggroToUnit(damageInfo.Instigator.ThingID, damageInfo.Amount * props.AggroPerDamage);
        }

        public void NewSight(Thing thing)
        {
           
            if (!HasAggro && canAggro) DropAnchor();
            if (!table.isAggroed(thing.ThingID))
            {
                DB.Msg("NewSight() 2: " + thing.ThingID);
                this.table.AddAggroToUnit(thing.ThingID, props.AggroOnSight);
            }
        }

        public void AggroEvent(Thing thing, float amount)
        {
            if (!HasAggro && canAggro) DropAnchor();
            this.table.AddAggroToUnit(thing.ThingID, amount);
        }


        //Leashing

        public void LeashNow()
        {
            table.dropAllAggro();
            this.canAggro = false;
            
            
            //path target to anchor point
        }
        public void DropAnchor()
        {
            this.AnchorPoint = mob.Position;
            DB.Msg("DropAnchor() 1: " + AnchorPoint.ToString());
        }
        public void TickLeash()
        {
            if (!AggroArea.Contains(mob.Position)) LeashNow();
        }




        //Vision

        public void TickVision()
        {
            if (mob.Spawned)
            {
                foreach (var item in mob.Map.mapPawns.AllPawnsSpawned.FindAll(s => Vision.Contains(s.Position)))
                {
                    
                    // if (props.AggroPlayerOnSight && item.Faction.Equals(Faction.OfPlayer))
                    {
                        NewSight(item);
                    }
                   // if (props.AggroNonPlayerOnSight && !item.Faction.Equals(Faction.OfPlayer))
                    {
                        //NewSight(item);
                    }
                }
            }
        }


        public HashSet<IntVec3> Vision
        {
            get
            {
                if (mob.Rotation == Rot4.East)
                {
                    return EastSight;
                }
                else if (mob.Rotation == Rot4.West)
                {
                    return WestSight;
                }
                else if (mob.Rotation == Rot4.North)
                {
                    return NorthSight;
                }
                else //(mob.Rotation == Rot4.South)
                {
                    return SouthSight;
                }
            }
        }

        public HashSet<IntVec3> sight => GenRadial.RadialCellsAround(mob.Position, props.AggroRadius, true).ToHashSet();
        public HashSet<IntVec3> WestSight => sight.Where(s => s.x < 0 && Mathf.Abs(s.x) > Mathf.Abs(s.z)).ToHashSet();
        public HashSet<IntVec3> EastSight => sight.Where(s => s.x > 0 && Mathf.Abs(s.x) > Mathf.Abs(s.z)).ToHashSet();
        public HashSet<IntVec3> NorthSight => sight.Where(s => s.z > 0 && Mathf.Abs(s.z) > Mathf.Abs(s.x)).ToHashSet();
        public HashSet<IntVec3> SouthSight => sight.Where(s => s.z < 0 && Mathf.Abs(s.z) > Mathf.Abs(s.x)).ToHashSet();
    }

}
