using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace MapToolBag
{
    /// <summary>
    /// A collection of methods for int grid allocation 
    /// </summary>
    [StaticConstructorOnStartup]

    public static class MapHandlerUtility
    {

        public static IntVec3 getPointRadialOutward(Vector3 origin, Vector3 Target, float knockback)
        {
            float angleTmp = (float)Math.Atan((Target.z - origin.z) / (Target.x - origin.x)) + ((Target.x - origin.x) < 0 ? 3.14f : 0f);
            var kb = new Vector3(knockback * (float)Math.Cos(angleTmp), 0, knockback * (float)Math.Sin(angleTmp));

            return (Target + kb).ToIntVec3();

        }


        public static IEnumerable<IntVec3> coneArea(IntVec3 center, int radius, int theta_i, int theta_f)
        {
            foreach (var point in circArea(center - new IntVec3(radius, 0, radius), new IntVec3(2 * radius, 0, 2 * radius)))
            {
                if (center.DistanceTo(point) <= radius && inAngle(center, point, (float)theta_i * (3.14f / 180f), (float)theta_f * (3.14f / 180f))) yield return point;

            }

            //yield break;
        }

        public static bool inAngle(IntVec3 center, IntVec3 point, float theta_i, float theta_f)
        {
            var dz = (float)(point.z - center.z);
            var dx = (float)(point.x - center.x);
            var pt = Math.Atan(dz / dx) + (dx < 0 ? 3.14f : 0f);

            var dt = theta_f - theta_i;
            if (dt < 0)
            {
                if (pt > theta_i || pt < theta_f) return true;
            }
            else
            {
                if (pt > theta_i && pt < theta_f) return true;
            }
            return false;
        }

        public static float getDistance(IntVec3 startPoint, IntVec3 endPoint)
        {
            return (float)Math.Sqrt(((endPoint.x - startPoint.x) * (endPoint.x - startPoint.x)) + ((endPoint.y - startPoint.y) * (endPoint.y - startPoint.y)) + ((endPoint.z - startPoint.z) * (endPoint.z - startPoint.z)));
        }

        public static IEnumerable<IntVec3> GetPointsOnLine(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                yield return new IntVec3((steep ? y : x), 0, (steep ? x : y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }

        public static IEnumerable<IntVec3> pointsBrushed(IntVec3 location, int xVal, int zVal, bool pos, bool mirror)
        {
            /*
             * 
             * PM|it
             * 00|10
             * 01|11
             * 11|11
             * 10|01
             * 
             */

            if (xVal == 0 && zVal == 0) yield return location;
            int ci = 0, ct = 0, dx = xVal, dz = zVal;

            if (!pos || (pos && mirror)) ci = -1;
            if (pos || (!pos && mirror)) ct = 1;
            for (int xdx = ci * dx; xdx <= ct * dx; xdx++)
            {
                for (int zdz = ci * dz; zdz <= ct * dz; zdz++)
                {
                    yield return new IntVec3(location.x + xdx, location.y, location.z + zdz);
                }
            }

        }

        public static IEnumerable<IntVec3> rectArea(IntVec3 botLeftCorner, IntVec3 topRightCorner, bool hollow = false)
        {
            for (int dx = 0; dx < topRightCorner.x; dx++)
            {
                for (int dz = 0; dz < topRightCorner.z; dz++)
                {

                    if (!hollow) yield return new IntVec3(botLeftCorner.x + dx, 0, botLeftCorner.z + dz);
                    else
                    {
                        if ((dx == 0 || dz == 0) || (dx == topRightCorner.x - 1 || dz == topRightCorner.z - 1)) yield return new IntVec3(botLeftCorner.x + dx, 0, botLeftCorner.z + dz);
                    }
                }
            }
        }


        public static IntVec3 getRoomSize(HashSet<IntVec3> roomCells, IntVec3 location)
        {
            int safety = 1000;
            bool run = true;
            int pdx = 0, ndx = 0, pdz = 0, ndz = 0, value = 0;
            do
            {
                value++;
                if (!roomCells.Contains(new IntVec3(location.x, 0, location.z - value)) && ndz == 0)
                {
                    ndz = value;
                }
                if (!roomCells.Contains(new IntVec3(location.x - value, 0, location.z)) && ndx == 0)
                {
                    ndx = value;
                }
                if (!roomCells.Contains(new IntVec3(location.x, 0, location.z + value)) && pdz == 0)
                {
                    pdz = value;
                }
                if (!roomCells.Contains(new IntVec3(location.x + value, 0, location.z)) && pdx == 0)
                {
                    pdx = value;
                }
                run = (pdx == 0 || ndx == 0 || pdz == 0 || ndz == 0) && value < safety;

            } while (run);
            return new IntVec3(pdx + ndx + 1, 0, pdz + ndz + 1);
        }
        /// <summary>
        /// Scans cells out from location in x and z direction to find distance to room edge 
        /// </summary>
        /// <param name="roomCells"></param>
        /// <param name="location"></param>
        /// <returns>pdx,ndx,pdz,ndz.</returns>
        public static List<int> getRoomSizes(HashSet<IntVec3> roomCells, IntVec3 location)
        {
            int safety = 1000;
            bool run = true;
            int pdx = 0, ndx = 0, pdz = 0, ndz = 0, value = 0;
            do
            {
                value++;
                if (!roomCells.Contains(new IntVec3(location.x, 0, location.z - value)) && ndz == 0)
                {
                    ndz = value;
                }
                if (!roomCells.Contains(new IntVec3(location.x - value, 0, location.z)) && ndx == 0)
                {
                    ndx = value;
                }
                if (!roomCells.Contains(new IntVec3(location.x, 0, location.z + value)) && pdz == 0)
                {
                    pdz = value;
                }
                if (!roomCells.Contains(new IntVec3(location.x + value, 0, location.z)) && pdx == 0)
                {
                    pdx = value;
                }
                run = (pdx == 0 || ndx == 0 || pdz == 0 || ndz == 0) && value < safety;

            } while (run);
            return new List<int>() { pdx, ndx, pdz, ndz };
        }

        public static IEnumerable<IntVec3> drunkenPath(IntVec3 pointOne, IntVec3 pointTwo, int pathWidth, int Amplitude = 4, int Frequency = 4, int Phase = 1)
        {

            int dx = pointTwo.x - pointOne.x;
            int dz = (pointTwo.z - pointOne.z);
            var set = genLine(pointOne, pointTwo, Amplitude, Amplitude, true, true);

            List<IntVec3> vals = new List<IntVec3>() { pointOne };//(dx < 0 || dz < 0) ? new List<IntVec3>() { pointTwo }:
            bool wrtx = Math.Abs(dx) >= Math.Abs(dz);
            Frequency = Frequency + Rand.RangeInclusive(0, Phase);
            if (wrtx)
            {
                for (int ind = 1; ind < Frequency; ind++)
                {
                    if (dx < 0)
                    {
                        vals.Add(set.ToList().FindAll(s => s.x == pointOne.x - ind * (dx / Frequency)).RandomElement());
                    }
                    else
                    {
                        vals.Add(set.ToList().FindAll(s => s.x == pointOne.x + ind * (dx / Frequency)).RandomElement());
                    }
                }

            }
            else
            {
                for (int ind = 1; ind < Frequency; ind++)
                {

                    if (dz < 0)
                    {
                        vals.Add(set.ToList().FindAll(s => s.z == pointOne.z - ind * (dz / Frequency)).RandomElement());
                    }
                    else
                    {
                        vals.Add(set.ToList().FindAll(s => s.z == pointOne.z + ind * (dz / Frequency)).RandomElement());
                    }
                }
            }
            //vals.Add(pointTwo);
            vals.Add(pointTwo);// (dx < 0 || dz < 0) ? pointOne :
            for (int ind = 0; ind < vals.Count() - 1; ind++)
            {
                foreach (var loc in genLine(vals[ind], vals[ind + 1], pathWidth, pathWidth, true, true))
                {
                    //Prim.Log2("DP: " + loc.ToString());
                    yield return loc;
                }
            }


        }
        /// <summary>
        /// Returns a collection of points allocated between pointOne and pointTwo that will attempt to avoid points in Repulsives  
        /// </summary>
        public static IEnumerable<IntVec3> soberPath(IntVec3 pointOne, IntVec3 pointTwo, int pathWidth, HashSet<IntVec3> Repulsives, int Amplitude = 4, int Frequency = 4, int Phase = 1)
        {
            int dx = pointTwo.x - pointOne.x;
            int dz = pointTwo.z - pointOne.z;
            var set = genLine(pointOne, pointTwo, Amplitude, Amplitude, true, true);

            List<IntVec3> vals = new List<IntVec3>() { pointOne };
            bool wrtx = Math.Abs(dx) >= Math.Abs(dz);
            Frequency = Frequency + Rand.RangeInclusive(0, Phase);
            if (wrtx)
            {
                for (int ind = 1; ind < Frequency; ind++)
                {
                    vals.Add(getPointFurthestFrom(set.ToHashSet(), Repulsives.Where(c => c.x == pointOne.x + ind * (dx / Frequency)).ToHashSet()));
                    //vals.Add(set.ToList().FindAll(s => s.x == pointOne.x + ind * (dx / Frequency)).RandomElement());

                }

            }
            else
            {
                for (int ind = 1; ind < Frequency; ind++)
                {
                    vals.Add(getPointFurthestFrom(set.ToHashSet(), Repulsives.Where(c => c.z == pointOne.z + ind * (dz / Frequency)).ToHashSet()));

                    //vals.Add(set.ToList().FindAll(s => s.z == pointOne.z + ind * (dz / Frequency)).RandomElement());

                }
            }
            vals.Add(pointTwo);

            for (int ind = 0; ind < vals.Count() - 1; ind++)
            {
                foreach (var loc in genLine(vals[ind], vals[ind + 1], pathWidth, pathWidth, true, true))
                {
                    //Prim.Log2("DP: " + loc.ToString());
                    yield return loc;
                }
            }


        }
        public static void RandomPointsInQuads(IntVec3 LotSize, out IntVec3 Q1, out IntVec3 Q2, out IntVec3 Q3, out IntVec3 Q4)
        {
            Q1 = MapHandlerUtility.rectArea(new IntVec3(LotSize.x / 2, 0, LotSize.z / 2), new IntVec3(LotSize.x / 2, 0, LotSize.z / 2)).RandomElement();
            Q2 = MapHandlerUtility.rectArea(new IntVec3(0, 0, LotSize.z / 2), new IntVec3(LotSize.x / 2, 0, LotSize.z / 2)).RandomElement();
            Q3 = MapHandlerUtility.rectArea(new IntVec3(0, 0, 0), new IntVec3(LotSize.x / 2, 0, LotSize.z / 2)).RandomElement();
            Q4 = MapHandlerUtility.rectArea(new IntVec3(LotSize.x / 2, 0, 0), new IntVec3(LotSize.x / 2, 0, LotSize.z / 2)).RandomElement();
        }
        public static HashSet<IntVec3> RandomPointsInQuads(IntVec3 LotCorner, IntVec3 LotSize)
        {
            return new HashSet<IntVec3>()
            {
            MapHandlerUtility.rectArea(new IntVec3(LotCorner.x + LotSize.x / 2, 0, LotCorner.z+LotSize.z / 2), new IntVec3(LotSize.x / 2, 0, LotSize.z / 2)).RandomElement(),

            MapHandlerUtility.rectArea(new IntVec3(LotCorner.x +0, 0, LotCorner.z+LotSize.z / 2), new IntVec3(LotSize.x / 2, 0, LotSize.z / 2)).RandomElement(),
            MapHandlerUtility.rectArea(new IntVec3(LotCorner.x +0, 0, LotCorner.z+0), new IntVec3(LotSize.x / 2, 0,LotSize.z / 2)).RandomElement(),
            MapHandlerUtility.rectArea(new IntVec3(LotCorner.x +LotSize.x / 2, 0, LotCorner.z+0), new IntVec3(LotSize.x / 2, 0, LotSize.z / 2)).RandomElement()
            };

        }


        public static IEnumerable<IntVec3> circArea(IntVec3 origin, IntVec3 limit, bool hollow = false)
        {
            float h_coef = ((limit.x - origin.x) / 2) + origin.x;
            float k_coef = ((limit.z - origin.z) / 2) + origin.z;
            float A_coef = 1f / (float)((limit.x - h_coef) * (limit.x - h_coef));
            float B_coef = 1f / (float)((limit.z - k_coef) * (limit.z - k_coef));

            foreach (IntVec3 point in rectArea(origin, limit))
            {
                if ((A_coef * ((point.x - h_coef) * (point.x - h_coef))) + (B_coef * ((point.z - k_coef) * (point.z - k_coef))) <= 1)
                {
                    if (!hollow) yield return point;
                    else
                    {
                        if ((A_coef * ((point.x - h_coef) * (point.x - h_coef))) + (B_coef * ((point.z - k_coef) * (point.z - k_coef))) >= 0.9) yield return point;
                    }
                }
            }
        }
        public static IEnumerable<IntVec3> circArea(IntVec3 center, int radx, int radz, bool hollow = false)
        {
            IntVec3 limit = new IntVec3(center.x + radx, 0, center.z + radz);
            IntVec3 origin = new IntVec3(center.x - radx, 0, center.z - radz);

            float h_coef = ((limit.x - origin.x) / 2) + origin.x;
            float k_coef = ((limit.z - origin.z) / 2) + origin.z;

            float A_coef = 1f / (float)((limit.x - h_coef) * (limit.x - h_coef));
            float B_coef = 1f / (float)((limit.z - k_coef) * (limit.z - k_coef));

            foreach (IntVec3 point in rectArea(origin, limit))
            {
                if ((A_coef * ((point.x - h_coef) * (point.x - h_coef))) + (B_coef * ((point.z - k_coef) * (point.z - k_coef))) <= 1)
                {
                    if (!hollow) yield return point;
                    else
                    {
                        if ((A_coef * ((point.x - h_coef) * (point.x - h_coef))) + (B_coef * ((point.z - k_coef) * (point.z - k_coef))) >= 0.9) yield return point;
                    }
                }
            }
        }
        public static IEnumerable<IntVec3> genLine(IntVec3 pointOne, IntVec3 pointTwo, int radWidthx = 0, int radWidthz = 0, bool positive = true, bool mirrored = false)
        {
            foreach (IntVec3 point in GetPointsOnLine(pointOne.x, pointOne.z, pointTwo.x, pointTwo.z))
            {
                foreach (IntVec3 location in pointsBrushed(point, radWidthx, radWidthz, positive, mirrored))
                {
                    yield return location;
                }
            }
        }

        public static IEnumerable<IntVec3> triArea(IntVec3 pointA, IntVec3 pointB, IntVec3 pointC, bool hollow = false, int radWidthx = 1, int radWidthz = 1, bool positive = true, bool mirrored = true)
        {
            if (!hollow)
            {
                IntVec3 botLeftCorner = new IntVec3(Math.Min(pointA.x, Math.Min(pointB.x, pointC.x)), 0, Math.Min(pointA.z, Math.Min(pointB.z, pointC.z)));
                IntVec3 topRightCorner = new IntVec3(Math.Max(pointA.x, Math.Max(pointB.x, pointC.x)), 0, Math.Max(pointA.z, Math.Max(pointB.z, pointC.z)));
                foreach (IntVec3 point in MapHandlerUtility.rectArea(botLeftCorner, topRightCorner)) // map.AllCells.ToList().FindAll(v => (v.x >= botLeftCorner.x) && (v.x <= topRightCorner.x) && (v.z >= botLeftCorner.z) && (v.z <= topRightCorner.z)))
                {
                    if (isInsideTriangle(pointA, pointB, pointC, point))
                    {
                        yield return point;
                    }
                }
            }
            else
            {
                for (int ind = 1; ind <= 3; ind++)
                {
                    if (ind == 1)
                    {
                        foreach (IntVec3 point in genLine(pointA, pointB, radWidthx, radWidthz, positive, mirrored))
                        {
                            yield return point;
                        }

                    }
                    if (ind == 2)
                    {
                        foreach (IntVec3 point in genLine(pointA, pointC, radWidthx, radWidthz, positive, mirrored))
                        {
                            yield return point;
                        }

                    }
                    if (ind == 3)
                    {
                        foreach (IntVec3 point in genLine(pointC, pointB, radWidthx, radWidthz, positive, mirrored))
                        {
                            yield return point;
                        }

                    }
                }
            }
        }

        public static double trigArea(IntVec3 pointA, IntVec3 pointB, IntVec3 pointC)
        {

            return Math.Abs((pointA.x * (pointB.z - pointC.z) + pointB.x * (pointC.z - pointA.z) + pointC.x * (pointA.z - pointB.z)) / 2.0);

        }
        public static bool isInsideTriangle(IntVec3 pointA, IntVec3 pointB, IntVec3 pointC, IntVec3 pointP)
        {
            return (trigArea(pointA, pointB, pointC) == trigArea(pointP, pointB, pointC) + trigArea(pointA, pointP, pointC) + trigArea(pointA, pointB, pointP));
        }

        public static List<IntVec3> FilterShapes(List<IntVec3> Input, List<IntVecShape> Shapes)
        {

            foreach (IntVecShape shape in Shapes)
            {

                if (shape.isNegative)
                {
                    Input.RemoveAll(s => !shape.InsideArea().ToList<IntVec3>().Contains(s));

                }
                else
                {
                    var cc = shape.InsideArea().ToList<IntVec3>();
                    Input.RemoveAll(s => cc.Contains(s));
                }
            }
            return Input;
        }


        public static IEnumerable<IntVec3> ProxyNoised(HashSet<IntVec3> SpaceCells, HashSet<IntVec3> TargetCells, int radius, float Threshold, float Variance, bool inclusive = false)
        {
            HashSet<IntVec3> HCirc = new HashSet<IntVec3>();
            if (inclusive) SpaceCells.RemoveWhere(d => TargetCells.Contains(d));
            foreach (IntVec3 cell in SpaceCells)
            {
                HCirc = circArea(new IntVec3(cell.x - radius, 0, cell.z - radius), new IntVec3(2 * radius, 0, 2 * radius)).ToHashSet<IntVec3>();
                var Overlap = (float)TargetCells.Intersect(HCirc).Count();
                var Total = (float)HCirc.Count();
                float ff = Overlap / Total;

                if (ff >= (Rand.Value * Variance) + Threshold)
                {
                    yield return cell;
                }

            }
        }


        public static IEnumerable<IntVec3> OutlineAreaCells(HashSet<IntVec3> Input)
        {
            IntVec3 tmpPoint;

            foreach (IntVec3 point in Input)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {

                        tmpPoint = new IntVec3(point.x + dx, 0, point.z + dz);
                        if (!Input.Contains(tmpPoint))
                        {
                            yield return tmpPoint;
                        }
                    }
                }
            }
        }
        public static IEnumerable<IntVec3> AreaEdgeCells(HashSet<IntVec3> Input)
        {

            bool flag = false;
            foreach (IntVec3 point in Input)
            {
                flag = false;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (!Input.Contains(new IntVec3(point.x + dx, 0, point.z + dz)))
                        {
                            flag = true;
                        }
                    }
                }
                if (flag) yield return point;
            }
        }
        /// <summary>
        /// Determins if the 
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <param name="Input">The space the point is in</param>
        /// <param name="heading">Output of the heading away from the wall</param>
        /// <returns>False if in open, in corner, or in knook. True if next to wall</returns>
        public static bool hasHeading(IntVec3 point, HashSet<IntVec3> Input, out Rot4 heading)
        {

            heading = new Rot4();// Rot4.South;
            var li = Input.ToList().FindAll(s => (s.x == point.x + 1 && s.z == point.z) || (s.x == point.x - 1 && s.z == point.z) || (s.z == point.z + 1 && s.x == point.x) || (s.z == point.z - 1 && s.x == point.x));

            if (li.Any())
            {
                if (li.Count() != 3)
                {
                    return false;
                }
                else
                {
                    int bx = li.Select(s => s.x - point.x).Sum();
                    int bz = li.Select(s => s.z - point.z).Sum();
                    //Prim.Log2("hasHeading: bx,bz =  " + bx.ToString() + ", " + bz.ToString());
                    if (bx != 0)
                    {
                        heading = bx > 0 ? Rot4.East : Rot4.West;
                    }
                    else
                    {
                        heading = bz > 0 ? Rot4.North : Rot4.South;
                    }
                    return true;

                }
            }
            else
            {
                return false;
            }


        }
        public static IntVec3 getPointFurthestFrom(HashSet<IntVec3> spaceCells, HashSet<IntVec3> CellSet)
        {
            Dictionary<IntVec3, float> ds = new Dictionary<IntVec3, float>();
            foreach (var cell in spaceCells)
            {
                ds.Add(cell, CellSet.Min(s => s.DistanceTo(cell)));

            }
            return ds.Where(c => c.Value == ds.Max(s => s.Value)).First().Key;
        }


    }//class

   
}//space
