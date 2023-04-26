
using System.Collections.Generic;
using System.Linq;

namespace MobileObjects
{
    public class AggroTable
    {

        //public List<UnitAggro> table = new List<UnitAggro>();
        public List<float> aggro = new List<float>(); //table.Select(s => s.aggro).ToList();
        public List<string> unit = new List<string>(); //table.Select(s => s.unit).ToList();

        public AggroTable() { }
        public string top => unit[aggro.IndexOf(aggro.Max())];
        public string bottom => unit[aggro.IndexOf(aggro.Min())];
        public float this[string unit] => isAggroed(unit) ? this.aggro[this.unit.IndexOf(unit)] : 0f;

        public bool isAggroed(string unit) => this.unit.Contains(unit);
        public void AddAggroSet(string thingID, float value = 0)
        {
            this.unit.Add(thingID);
            this.aggro.Add(value);
        }

        public void SetAggroOfUnit(string unit, float newValue)
        {
            if (!isAggroed(unit))
            {
                AddAggroSet(unit, newValue);
            }
            else
            {
                this.aggro[this.unit.IndexOf(unit)] = newValue;
            }

        }

        public void AddAggroToUnit(string unit, float value)
        {
            DB.Msg("AddAggroToUnit() 1: " + unit + " : " +this[unit] + value);
            SetAggroOfUnit(unit, this[unit] + value);
        }
        public void dropAggro(string unit)
        {
            int n = this.unit.IndexOf(unit);
            this.unit[n] = null;
            this.aggro[n] = 0;
        }
        public void dropAllAggro()
        {
            unit.Clear();
            aggro.Clear();
        }

    }
}
