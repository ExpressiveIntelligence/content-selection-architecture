using System.Collections.Generic;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    /*
     * Base class for all KnowledgeSources that select Units from an InputPool, perform some processing on them, and copy the modified Units to an OutputPool (leaving the
     * Units in the InputPool unchanged). This class is not abstract because it can be used to perform a copy operation with no processing. 
     */
    public class KS_ScheduledFilterSelector : KS_ScheduledContentPoolCollector
    {
        // Output pool for this filter. 
        public string OutputPool { get; protected set; }

        /*
         * Static output pool name enumerator. Since all instances of the class share the same enumerator, they will each get a unique DefaultOutputPoolName.
         */
        private static IEnumerator<string> m_OutputPoolNameEnumerator = OutputPoolNameEnumerator("Filtered");

        /*
         * The DefaultOutputPoolName is overriden in each child class. This means that the iterator for this parent class iterates (e.g. DefaultOutputPoolName
         * iterating from Filtered0 to Filtered1 even when child classes are instantiated. But every instance of any KS_ScheduledFilterSelector will still
         * have a unique DefaultOutputPoolName, even if they are not consecutive.
         */
        public virtual string DefaultOutputPoolName { get; } = GenDefaultOutputPoolName(m_OutputPoolNameEnumerator);

        /*
         * Given the class outputPoolNameEnumerator, returns the next unique DefaultOutputPoolName. 
         */
        protected static string GenDefaultOutputPoolName(IEnumerator<string> outputPoolNameEnumerator)
        {
            outputPoolNameEnumerator.MoveNext();
            return outputPoolNameEnumerator.Current;
        }

        /*
         * Returns an enumerator of unique OutputPool names based on a prefix.
         */
        protected static IEnumerator<string> OutputPoolNameEnumerator(string prefix)
        {
            uint gensymCounter = 0;
            while (true)
            {
                yield return prefix + gensymCounter++;
            }
        }

        /*
         * Copy a Unit to the output pool.
         */
        protected Unit CopyUnitToOutputPool(Unit unit)
        {
            return CopyUnitToPool(unit, OutputPool);
        }

        /*
         * On Execute(), the abstract FilterSelector copys the filtered CUs from the input pool to the output pool.  
         */
        protected override void Execute(object[] boundVars)
        {
            IEnumerable<Unit> units = (IEnumerable<Unit>)boundVars[FilteredUnits];

            foreach (Unit unit in units)
            {
                CopyUnitToOutputPool(unit);
            }
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard) : base(blackboard)
        {
            OutputPool = DefaultOutputPoolName;
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool)
        {
            OutputPool = DefaultOutputPoolName;
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool)
        {
            OutputPool = outputPool ?? DefaultOutputPoolName;
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard, FilterCondition filter) : base(blackboard, filter)
        {
            OutputPool = DefaultOutputPoolName;
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
            OutputPool = DefaultOutputPoolName;
        }

        /*
        * ScheduledFilterSelector constructed with both an input pool and a filter specified using the conjunction of SelectFromPool and filter 
        * as the FilterConditionDel.         
        */
        public KS_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
            OutputPool = outputPool ?? DefaultOutputPoolName;
        }
    }
}
