using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledFilterSelector : ScheduledKnowledgeSource
    {
        // Name of the bound activation variable
        protected const string FilteredUnits = "FilteredUnits";

        // Input and output pools for this filter. 
        public string InputPool { get; }
        public string OutputPool { get; }

        // Define the type for the FilterCondition delegate
        public delegate bool FilterCondition(Unit unit);

        // Define the field that stores a FilterCondition delegate
        protected FilterCondition FilterConditionDel;

        // The default filter condition does no filtering. 
        public static bool DefaultFilterCondition(Unit _)
        {
            return true;
        }

        public static bool SelectFromPool(Unit unit, string inputPool)
        {
            return unit.HasComponent<KC_ContentPool>() && unit.ContentPoolEquals(inputPool);
        }

        private bool SelectFromPool(Unit unit)
        {
            return SelectFromPool(unit, InputPool);
        }

        public static FilterCondition GenerateHasComponent<T>() where T : KnowledgeComponent
        {
            return (Unit unit) => unit.HasComponent<T>();
        }

        protected override IDictionary<string, object>[] Precondition()
        {
            var units = from unit in m_blackboard.LookupUnits<Unit>()
                               where FilterConditionDel(unit)
                               select unit;

            if (units.Any())
            {
                // There is at least one unit passing the conditions 


                // The binding contains the enumeration of all the units that pass the filter condition. 
                // fixme: Execute will copy all the units in the enumeration into the output pool. 
                var bindings = new IDictionary<string, object>[1];

                bindings[0] = new Dictionary<string, object>
                {
                    [FilteredUnits] = units
                };

                return bindings;
            }
            else
            {
                // No units matching the conditions in the InputPool - return empty bindings (length 0). 
                return m_emptyBindings;
            }
        }

        protected Unit CopyUnitToOutputPool(Unit unit)
        {
            Unit newUnit = new Unit(unit);
            newUnit.SetContentPool(OutputPool);
            m_blackboard.AddUnit(newUnit);
            m_blackboard.AddLink(unit, newUnit, LinkTypes.L_SelectedContentUnit, true); // fixme: need a more general link type for copies between pools
            return newUnit;
        }

        protected IEnumerable<ContentUnit> UnitsFilteredByPrecondition(IDictionary<string, object> boundVars)
        {
            return (IEnumerable<ContentUnit>)boundVars[FilteredUnits];
        }

        /*
         * On Execute(), the abstract FilterSelector copys the filtered CUs from the input pool to the output pool.  
         */
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            var units = UnitsFilteredByPrecondition(boundVars);
            foreach (Unit unit in units)
            {
                CopyUnitToOutputPool(unit);
            }
        }

        public KS_KC_ScheduledFilterSelector(IBlackboard blackboard, string outputPool) : base(blackboard)
        {
            OutputPool = outputPool;
            FilterConditionDel = DefaultFilterCondition;
        }

        public KS_KC_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard)
        {
            InputPool = inputPool;
            OutputPool = outputPool;
            FilterConditionDel = SelectFromPool;
        }

        public KS_KC_ScheduledFilterSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard)
        {
            Debug.Assert(filter != null);

            OutputPool = outputPool;
            FilterConditionDel = filter;
        }

        /*
        * ScheduledFilterSelector constructed with both an input pool and a filter specified using the conjunction of SelectFromPool and filter 
        * as the FilterConditionDel.         
        */
        public KS_KC_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter) : base(blackboard)
        {
            Debug.Assert(filter != null);
            Debug.Assert(inputPool != null);

            InputPool = inputPool;
            OutputPool = outputPool;
            FilterConditionDel = (Unit unit) => SelectFromPool(unit) && filter(unit);
        }
    }
}
