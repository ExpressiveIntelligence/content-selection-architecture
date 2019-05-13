using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;
using Prolog;

namespace CSA.KnowledgeSources
{
    /*
     * fixme: perhaps knowledge sources that manipulate meta-data will inherit from a different parent than ScheduledFilterSelector. For now we'll use
     * ScheduledFilterSelector since the precondition is almost identical. It's the action to take that changes. 
     */
    [Obsolete("Use KnowledgeComponent-based ScheduledPrologEval.")]
    public class KS_ScheduledPrologEval : KS_ScheduledFilterSelector
    {
        private const string PrologEvalRequest = "PrologEvalRequest";

        public const string DefaultOutputPoolName = "PrologQueryEvaluted";

        /*
         * Convenience utility for creating a filter method which filters by the prolog evaluation being true or false (== to the param result). 
         */
        public static FilterCondition FilterByPrologResult(bool result)
        {
            return (ContentUnit contentUnit) => contentUnit.HasMetadataSlot(ApplTestResult) && (bool)contentUnit.Metadata[ApplTestResult] == result;
        }

        protected override IDictionary<string, object>[] Precondition()
        {
            /* 
             * fixme: the logic for for processing the U_PrologEvalRequest is almost identical to the logic for processing U_IDSelectionRequest. 
             * Come up with a general inherited mechanism for managing requests. 
             * fixme: consider adding additional fields to request units to indicate additional filtering, so that filter logic is associated with the 
             * request rather than the KS.
             */
            // Use LINQ to create a collection of the requested U_PrologEvalQueries on the blackboard.
            var requests = m_blackboard.LookupUnits<U_PrologEvalRequest>();

            if (requests.Any())
            {
                // There are some requests - iterate through each of the requests creating bindings for each request

                var bindings = new IDictionary<string, object>[requests.Count()];

                int i = 0;
                foreach (var request in requests)
                {
                    bindings[i++] = new Dictionary<string, object>
                    {
                        [PrologEvalRequest] = request
                    };
                }
                return bindings;
            }
            else
            {
                return m_emptyBindings;
            }
        }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            string prologQuerySlotName = ((U_PrologEvalRequest)boundVars[PrologEvalRequest]).PrologQuerySlotName;
            string prologResultSlotName = ((U_PrologEvalRequest)boundVars[PrologEvalRequest]).PrologResultSlotName;
            string prologBoundVarSlotName = ((U_PrologEvalRequest)boundVars[PrologEvalRequest]).PrologBoundVarSlotName;
            var contentUnitsWithQuerySlot = from contentUnit in m_blackboard.LookupUnits<ContentUnit>()
                                            where FilterConditionDel(contentUnit) // where the content unit satisfies some user provided filter condition 
                                            where contentUnit.HasMetadataSlot(prologQuerySlotName) // where the content unit has the prolog query
                                            select contentUnit;

            U_PrologKB prologKB = m_blackboard.LookupSingleton<U_PrologKB>();

#if UNIT_TEST
            XunitOutput?.WriteLine("In PrologEval.Execute()");
#endif

            /* 
             * fixme: the copy logic won't work with multiple prolog eval requests - a different copy will be made for each unique query slot instead
             * of evaluating all the query slots on each content unit. Resolution: use a single eval request with arrays of slots. 
             */
            foreach (var contentUnit in contentUnitsWithQuerySlot)
            {
#if UNIT_TEST
                XunitOutput?.WriteLine("Evaluting prolog eval on ContentUnit: " + contentUnit.Metadata[CUSlots.ContentUnitID]);
#endif

                ContentUnit contentUnitCopy = CopyCUToOutputPool(contentUnit);
                var result = prologKB.SolveForParsed((string)contentUnitCopy.Metadata[prologQuerySlotName]);

#if UNIT_TEST
                XunitOutput?.WriteLine("Result of evaluation: " + result);
#endif

                contentUnitCopy.Metadata[prologResultSlotName] = result != null;
                if (prologBoundVarSlotName != null)
                {
                    contentUnitCopy.Metadata[prologBoundVarSlotName] = result;
                }
            }

            m_blackboard.RemoveUnit((IUnit)boundVars[PrologEvalRequest]);
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard) : base(blackboard, DefaultOutputPoolName)
        {
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool)
        {
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool, outputPool)
        {
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard, outputPool, filter)
        {
        }
    }
}
