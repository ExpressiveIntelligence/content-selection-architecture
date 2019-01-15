using System;
using System.Collections.Generic;

namespace CSACore
{
    public class Blackboard : IBlackboard
    {
        private readonly IDictionary<string, ISet<IUnit>> dict = new Dictionary<string, ISet<IUnit>>(); 

        // Duplicate units (by reference) not allowed on the blackboard. Using set semantics for Units sharing the same type. 
        public void AddUnit(IUnit unit)
        {

            if (LookupUnits(unit, out ISet<IUnit> units))
            {
                units.Add(unit);

            }
            else
            {
                ISet<IUnit> newUnits = new HashSet<IUnit>
                {
                    unit
                };
                dict.Add(GetUnitTypeName(unit), newUnits);
            }

        }

        public bool DeleteUnit(IUnit unit)
        {
            if (LookupUnits(unit, out ISet<IUnit> units))
            {
                if (units.Remove(unit))
                {
                    if (units.Count == 0)
                    {
                        dict.Remove(GetUnitTypeName(unit));
                    }
                    return true; 
                }

            }
            return false; 
        }

        public ISet<IUnit> LookupUnits(string unitType)
        {
            return dict.TryGetValue(unitType, out ISet<IUnit> units) ?  new HashSet<IUnit>(units) : new HashSet<IUnit>();
        }

        public bool ContainsUnit(IUnit unit)
        {
            return LookupUnits(unit, out ISet<IUnit> units) && units.Contains(unit);
        }

        protected string GetUnitTypeName(IUnit u)
        {
            return u.GetType().FullName;
        }

        private bool LookupUnits(IUnit unit, out ISet<IUnit> units)
        {
            string typeName = GetUnitTypeName(unit);
            units = null;

            return dict.TryGetValue(typeName, out units);
        }

        // fixme: add support for hierarchical blackboards and spaces with special indexing (efficient lookup of units by properties rather than just class)
    }
}
