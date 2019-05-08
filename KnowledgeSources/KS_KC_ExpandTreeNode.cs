using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.KUProps;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ExpandTreeNode : KS_KC_ContentPoolCollector
    {
        protected const string Decomposition = "Decomposition";
        protected const string NodeToExpand = "NodeToExpand";

        protected override IDictionary<string, object>[] Precondition()
        {
            // First execute the base precondition to get the decomposition to use.
            var decompBinding = base.Precondition();

            if (decompBinding.Any())
            {
                // A decomposition was found - continue

                // There should be exactly one set of bindings. 
                Debug.Assert(decompBinding.Length == 1);

                var filteredDecomps = UnitsFilteredByPrecondition(decompBinding[0]);

                // There should be one decomposition in the binding
                Debug.Assert(filteredDecomps.Count() == 1);

                /*
                 * Grab the reference to the current leaf node (non-terminal) we're expanding. 
                 */
                var nodeToExpand = from unit in m_blackboard.LookupUnits<Unit>()
                                   where unit.HasComponent<KC_UnitReference>() && unit.ReferenceNameEquals(CurrentTreeNodeExpansion)
                                   select unit.GetUnitReference();

                if (nodeToExpand.Any())
                {
                    // At least one node to expand was found - continue. 

                    // There should just be one nodeToExpandRef
                    Debug.Assert(nodeToExpand.Count() == 1);

                    // Create the new bindings with the decomposition and the node to expand.
                    IDictionary<string, object>[] bindings = new Dictionary<string, object>[1];
                    bindings[0] = new Dictionary<string, object>
                    {
                        [Decomposition] = filteredDecomps.First(),
                        [NodeToExpand] = nodeToExpand
                    };

                    return bindings;
                }
            }
            // No decomposition or no node to expand was found - return empty bindings.
            return m_emptyBindings;
        }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            Unit nodeToExpand = (Unit)boundVars[NodeToExpand];
            Unit decomposition = (Unit)boundVars[Decomposition];

            Unit ruleNode = new Unit(decomposition);
            // Remove the KC_Decomposition (not needed) and KC_ContentPool (will cause node to be prematurely cleaned up) components
            ruleNode.RemoveComponent(ruleNode.GetComponent<KC_Decomposition>());
            ruleNode.RemoveComponent(ruleNode.GetComponent<KC_ContentPool>());

            // Add a tree node component with the parent being the node to expand. 
            // fixme: consider defining conversion operators so this looks like
            // new KC_TreeNode((KC_TreeNode)nodeToExpand);
            ruleNode.AddComponent(new KC_TreeNode(nodeToExpand.GetComponent<KC_TreeNode>()));
            m_blackboard.AddUnit(ruleNode);

            // For each of the Units in the decomposition, add them to the tree as children of ruleCopy. 
            foreach (Unit child in decomposition.GetDecomposition())
            {
                // Make a copy of Unit in the decomposition and add it to the tree. 
                Unit childNode = new Unit(child);
                m_blackboard.AddUnit(childNode);
                childNode.AddComponent(new KC_TreeNode(ruleNode.GetComponent<KC_TreeNode>()));
            }
        }

        protected KS_KC_ExpandTreeNode(IBlackboard blackboard) : base(blackboard)
        {
            FilterConditionDel = DefaultFilterCondition;
        }

        public KS_KC_ExpandTreeNode(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool)
        {
        }

        protected KS_KC_ExpandTreeNode(IBlackboard blackboard, FilterCondition filter) : base(blackboard, filter)
        {
        }

        /*
         * ScheduledFilterSelector constructed with both an input pool and a filter specified using the conjunction of SelectFromPool and filter 
         * as the FilterConditionDel.         
         */
        protected KS_KC_ExpandTreeNode(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
        }

    }
}
