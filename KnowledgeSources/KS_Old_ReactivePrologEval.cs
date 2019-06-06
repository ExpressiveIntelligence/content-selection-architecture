using System;
using System.Linq;
using System.Collections.Generic;
using CSA.KnowledgeUnits;
using CSA.Core;
using static CSA.KnowledgeUnits.KUProps;

namespace CSA.KnowledgeSources
{
    /*
     * fixme: ReactivePrologEval has not been tested and is currently deprecated by ScheduledPrologEval. 
     * I will return to making ReactivePrologEval work when I have a general solution for reactive knowledge source activation. 
     */
    [Obsolete("Use ScheduledPrologEval until a reactive version of this has been implemented.")]
    public class KS_Old_ReactivePrologEval : ReactiveKnowledgeSource
    {
        // Name of the bound context variables
        private const string PrologEvalRequest = "PrologEvalRequest";
        private const string PrologKB = "PrologKB";
 
        public override IKnowledgeSourceActivation[] Precondition()
        {
            // Use LINQ to create a collection of the requested U_PrologEvalQueries on the blackboard.
            var requests = from request in m_blackboard.LookupUnits<U_PrologEvalRequest>() // Lookup ID select requests
                           where // where the request has not been previously matched by this knowledge source precondition
                            (!request.Slots.ContainsKey(KSPreconditionMatched)) ||
                            (!((ISet<ReactiveKnowledgeSource>)request.Slots[KSPreconditionMatched]).Contains(this))
                          select request;

            IKnowledgeSourceActivation[] activations = new IKnowledgeSourceActivation[requests.Count()];

            // Currently only support one prolog KB 
            // fixme: eventually may want to support multiple prolog KBs so will need mechanism for supporting them as well as inheritance 

            var prologKB = m_blackboard.LookupSingleton<U_PrologKB>(); // Lookup prolog kbs.
                           
            // Iterate through each of the requests, creating KnowledgeSourceActivations
            int i = 0;
            foreach (var request in requests)
            {
                var boundVars = new Dictionary<string, object>
                {
                    [PrologEvalRequest] = request,
                    [PrologKB] = prologKB
                };

                activations[i++] = new KnowledgeSourceActivation(this, boundVars);

                // fixme: this bit of boilerplate code for marking a knowledge unit as having already participated in a matched precondition 
                // should be baked into the infrastructure somewhere so knowledge source implementers don't always have to do this. 
                // A good place to add this would be on Unit (and declare a method on IUnit).
                if (request.Slots.ContainsKey(KSPreconditionMatched))
                {
                    ((HashSet<ReactiveKnowledgeSource>)request.Slots[KSPreconditionMatched]).Add(this);
                }
                else
                {
                    request.Slots[KSPreconditionMatched] = new HashSet<ReactiveKnowledgeSource> { this };
                }
            }

            return activations;
        }

        internal override bool EvaluateObviationCondition(IDictionary<string, object> boundVars)
        {
            return !(m_blackboard.ContainsUnit((IUnit)boundVars[PrologEvalRequest]) && m_blackboard.ContainsUnit((IUnit)boundVars[PrologKB]));
        }

        internal override void Execute(IDictionary<string, object> boundVars)
        {
            // fixme: finish implementing the Execute() method on KS_PrologEval. 

            // var contentUnits = (())
            /*string targetContentUnitID = ((U_IDSelectRequest)boundVars[IDSelectRequest]).TargetContentUnitID;
            var contentUnits = from contentUnit in m_blackboard.LookupUnits(ContentUnit.TypeName) // lookup content units
                               where ((ContentUnit)contentUnit).HasMetadataSlot(CUSlots.ContentUnitID) // where the content unit has an ID
                               where ((ContentUnit)contentUnit).Metadata[CUSlots.ContentUnitID].Equals(targetContentUnitID) // and the ID equals the target ID
                               select contentUnit;
            //fixme: for the purposes of getting something running quickly, I'm doing the random selection among potentially multiple content units here.
            // However, this should be done as a pooling process with some other selector selecting from the pool.
            if (contentUnits.Count() > 0)
            {
                // One or more content units matching the ContentUnitID in the U_IDQuery were found.
                // fixme: if no matching content unit was found, perhaps the KS should post something indiciating the execution failed. 
                // Or this could be done via tracing, with a KS that looks for tracing patterns, though this will require separate "failure patterns"
                // for each case. So probably better to have more general semantics for KSs to post success and failure into the trace. 

                int r = m_rand.Next(contentUnits.Count());
                ContentUnit randUnit = (ContentUnit)contentUnits.ElementAt(r);

                // Need to store the selected content unit. Creating a new type to do this seems awkward (SelectedContentUnit). 
                // Could do this with links: SelectedContentUnit link points at the correct CU. But what should it point from?
                // Default blackboard indexing always uses the type as the hashbucket. This is going to result in creating a zillion types. 
                // Solution: Add another slot called SelectedContentUnit and add it to a copy of the selected content unit. Value of the new slot is null. 

                // Selected content unit stored as a new ContentUnit with the SelectedContentUnit property set. It is linked back to the original content unit. 
                ContentUnit newUnit = new ContentUnit(randUnit);
                newUnit.Metadata[CUSlots.SelectedContentUnit] = null;
                m_blackboard.AddUnit(newUnit);
                m_blackboard.AddLink(randUnit, newUnit, LinkTypes.L_SelectedContentUnit);
            }
            m_blackboard.RemoveUnit((IUnit)boundVars[IDSelectRequest]); */

        }

        public KS_Old_ReactivePrologEval(IBlackboard blackboard) : base(blackboard)
        {
        }
    }
}
