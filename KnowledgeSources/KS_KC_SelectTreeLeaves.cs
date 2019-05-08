using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    /*
     * KS_SelectTreeLeaf looks through all the leaves of a tree for leaves passing the filter and copies them to the 
     * filter pool. Further filters can then downselect to one (or potentially more?) tree leaves to process, where "processing"
     * might mean expand (in the case of a grammar or search tree), execute (in the case of the ABL ABT or a behavior tree), or
     * other semantics. 
     */
    public class KS_KC_SelectTreeLeaves : KS_KC_ScheduledFilterSelector
    {

        // fixme: remove this string constant when the Dictionary is replaced with an array of object.
        public const string DefaultOutputPoolName = "SelectedLeafNodes";

        // Default filter pool name
        public const string LeafNodes = "LeafNodes";

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

        public KS_KC_SelectTreeLeaves(IBlackboard blackboard, string outputPool = LeafNodes) : base(blackboard, outputPool)
        {
            FilterConditionDel = SelectTreeLeaf;
        }


        public KS_KC_SelectTreeLeaves(IBlackboard blackboard, FilterCondition filter, string outputPool = LeafNodes) : base(blackboard, outputPool)
        {
            FilterConditionDel = (Unit unit) => SelectTreeLeaf(unit) && filter(unit);
        }
    }
}