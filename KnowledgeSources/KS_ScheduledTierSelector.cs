using System;
using System.Collections.Generic;
using System.Linq;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledTierSelector : KS_ScheduledFilterSelector
    {
        public const string DefaultOutputPoolName = "SelectedByTier";

        public string TierSlot { get; }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            ContentUnit[] contentUnits = ContentUnitsFilteredByPrecondition(boundVars).ToArray();

            Array.Sort(contentUnits, (x, y) => ((IComparable)x.Metadata[TierSlot]).CompareTo(y) );

            object slotValToInclude = contentUnits[0].Metadata[TierSlot];
            foreach(ContentUnit contentUnit in contentUnits)
            {
                if (contentUnit.Metadata[TierSlot].Equals(slotValToInclude))
                {
                    CopyCUToOutputPool(contentUnit);
                }
                else
                {
                    break;
                }
            }
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string tierSlot) : base(blackboard, DefaultOutputPoolName)
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string outputPool, string tierSlot) : base(blackboard, outputPool)
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string inputPool, string outputPool, string tierSlot) : base(blackboard, inputPool, outputPool)
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string outputPool, string tierSlot, FilterCondition filter) : base(blackboard, outputPool, filter)
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string inputPool, string outputPool, string tierSlot, FilterCondition filter)
            : base(blackboard, inputPool, outputPool, filter)
        {
            TierSlot = tierSlot;
        }

    }
}
