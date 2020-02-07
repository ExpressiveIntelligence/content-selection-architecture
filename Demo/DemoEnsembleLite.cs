using CSA.Controllers;
using CSA.Core;
using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
using static CSA.Demo.ContentUnitSetupForDemos;
using static CSA.KnowledgeUnits.KCNames;
using System.Linq;

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

        private const string EvalRulesPool = "evalRules";

        // Name of the pool containing satisfied rules.
        private const string SatisfiedRulesPool = "satisfiedRules";

        // Name of the pool containing the weighted dialog
        public const string WeightedDialogPool = "weightedDialog";

        // Name of the pool containing the highest weighted (highest utility) dialog lines.
        public const string HighestWeightedDialogPool = "highestWeightedDialog";

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
            var prologEval = new KS_ScheduledPrologEval(Blackboard, inputPool: RulesPool, outputPool: EvalRulesPool, ApplTest_Prolog);

            /*
             * fixme: consider creating a filtered by boolean result KS or a more general filter by KC_EvaluatedExpression (once I have more EvaluatedExpressions than Prolog). 
             * This filter selector selects all the rules whose prolog expression evaluated to true. 
             */
            var filterByPrologResult = new KS_ScheduledFilterSelector(Blackboard,
                inputPool: EvalRulesPool,
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
             * This KS selects the most highly weighted dialog lines into a pool (e.g. if the highest weighting is 10, and their are two dialog lines
             * that share that weight, it will copy both of them into the output pool).  
             */
            var selectHighestWeightedDialog = new KS_ScheduledHighestTierSelector<KC_Utility>(Blackboard, WeightedDialogPool, HighestWeightedDialogPool);

            /*
             * This KS prints out the units in the HighestWeightedDialogPool (for debugging).
             */
            var printHighestWeightedDialog = new KS_ScheduledPrintPool(Blackboard, HighestWeightedDialogPool);

            /*
             * This KS selects one line at random from the HighestWeightedDialogPool.
             */
            var selectDialogLine = new KS_ScheduledUniformDistributionSelector(Blackboard,
                inputPool: HighestWeightedDialogPool, outputPool: SelectedDialogPool, numberToSelect: 1);

            /*
             * This KS prints out the units in the SelectedDialogPool (there will be only one dialog line in this pool). (for debugging)
             */
            var printSelectedDialogLine = new KS_ScheduledPrintPool(Blackboard, SelectedDialogPool);

            /*
             * This KS is updating the prolog KB based on the effects on the selected dialog line.
             * fixme: for now I'm just using KS_ScheduledExecute to call the SelectChoice_PrologKBChanges. Need to refactor SelectChoice_PrologKBChanges because it's not just tied to SelectChoice and possibly move KB updates into
             * a special purpose KS. 
             */
            var updatePrologKB = new KS_ScheduledExecute(
                () =>
                {
                    var selectedDialodLines = from Unit node in Blackboard.LookupUnits<Unit>()
                                              where ScheduledKnowledgeSource.SelectFromPool(node, SelectedDialogPool)
                                              select node;

                    foreach (Unit unit in selectedDialodLines)
                    {
                        /*
                         * SelectChoice_PrologKBChanges() is an event handler, so it expects a sender as well as for args to be wrapped in EventArgs.
                         * So I'm using this as the sender (PrologKBChanges doesn't actually use the sender) wrapping in SelectChoiceEventArgs.
                         */
                        var eventArgs = new SelectChoiceEventArgs(unit, Blackboard);
                        EventHandlers_ChoicePresenter.SelectChoice_PrologKBChanges(this, eventArgs);
                    }
                }
            );

            /*
             * This KS cleans up all the pools that were created during the selection process.
             */
            var poolCleaner = new KS_ScheduledFilterPoolCleaner(
                Blackboard,
                new string[]
                {
                    EvalRulesPool,
                    SatisfiedRulesPool,
                    WeightedDialogPool,
                    HighestWeightedDialogPool,
                    SelectedDialogPool
                }
            );

            Controller = new ScheduledSequenceController();
            Controller.AddKnowledgeSource(prologEval);
            Controller.AddKnowledgeSource(filterByPrologResult);
            Controller.AddKnowledgeSource(printFilteredRules);
            Controller.AddKnowledgeSource(weightDialogWithRules);
            Controller.AddKnowledgeSource(printWeightedDialog);
            Controller.AddKnowledgeSource(selectHighestWeightedDialog);
            Controller.AddKnowledgeSource(printHighestWeightedDialog);
            Controller.AddKnowledgeSource(selectDialogLine);
            Controller.AddKnowledgeSource(printSelectedDialogLine);
            Controller.AddKnowledgeSource(updatePrologKB);
            Controller.AddKnowledgeSource(poolCleaner);
        }
    }
}
