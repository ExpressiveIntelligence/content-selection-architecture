using System;
using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledLowestTierSelector<T> : KS_ScheduledTierSelector<T> where T : KnowledgeComponent, IComparable
    {
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            // Returns array sorted lowest to highest value on KnowledgeComponent T
            Unit[] units = SortUnitsFilteredByPrecondition(boundVars);

            // Copy the lowest-tier units to the output pool 
            IComparable lowestValueComponent = units[0].GetComponent<T>();
            foreach (Unit unit in units)
            {
                if (unit.GetComponent<T>().CompareTo(lowestValueComponent) == 0)
                {
                    CopyUnitToOutputPool(unit);
                }
                else
                {
                    break;
                }
            }
        }

        public KS_ScheduledLowestTierSelector(IBlackboard blackboard) : base(blackboard)
        {
        }

        public KS_ScheduledLowestTierSelector(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool)
        {
        }

        public KS_ScheduledLowestTierSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool, outputPool)
        {
        }

        public KS_ScheduledLowestTierSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard, outputPool, filter)
        {
        }

        public KS_ScheduledLowestTierSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter)
            : base(blackboard, inputPool, outputPool, filter)
        {
        } 

    }
}
