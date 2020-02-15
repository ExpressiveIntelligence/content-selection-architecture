using CSA.Core;
using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    /*
     * KS_SelectTreeLeaf looks through all the leaves of a tree for leaves passing the filter and copies them to the 
     * filter pool. Further filters can then downselect to one (or potentially more?) tree leaves to process, where "processing"
     * might mean expand (in the case of a grammar or search tree), execute (in the case of the ABL ABT or a behavior tree), or
     * other semantics. 
     */
    public class KS_ScheduledSelectTreeLeaves : KS_ScheduledFilterSelector
    {
        /*
         * fixme: in general need a content pool specifier as there may be other leaf nodes on the blackboard not associated with the tree of interest.
         * Currently the constructors of this class don't support having trees live in content pools.         
         */

        /*
         * Initializing the enumerator of unique output pool names (static) and the initialization of the DefaultOutputPoolName (instance).
         */
        private static readonly IEnumerator<string> m_OutputPoolNameEnumerator = OutputPoolNameEnumerator("LeafNodes");
        public override string DefaultOutputPoolName { get; } = GenDefaultOutputPoolName(m_OutputPoolNameEnumerator);


        /*
         * Given a Unit, returns true if the unit is the leaf of a tree, false otherwise. 
         */
        private bool SelectTreeLeaf(Unit node)
        {
            return node.HasComponent<KC_TreeNode>() && node.IsTreeLeaf();
        }

        /*
         * The constructors don't currently support having a tree live in a partcular content pool. KS_SelectTreeLeaves assumes that there is only one global tree on the 
         * blackboard. 
         * fixme: figure out how to best handle multiple trees when this case comes up. 
         */

        public KS_ScheduledSelectTreeLeaves(IBlackboard blackboard) : base(blackboard)
        {
            FilterConditionDel = SelectTreeLeaf;
        }

        /*
         * fixme: This constructor breaks the rule for KS_ScheduledFilterSelectors that if there is one string parameter it is the inputPool.
         * To make the constructor patterns match we also need to make sure that the KSs which add leaves are doing so in the right pool.
         * This is, we could easily change this class to search for tree leaves in a specific input pool, but KSs down the line that modify the tree wouldn't know
         * about it. 
         */
        public KS_ScheduledSelectTreeLeaves(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool)
        {
            FilterConditionDel = SelectTreeLeaf;
        }

        public KS_ScheduledSelectTreeLeaves(IBlackboard blackboard, FilterCondition filter) : base(blackboard)
        {
            FilterConditionDel = (Unit unit) => SelectTreeLeaf(unit) && filter(unit);
        }

        /*
         * fixme: This constructor breaks the rule for KS_ScheduledFilterSelectors that if there is one string parameter it is the inputPool.
         * To make the constructor patterns match we also need to make sure that the KSs which add leaves are doing so in the right pool.
         * This is, we could easily change this class to search for tree leaves in a specific input pool, but KSs down the line that modify the tree wouldn't know
         * about it. 
         */
        public KS_ScheduledSelectTreeLeaves(IBlackboard blackboard, FilterCondition filter, string outputPool) : base(blackboard, outputPool)
        {
            FilterConditionDel = (Unit unit) => SelectTreeLeaf(unit) && filter(unit);
        }
    }
}