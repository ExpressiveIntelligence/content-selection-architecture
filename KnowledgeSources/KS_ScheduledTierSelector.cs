using System;
using System.Collections.Generic;
using System.Linq;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    /* 
     * KS_KC_ScheduledTierSelector is an abstract class which serves as the parents for all the varieties of selecting based on a sortable criterion, including
     * KS_HighestTierSelector, KS_LowestTierSelector, KS_HighestNTierSelector, KS_LowestNTierSelector.   
     * The type variable is used to specify the type of the KnowledgeComponent being used for selection.
     */
    public abstract class KS_ScheduledTierSelector<T> : KS_ScheduledFilterSelector where T : KnowledgeComponent, IComparable
    {
        public const string DefaultOutputPoolName = "SelectedByTier";

        /*
         * Returns an array of the Units filtered by the precondition sorted from lowest to highest value on KnowledgeComponent T
         */
        protected Unit[] SortUnitsFilteredByPrecondition(IDictionary<string, object> boundVars)
        {
            Unit[] units = UnitsFilteredByPrecondition(boundVars).ToArray();

            // Sort the units by the KnowledgeComponent T, from smallest to largest
            Array.Sort(units, (x, y) => x.GetComponent<T>().CompareTo(y.GetComponent<T>()));

            return units;
        }

        protected KS_ScheduledTierSelector(IBlackboard blackboard) : base(blackboard, DefaultOutputPoolName, GenerateHasComponent<T>())
        {
        }

        protected KS_ScheduledTierSelector(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool, GenerateHasComponent<T>())
        {
        }

        protected KS_ScheduledTierSelector(IBlackboard blackboard, string inputPool, string outputPool) :
            base(blackboard, inputPool, outputPool, GenerateHasComponent<T>())
        {
        }

        protected KS_ScheduledTierSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) :
            base(blackboard, outputPool, (Unit u) => filter(u) && GenerateHasComponent<T>() (u))
        {
        }

        protected KS_ScheduledTierSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter)
            : base(blackboard, inputPool, outputPool, (Unit u) => filter(u) && GenerateHasComponent<T>() (u))
        {
        }

     }
}
