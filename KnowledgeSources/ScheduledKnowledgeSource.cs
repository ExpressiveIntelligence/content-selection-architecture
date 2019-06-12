﻿using System.Collections.Generic;
#if UNIT_TEST
using Xunit.Abstractions;
#endif
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
         * the public Execute() method. It returns an array of object arrays, each of which stores the variable bindings 
         * for a paricular collection of arguments. The knowledge source then processes each of these bindings 
         * (collection of arguments). 
         */
         // fixme: should be able to make this an ITuple array and use ValueTuples to represent bindings. But there's some kind of version issue.  
        protected abstract object[][] Precondition();

        /*
         * Executes the knowledge source given a particular set of arguments to operate on. 
         * Argument: A dictionary of bound variables that were bound by the Precondition.        
         */
        protected abstract void Execute(object[] boundVars);

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
         * An object[][] of length 0. Can be returned from Precondition() when there's no matching data
         * to marshal without having to construct one.
         */
        protected readonly object[][] m_emptyBindings;

        protected ScheduledKnowledgeSource(IBlackboard blackboard)
        {
            m_blackboard = blackboard;
            m_emptyBindings = new object[0][];
            Properties = new Dictionary<string, object>();
        }
    }
}
