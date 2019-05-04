using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.KUProps;
using CSA.KnowledgeSources;

namespace CSA.Controllers
{
    /*
     * Controller class for doing context-free grammar expansions. Currently I'm hardcoding the logic for grammar 
     * expansions into the controller until I have some other tree expansion cases which I can then use to generalize this.     
     */
    public class CFGExpansionController : IScheduledController
    {
        public Unit RootNode { get; }

        private readonly IBlackboard blackboard;

        // fixme: currently hardcoding the knowledge sources
        private readonly KS_ScheduledExecute m_pickLeftmostNodeToExpand;
        private readonly KS_KC_ScheduledIDSelector m_lookupGrammarRules;
        private readonly KS_KC_ScheduledUniformDistributionSelector m_chooseGrammarRule;
        private readonly KS_ScheduledExecute m_treeExpander;
        private readonly KS_KC_ScheduledFilterPoolCleaner m_cleanSelectionPools;
        private readonly KS_ScheduledExecute m_addGeneratedSequence;

        // fixme: this is a hack just to keep the execute from adding more than one U_GeneratedSequence to the blackboard
        // private bool finished = false;

        /*
         * Prints out the generated grammar tree starting with the root node.
         */
        private void PrintTree()
        {
            Console.WriteLine(RootNode);
            foreach(var child in RootNode.GetTreeChildren())
            {
                PrintTreeHelper(1, child.ContainingUnit);
            }
        }

        private void PrintTreeHelper(uint indent, Unit node)
        {
            Console.WriteLine(IndentLines(indent, node.ToString()));
            foreach(var child in node.GetTreeChildren())
            {
                PrintTreeHelper(indent + 1, child.ContainingUnit);
            }
        }

