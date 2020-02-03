using CSA.Core;
using CSA.KnowledgeUnits;
using System;
using System.Linq;
using System.Collections.Generic;
namespace CSA.KnowledgeSources
{
    /*
     * Takes two different input pools and combines them to create an output pool of utility weighted Units. One input pool, UtilitySourcePool, is a collection
     * of utility sources. Each of these utility sources contributes utility increments (decrements) to items in the second input pool, the SelectionPool. This is
     * a pool of Units that we want to select from based on utility weightings. The OutputPool consists of copies of items from the SelectionPool with utilities
     * updated by the Units in the UtilitySourcePool. 
     */
    public class KS_UtilitySum : ScheduledKnowledgeSource
    {
        // Indices for unit and utility source bindings. 
        protected const int UnitToWeight = 0;
        protected const int UtilitySources = 1;

        /*
         * Pool of utility sources to apply. This will add increments (decrements) of utility to items in the SelectionPool. 
         */
        public string UtilitySourcePool { get; }

        /*
         * Pool of Units that will be selected from based on utilities applied from the UtilitySourcePool.
         */
        public string SelectionPool { get;  }

        /*
         * Contains a copy of the Units from the SelectionPool with utilities derived from the SelectionPool.
         */
        public string OutputPool { get;  }

        /*
         * Creates bindings consisting of an IEnumerable of utility sources to apply to a unit from the selection pool, and the Unit(s) to apply this to. In normal
         * opertion this will be a single Unit that is being weighted, but making this an IEnumerable for now in case multiple Units in the selection pool share the
         * same ID.
         * fixme: currently assuming that utility sources use a unit ID to indicate the Unit in the selection pool that this applies to.
         * We will eventually want to generalize this to other ways of tying utility sources to selection pool units, such as arbitrary tags or cosign similarity.
         * This may involve creating and executing KSs inside of the Precondition as a way to reuse code, but will cross that bridge later. 
         */
        protected override object[][] Precondition()
        {
            /*
             * First LINQ to grab the utility sources. 
             */
            var utilitySources = from unit in m_blackboard.LookupUnits<Unit>()
                                 where SelectFromPool(unit, UtilitySourcePool)
                                 where unit.HasComponent<KC_IDSelectionRequest>() && unit.HasComponent<KC_Utility>()
                                 select unit;

            /*
             * LINQ to do a group join of utility sources whose targetID matches the ID of each unit in the selection pool.
             * Results an IEnumerable of Unit (to weight) utility sources (to apply) pairs. 
             */
            var weightings = from unit in m_blackboard.LookupUnits<Unit>()
                             where SelectFromPool(unit, SelectionPool)
                             join utilitySource in utilitySources on unit.GetUnitID() equals utilitySource.GetTargetUnitID() into utilitySourceGroup
                             select new {unit , utilitySourceGroup};

            if (weightings.Any())
            {
                var bindings = new object[weightings.Count()][];

                int i = 0;
                foreach (var weighting in weightings)
                {
                    object[] binding = new object[2];
                    binding[UnitToWeight] = weighting.unit;
                    binding[UtilitySources] = weighting.utilitySourceGroup;

                    bindings[i++] = binding;

                }
                return bindings;
            }
            else
            {
                return m_emptyBindings;
            }

        }

        /*
         * Called with a object[2] array, where boundVars[UnitToWeight] is the Unit which should be copied to the OutputPool and weighted, while
         * boundVars[UtilitySources] is an IEnumerable<Unit> containing the utility sources to sum to determine the utility (weighting) of the UnitToWeight.
         */
        protected override void Execute(object[] boundVars)
        {
            // Grab the arguments. 
            Unit unitToWeight = boundVars[UnitToWeight] as Unit;
            IEnumerable<Unit> utilitySources = boundVars[UtilitySources] as IEnumerable<Unit>;

            // Make a copy of the unitToWeight in the OutputPool.
            Unit unitCopy = CopyUnitToPool(unitToWeight, OutputPool);

            /*
             * If the copy of the unitToWeight doesn't already have a KC_Utility, add one. If it does already have a KC_Utility, then this is an initial weighting
             * that we will be adding to (subtracting from) as we sum our utility sources. 
             */
            if (!unitCopy.HasComponent<KC_Utility>())
            {
                unitCopy.AddComponent(new KC_Utility(0));
            }

            /*
             * Sum the utility sources. 
             */
            double utilitySum = 0;
            foreach (Unit utilitySource in utilitySources)
            {
                utilitySum += utilitySource.GetUtility();
            }

            // Update the utility of the copy of the unitToWeight. 
            unitCopy.SetUtility(unitCopy.GetUtility() + utilitySum);
        }

        public KS_UtilitySum(IBlackboard blackboard, string utilitySourcePool, string selectionPool, string outputPool) : base(blackboard)
        {
            UtilitySourcePool = utilitySourcePool;
            SelectionPool = selectionPool;
            OutputPool = outputPool;
        }


    }
}
