using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public abstract class KnowledgeSource : IKnowledgeSource
    {
        public IDictionary<string, object> Properties { get; }

        protected readonly IBlackboard m_blackboard; 

        // fixme: consider creating a separate, weaker trigger condition as an optimization.
        // Returns activations of the knowledge source.
        public abstract IKnowledgeSourceActivation[] Precondition();
 
        // If this condition is satisfied, the knowledge source activation should be removed from the agenda. 
        // Argument: A dictionary of bound variables that were bound by the Precondition.
        internal abstract bool EvaluateObviationCondition(IDictionary<string, object> boundVars);

        // Executes the KS 
        // Argument: A dictionary of bound variables that were bound by the Precondition.
        internal abstract void Execute(IDictionary<string, object> boundVars);
   
        public KnowledgeSource(IBlackboard blackboard)
        {
            m_blackboard = blackboard;
            Properties = new Dictionary<string, object>();
        }
    }
}
