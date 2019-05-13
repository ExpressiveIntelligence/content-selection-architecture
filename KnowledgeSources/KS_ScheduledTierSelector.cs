using System;
using System.Collections.Generic;
using System.Linq;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    [Obsolete("Use classes in the KnowledgeComponent-based ScheduledTierSelector hierarchy.")]
    public class KS_ScheduledTierSelector : KS_ScheduledFilterSelector
    {
        public const string DefaultOutputPoolName = "SelectedByTier";

        // The name of the content unit slot to use for sorting into tiers.
        public string TierSlot { get; }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            ContentUnit[] contentUnits = ContentUnitsFilteredByPrecondition(boundVars).ToArray();

            // Sort the content units by the TierSlot, largest to smallest. 
            Array.Sort(contentUnits, (x, y) => ((IComparable)y.Metadata[TierSlot]).CompareTo(x.Metadata[TierSlot]) );

            // Copy the highest-tier content units to the output pool 
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


        public KS_ScheduledTierSelector(IBlackboard blackboard, string tierSlot) : base(blackboard, DefaultOutputPoolName, GenerateHasMetadataSlotFilter(tierSlot))
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string outputPool, string tierSlot) : base(blackboard, outputPool, GenerateHasMetadataSlotFilter(tierSlot))
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string inputPool, string outputPool, string tierSlot) : 
            base(blackboard, inputPool, outputPool, GenerateHasMetadataSlotFilter(tierSlot))
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string outputPool, FilterCondition filter, string tierSlot) : 
            base(blackboard, outputPool, (ContentUnit cu) => filter(cu) && GenerateHasMetadataSlotFilter(tierSlot)(cu))
        {
            TierSlot = tierSlot;
        }

        public KS_ScheduledTierSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter, string tierSlot)
            : base(blackboard, inputPool, outputPool, (ContentUnit cu) => filter(cu) && GenerateHasMetadataSlotFilter(tierSlot)(cu))
        {
            TierSlot = tierSlot;
        }

    }
}
