using System.Collections.Generic;
using Xunit.Abstractions;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public abstract class ScheduledKnowledgeSource : IScheduledKnowledgeSource
    {
        public IDictionary<string, object> Properties { get; }

#if UNIT_TEST
        public ITestOutputHelper XunitOutput { get; set; }
#endif

        protected readonly IBlackboard m_blackboard;

        /*
         * On ScheduledKnowledgeSources, Precondition() is used to marshal data to operate on. It is called from 
         * the public Execute() method. It returns an array of Dictionaries, each of which stores the variable bindings 
         * for a paricular collection of arguments. The knowledge source then processes each of these bindings 
         * (collection of arguments). 
         */
        protected abstract IDictionary<string, object>[] Precondition();

        /*
         * Executes the knowledge source given a particular set of arguments to operate on. 
         * Argument: A dictionary of bound variables that were bound by the Precondition.        
         */
        protected abstract void Execute(IDictionary<string, object> boundVars);

        /*
         * Executes the ScheduledKnowledgeSource. This involves calling the Precondition() in order to marshal the data to 
         * operate on, then calling Execute() on each of the resulting collections of arguments.          
         */
        public void Execute()
        {
            var boundInstances = Precondition();
            foreach (var binding in boundInstances)
            {
                Execute(binding);
            }
        }

        /*
         * An IDictionary array of length 0. Can be returned from Precondition() when there's no matching data
         * to marshal without having to construct one.
         */
        protected readonly IDictionary<string, object>[] m_emptyBindings;

        public ScheduledKnowledgeSource(IBlackboard blackboard)
        {
            m_blackboard = blackboard;
            m_emptyBindings = new IDictionary<string, object>[0];
            Properties = new Dictionary<string, object>();
        }
    }
}
