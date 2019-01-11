using System.Collections.Generic;
using System.Dynamic;
using CSACore;

namespace KnowledgeSources
{
    public abstract class KnowledgeSource
    {
        protected readonly IBlackboard m_blackboard; 

        // A list of variable bindings (stored as string/object pairs) for each unique match of the precondition. 
        protected readonly List<ExpandoObject> m_contexts;

        // Variable bindings for a particular instantiation of the knowledge source. 
        protected dynamic m_boundVars;

        // The precondition for this knolwedge source to be executable. The precondition can bind variables. 
        // For each unique pair of variable bindings, a unique copy of the knowledge source is added to the agenda. 
        // The Preconditon() method is reponsible for saving bindings to a list of bindings. If the precondition doen't match
        // at all, the list of bindings will be empty.
        // fixme: consider creating a separate, weaker trigger condition as an optimization.
        // fixme: consider making this non-abstract to add assert condition !Executable.  
        public abstract void EvaluatePrecondition();

        // First evaluates the precondition. Then returns true if the precondition was satisfied at least once, false otherwise. 
        public IList<KnowledgeSource> Precondition()
        {
            m_contexts.RemoveRange(0, m_contexts.Count);
            EvaluatePrecondition();
            var executableList = new List<KnowledgeSource>(); 
            foreach (ExpandoObject o in m_contexts)
            {
                executableList.Add(Factory(m_blackboard, o));
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

        protected abstract KnowledgeSource Factory(IBlackboard blackboard, ExpandoObject boundVars);

        protected KnowledgeSource(IBlackboard blackboard)
        {
            m_boundVars = null; // When a knowledge source is first created, it is not executable. 
            m_contexts = new List<ExpandoObject>();
            m_blackboard = blackboard;
        }

        protected KnowledgeSource(IBlackboard blackboard, ExpandoObject boundVars)
        {
            m_boundVars = boundVars;
            m_contexts = null;
            m_blackboard = blackboard; 
        }

    }
}
