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
            var selectTreeLeaves = new KS_ScheduledSelectTreeLeaves(Blackboard, KS_ScheduledContentPoolCollector.GenerateHasComponent<KC_IDSelectionRequest>());
            GenerateTree.AddKnowledgeSource(selectTreeLeaves);

            var highestTierSelector = new KS_ScheduledHighestTierSelector<KC_Order>(
                Blackboard,
                inputPool: selectTreeLeaves.OutputPool);
            GenerateTree.AddKnowledgeSource(highestTierSelector);

            GenerateTree.AddKnowledgeSource(new KS_ScheduledProcessTreeNode(
                Blackboard,
                inputPool: highestTierSelector.OutputPool,
                processNode: KS_ScheduledProcessTreeNode.ActivateIDRequest));

            var idSelector = new KS_ScheduledIDSelector(Blackboard, inputPool: grammarPool);
            GenerateTree.AddKnowledgeSource(idSelector);

            var uniformSelector = new KS_ScheduledUniformDistributionSelector(
                Blackboard,
                inputPool: idSelector.OutputPool,
                outputPool: null,
                numberToSelect: 1);
            GenerateTree.AddKnowledgeSource(uniformSelector);

            GenerateTree.AddKnowledgeSource(new KS_ScheduledDefaultGrammarNonterminalDecomposition(
                Blackboard,
                inputPool: uniformSelector.OutputPool));

            GenerateTree.AddKnowledgeSource(new KS_ScheduledExpandTreeNode(
                Blackboard,
                inputPool: uniformSelector.OutputPool));

            GenerateTree.AddKnowledgeSource(new KS_ScheduledFilterPoolCleaner(
                Blackboard,
                new string[] {
                    selectTreeLeaves.OutputPool,
                    highestTierSelector.OutputPool,
                    idSelector.OutputPool,
                    uniformSelector.OutputPool
                   }));

            CleanTree = new KS_ScheduledCleanTree(Blackboard);

            LinearizeTreeLeaves = new KS_ScheduledLinearizeTreeLeaves(Blackboard);
        }
    }
}
