using System.Collections.Generic;
using Verse;
namespace MapToolBag
{

    public class IntVecShape
    {
        public string ShapeNameTag;
        public string ShapeDefTag;
        public string ShapeStuffTag;
        public string ShapeName;
        public IntVec3 PointA = new IntVec3();
        public IntVec3 PointB = new IntVec3();
        public IntVec3 PointC = new IntVec3(1, 0, 1);
        public IntVec3 PointD = new IntVec3();
        public bool isNegative = false;

        public bool isHollow = false;
        public IntVecShape()
        {

        }
        public IntVecShape(string newShapeName, IntVec3 newPointA, IntVec3 newPointB, IntVec3 newPointC, IntVec3 newPointD, bool IsNegative = false)
        {
            this.PointA = newPointA;
            this.PointB = newPointB;
            this.PointC = newPointC;
            this.PointD = newPointD;
            this.ShapeName = newShapeName;
            this.isNegative = IsNegative;
        }
        public IntVecShape(string newShapeName, string ShapeNameTag, string ShapeDefTag, string ShapeStuffTag, IntVec3 newPointA, IntVec3 newPointB, IntVec3 newPointC, IntVec3 newPointD, bool IsNegative = false, bool isHollow = false)
        {

            this.PointA = newPointA;
            this.PointB = newPointB;
            this.PointC = newPointC;
            this.PointD = newPointD;
            this.ShapeDefTag = ShapeDefTag;
            this.ShapeNameTag = ShapeNameTag;
            this.ShapeStuffTag = ShapeStuffTag;
            this.ShapeName = newShapeName;
            this.isNegative = IsNegative;
            this.isHollow = isHollow;
        }
        public IntVecShape(string newShapeName, IntVec3 newPointA, IntVec3 newPointB, bool IsNegative = false)
        {
            this.PointA = newPointA;
            this.PointC = new IntVec3(1, 0, 1);
            this.PointB = newPointB;
            this.ShapeName = newShapeName;

            this.isNegative = IsNegative;
        }
        public IEnumerable<IntVec3> InsideArea()

        {

            if (this.ShapeName == "Line" || this.ShapeName == "line")
            {
                foreach (IntVec3 point in MapHandlerUtility.genLine(this.PointA, this.PointB, this.PointC.x, this.PointC.z, PointD.x > 0, PointD.z > 0))
                {
                    yield return point;
                }
            }
            else if (this.ShapeName == "Triangle" || this.ShapeName == "triangle")
            {
                foreach (IntVec3 point in MapHandlerUtility.triArea(this.PointA, this.PointB, this.PointC, isHollow, PointD.x, PointD.z))
                {
                    yield return point;
                }
            }
            else if (this.ShapeName.Equals("Rectangle") || this.ShapeName == "rectangle")
            {

                foreach (IntVec3 point in MapHandlerUtility.rectArea(this.PointA, this.PointB, isHollow))
                {

                    yield return point;
                }
            }
            else if (this.ShapeName.Equals("Circle") || this.ShapeName.Equals("circle"))
            {
                foreach (IntVec3 point in MapHandlerUtility.circArea(this.PointA, this.PointB, isHollow))
                {
                    yield return point;
                }
            }

            else if (this.ShapeName.Equals("Point") || this.ShapeName.Equals("point"))
            {
                yield return PointA;
            }
            else if (this.ShapeName.Equals("Points4") || this.ShapeName.Equals("point4"))
            {
                foreach (var point in new List<IntVec3>() { PointA, PointB, PointC, PointD })
                {
                    yield return point;
                }

            }
            else if (this.ShapeName.Equals("Points2") || this.ShapeName.Equals("point2"))
            {
                foreach (var point in new List<IntVec3>() { PointA, PointB })
                {
                    yield return point;
                }

            }
            else if (this.ShapeName.Equals("Points3") || this.ShapeName.Equals("point3"))
            {
                foreach (var point in new List<IntVec3>() { PointA, PointB, PointC })
                {
                    yield return point;
                }

            }
            else if (this.ShapeName.Equals("DrunkPath") || this.ShapeName.Equals("drunkPath"))
            {
                foreach (IntVec3 point in MapHandlerUtility.drunkenPath(this.PointA, this.PointB, this.PointC.x, this.PointC.z, this.PointD.x, this.PointD.z))
                {
                    yield return point;
                }

            }
            else if (this.ShapeName.Equals("Sector") || this.ShapeName.Equals("sector"))
            {

                /* foreach (IntVec3 point in MapHandlerUtility.coneArea(this.PointA, 0, 0,0))
                 {
                     yield return point;
                 }*/
            }

            else if (this.ShapeName.Equals("TargetRadius") || this.ShapeName.Equals("target"))
            {
                foreach (IntVec3 point in GenRadial.RadialCellsAround(this.PointA, this.PointC.z, true))
                {
                    yield return point;
                }
            }
            else if (this.ShapeName == "TargetLine" || this.ShapeName == "targetline")
            {

                foreach (IntVec3 point in MapHandlerUtility.genLine(this.PointA, this.PointB, this.PointC.x, this.PointC.x, true, true))
                {
                    yield return point;
                }
            }



            else
            {
                Log.Error("Invalid Shape Name: " + this.ShapeName + ".  Defaulting to Rectangle");
                foreach (IntVec3 point in MapHandlerUtility.rectArea(this.PointA, this.PointB)) yield return point;
            }

        }

    }
}
