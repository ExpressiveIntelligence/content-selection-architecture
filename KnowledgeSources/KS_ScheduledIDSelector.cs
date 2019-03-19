using System.Collections.Generic;
using System.Linq;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledIDSelector : KS_ScheduledFilterSelector
    {
        // fixme: remove
        // Name of the bound context variable
        // private const string IDSelectRequest = "IDSelectRequest";

        public const string DefaultOutputPoolName = "SelectedByID";

        protected override IDictionary<string, object>[] Precondition()
        {
            // fixme: consider adding additional fields to request units to indicate additional filtering, so that filter logic is associated with the request rather than the KS.
            // Use LINQ to create a collection of the requested U_IDSelectRequests on the blackboard.
            ISet<IUnit> requests = m_blackboard.LookupUnits(U_IDSelectRequest.TypeName);
     
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
                    var contentUnits = from contentUnit in m_blackboard.LookupUnits(ContentUnit.TypeName) // lookup content units
                                       let castCU = contentUnit as ContentUnit
                                       where FilterConditionDel(castCU) // where the content unit satisfies some user provided filter condition 
                                       where (castCU).HasMetadataSlot(ContentUnitID) // where the content unit has an ID
                                       where (castCU).Metadata[ContentUnitID].Equals(targetContentUnitID) // and the ID equals the target ID
                                       select castCU;

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

        // fixme: remove Execute() once testing has confirmed that the default Execute() works
        /*
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            string targetContentUnitID = ((U_IDSelectRequest)boundVars[IDSelectRequest]).TargetContentUnitID;
            var contentUnits = from contentUnit in m_blackboard.LookupUnits(ContentUnit.TypeName) // lookup content units
                               where FilterConditionDel((ContentUnit)contentUnit) // where the content unit satisfies some user provided filter condition 
                               where ((ContentUnit)contentUnit).HasMetadataSlot(ContentUnitID) // where the content unit has an ID
                               where ((ContentUnit)contentUnit).Metadata[ContentUnitID].Equals(targetContentUnitID) // and the ID equals the target ID
                               select contentUnit;

            // If there are any selected content units, write copies of them to a SelectedContentUnit output pool
            if (contentUnits.Any())
            {
                // One or more content units matching the ContentUnitID in the U_IDQuery were found.
                // fixme: if no matching content unit was found, perhaps the KS should post something indiciating the execution failed. 
                // Or this could be done via tracing, with a KS that looks for tracing patterns, though this will require separate "failure patterns"
                // for each case. So probably better to have more general semantics for KSs to post success and failure into the trace. 

                foreach (var contentUnit in (IEnumerable<ContentUnit>)contentUnits)
                {
                    CopyCUToOutputPool(contentUnit);
                }
            }
            m_blackboard.RemoveUnit((IUnit)boundVars[IDSelectRequest]); // Remove the U_IDSelectRequest from the blackboard
        }
        */

        
        public KS_ScheduledIDSelector(IBlackboard blackboard) : base(blackboard, DefaultOutputPoolName)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool, outputPool)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard, outputPool, filter)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter) : 
            base(blackboard, inputPool, outputPool, filter)
        {
        }
    }
}
