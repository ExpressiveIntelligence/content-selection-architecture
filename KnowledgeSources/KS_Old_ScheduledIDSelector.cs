using System;
using System.Collections.Generic;
using System.Linq;
using CSA.Core;
#pragma warning disable CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.CUSlots;
#pragma warning restore CS0618 // Type or member is obsolete
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    [Obsolete("Use KnowledgeComponent-based ScheduledFilterSelector.")]
    public class KS_Old_ScheduledIDSelector : KS_Old_ScheduledFilterSelector
    {
        // fixme: remove
        // Name of the bound context variable
        // private const string IDSelectRequest = "IDSelectRequest";

        public const string DefaultOutputPoolName = "SelectedByID";

        protected override IDictionary<string, object>[] Precondition()
        {
            // fixme: consider adding additional fields to request units to indicate additional filtering, so that filter logic is associated with the request rather than the KS.
            // Use LINQ to create a collection of the requested U_IDSelectRequests on the blackboard.
            ISet<U_IDSelectRequest> requests = m_blackboard.LookupUnits<U_IDSelectRequest>();
     
            if (requests.Any())
            {
                // There are some requests - iterate through each of the requests creating bindings for the filtered content units

                var bindings = new IDictionary<string, object>[requests.Count()];

                int i = 0;
                foreach (U_IDSelectRequest request in requests)
                {
                    /* 
                     * fixme: the logic below could potentially be pushed into a call to base.Precondition() with an appropriate definition of the filter.
                     * Then bindining would be set by:
                     * bindings[i++] = base.Precondition();
                     * with an appropriate change to the FilterConditionDel in each iteration. 
                     */                    
                    
                    string targetContentUnitID = request.TargetContentUnitID;
                    var contentUnits = from contentUnit in m_blackboard.LookupUnits<ContentUnit>() // lookup content units
                                       where FilterConditionDel(contentUnit) // where the content unit satisfies some user provided filter condition 
                                       where (contentUnit).HasMetadataSlot(ContentUnitID) // where the content unit has an ID
                                       where (contentUnit).Metadata[ContentUnitID].Equals(targetContentUnitID) // and the ID equals the target ID
                                       select contentUnit;

                    bindings[i++] = new Dictionary<string, object>
                    {
                        [FilteredContentUnits] = contentUnits
                    };
                    m_blackboard.RemoveUnit(request); // Remove the U_IDSelectRequest from the blackboard
                }
                return bindings;
            }
            else
            {
                return m_emptyBindings;
            }

        }
        
        public KS_Old_ScheduledIDSelector(IBlackboard blackboard) : base(blackboard, DefaultOutputPoolName)
        {
        }

        public KS_Old_ScheduledIDSelector(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool)
        {
        }

        public KS_Old_ScheduledIDSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool, outputPool)
        {
        }

        public KS_Old_ScheduledIDSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard, outputPool, filter)
        {
        }

        public KS_Old_ScheduledIDSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter) : 
            base(blackboard, inputPool, outputPool, filter)
        {
        }
    }
}
