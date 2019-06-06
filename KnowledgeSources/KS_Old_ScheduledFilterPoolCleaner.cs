using System;
using System.Collections.Generic;
using CSA.Core;
#pragma warning disable CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.CUSlots;
#pragma warning restore CS0618 // Type or member is obsolete

namespace CSA.KnowledgeSources
{
    /* 
     * KS_FilterPoolCleaner deletes ContentUnits from the specified pools. 
     * fixme: Like ChoicePresenter, FilterPoolCleaner doesn't copy anything with an output pool. Consider a refactor where the logic for selecting
     * a collection of content units based on filter condition is pushed up into a class above KS_ScheduledFilterSelector. 
     */
    [Obsolete("Use KnowledgeComponent-based version of ScheduledFilterPoolCleaner.")]
    public class KS_Old_ScheduledFilterPoolCleaner : KS_Old_ScheduledFilterSelector
    {
        private static bool SelectFromInputPools(string[] inputPools, ContentUnit cu)
        {
            if (cu.HasMetadataSlot(ContentPool))
            {
                foreach(string pool in inputPools)
                {
                    if (cu.Metadata[ContentPool].Equals(pool)) return true; 
                }
            }
            return false; 
        }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            IEnumerable<ContentUnit> contentUnits = ContentUnitsFilteredByPrecondition(boundVars);
            foreach (var cu in contentUnits)
            {
                m_blackboard.RemoveUnit(cu);
            }
        }

        public KS_Old_ScheduledFilterPoolCleaner(IBlackboard blackboard, string[] inputPools) :
            base(blackboard, null, (cu) => SelectFromInputPools(inputPools, cu))
        {
        }
    }
}
