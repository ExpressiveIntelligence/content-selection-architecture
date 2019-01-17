using System.Collections.Generic;
using CSACore;

namespace KnowledgeSources
{
    public abstract class KnowledgeSource
    {
        public IDictionary<string, object> Properties { get; }

        protected readonly IBlackboard m_blackboard; 

        // A list of variable bindings (stored as string/object pairs) for each unique match of the precondition. 
        protected readonly List<IDictionary<string, object>> m_contexts;

        // Variable bindings for a particular instantiation of the knowledge source. 
        protected IDictionary<string, object> m_boundVars;

        // The precondition for this knolwedge source to be executable. The precondition can bind variables. 
        // For each unique pair of variable bindings, a unique copy of the knowledge source is added to the agenda. 
        // The Preconditon() method is reponsible for saving bindings to a list of bindings. If the precondition doen't match
        // at all, the list of bindings will be empty.
        // fixme: consider creating a separate, weaker trigger condition as an optimization.
        // fixme: consider making this non-abstract to add assert condition !Executable.  
        protected abstract void EvaluatePrecondition();

        // First evaluates the precondition. Then returns an enumerable of instantiated knowledge sources (one for each unique combination of units
        // the precondition is satisfied by). The list is empty if the precondition is not satisfied.  
        public IEnumerable<KnowledgeSource> Precondition()
        {
            m_contexts.RemoveRange(0, m_contexts.Count);
            EvaluatePrecondition();
            var executableList = new List<KnowledgeSource>(); 
            foreach (IDictionary<string, object> dict in m_contexts)
            {
                executableList.Add(Factory(m_blackboard, dict, this));
            }
            return executableList;
        }

        // If this condition is satisfied, the executable KS should be removed from the agenda. 
        // fixme: consider making this non-abstract to add assert condition that KS is Executable. 
        public abstract bool EvaluateObviationCondition();

        // Executes the KS 
        public abstract void Execute();
   
        // True if this knowledge source is executable (has boundVars defined), false otherwise. The Executable KSs form the agenda.
        public bool Executable => m_boundVars != null;

        protected abstract KnowledgeSource Factory(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks);

        public KnowledgeSource(IBlackboard blackboard)
        {
            m_boundVars = null; // When a knowledge source is first created, it is not executable. 
            m_contexts = new List<IDictionary<string, object>>();
            m_blackboard = blackboard;
            Properties = new Dictionary<string, object>();
        }

        protected KnowledgeSource(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks)
        {
            m_boundVars = boundVars;
            m_contexts = null;
            m_blackboard = blackboard;
            Properties = new Dictionary<string, object>(ks.Properties);
        }

    }
}
