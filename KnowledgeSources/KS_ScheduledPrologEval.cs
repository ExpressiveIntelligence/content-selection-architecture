using System;
using System.Collections.Generic;
using System.Linq;
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
             */
            // fixme: consider adding additional fields to request units to indicate additional filtering, so that filter logic is associated with the request rather than the KS.
            // Use LINQ to create a collection of the requested U_PrologEvalQueries on the blackboard.
            var requests = from request in m_blackboard.LookupUnits(U_PrologEvalRequest.TypeName) // Lookup ID select requests
                           select request;

            if (requests.Any())
            {
                // There are some requests - iterate through each of the requests creating bindings for the filtered content units

                var bindings = new IDictionary<string, object>[requests.Count()];

                int i = 0;
                foreach (var request in (IEnumerable<U_PrologEvalRequest>)requests)
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
                                            where FilterConditionDel((ContentUnit)contentUnit) // where the content unit satisfies some user provided filter condition 
                                            where ((ContentUnit)contentUnit).HasMetadataSlot(prologQuerySlotName) // where the content unit has the prolog query
                                            select contentUnit;

            var prologKB = from kb in m_blackboard.LookupUnits(U_PrologKB.TypeName)
                           select kb;

            if (prologKB.Count() == 1)
            {
                KnowledgeBase kb = ((U_PrologKB)prologKB.First()).KB;

                /* 
                 * fixme: the copy logic won't work with multiple prolog eval requests - a different copy will be made for each unique query slot instead
                 * of evaluating all the query slots on each content unit. 
                 */
                foreach (var contentUnit in (IEnumerable<ContentUnit>)contentUnitsWithQuerySlot)
                {
                    ContentUnit contentUnitCopy = CopyCUToOutputPool(contentUnit);
                    var query = ISOPrologReader.Read((string)contentUnitCopy.Metadata[prologQuerySlotName]);
                    if (!(query is Structure colonExpression) || !colonExpression.IsFunctor(Symbol.Colon, 2))
                    {
                        // The query is not a colon expression - use isTrue and ignore prologBoundVarSlotName
                        contentUnitCopy.Metadata[prologResultSlotName] = kb.IsTrue(query);
                        if (prologBoundVarSlotName != null)
                        {
                            contentUnitCopy.Metadata[prologBoundVarSlotName] = null;
                        }
                    }
                    else
                    {
                        // The query is a colon expression - use SolveFor and set the prolog bound variable slot
                        var result = kb.SolveFor((LogicVariable)colonExpression.Argument(0), colonExpression.Argument(1), null, false);
                        if (result != null)
                        {
                            contentUnitCopy.Metadata[prologResultSlotName] = true;
                        }
                        else
                        {
                            contentUnitCopy.Metadata[prologResultSlotName] = false;
                        }

                        if (prologBoundVarSlotName != null)
                        {
                            contentUnitCopy.Metadata[prologBoundVarSlotName] = result;
                        }
                    }
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
