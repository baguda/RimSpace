using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Reflection;
using System.IO;

namespace MapToolBag
{
 

    public class CellSets : IExposable
    {
        private List<HashSet<IntVec3>> Sets = new List<HashSet<IntVec3>>();
        private List<string> SetNames = new List<string>();
        private List<string> SetDefNames = new List<string>();
        private List<string> SetStuffNames = new List<string>();
        private List<Tuple<int, bool, bool, bool>> setData = new List<Tuple<int, bool, bool, bool>>(); // Rot,Eraser,roofed,cleared
        public static HashSet<IntVec3> Set = new HashSet<IntVec3>();

        void IExposable.ExposeData()
        {
            int ind = 0;
            foreach (var set in Sets)
            {
                Set = set;
                Scribe_Collections.Look<IntVec3>(ref Set, "Set", LookMode.Value);
            }
            Scribe_Collections.Look<HashSet<IntVec3>>(ref Sets, "Sets", LookMode.Value);
            Scribe_Collections.Look<string>(ref SetDefNames, "SetDefNames", LookMode.Value);
            Scribe_Collections.Look<string>(ref SetStuffNames, "SetStuffNames", LookMode.Value);
            Scribe_Collections.Look<Tuple<int, bool, bool, bool>>(ref setData, "setData", LookMode.Value);
        }

        public CellSets()
        {
        }
        public CellSets(List<string> SetNames)
        {
            foreach (var item in SetNames)
            {
                AddSet(item, null);
            }
        }
        public CellSets(CellSets data1, CellSets data2)
        {
            foreach (var item in data1.SetNames)
            {
                AddSet(item, data1.getSetDefName(item), data1.getSetNamed(item), data1.getSetStuffName(item));
            }
            foreach (var item in data2.SetNames)
            {
                AddSet(item, data2.getSetDefName(item), data2.getSetNamed(item), data2.getSetStuffName(item));
            }
        }
        public CellSets(List<CellSets> data)
        {
            foreach (var bank in data)
            {
                foreach (var item in bank.SetNames)
                {
                    AddSet(item, bank.getSetDefName(item), bank.getSetNamed(item), bank.getSetStuffName(item));
                }

            }
        }
        public void AddSet(string setName, string setDefName, string setStuffName = null)
        {
            Sets.Add(new HashSet<IntVec3>());
            SetNames.Add(setName);
            SetDefNames.Add(setDefName);
            SetStuffNames.Add(setDefName);
            setData.Add(null);


        }
        public void AddSet(string setName, string setDefName, IntVecShape Shape, string setStuffName = null)
        {
            Sets.Add(Shape.InsideArea().ToHashSet<IntVec3>());
            setData.Add(new Tuple<int, bool, bool, bool>(Shape.PointA.y, Shape.PointB.y > 0, Shape.PointC.y > 0, Shape.PointD.y > 0));

            SetNames.Add(setName);
            SetDefNames.Add(setDefName);
            SetStuffNames.Add(setStuffName);
        }

        public void AddSet(string setName, string setDefName, HashSet<IntVec3> Values, string setStuffName = null, Tuple<int, bool, bool, bool> SetData = null)
        {
            Sets.Add(Values);
            SetNames.Add(setName);
            SetDefNames.Add(setDefName);
            SetStuffNames.Add(setStuffName);
            setData.Add(SetData);
        }

        public HashSet<IntVec3> getSetNamed(string SetName)
        {

            return Sets[getSetIndex(SetName)];

        }

        public void UpdateSet(string SetName, HashSet<IntVec3> NewSet)
        {
            Sets.Replace<HashSet<IntVec3>>(getSetNamed(SetName), NewSet);
        }

        public void AddToSet(string SetName, IntVec3 AddedValue)
        {
            getSetNamed(SetName).Add(AddedValue);
        }

        public HashSet<IntVec3> getUnion(string SetName, HashSet<IntVec3> AddedValue)
        {
            var result = getSetNamed(SetName);
            result.UnionWith(AddedValue);
            return result;
        }
        public HashSet<IntVec3> getUnion(string SetName, string AddedSetName)
        {
            var result = getSetNamed(SetName);
            result.UnionWith(getSetNamed(AddedSetName));
            return result;
        }
        public void setUnion(string SetName, HashSet<IntVec3> AddedValue)
        {
            getSetNamed(SetName).UnionWith(AddedValue);
        }
        public void setUnion(string SetName, string AddedSetName)
        {
            getSetNamed(SetName).UnionWith(getSetNamed(AddedSetName));
        }
        public void setIntersect(string SetName, HashSet<IntVec3> AddedValue)
        {
            getSetNamed(SetName).IntersectWith(AddedValue);
        }
        public void setIntersect(string SetNameA, string SetNameB)
        {
            getSetNamed(SetNameA).IntersectWith(getSetNamed(SetNameB));
        }
        public HashSet<IntVec3> getIntersect(string SetName, HashSet<IntVec3> AddedValue)
        {
            var result = getSetNamed(SetName);
            result.IntersectWith(AddedValue);
            return result;
        }
        public HashSet<IntVec3> getIntersect(string SetName, string AddedSetName)
        {
            var result = getSetNamed(SetName);
            result.IntersectWith(getSetNamed(AddedSetName));
            return result;
        }
        public IEnumerable<string> getSetNames()
        {
            foreach (string name in this.SetNames)
            {
                yield return name;
            }
        }
        public IEnumerable<string> getSetDefNames()
        {
            foreach (string name in this.SetDefNames)
            {
                yield return name;
            }
        }
        public int getSetIndex(string SetName)
        {
            return this.SetNames.IndexOf(SetName);

        }
        public string getSetDefName(string SetName)
        {
            return this.SetDefNames[getSetIndex(SetName)];
        }
        public IEnumerable<string> getSetStuffNames()
        {
            foreach (string name in this.SetStuffNames)
            {
                yield return name;
            }
        }
        public string getSetStuffName(string SetName)
        {
            return this.SetStuffNames[getSetIndex(SetName)];
        }
        public Tuple<int, bool, bool, bool> getSetData(string SetName)
        {
            return this.setData[getSetIndex(SetName)];
        }



        public void setSetDefName(string SetName, string defName)
        {
            SetDefNames[getSetIndex(SetName)] = defName;
        }
        public void setSetStuffDefName(string SetName, string stuffDefName)
        {
            SetStuffNames[getSetIndex(SetName)] = stuffDefName;
        }

        public void spawnSet(string setName, Map map)
        {
            foreach (var point in this.getSetNamed(setName))
            {
                MapWorkerUtility.AdaptiveGen(map, new IntVec3(point.x, 0, point.z), getSetDefName(setName), getSetStuffName(setName), point.y, false, false, true);
            }
        }
    }

}
