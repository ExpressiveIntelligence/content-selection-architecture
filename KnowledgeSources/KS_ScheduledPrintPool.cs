using System;
using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    /*
     * Given a filter condition (the default filter condition is the constant true), prints all Units in the input pool matching the condition.
     */
    public class KS_ScheduledPrintPool : KS_ScheduledContentPoolCollector
    {
        /*
         * This will be called with the array of bound variables matched by the precondition of this KS. The inherited precondition from KS_ScheduledContentPoolCollector
         * will create an IEnumerable of all the Units that pass the filter condition and add this IEnumerable to the bound variables. Execute(object[] boundVars)
         * iterates over this IEnumerable, printing each Unit to console. 
         */
        protected override void Execute(object[] boundVars)
        {
            IEnumerable<Unit> units = (IEnumerable<Unit>)boundVars[FilteredUnits];

            Console.WriteLine("Units in pool " + InputPool + " matching filter condition:");

            foreach (Unit unit in units)
            {
                Console.WriteLine(unit);
            }

        }

        public KS_ScheduledPrintPool(IBlackboard blackboard) : base(blackboard)
        {
        }

        public KS_ScheduledPrintPool(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool)
        {
        }

        public KS_ScheduledPrintPool(IBlackboard blackboard, FilterCondition filter) : base(blackboard,filter)
        {
        }

        public KS_ScheduledPrintPool(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
        }

    }
}
