using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MobileObjects
{
    public class Mob_Path
    {

        public Mob mob;
        public Mob_MotionTracker curMotion;
        public int moveDuration;
        public IntVec3 TargetLoc;
        public IntVec3 OriginLoc;
        public MapGenFloatGrid Igrid;

        public IntVec3 curCell = new IntVec3();
        public IntVec3 PrevCell = new IntVec3();
        public IntVec3 NextCell = new IntVec3();

        public bool stopped = false;
        public float DurationScaler = 1f;

        public Vector3 ExactLocation => curMotion.curLoc;


        public Mob_AggroWorker aggro => mob.MobAggro;
        public Map map => mob.Map;


        public Mob_Path(Mob mob, IntVec3 TargetLocation)
        {
            DB.Msg("Mob_Path 1: " + TargetLocation.ToString()); ;
            this.mob = mob;
            TargetLoc = TargetLocation;
            OriginLoc = mob.Position;

            curCell = OriginLoc;

            Igrid = MakeIntegrationGrid(TargetLoc, 50f);
            NextCell = getNextCell();
            this.curMotion = new Mob_MotionTracker(this.curCell, this.NextCell, (int)(DurationScaler * aggro.props.moveDuration));

        }

        public bool TickMovement() // checks to see if motion is finished then starts next on path. return true when motion is finished
        {
            if (!stopped)
            {
                if (curMotion != null && curMotion.finished)
                {
                    startNewMotion();
                    return true;
                }

                FaceNextCell();

            }
            return false;
        }
        public void startNewMotion()
        {
            this.PrevCell = curCell;
            this.curCell = NextCell;
            this.NextCell = getNextCell();
            this.curMotion = new Mob_MotionTracker(this.curCell, this.NextCell, (int)(DurationScaler * aggro.props.moveDuration));
        }

        public MapGenFloatGrid MakeIntegrationGrid(IntVec3 target, float range)
        {
           // DB.Msg("MakeIntegrationGrid 1: "+target.ToString());
            HashSet<IntVec3> SetList = new HashSet<IntVec3>();
            HashSet<IntVec3> GetList = new HashSet<IntVec3>();
            MapGenFloatGrid Igrid = new MapGenFloatGrid(map);
            Igrid[target] = 0;
            SetList.Add(target);
            for(int ind =1; ind < Mathf.Max(mob.Map.Size.x, mob.Map.Size.z); ind++)
            {
               // DB.Msg("MakeIntegrationGrid 2..: " + ind.ToString()); 
                GetList = mob.Map.AllCells.Where(s => ( //s.x == target.x + ind || s.x == target.x - ind || s.z == target.z + ind || s.z == target.z - ind


                s.x <= target.x + ind && s.x >= target.x - ind && s.z <= target.z + ind && s.z >= target.z - ind 
                && !s.Equals(target) && !SetList.Contains(s)
                
                ) && s.InBounds(mob.Map)).ToHashSet();
               // DB.Msg("MakeIntegrationGrid 3..: " + GetList.Count.ToString().ToString());
                foreach (var point in GetList)
                {
                    Igrid[point] = ind + mob.Map.pathing.Normal.pathGrid.PerceivedPathCostAt(point);
                   // DB.Msg("MakeIntegrationGrid 4..: " + Igrid[point].ToString());
                }
                SetList.AddRange(GetList);
                GetList.Clear();
            }
            return Igrid;

        }
        public IntVec3 getNextCell() // select adjacent cell with lowest rating 
        {
            
            List<IntVec3> buffer = new List<IntVec3>();
            for (int ind = -1; ind <= 1; ind++)
            {
                for (int inx = -1; inx <= 1; inx++)
                {
                    IntVec3 loc = new IntVec3(curCell.x + inx, curCell.y, curCell.z + ind);
                    if (loc != curCell || loc != PrevCell)
                    {
                        buffer.Add(loc);
                    }
                }
            }
            var rr = buffer.Find(s => Igrid[s] == buffer.Select(d => Igrid[d]).Min());
            DB.Msg("getNextCell 1: "+rr.ToString());
            return rr;

        }
        public void FaceNextCell()
        {
            if (this.NextCell == this.curCell)
            {
                return;
            }
            float angle = (this.NextCell - this.curCell).ToVector3().AngleFlat();
            this.mob.Rotation = Pawn_RotationTracker.RotFromAngleBiased(angle);
        }
        public Vector3 CalculateVectorBetweenPoints(IntVec3 origin, IntVec3 Target, float Amplitude)
        {
            float dx = (Target.x - origin.x);
            dx = dx != 0 ? dx : 0.001f;
            float angleTmp = (float)Math.Atan((Target.z - origin.z) / dx) + (dx < 0 ? 3.14f : 0f);
            return new Vector3(Amplitude * (float)Math.Cos(angleTmp), 0, Amplitude * (float)Math.Sin(angleTmp));


        }
       


    }


    public class Mob_Mover // controls the path the mob is on, updates path to current target or new location if needed+
    {
        public Mob mob;
        public Mob_Path curPath;
        public virtual Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation((curPath.NextCell.ToVector3Shifted() - curPath.curCell.ToVector3Shifted()).Yto0());
            }
        }
        public Vector3 ExactLocation => curPath.ExactLocation;
        public IntVec3? curEndLoc
        {
            get
            {
                if (curPath != null)
                {
                    return curPath.TargetLoc;
                }
                else return null;
            }
        }
        public bool hasPath => curPath != null;

        public Mob_Mover(Mob mob)
        {
            this.mob = mob;
        }
        public void startNewPath(IntVec3 TargetLocation) // main input for target location
        {
            curPath = new Mob_Path(mob, TargetLocation);

        }
        public void stopCurPath() { }

        public void wipeCurPath()
        {
            this.curPath = null;
        }

        public bool checkLocation(IntVec3 location)
        {
            DB.Msg("checkLocation: ");

           DB.Msg("checkLocation: " + curEndLoc != null ? curEndLoc.ToString() : "null" + location.ToString());
            DB.Msg("checkLocation: " + location.ToString());

            return curEndLoc != null?location.Equals(curEndLoc):true;
        }

        public void TickMover()
        {
            // if no path then do nothing, wait for aggro worker to pass a target location, in which case, start new path to new location,
            // if on path then tick the path motion, on motion change, check for updated target location, if updated then stop current path and start a new path 


            if (hasPath)  // has
            {
                DB.Msg("TickMover 1: ");
                if (curPath.TickMovement())
                {
                    var t = mob.MobAggro.getNewTargetLoc;
                    DB.Msg("TickMover 2: " +t.ToString());
                    if (!checkLocation(t))
                    {
                        stopCurPath();
                        startNewPath(t);
                    }
                }
            }
            else
            {
                var t = mob.MobAggro.getNewTargetLoc;
                if (checkLocation(t))
                {
                    DB.Msg("TickMover 3: " + t.ToString());
                    startNewPath(t);
                }
            }
        }

    }
    public class Mob_MotionTracker //sprite movement tracker between cells, 
    {
        public IntVec3 startPos;
        public IntVec3 endPos;

        public int startTick;
        public int duration;

        public Mob_MotionTracker(IntVec3 StartPos, IntVec3 endPos, int Ticks)
        {
            this.duration = Ticks;
            this.startPos = StartPos;
            this.endPos = endPos;
            this.startTick = curTick;

        }
        public bool finished => curTick >= endTick;
        public int curTick => Find.TickManager.TicksGame;
        public int endTick => startTick + duration;
        public int ticksLeft => endTick - curTick;
        public float progress => 1f - ((float)ticksLeft / (float)duration);
        public Vector3 startLoc => startPos.ToVector3Shifted();
        public Vector3 endLoc => endPos.ToVector3Shifted();
        public Vector3 difVec => endLoc - startLoc;
        public virtual Vector3 curLoc => this.startLoc.Yto0() + (difVec).Yto0() * this.progress;

    }
}
