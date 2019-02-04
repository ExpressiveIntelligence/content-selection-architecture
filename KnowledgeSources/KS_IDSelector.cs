using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using CSA.KnowledgeUnits;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_IDSelector : KnowledgeSource
    {
        // Name of the bound context variable
        private const string IDQuery = "IDQuery";

        // fixme: Used to randomly select from among multiple CU indices. Should be delete when this is converted into a pooling selector. 
        private static readonly Random m_rand = new Random();

        public override IKnowledgeSourceActivation[] Precondition()
        {
            // Use LINQ to create a collection of the requested U_IDQueries on the blackboard.
            var queries = from query in m_blackboard.LookupUnits(U_IDQuery.TypeName) // Lookup ID queries
                          where // where the query has not been previously matched by this knowledge source precondition
                            (!query.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched)) ||
                            (!((ISet<KnowledgeSource>)query.Properties[U_PropertyNames.KSPreconditionMatched]).Contains(this))
                          select query;

            IKnowledgeSourceActivation[] activations = new IKnowledgeSourceActivation[queries.Count()];

            // Iterate through each of the queries, creating KnowledgeSourceActivations
            int i = 0; 
            foreach (var query in queries)
            {
                var boundVars = new Dictionary<string, object>
                {
                    [IDQuery] = query
                };

                activations[i++] = new KnowledgeSourceActivation(this, boundVars);

                // fixme: this bit of boilerplate code for marking a knowledge unit as having already participated in a matched precondition 
                // should be baked into the infrastructure somewhere so knowledge source implementers don't always have to do this. 
                // A good place to add this would be on Unit (and declare a method on IUnit).
                if (query.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched))
                {
                    ((HashSet<KnowledgeSource>)query.Properties[U_PropertyNames.KSPreconditionMatched]).Add(this);
                }
                else
                {
                    query.Properties[U_PropertyNames.KSPreconditionMatched] = new HashSet<KnowledgeSource> { this };
                }
            }

            return activations;
        }

        internal override bool EvaluateObviationCondition(IDictionary<string, object> boundVars)
        {
            return !m_blackboard.ContainsUnit((IUnit)boundVars[IDQuery]);
        }

        internal override void Execute(IDictionary<string, object> boundVars)
        {
            string targetContentUnitID = ((U_IDQuery)boundVars[IDQuery]).TargetContentUnitID;
            var contentUnits = from contentUnit in m_blackboard.LookupUnits(ContentUnit.TypeName) // lookup content units
                               where ((ContentUnit)contentUnit).HasMetadataSlot(CU_SlotNames.ContentUnitID) // where the content unit has an ID
                               where ((ContentUnit)contentUnit).Metadata[CU_SlotNames.ContentUnitID].Equals(targetContentUnitID) // and the ID equals the target ID
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
                newUnit.Metadata[CU_SlotNames.SelectedContentUnit] = null;
                m_blackboard.AddUnit(newUnit);
                m_blackboard.AddLink(randUnit, newUnit, LinkTypes.L_SelectedContentUnit);
            }
            m_blackboard.RemoveUnit((IUnit)boundVars[IDQuery]);
         }
         
        public KS_IDSelector(IBlackboard blackboard) : base(blackboard)
        {

        }
    }
}