        private string IndentLines(uint indent, string s)
        {
            StringReader reader = new StringReader(s);
            StringBuilder sb = new StringBuilder(100);
            string line; 
            while ((line = reader.ReadLine()) != null)
            {
                Indent(indent, sb);
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        private void Indent(uint indent, StringBuilder sb)
        {
            for(uint i = 0; i < indent; i++)
            {
                sb.Append("  ");
            }
        }

        /*
         * Executes one tree node update. Must be repeatedly called in an loop in order to fully expand a grammar request.
         * May need to change this, as this approach will throttle grammar expansion at 60 nodes per second in Unity, 
         * which may be too slow for large grammars. 
         */
        public void Execute()
        {
            var nonTerminalLeafNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                           where node.HasComponent<KC_TreeNode>() && node.IsTreeLeaf()
                                           where node.HasComponent<KC_IDSelectionRequest>()
                                           select node;

            // fixme: generated sequence gets spit out twice
            if (nonTerminalLeafNodes.Any())
            {
                // PrintTree();
                m_pickLeftmostNodeToExpand.Execute();
                m_lookupGrammarRules.Execute();
                m_chooseGrammarRule.Execute();
                m_treeExpander.Execute();
                m_cleanSelectionPools.Execute();
            }
            else
            {
                m_addGeneratedSequence.Execute();
            }
        }

        private void AddLeafs(Unit unit, IList<Unit> leafs)
        {
            if (unit.IsTreeLeaf())
            {
                leafs.Add(unit);
            }
            else
            {
                foreach(KC_TreeNode child in unit.GetTreeChildren())
                {
                    AddLeafs(child.ContainingUnit, leafs);
                }
            }
        }

        public CFGExpansionController(Unit rootNode, string grammarRulePool, IBlackboard blackboard)
        {
            this.blackboard = blackboard;

            RootNode = rootNode;
            
            m_pickLeftmostNodeToExpand = new KS_ScheduledExecute(
                () =>
                {
                    var nonTerminalLeafNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                               where node.HasComponent<KC_TreeNode>() && node.IsTreeLeaf()
                                               where node.HasComponent<KC_IDSelectionRequest>()
                                               select node;

                    if (nonTerminalLeafNodes.Any())
                    {
                        nonTerminalLeafNodes.First().SetActiveRequest(true);

                        /*
                         * Save a reference to the current tree node we're expanding on the blackboard. 
                         */
                        Unit leafExpansionRef = new Unit();
                        leafExpansionRef.AddComponent(new KC_UnitReference(CurrentTreeNodeExpansion, true, nonTerminalLeafNodes.First()));
                        blackboard.AddUnit(leafExpansionRef);
                    }

                    // fixme: used to sort by the WithinTreeLevelCount to ensure that non-terminals were expanded left to right. Come back to this if it becomes an issue. 
                    /* if (nonTerminalLeafNodes.Any())
                    {
                        Unit[] sortedLeafNodes = nonTerminalLeafNodes.ToArray();
                        Array.Sort(sortedLeafNodes, (x, y) => ((IComparable)x.Slots[WithinTreeLevelCount]).CompareTo(y.Slots[WithinTreeLevelCount]));
                        blackboard.AddUnit(new U_IDSelectRequest((string)sortedLeafNodes[0].Slots[GrammarNonTerminal]));
                        sortedLeafNodes[0].Slots[CurrentSymbolExpansion] = true;
                    } */
                }
            );

            // string idOutputPool = "pool" + DateTime.Now.Ticks;
            string idOutputPool = "idOutputPool";
            m_lookupGrammarRules = new KS_KC_ScheduledIDSelector(blackboard, grammarRulePool, idOutputPool);

            // string uniformDistOutputPool = "pool" + DateTime.Now.Ticks;
            string uniformDistOutputPool = "uniformDistOutputPool";
            m_chooseGrammarRule = new KS_KC_ScheduledUniformDistributionSelector(blackboard, idOutputPool, uniformDistOutputPool, 1);

            m_treeExpander = new KS_ScheduledExecute(
                () =>
                {
                    var rule = from unit in blackboard.LookupUnits<Unit>()
                               where unit.HasComponent<KC_ContentPool>() && unit.ContentPoolEquals(uniformDistOutputPool)
                               select unit;

                    /*
                     * Grab the reference to the current leaf node we're expanding. 
                     */
                    var nodeToExpandQuery = from unit in blackboard.LookupUnits<Unit>()
                                            where unit.HasComponent<KC_UnitReference>()
                                            select unit;
                    Unit nodeToExpandRef = nodeToExpandQuery.First();

                    if (rule.Any())
                    {
                        Debug.Assert(rule.Count() == 1); // Only one rule is picked to expand a symbol

                        Debug.Assert(nodeToExpandQuery.Count() == 1); // Should be only one reference we're expanding. 

                        Unit selectedRule = rule.First();
                        Unit ruleNode = new Unit(selectedRule);
                        // Remove the KC_Decomposition (not needed) and KC_ContentPool (will cause node to be prematurely cleaned up) components
                        ruleNode.RemoveComponent(ruleNode.GetComponent<KC_Decomposition>());
                        ruleNode.RemoveComponent(ruleNode.GetComponent<KC_ContentPool>());

                        // fixme: consider defining conversion operators so this looks like
                        // new KC_TreeNode((KC_TreeNode)nodeToExpand);
                        ruleNode.AddComponent(new KC_TreeNode(nodeToExpandRef.GetUnitReference().GetComponent<KC_TreeNode>()));
                        blackboard.AddUnit(ruleNode);

                        // For each of the Units in the decomposition, add them to the tree as children of ruleCopy. 
                        foreach (Unit child in selectedRule.GetDecomposition())
                        {
                            // Make a copy of Unit in the decomposition and add it to the tree. 
                            Unit childNode = new Unit(child);
                            blackboard.AddUnit(childNode);
                            childNode.AddComponent(new KC_TreeNode(ruleNode.GetComponent<KC_TreeNode>()));
                        }
                    }
                    else
                    {
                        // No rule was found. Create a pseudo-decomposition consisting of just the TargetUnitID in ## (borrowing from Tracery). 
                        Unit noRuleTextDecomp = new Unit();
                        noRuleTextDecomp.AddComponent(new KC_TreeNode(nodeToExpandRef.GetUnitReference().GetComponent<KC_TreeNode>()));
                        noRuleTextDecomp.AddComponent(new KC_Text("#" + nodeToExpandRef.GetUnitReference().GetTargetUnitID() + "#", true));
                        blackboard.AddUnit(noRuleTextDecomp);
                    }
                    blackboard.RemoveUnit(nodeToExpandRef); // Remove the reference to the leaf node to expand (it has been expanded).
                }
            );

            m_cleanSelectionPools = new KS_KC_ScheduledFilterPoolCleaner(blackboard, new string[] { idOutputPool, uniformDistOutputPool });

            bool GenSequencePrecond()
            {
                var leafNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                where node.HasComponent<KC_TreeNode>() && node.IsTreeLeaf()
                                select node;

                // This is ready to run if no leaf node contains a KC_IDSelectionRequest component (meaning it's a non-terminal). 
                return leafNodes.All(node => !node.HasComponent<KC_IDSelectionRequest>());
            }


            void GenSequenceExec()
            {
                // Walk the tree to find the leafs from left to right. 
                IList<Unit> leafs = new List<Unit>();

                AddLeafs(RootNode, leafs);

                // Write out the leafs of the generated tree
                foreach(Unit leaf in leafs)
                {
                    Console.Write(leaf.GetText() + " ");
                }

                // Delete the tree. 
                var treeNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                where node.HasComponent<KC_TreeNode>()
                                select node;

                foreach(var node in treeNodes)
                {
                    blackboard.RemoveUnit(node);
                }
            }

            m_addGeneratedSequence = new KS_ScheduledExecute(GenSequenceExec, GenSequencePrecond);

            // fixme: old code for linearizing the leaves of the tree. 
            /* () =>
            {
                var leafNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                where node.HasComponent<KC_TreeNode>() && node.IsTreeLeaf()
                                select node;

                // If all the leaf nodes in the tree are terminal nodes then we're done
                if (leafNodes.All(node => node.HasSlot(GrammarTerminal)))
                {
                    // Sort the leaf nodes by their level within the three 
                    Unit[] sortedLeafNodes = leafNodes.ToArray();
                    Array.Sort(sortedLeafNodes, (x, y) => ((IComparable)x.Slots[WithinTreeLevelCount]).CompareTo(y.Slots[WithinTreeLevelCount]));

                    U_GeneratedSequence generatedSequence = new U_GeneratedSequence(sortedLeafNodes);
                    _ = blackboard.AddUnit(generatedSequence);


                    // Remove all the nodes in the tree
                    // fixme: doing this awkward thing where I grab all the Units separately from all the ContentUnits, since I don't index Units under all of their 
                    // type names. Once I've figured out a final representation for tree nodes, fix this. 
                    //
                    var units_treeNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                          where node.HasSlot(IsTreeNode) && (bool)node.Slots[IsTreeNode]
                                          select node;

                    var contentUnits_treeNodes = from ContentUnit node in blackboard.LookupUnits<ContentUnit>()
                                                 where node.HasSlot(IsTreeNode) && (bool)node.Slots[IsTreeNode]
                                                 select node;
                    foreach (Unit u in units_treeNodes)
                    {
                        _ = blackboard.RemoveUnit(u);
                    }

                    foreach (ContentUnit cu in contentUnits_treeNodes)
                    {
                        _ = blackboard.RemoveUnit(cu);
                    }
            } */
        }
    }
}
