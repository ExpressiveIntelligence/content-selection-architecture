using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    /* 
     * Given a tree, adds a Unit to the blackboard containing the left-to-right sequence of tree leaves. 
     */
    public class KS_ScheduledLinearizeTreeLeaves : KS_ScheduledContentPoolCollector
    {

        /*
         * Given a unit with a KC_TreeNode component, adds all the leaves to the leaves parameter from left to right. 
         */
        private void AddLeaves(Unit unit, IList<Unit> leaves)
        {
            if (unit.IsTreeLeaf())
            {
                leaves.Add(unit);
            }
            else
            {
                foreach (KC_TreeNode child in unit.GetTreeChildren())
                {
                    AddLeaves(child.ContainingUnit, leaves);
                }
            }
        }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            IEnumerable<Unit> treeRoots = UnitsFilteredByPrecondition(boundVars);

            // There should be one tree root that we're collecting the leaves for.
            Debug.Assert(treeRoots.Count() == 1);

            IList<Unit> leaves = new List<Unit>();
            AddLeaves(treeRoots.First(), leaves);

            Unit unit = new Unit();
            unit.AddComponent(new KC_Sequence(leaves.ToArray(), true));
            m_blackboard.AddUnit(unit);
        }

        protected static bool SelectTreeRoot(Unit unit)
        {
            return unit.HasComponent<KC_TreeNode>() && unit.IsTreeRoot();
        }

        public KS_ScheduledLinearizeTreeLeaves(IBlackboard blackboard) : base(blackboard, SelectTreeRoot)
        {
        }

        public KS_ScheduledLinearizeTreeLeaves(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool, SelectTreeRoot)
        {
        }

        public KS_ScheduledLinearizeTreeLeaves(IBlackboard blackboard, FilterCondition filter) :
            base(blackboard, (Unit unit) => SelectTreeRoot(unit) && filter(unit))
        {
        }

        public KS_ScheduledLinearizeTreeLeaves(IBlackboard blackboard, string inputPool, FilterCondition filter) :
            base(blackboard, inputPool, (Unit unit) => SelectTreeRoot(unit) && filter(unit))
        {
        }
    }
}
