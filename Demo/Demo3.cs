using CSA.Controllers;
using CSA.Core;
using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;

namespace CSA.Demo
{
    public class Demo3
    {
        public IBlackboard Blackboard { get; }

        public ScheduledSequenceController GenerateTree { get; }

        public KS_ScheduledCleanTree CleanTree { get; }

        public KS_ScheduledLinearizeTreeLeaves LinearizeTreeLeaves { get; }

        private const string grammarPool = "GrammarRulePool";

        public Demo3()
        {
            Blackboard = new Blackboard();
            // _ = ContentUnitSetupForDemos.Demo3_1_DefineUnits(Blackboard, grammarPool);
            _ = ContentUnitSetupForDemos.Demo3_2_DefineUnits(Blackboard, grammarPool);

            GenerateTree = new ScheduledSequenceController();

            // GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledPrintTree(Blackboard));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledSelectTreeLeaves(
                Blackboard,
                KS_ScheduledContentPoolCollector.GenerateHasComponent<KC_IDSelectionRequest>()));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledHighestTierSelector<KC_Order>(
                Blackboard, 
                KS_ScheduledSelectTreeLeaves.DefaultOutputPoolName, 
                KS_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledProcessTreeNode(
                Blackboard,
                KS_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName,
                KS_ScheduledProcessTreeNode.ActivateIDRequest));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledIDSelector(
                Blackboard,
                grammarPool,
                KS_ScheduledIDSelector.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledUniformDistributionSelector(
                Blackboard,
                KS_ScheduledIDSelector.DefaultOutputPoolName,
                KS_ScheduledUniformDistributionSelector.DefaultOutputPoolName,
                1));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledDefaultGrammarNonterminalDecomposition(
                Blackboard,
                KS_ScheduledUniformDistributionSelector.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledExpandTreeNode(
                Blackboard,
                KS_ScheduledUniformDistributionSelector.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_ScheduledFilterPoolCleaner(
                Blackboard,
                new string[] {
                    KS_ScheduledSelectTreeLeaves.DefaultOutputPoolName, 
                    KS_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName,
                    KS_ScheduledIDSelector.DefaultOutputPoolName,
                    KS_ScheduledUniformDistributionSelector.DefaultOutputPoolName
                  }));

            CleanTree = new KS_ScheduledCleanTree(Blackboard);

            LinearizeTreeLeaves = new KS_ScheduledLinearizeTreeLeaves(Blackboard);
        }
    }
}
