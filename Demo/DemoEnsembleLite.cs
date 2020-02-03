using CSA.Controllers;
using CSA.Core;
using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
using static CSA.Demo.ContentUnitSetupForDemos;
using static CSA.KnowledgeUnits.KCNames;

namespace CSA.Demo
{
    /*
     * This demo currently does one shot selection, testing rules, weighting the dialog accordingly, and selecting the highest weighted dialog line.
     */
    public class DemoEnsembleLite
    {
        public IBlackboard Blackboard { get; }
        public ScheduledSequenceController Controller { get; }

        // Name of the rules pool
        public const string RulesPool = "rules";

        // Name of the dialog pool
        public const string DialogPool = "dialog";

        // Name of the pool containing satisfied rules.
        private const string SatisfiedRulesPool = "satisfiedRules";

        // Name of the pool containing the weighted dialog
        public const string WeightedDialogPool = "weightedDialog";

        // Name of the pool containing the selected dialog line.
        public const string SelectedDialogPool = "selectedDialog";

       public DemoEnsembleLite()
        {
            Blackboard = new Blackboard();
            DemoEnsemble_DefineUnits(Blackboard);

            /*
             * This prolog evaluator evauates the conditions of rules in the rules pool.
             * Construct the prolog evaluator with the following parameters:
             * * The blackboard on which to do the work.
             * * The input pool to look for ContentUnits with prolog expressions to evaluate.
             * * The name of the prolog expression to evaluate (in this case the prolog expression named ApplTest_Prolog, for which a string constant is defined in KCNames).
             */
            var prologEval = new KS_ScheduledPrologEval(Blackboard, inputPool: RulesPool, ApplTest_Prolog);

            /*
             * fixme: consider creating a filtered by boolean result KS or a more general filter by KC_EvaluatedExpression (once I have more EvaluatedExpressions than Prolog). 
             * This filter selector selects all the rules whose prolog expression evaluated to true. 
             */
            var filterByPrologResult = new KS_ScheduledFilterSelector(Blackboard,
                inputPool: prologEval.OutputPool,
                outputPool: SatisfiedRulesPool,
                filter: KS_ScheduledPrologEval.FilterByPrologResult(ApplTest_Prolog, true));

            /*
             * This KS prints out info about satisified rules (for debugging).
             */
            var printFilteredRules = new KS_ScheduledPrintPool(Blackboard, SatisfiedRulesPool);

            /*
             * This KS takes a pool of things to weight (in this case dialog) and a pool of utility sources (in this case satisfied rules) and sums the utility sources
             * applicable to each unit to weight to compute a new pool of weighted units. 
             */
            var weightDialogWithRules = new KS_UtilitySum(Blackboard, SatisfiedRulesPool, DialogPool, WeightedDialogPool);

            /*
             * This KS prints out the pool of weighted dialog (for debugging).
             */
            var printWeightedDialog = new KS_ScheduledPrintPool(Blackboard, WeightedDialogPool);

            /*
             * This KS selects a dialog line from among the mostly highly weighted dialog lines (e.g. if the highest weighting is 10, and their are two dialog lines
             * that share that weight, it will pick one of the two at random). 
             */
            var selectDialogLine = new KS_ScheduledHighestTierSelector<KC_Utility>(Blackboard, WeightedDialogPool, SelectedDialogPool);

            /*
             * This KS prints out the units in the SelectedDialogPool (there will be only one dialog line in this pool). (for debugging)
             */
            var printSelectedDialogLine = new KS_ScheduledPrintPool(Blackboard, SelectedDialogPool);

            /*
             * This KS cleans up all the pools that were created during the selection process.
             */

            var poolCleaner = new KS_ScheduledFilterPoolCleaner(
                Blackboard,
                new string[]
                {
                    SatisfiedRulesPool,
                    WeightedDialogPool,
                    SelectedDialogPool
                }
            );

            Controller = new ScheduledSequenceController();
            Controller.AddKnowledgeSource(prologEval);
            Controller.AddKnowledgeSource(filterByPrologResult);
            Controller.AddKnowledgeSource(printFilteredRules);
            Controller.AddKnowledgeSource(weightDialogWithRules);
            Controller.AddKnowledgeSource(printWeightedDialog);
            Controller.AddKnowledgeSource(selectDialogLine);
            Controller.AddKnowledgeSource(printSelectedDialogLine);
            Controller.AddKnowledgeSource(poolCleaner);
        }
    }
}
