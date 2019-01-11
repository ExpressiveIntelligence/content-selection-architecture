using System.Linq;
using System.Dynamic;
using KnowledgeUnits;
using System.Collections.Generic;
using CSACore;

namespace KnowledgeSources
{
    public class KS_IDSelector : KnowledgeSource
    {
        // Name of the bound context variable
        // private const string IDQuery = "IDQuery";

        public override void EvaluatePrecondition()
        {
            // Use LINQ to create a collection of the requested U_IDQueries on the blackboard.
            var queries = from query in m_blackboard.LookupUnits(KU_Names.U_IDQuery)
                          select query;

            // Iterate through each of the queries, creating context entries
            foreach (var query in queries)
            {
                dynamic boundVars = new ExpandoObject();
                boundVars.IDQuery = query;
                m_contexts.Add(boundVars);
            }

        }

        public override bool EvaluateObviationCondition()
        {
            return m_blackboard.ContainsUnit((IUnit)m_boundVars.IDQuery);
        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }

        protected override KnowledgeSource Factory(IBlackboard blackboard, ExpandoObject boundVars)
        {
            return new KS_IDSelector(blackboard, boundVars);
        }

        private KS_IDSelector(IBlackboard blackboard, ExpandoObject boundVars) : base(blackboard, boundVars)
        {
        }
    }
}
