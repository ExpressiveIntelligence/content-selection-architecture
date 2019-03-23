using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using CSA.KnowledgeUnits;
using Prolog;

namespace CSA.KnowledgeSources
{
    /*
     * fixme: perhaps knowledge sources that manipulate meta-data will inherit from a different parent than ScheduledFilterSelector. For now we'll use
     * ScheduledFilterSelector since the precondition is almost identical. It's the action to take that changes. 
     */

    public class KS_ScheduledPrologEval : KS_ScheduledFilterSelector
    {
        private const string PrologEvalRequest = "PrologEvalRequest";

        public const string DefaultOutputPoolName = "PrologQueryEvaluted";

        protected override IDictionary<string, object>[] Precondition()
        {
            /* 
             * fixme: the logic for for processing the U_PrologEvalRequest is almost identical to the logic for processing U_IDSelectionRequest. 
             * Come up with a general inherited mechanism for managing requests. 
             * fixme: consider adding additional fields to request units to indicate additional filtering, so that filter logic is associated with the 
             * request rather than the KS.
             */
            // Use LINQ to create a collection of the requested U_PrologEvalQueries on the blackboard.
            var requests = m_blackboard.LookupUnits(U_PrologEvalRequest.TypeName);

            if (requests.Any())
            {
                // There are some requests - iterate through each of the requests creating bindings for the filtered content units

                var bindings = new IDictionary<string, object>[requests.Count()];

                int i = 0;
                foreach (U_PrologEvalRequest request in requests)
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
            var contentUnitsWithQuerySlot = from contentUnit in m_blackboard.LookupUnits(ContentUnit.TypeName)
                                            let cuCast = contentUnit as ContentUnit
                                            where FilterConditionDel(cuCast) // where the content unit satisfies some user provided filter condition 
                                            where cuCast.HasMetadataSlot(prologQuerySlotName) // where the content unit has the prolog query
                                            select cuCast;

            var prologKB = m_blackboard.LookupUnits(U_PrologKB.TypeName);

            XunitOutput?.WriteLine("In PrologEval.Execute()");

            Debug.Assert(prologKB.Count == 1);

            U_PrologKB kb = (U_PrologKB)prologKB.First();

            /* 
             * fixme: the copy logic won't work with multiple prolog eval requests - a different copy will be made for each unique query slot instead
             * of evaluating all the query slots on each content unit. Resolution: use a single eval request with arrays of slots. 
             */
            foreach (var contentUnit in contentUnitsWithQuerySlot)
            {
                XunitOutput?.WriteLine("Evaluting prolog eval on ContentUnit: " + contentUnit.Metadata[CUSlots.ContentUnitID]);
                ContentUnit contentUnitCopy = CopyCUToOutputPool(contentUnit);
                var result = kb.SolveForParsed((string)contentUnitCopy.Metadata[prologQuerySlotName]);
                XunitOutput?.WriteLine("Result of evaluation: " + result);
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
