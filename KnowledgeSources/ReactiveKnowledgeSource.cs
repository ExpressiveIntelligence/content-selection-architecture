using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public abstract class ReactiveKnowledgeSource : IReactiveKnowledgeSource
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

        protected readonly IList<HashSet<IUnit>> m_previousMatchSets;

        /*
         * An IKnowledgeSourceActivation array of length 0. Can be returned from Precondition() when there's no match without having to construct one.
         */
        protected readonly IKnowledgeSourceActivation[] m_emptyActivations;

        public ReactiveKnowledgeSource(IBlackboard blackboard)
        {
            m_blackboard = blackboard;
            m_previousMatchSets = new List<HashSet<IUnit>>();
            m_emptyActivations = new KnowledgeSourceActivation[0];
            Properties = new Dictionary<string, object>();
        }
    }
}
