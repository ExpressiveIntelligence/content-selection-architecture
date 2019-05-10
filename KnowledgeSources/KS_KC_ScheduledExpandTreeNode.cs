using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.KUProps;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledExpandTreeNode : KS_KC_ScheduledContentPoolCollector
    {
        protected const string Decomposition = "Decomposition";
        protected const string NodeToExpandRef = "NodeToExpand";
        protected const string OrderCounter = "OrderCounter";

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
                var nodeToExpandQuery = from unit in m_blackboard.LookupUnits<Unit>()
                                        where unit.HasComponent<KC_UnitReference>() && unit.ReferenceNameEquals(CurrentTreeNodeExpansion)
                                        select unit;

                /*
                 * Grab a reference to the order counter. Currently assume there's only one order counter. 
                 * fixme: consider adding a name to the order counter so there can be multipe counters.                 
                 */ 
                var orderCounterQuery = from unit in m_blackboard.LookupUnits<Unit>()
                                        where unit.HasComponent<KC_OrderCounter>()
                                        select unit;

                if (nodeToExpandQuery.Any())
                {
                    // At least one node to expand was found - continue. 

                    // There should just be one nodeToExpandRef
                    Debug.Assert(nodeToExpandQuery.Count() == 1);

                    // Currently assuming there are 0 or 1 order counters. 
                    Debug.Assert(!orderCounterQuery.Any() || orderCounterQuery.Count() == 1);

                    // Store the first order counter unit found (assumption is there is only one) or null if none were found.
                    Unit orderCounterUnit = orderCounterQuery.FirstOrDefault();

                    // Create the new bindings with the decomposition and the node to expand.
                    IDictionary<string, object>[] bindings = new Dictionary<string, object>[1];


                    bindings[0] = new Dictionary<string, object>
                    {
                        [Decomposition] = filteredDecomps.First(),
                        [NodeToExpandRef] = nodeToExpandQuery.First(),
                        [OrderCounter] = orderCounterUnit
                    };

                    return bindings;
                }
            }
            // No decomposition or no node to expand was found - return empty bindings.
            return m_emptyBindings;
        }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            Unit nodeToExpandRef = (Unit)boundVars[NodeToExpandRef];
            Unit decomposition = (Unit)boundVars[Decomposition];
            Unit orderCounter = (Unit)boundVars[OrderCounter];

            Unit ruleNode = new Unit(decomposition);
            // Remove the KC_Decomposition (not needed) and KC_ContentPool (will cause node to be prematurely cleaned up) components
            ruleNode.RemoveComponent(ruleNode.GetComponent<KC_Decomposition>()); // fixme: consider adding AddSingleComponent<T> and RemoveSingletonComponent<T> to simplify working with this common case
            ruleNode.RemoveComponent(ruleNode.GetComponent<KC_ContentPool>()); // fixme: consider adding AddSingleComponent<T> and RemoveSingletonComponent<T> to simplify working with this common case

            // Add a tree node component with the parent being the node to expand. 
            // fixme: consider defining conversion operators so this looks like
            // new KC_TreeNode((KC_TreeNode)nodeToExpand);
            ruleNode.AddComponent(new KC_TreeNode(nodeToExpandRef.GetUnitReference().GetComponent<KC_TreeNode>()));

            // Now that we've used it to add to the tree, remove the nodeToExpandRef from the blackboard
            m_blackboard.RemoveUnit(nodeToExpandRef);

            // fixme: Add an KC_Order to the nodes being added. In the general case may not need this - figure out how to refactor it later when other tree expansion 
            // examples exist. 
            if (orderCounter != null)
            {
                int order = orderCounter.IncOrderCounter();
                ruleNode.AddComponent(new KC_Order(order, true));
            }

            m_blackboard.AddUnit(ruleNode);

            // For each of the Units in the decomposition, add them to the tree as children of ruleCopy. 
            foreach (Unit child in decomposition.GetDecomposition())
            {
                // Make a copy of Unit in the decomposition and add it to the tree. 
                Unit childNode = new Unit(child);
                m_blackboard.AddUnit(childNode);
                childNode.AddComponent(new KC_TreeNode(ruleNode.GetComponent<KC_TreeNode>()));
                if (orderCounter != null)
                {
                    int order = orderCounter.IncOrderCounter();
                    childNode.AddComponent(new KC_Order(order, true));
                }
            }
        }

        protected KS_KC_ScheduledExpandTreeNode(IBlackboard blackboard) : base(blackboard)
        {
            FilterConditionDel = DefaultFilterCondition;
        }

        public KS_KC_ScheduledExpandTreeNode(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool)
        {
        }

        protected KS_KC_ScheduledExpandTreeNode(IBlackboard blackboard, FilterCondition filter) : base(blackboard, filter)
        {
        }

        /*
         * ScheduledFilterSelector constructed with both an input pool and a filter specified using the conjunction of SelectFromPool and filter 
         * as the FilterConditionDel.         
         */
        protected KS_KC_ScheduledExpandTreeNode(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
        }

    }
}
