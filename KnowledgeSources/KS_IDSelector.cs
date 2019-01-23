﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using KnowledgeUnits;
using CSACore;

namespace KnowledgeSources
{
    public class KS_IDSelector : KnowledgeSource
    {
        // Name of the bound context variable
        private const string IDQuery = "IDQuery";

        // fixme: Used to randomly select from among multiple CU indices. Should be delete when this is converted into a pooling selector. 
        private static readonly Random m_rand = new Random();

        protected override void EvaluatePrecondition()
        {
            // Use LINQ to create a collection of the requested U_IDQueries on the blackboard.
            var queries = from query in m_blackboard.LookupUnits(U_IDQuery.TypeName)
                          where 
                            (!query.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched)) ||
                            (!((ISet<KnowledgeSource>)query.Properties[U_PropertyNames.KSPreconditionMatched]).Contains(this))
                          select query;

            // Iterate through each of the queries, creating context entries
            foreach (var query in queries)
            {
                var boundVars = new Dictionary<string, object>
                {
                    [IDQuery] = query
                };
                m_contexts.Add(boundVars);

                // fixme: this bit of boilerplate code for marking a knowledge unit as having already participated in a matched precondition 
                // should be baked into the infrastructure somewhere so knowledge source implementers don't always have to do this. 
                if (query.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched))
                {
                    ((HashSet<KnowledgeSource>)query.Properties[U_PropertyNames.KSPreconditionMatched]).Add(this);
                }
                else
                {
                    query.Properties[U_PropertyNames.KSPreconditionMatched] = new HashSet<KnowledgeSource> { this };
                }
            }
        }

        public override bool EvaluateObviationCondition()
        {
            Debug.Assert(Executable);
            return !m_blackboard.ContainsUnit((IUnit)m_boundVars[IDQuery]);
        }

        public override void Execute()
        {
            Debug.Assert(Executable);
            string targetContentUnitID = ((U_IDQuery)m_boundVars[IDQuery]).TargetContentUnitID;
            var contentUnits = from contentUnit in m_blackboard.LookupUnits(ContentUnit.TypeName)
                               where ((ContentUnit)contentUnit).HasMetadataSlot(CU_SlotNames.ContentUnitID)
                               where ((ContentUnit)contentUnit).Metadata[CU_SlotNames.ContentUnitID].Equals(targetContentUnitID)
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

                ContentUnit newUnit = new ContentUnit(randUnit);
                newUnit.Metadata[CU_SlotNames.SelectedContentUnit] = null;
                m_blackboard.AddUnit(newUnit);
                m_blackboard.AddLink(randUnit, newUnit, LinkTypes.L_SelectedContentUnit);
            }
            m_blackboard.RemoveUnit((IUnit)m_boundVars[IDQuery]);
            m_boundVars = null;
         }

        // fixme: the factory and constructor stuff for knowledge sources (which supports constructing knowledge sources with bound variables)
        // is a bit confusing. See if there's a cleaner way to represent active KSs vs. KSs on the agenda, perhaps with an ActivatedKS class which wraps the KS and its bound variables.
        protected override KnowledgeSource Factory(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks)
        {
            return new KS_IDSelector(blackboard, boundVars, ks);
        }

        private KS_IDSelector(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks) : base(blackboard, boundVars, ks)
        {
        }

        public KS_IDSelector(IBlackboard blackboard) : base(blackboard)
        {
        }
    }
}
