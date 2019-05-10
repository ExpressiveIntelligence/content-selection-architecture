using System;
using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledHighestTierSelector<T> : KS_KC_ScheduledTierSelector<T> where T : KnowledgeComponent, IComparable
    {

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            // Returns array sorted lowest to highest value on KnowledgeComponent T
            Unit[] units = SortUnitsFilteredByPrecondition(boundVars);

            // Copy the highest-tier units to the output pool 
            IComparable highestValueComponent = units[units.Length - 1].GetComponent<T>();
            for(int i = units.Length - 1; i >= 0; i--)
            {
                if (units[i].GetComponent<T>().CompareTo(highestValueComponent) == 0)
                {
                    CopyUnitToOutputPool(units[i]);
                }
                else
                {
                    break;
                }
            }
        }

        public KS_KC_ScheduledHighestTierSelector(IBlackboard blackboard) : base(blackboard)
        {
        }

        public KS_KC_ScheduledHighestTierSelector(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool)
        {
        }

        public KS_KC_ScheduledHighestTierSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool, outputPool)
        {
        }

        public KS_KC_ScheduledHighestTierSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard, outputPool, filter)
        {
        }

        public KS_KC_ScheduledHighestTierSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter)
            : base(blackboard, inputPool, outputPool, filter)
        {
        }
    }
}
