using System.Collections.Generic;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.KnowledgeSources
{
    /* 
     * KS_FilterPoolCleaner deletes ContentUnits from the specified pools. 
     * fixme: Like ChoicePresenter, FilterPoolCleaner doesn't copy anything with an output pool. Consider a refactor where the logic for selecting
     * a collection of content units based on filter condition is pushed up into a class above KS_ScheduledFilterSelector. 
     */
    public class KS_ScheduledFilterPoolCleaner : KS_ScheduledFilterSelector
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

        public KS_ScheduledFilterPoolCleaner(IBlackboard blackboard, string[] inputPools) :
            base(blackboard, null, (cu) => SelectFromInputPools(inputPools, cu))
        {
        }
    }
}
