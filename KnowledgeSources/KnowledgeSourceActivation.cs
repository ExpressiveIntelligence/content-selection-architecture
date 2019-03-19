using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    public class KnowledgeSourceActivation : IKnowledgeSourceActivation
    {
        // Variable bindings bound by the precondition for this activation 
        protected readonly IDictionary<string, object> m_boundVars;

        // KS associated with this activation. 
        protected readonly ReactiveKnowledgeSource m_ks;

        // Accessor for properties of the associated knowledge source. 
        public IDictionary<string, object> Properties => m_ks.Properties;

        public bool EvaluateObviationCondition()
        {
            return m_ks.EvaluateObviationCondition(m_boundVars);
        }

        public void Execute()
        {
            m_ks.Execute(m_boundVars);
        }

        public KnowledgeSourceActivation(ReactiveKnowledgeSource ks, IDictionary<string, object> boundVars)
        {
            m_ks = ks;
            m_boundVars = boundVars;
        }
    }
}
