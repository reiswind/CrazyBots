using Engine.Algorithms;
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Master
{

    public class Units
    {
        private Dictionary<Position2, Unit> units;
        private Dictionary<string, Unit> unitsById;
        public Map Map;

        public Units(Map map)
        {
            Map = map;
            units = new Dictionary<Position2, Unit>();
            unitsById = new Dictionary<string, Unit>();
        }

        public Dictionary<Position2, Unit> List
        {
            get { return units; }
        }
        
        public void Add(Unit unit)
        {
            if (!unitsById.ContainsKey(unit.UnitId))
                unitsById.Add(unit.UnitId, unit);
            if (unit.Pos != Position2.Null)
                units.Add(unit.Pos, unit);
        }

        public void Remove(Position2 pos)
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
        public void Remove(string untitId)
        {
            if (unitsById.ContainsKey(untitId))
            {
                if (!unitsById.Remove(untitId))
                    throw new Exception("wrong");
            }
            else
            {
                throw new Exception("wrong");
            }
        }
        public Unit GetUnitAt(Position2 pos)
        {
            Unit unit;
            units.TryGetValue(pos, out unit);
            return unit;
        }

        public Unit FindUnit(string unitId)
        {
            Unit unit;
            unitsById.TryGetValue(unitId, out unit);
            return unit;
        }
    }

}
