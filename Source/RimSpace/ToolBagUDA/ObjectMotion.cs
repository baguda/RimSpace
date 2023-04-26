using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI
{
    public abstract class ObjectMotion
    {
        public Vector3 Pos0 = new Vector3();
        public Vector3 Vel0 = new Vector3();
        public int Time0;
        private Vector3 Pos = new Vector3();
        private Vector3 Vel = new Vector3();
        private Vector3 Accl = new Vector3();
        public bool useGenFunc = false;
        public Vector3 PosVec => this.Pos;
        public Vector3 VelVec => this.Vel;

        private Func<int, Vector3, Vector3, Vector3, Vector3> NewtonPosFunc => delegate (int t, Vector3 x, Vector3 v, Vector3 a)
        {
            return (x + (v * t) + (0.5f * a * t * t));
        };
        private Func<int, Vector3, Vector3, Vector3> NewtonVelFunc => delegate (int t, Vector3 v, Vector3 a)
        {
            return ((v) + (a * t));
        };
        public Func<int, Vector3, Vector3, Vector3, Vector3> GenPosFunc;
        public Func<int, Vector3, Vector3, Vector3> GenVelFunc;
        public ObjectMotion()
        {

        }
        public ObjectMotion(Vector3 InitialPosition, Vector3 InitialVelocity)
        {
            initMotion(InitialPosition, InitialVelocity);


        }
        public void initMotion(Vector3 InitialPosition, Vector3 InitialVelocity)
        {
            this.Pos0 = InitialPosition;
            this.Vel0 = InitialVelocity;
            this.Pos = this.Pos0;
            this.Vel = this.Vel0;
            this.Time0 = Find.TickManager.TicksGame;

        }
        public void updateMotion(int deltaTime, Vector3 Acceleration, bool inTicks = true)
        {
            this.Accl = Acceleration;
            this.Pos = updatePosition(deltaTime, Pos, Vel, Accl, useGenFunc, inTicks);
            this.Vel = updateVelocity(deltaTime, Vel, Accl, useGenFunc, inTicks);

        }
        public Vector3 updatePosition(int deltaTime, Vector3 Position, Vector3 Velocity, Vector3 Acceleration, bool useGenFunc = false, bool inTicks = true)
        {
            deltaTime *= inTicks ? 60 : 1;
            if (!useGenFunc)
            {
                return NewtonPosFunc.Invoke(deltaTime, Position, Velocity, Acceleration);
            }
            else
            {
                return GenPosFunc.Invoke(deltaTime, Position, Velocity, Acceleration);
            }
        }
        public Vector3 updateVelocity(int deltaTime, Vector3 Velocity, Vector3 Acceleration, bool useGenFunc = false, bool inTicks = true)
        {
            deltaTime *= inTicks ? 60 : 1;
            if (!useGenFunc)
            {
                return NewtonVelFunc.Invoke(deltaTime, Velocity, Acceleration);
            }
            else
            {
                return GenVelFunc.Invoke(deltaTime, Velocity, Acceleration);
            }
        }
        public Vector3 doPostionTick()
        {
            var deltaTime = 1;
            if (!useGenFunc)
            {
                return NewtonPosFunc.Invoke(deltaTime, Pos, Vel, Accl);
            }
            else
            {
                return GenPosFunc.Invoke(deltaTime, Pos, Vel, Accl);
            }
        }
        public Vector3 doVelocityTick()
        {
            var deltaTime = 1;
            if (!useGenFunc)
            {
                return NewtonVelFunc.Invoke(deltaTime, Vel, Accl);
            }
            else
            {
                return GenVelFunc.Invoke(deltaTime, Vel, Accl);
            }
        }

        public Vector3 getPostionAtTime(int tick)
        {
            var deltaTime = tick - Time0;
            deltaTime *= 60;
            if (!useGenFunc)
            {
                return NewtonPosFunc.Invoke(deltaTime, Pos, Vel, Accl);
            }
            else
            {
                return GenPosFunc.Invoke(deltaTime, Pos, Vel, Accl);
            }
        }
        public Vector3 getVelocityAtTime(int tick)
        {
            var deltaTime = tick - Time0;
            deltaTime *= 60;
            if (!useGenFunc)
            {
                return NewtonVelFunc.Invoke(deltaTime, Vel, Accl);
            }
            else
            {
                return GenVelFunc.Invoke(deltaTime, Vel, Accl);
            }
        }


        public void TickMotion(Vector3 Acceleration = new Vector3())
        {

            this.Accl = Acceleration;
            this.Pos = this.doPostionTick();
            this.Vel = this.doVelocityTick();
        }
        public void TickMotion(Vector3 Velocity = new Vector3(), Vector3 Acceleration = new Vector3())
        {

            this.Vel = Velocity;
            this.Accl = Acceleration;
            this.Pos = this.doPostionTick();
        }

        public virtual IEnumerable<Vector3> getPath(Map map, int Duration = 2400)
        {

            Vector3 TmpPos = Pos0;
            Vector3 TmpAccl = new Vector3(0f, 0f, 0f);
            Vector3 TmpVel = Vel0;
            for (int dt = 0; dt <= Duration; dt++)
            {
                //foreach (var point in getAoE(TmpPos.ToIntVec3(), dt))
                {
                    yield return TmpPos;// point.ToVector3();
                }

                TmpPos = updatePosition(1, TmpPos, TmpVel, TmpAccl, useGenFunc, false);
                TmpVel = updateVelocity(1, TmpVel, TmpAccl, useGenFunc, false);
            }
        }



    }


}
