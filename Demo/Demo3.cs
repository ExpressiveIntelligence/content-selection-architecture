using CSA.Controllers;
using CSA.Core;
using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;

namespace CSA.Demo
{
    public class Demo3
    {
        public IBlackboard Blackboard { get; }

        // fixme: remove
        // public CFGExpansionController Controller { get; }
        
        public ScheduledSequenceController Controller { get; }

        private const string grammarPool = "GrammarRulePool";

        public Demo3()
        {
            Blackboard = new Blackboard();
            // _ = ContentUnitSetupForDemos.Demo3_1_DefineUnits(Blackboard, grammarPool);
            _ = ContentUnitSetupForDemos.Demo3_2_DefineUnits(Blackboard, grammarPool);

            Controller = new ScheduledSequenceController();

            Controller.AddKnowledgeSource(new KS_KC_ScheduledPrintTree(Blackboard));
            Controller.AddKnowledgeSource(new KS_KC_SelectTreeLeaves(
                Blackboard,
                KS_KC_ContentPoolCollector.GenerateHasComponent<KC_IDSelectionRequest>()));
            Controller.AddKnowledgeSource(new KS_KC_HighestTierSelector<KC_Order>(
                Blackboard, 
                KS_KC_SelectTreeLeaves.DefaultOutputPoolName, 
                KS_KC_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName));
            Controller.AddKnowledgeSource(new KS_KC_ProcessTreeNode(
                Blackboard,
                KS_KC_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName,
                KS_KC_ProcessTreeNode.ActivateIDRequest));
            Controller.AddKnowledgeSource(new KS_KC_ScheduledIDSelector(
                Blackboard,
                grammarPool,
                KS_KC_ScheduledIDSelector.DefaultOutputPoolName));
            Controller.AddKnowledgeSource(new KS_KC_ScheduledUniformDistributionSelector(
                Blackboard,
                KS_KC_ScheduledIDSelector.DefaultOutputPoolName,
                KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName,
                1));
            Controller.AddKnowledgeSource(new KS_KC_DefaultGrammarNonterminalDecomposition(
                Blackboard,
                KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName));
            Controller.AddKnowledgeSource(new KS_KC_ExpandTreeNode(
                Blackboard,
                KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName));
            Controller.AddKnowledgeSource(new KS_KC_ScheduledFilterPoolCleaner(
                Blackboard,
                new string[] {
                    KS_KC_SelectTreeLeaves.DefaultOutputPoolName, 
                    KS_KC_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName,
                    KS_KC_ScheduledIDSelector.DefaultOutputPoolName,
                    KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName
                  }));

            // fixme: remove
            // Controller = new CFGExpansionController(expansionTreeRootNode, grammarPool, Blackboard);
        }
    }
}
