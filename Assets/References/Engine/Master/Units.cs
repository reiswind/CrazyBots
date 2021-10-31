using Engine.Algorithms;
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Master
{

    public class Units
    {
        private SortedDictionary<ulong, Unit> units;
        public List<Unit> UnitsOnSameulong;
        public Map Map;

        public Units(Map map)
        {
            Map = map;
            units = new SortedDictionary<ulong, Unit>();
            UnitsOnSameulong = new List<Unit>();
        }

        public SortedDictionary<ulong, Unit> List
        {
            get { return units; }
        }
        
        public void Add(Unit unit)
        {
            units.Add(unit.Pos, unit);
        }

        public void Remove(ulong pos)
        {
            if (units.ContainsKey(pos))
            {
                if (!units.Remove(pos))
                    throw new Exception("wrong");
            }
            else
            {
                throw new Exception("wrong");
            }
        }

        public bool IsAlive(Unit unitInQuestion)
        {
            foreach (Unit unit in units.Values)
            {
                if (unit == unitInQuestion)
                {
                    return true;
                }
            }
            return false;
        }


        public void ChangePos(ulong from, ulong destination)
        {
            if (from == destination)
            {
                return;
            }

            Unit unitAt = null;
            
            if (units.TryGetValue(from, out unitAt))
            {
                if (!units.Remove(from))
                    throw new Exception("unexpected");
            }
            else
            {
                throw new Exception("unexpected");
            }

            unitAt.Pos = destination;

            if (units.ContainsKey(destination))
            {
                throw new Exception("unexpected");
            }
            else
            {
                units.Add(destination, unitAt);
            }
        }

        public Unit GetUnitAt(ulong pos)
        {
            Unit unit;
            units.TryGetValue(pos, out unit);
            return unit;
        }

        public Unit FindUnit(string unitId)
        {
            foreach (Unit unit in units.Values)
            {
                if (unit.UnitId == unitId)
                    return unit;
            }
            return null;
        }
    }

}
