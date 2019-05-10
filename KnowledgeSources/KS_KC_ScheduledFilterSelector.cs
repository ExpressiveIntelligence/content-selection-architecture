using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledFilterSelector : KS_KC_ContentPoolCollector
    {
        // Output pool for this filter. 
        public string OutputPool { get; }

        /*
         * Copy a Unit to the output pool.
         */
        protected Unit CopyUnitToOutputPool(Unit unit)
        {
            Unit newUnit = new Unit(unit);

            /* 
             * If there is an existing content pool component remove the componenet before adding a new one with the new pool. The case in which there won't be a content pool
             * component is when copying from the global pool (no pool) into a pool.             
             */
            if (newUnit.HasComponent<KC_ContentPool>())
            {
                newUnit.RemoveComponent(newUnit.GetComponent<KC_ContentPool>());
            }
            newUnit.AddComponent(new KC_ContentPool(OutputPool, true));

            m_blackboard.AddUnit(newUnit);
            m_blackboard.AddLink(unit, newUnit, LinkTypes.L_SelectedContentUnit, true); // fixme: need a more general link type for copies between pools
            return newUnit;
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
        }

        public KS_KC_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool)
        {
            OutputPool = outputPool;
        }

        public KS_KC_ScheduledFilterSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard, filter)
        {
            OutputPool = outputPool;
        }

        /*
        * ScheduledFilterSelector constructed with both an input pool and a filter specified using the conjunction of SelectFromPool and filter 
        * as the FilterConditionDel.         
        */
        public KS_KC_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
            OutputPool = outputPool;
        }
    }
}
