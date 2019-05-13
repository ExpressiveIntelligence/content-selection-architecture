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

        public KS_KC_ScheduledCleanTree CleanTree { get; }

        public KS_KC_ScheduledLinearizeTreeLeaves LinearizeTreeLeaves { get; }

        private const string grammarPool = "GrammarRulePool";

        public Demo3()
        {
            Blackboard = new Blackboard();
            // _ = ContentUnitSetupForDemos.Demo3_1_DefineUnits(Blackboard, grammarPool);
            _ = ContentUnitSetupForDemos.Demo3_2_DefineUnits(Blackboard, grammarPool);

            GenerateTree = new ScheduledSequenceController();

            // GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledPrintTree(Blackboard));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledSelectTreeLeaves(
                Blackboard,
                KS_KC_ScheduledContentPoolCollector.GenerateHasComponent<KC_IDSelectionRequest>()));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledHighestTierSelector<KC_Order>(
                Blackboard, 
                KS_KC_ScheduledSelectTreeLeaves.DefaultOutputPoolName, 
                KS_KC_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledProcessTreeNode(
                Blackboard,
                KS_KC_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName,
                KS_KC_ScheduledProcessTreeNode.ActivateIDRequest));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledIDSelector(
                Blackboard,
                grammarPool,
                KS_KC_ScheduledIDSelector.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledUniformDistributionSelector(
                Blackboard,
                KS_KC_ScheduledIDSelector.DefaultOutputPoolName,
                KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName,
                1));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledDefaultGrammarNonterminalDecomposition(
                Blackboard,
                KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledExpandTreeNode(
                Blackboard,
                KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName));
            GenerateTree.AddKnowledgeSource(new KS_KC_ScheduledFilterPoolCleaner(
                Blackboard,
                new string[] {
                    KS_KC_ScheduledSelectTreeLeaves.DefaultOutputPoolName, 
                    KS_KC_ScheduledTierSelector<KC_Order>.DefaultOutputPoolName,
                    KS_KC_ScheduledIDSelector.DefaultOutputPoolName,
                    KS_KC_ScheduledUniformDistributionSelector.DefaultOutputPoolName
                  }));

            CleanTree = new KS_KC_ScheduledCleanTree(Blackboard);

            LinearizeTreeLeaves = new KS_KC_ScheduledLinearizeTreeLeaves(Blackboard);
        }
    }
}
