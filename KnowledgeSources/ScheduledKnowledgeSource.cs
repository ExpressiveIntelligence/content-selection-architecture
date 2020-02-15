using CSA.Core;
using System.Collections.Generic;
#if UNIT_TEST
using Xunit.Abstractions;
#endif

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
         * Utility method for testing whether a unit is in a given pool.
         * fixme: determine if this should be public or protected.
         * Currently I am wanting to call this in the context of a KS_ScheduledExecute which is not a KS_ScheduledKnowledgeSource,
         * so either need to refactor KS_ScheduledExecute to make it conform to KS_ScheduledKnowledgeSource, leave this public, or create a new
         * class containing static utility methods. 
         */
        public static bool SelectFromPool(Unit unit, string inputPool)
        {
            return unit.HasComponent<KC_ContentPool>() && unit.ContentPoolEquals(inputPool);
        }

        /*
         * Utility method for copying a unit to a new content pool. 
         */
        protected Unit CopyUnitToPool(Unit unit, string pool)
        {
            Unit newUnit = new Unit(unit);

            /* 
             * If there is an existing content pool component remove the componenet before adding a new one with the new pool. The case in which there won't be a content pool
             * component is when copying from the global pool (no pool) into a pool.             
             */
            if (newUnit.HasComponent<KC_ContentPool>())
            {
                newUnit.RemoveComponent(newUnit.GetComponent<KC_ContentPool>());
            }
            newUnit.AddComponent(new KC_ContentPool(pool, true));

            m_blackboard.AddUnit(newUnit);
            m_blackboard.AddLink(unit, newUnit, LinkTypes.L_SelectedUnit, true); // fixme: need a more general link type for copies between pools
            return newUnit;
        }

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
