using System.Collections.Generic;
using CSA.KnowledgeUnits;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledCleanTree : KS_KC_ScheduledContentPoolCollector
    {
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            var treeNodes = UnitsFilteredByPrecondition(boundVars);
            foreach(Unit node in treeNodes)
            {
                m_blackboard.RemoveUnit(node);
            }
        }

        public KS_KC_ScheduledCleanTree(IBlackboard blackboard) : base(blackboard, GenerateHasComponent<KC_TreeNode>())
        {
        }

        public KS_KC_ScheduledCleanTree(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool, GenerateHasComponent<KC_TreeNode>())
        {
        }

        public KS_KC_ScheduledCleanTree(IBlackboard blackboard, FilterCondition filter) : 
            base(blackboard, (Unit unit) => GenerateHasComponent<KC_TreeNode>()(unit) && filter(unit))
        {
        }

        public KS_KC_ScheduledCleanTree(IBlackboard blackboard, string inputPool, FilterCondition filter) :
            base(blackboard, inputPool, (Unit unit) => GenerateHasComponent<KC_TreeNode>()(unit) && filter(unit))
        {
        }
    }
}

