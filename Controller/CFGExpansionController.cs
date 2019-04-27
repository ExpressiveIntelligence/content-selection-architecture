using System;
using System.Diagnostics;
using System.Linq;
using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.KUProps;
using static CSA.KnowledgeUnits.CUSlots;
using static CSA.KnowledgeUnits.LinkTypes;
using CSA.KnowledgeSources;

namespace CSA.Controllers
{
    /*
     * Controller class for doing context-free grammar expansions. Currently I'm hardcoding the logic for grammar 
     * expansions into the controller until I have some other tree expansion cases which I can then use to generalize this.     
     */
    public class CFGExpansionController : IScheduledController
    {
        public IUnit RootNode { get; }

        private readonly IBlackboard blackboard;

        // fixme: currently hardcoding the knowledge sources
        private readonly KS_ScheduledExecute m_addIDRequest;
        private readonly KS_ScheduledIDSelector m_lookupGrammarRules;
        private readonly KS_ScheduledUniformDistributionSelector m_chooseGrammarRule;
        private readonly KS_ScheduledExecute m_addRuleToTree;
        private readonly KS_ScheduledFilterPoolCleaner m_cleanSelectionPools;
        private readonly KS_ScheduledExecute m_addGeneratedSequence;

        // fixme: this is a hack just to keep the execute from adding more than one U_GeneratedSequence to the blackboard
        private bool finished = false; 

        /*
         * Executes one tree node update. Must be repeatedly called in an loop in order to fully expand a grammar request.
         * May need to change this, as this approach will throttle grammar expansion at 60 nodes per second in Unity, 
         * which may be too slow for large grammars. 
         */
        public void Execute()
        {
            if (!finished)
            {
                m_addIDRequest.Execute();
                m_lookupGrammarRules.Execute();
                m_chooseGrammarRule.Execute();
                m_addRuleToTree.Execute();
                m_cleanSelectionPools.Execute();
                if (!blackboard.Changed)
                {
                    finished = true;
                    m_addGeneratedSequence.Execute();
                    U_GeneratedSequence sequence = blackboard.LookupSingleton<U_GeneratedSequence>();
                    foreach (IUnit unit in sequence.Sequence)
                    {
                        Console.WriteLine(unit);
                    }
                }
            }
        }

        public CFGExpansionController(IUnit rootNode, string grammarRulePool, IBlackboard blackboard)
        {
            this.blackboard = blackboard;

            Debug.Assert(rootNode.HasSlot(GrammarNonTerminal));
            RootNode = rootNode;
            RootNode.Slots[IsTreeNode] = true;
            RootNode.Slots[IsLeafNode] = true;
            RootNode.Slots[WithinTreeLevelCount] = 1;

            
            m_addIDRequest = new KS_ScheduledExecute(
                () =>
                {
                    var nonTerminalLeafNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                               where node.HasSlot(IsLeafNode) && (bool)node.Slots[IsLeafNode] && node.HasSlot(GrammarNonTerminal)
                                               select node;

                    if (nonTerminalLeafNodes.Any())
                    {
                        Unit[] sortedLeafNodes = nonTerminalLeafNodes.ToArray();
                        Array.Sort(sortedLeafNodes, (x, y) => ((IComparable)x.Slots[WithinTreeLevelCount]).CompareTo(y.Slots[WithinTreeLevelCount]));
                        blackboard.AddUnit(new U_IDSelectRequest((string)sortedLeafNodes[0].Slots[GrammarNonTerminal]));
                        sortedLeafNodes[0].Slots[CurrentSymbolExpansion] = true;
                    }
                }
            );

            // string idOutputPool = "pool" + DateTime.Now.Ticks;
            string idOutputPool = "idOutputPool";
            m_lookupGrammarRules = new KS_ScheduledIDSelector(blackboard, grammarRulePool, idOutputPool);

            // string uniformDistOutputPool = "pool" + DateTime.Now.Ticks;
            string uniformDistOutputPool = "uniformDistOutputPool";
            m_chooseGrammarRule = new KS_ScheduledUniformDistributionSelector(blackboard, idOutputPool, uniformDistOutputPool, 1);

            m_addRuleToTree = new KS_ScheduledExecute(
                () =>
                {
                    var rule = from contentUnit in blackboard.LookupUnits<ContentUnit>()
                               where contentUnit.HasMetadataSlot(ContentPool) && contentUnit.Metadata[ContentPool].Equals(uniformDistOutputPool)
                               select contentUnit;

                    if (rule.Any())
                    {
                        /*
                         * fixme: need a way to refer to sortedLeafNodes[0] from the first KS_ScheduledExecute.
                         * Some Options:
                         *    * use a field within the controller (though violates the principle of using the blackboard for communication)
                         *    * set a property on the node (something like: CurrentSymbolExpansion) and use a blackboard query within here                         
                         */

                        Debug.Assert(rule.Count() == 1); // Only one rule is picked to expand a symbol

                        var nodeToExpandQuery = from unit in blackboard.LookupUnits<Unit>()
                                                where unit.HasSlot(CurrentSymbolExpansion) && (bool)unit.Slots[CurrentSymbolExpansion]
                                                select unit;

                        Debug.Assert(nodeToExpandQuery.Count() == 1);

                        Unit nodeToExpand = nodeToExpandQuery.First();
                        nodeToExpand.Slots[IsLeafNode] = false;
                        nodeToExpand.Slots[CurrentSymbolExpansion] = false;

                        ContentUnit ruleCopy = new ContentUnit(rule.First());
                        ruleCopy.Slots[IsTreeNode] = true;
                        ruleCopy.Slots[IsLeafNode] = false;
                        blackboard.AddUnit(ruleCopy);
                        bool result = blackboard.AddLink(nodeToExpand, ruleCopy, L_Tree, true);
                        Debug.Assert(result);

                        int i = 0;
                        foreach (IUnit child in (Unit[])ruleCopy.Metadata[GrammarRuleRHS])
                        {
                            // The GrammarNonterminal and GrammarTerminal properties are defined on the Units in Metadata[GrammarRuleRHS]
                            blackboard.AddUnit(child);
                            child.Slots[IsTreeNode] = true;
                            child.Slots[IsLeafNode] = true;
                            child.Slots[WithinTreeLevelCount] = i++;
                            result = blackboard.AddLink(ruleCopy, child, L_Tree, true);
                            Debug.Assert(result);
                        }
                    }
                }
            );

            m_cleanSelectionPools = new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { idOutputPool, uniformDistOutputPool });

            m_addGeneratedSequence = new KS_ScheduledExecute(
                () =>
                {
                    var leafNodes = from Unit node in blackboard.LookupUnits<Unit>()
                                    where node.HasSlot(IsLeafNode) && (bool)node.Slots[IsLeafNode]
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
                        /* fixme: doing this awkward thing where I grab all the Units separately from all the ContentUnits, since I don't index Units under all of their 
                         * type names. Once I've figured out a final representation for tree nodes, fix this. 
                         */
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
                    }
                }
            );
        }
    }
}
