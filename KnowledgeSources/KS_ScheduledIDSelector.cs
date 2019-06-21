using System.Collections.Generic;
using System.Linq;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    // fixme: consider making a KS_ScheduledQuery class of which KS_ScheduledIDSelector is a subtype.
    public class KS_ScheduledIDSelector : KS_ScheduledFilterSelector
    {
        /*
         * Initializing the enumerator of unique output pool names (static) and the initialization of the DefaultOutputPoolName (instance).
         */
        private static readonly IEnumerator<string> m_OutputPoolNameEnumerator = OutputPoolNameEnumerator("SelectedByID");
        public override string DefaultOutputPoolName { get; } = GenDefaultOutputPoolName(m_OutputPoolNameEnumerator);

        protected override object[][] Precondition()
        {
            // fixme: consider adding additional fields to request units to indicate additional filtering, so that filter logic is associated with the request rather than the KS.
            // Use LINQ to create a collection of the requested U_IDSelectRequests on the blackboard.
            var requests = from unit in m_blackboard.LookupUnits<Unit>()
                           where unit.HasComponent<KC_IDSelectionRequest>()
                           where unit.GetActiveRequest()
                           select unit;

            if (requests.Any())
            {
                // There are some requests - iterate through each of the requests creating bindings for the filtered content units

                var bindings = new object[requests.Count()][];

                int i = 0;
                foreach (Unit request in requests)
                {
                    /* 
                     * fixme: the logic below could potentially be pushed into a call to base.Precondition() with an appropriate definition of the filter.
                     * Then bindining would be set by:
                     * bindings[i++] = base.Precondition();
                     * with an appropriate change to the FilterConditionDel in each iteration. 
                     */

                    string targetUnitID = request.GetTargetUnitID();
                    var units = from unit in m_blackboard.LookupUnits<Unit>() // lookup knowledge units
                                where FilterConditionDel(unit) // where the unit satisfies some user provided filter condition 
                                where unit.HasComponent<KC_UnitID>() // where the unit has an ID
                                where unit.UnitIDEquals(targetUnitID) // and the ID equals the target ID
                                select unit;

                    bindings[i++] = new object[] { units };
                    // Don't want to remove it from the blackboard as some IDSelectionRequests are part of trees (and will thus be the parent for the decomposition). 
                    // Instead mark the request as not active. Something else will have to clean up inactive reqeusts. 
                    request.SetActiveRequest(false);
                }
                return bindings;
            }
            else
            {
                return m_emptyBindings;
            }

        }

        public KS_ScheduledIDSelector(IBlackboard blackboard) : base(blackboard)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool, outputPool)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
        }

        public KS_ScheduledIDSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter) :
            base(blackboard, inputPool, outputPool, filter)
        {
        }
    }
}
