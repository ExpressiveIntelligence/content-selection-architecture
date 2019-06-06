using System;
using System.Linq;
using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
using CSA.Controllers;
using static CSA.Demo.ContentUnitSetupForDemos;
using static CSA.KnowledgeUnits.KCNames;

namespace CSA.Demo
{
    public class Demo2
    {
        // Pool for units that have been selected by ID
        public const string IDSelectedPool = KS_ScheduledIDSelector.DefaultOutputPoolName;

        // Pool for units on which prolog expressions have been evaluated
        public const string PrologEvaledPool = KS_ScheduledPrologEval.DefaultOutputPoolName;

        // Pool for units for which their prolog expression evaluated to true
        public const string FilteredPrologResultPool = "FilteredPrologResultPool";

        // Pool for units uniformly selected from FilteredPrologResultPool
        public const string UniformlySelectedOutputPool = KS_ScheduledChoicePresenter.DefaultChoicePresenterInputPool;
 
        public IBlackboard Blackboard { get; }
        public ScheduledSequenceController Controller { get; }

        private readonly KS_ScheduledIDSelector m_IDSelector;
        private readonly KS_ScheduledPrologEval m_prologEval;
        private readonly KS_ScheduledFilterSelector m_filterByPrologResult;
        private readonly KS_ScheduledUniformDistributionSelector m_uniformRandomSelector;
        private readonly KS_ScheduledChoicePresenter m_choicePresenter;
        private readonly KS_ScheduledFilterPoolCleaner m_filterPoolCleaner;

        public void AddChoicePresenterHandler(EventHandler<PresenterExecuteEventArgs> handler)
        {
            m_choicePresenter.PresenterExecute += handler;
        }

        public void AddSelectChoicePresenterHandler(EventHandler<SelectChoiceEventArgs> handler)
        {
            m_choicePresenter.PresenterSelectChoice += handler;
        }

        public Demo2()
        {
            Blackboard = new Blackboard();
            Demo2_DefineUnits(Blackboard);

            m_IDSelector = new KS_ScheduledIDSelector(Blackboard);

            m_prologEval = new KS_ScheduledPrologEval(Blackboard,
                IDSelectedPool,
                PrologEvaledPool,
                ApplTest_Prolog);

            /*
             * fixme: consider creating a filtered by boolean result KS or a more general filter by KC_EvaluatedExpression (once I have more EvaluatedExpressions than Prolog). 
             */
            m_filterByPrologResult = new KS_ScheduledFilterSelector(Blackboard, 
                PrologEvaledPool,
                FilteredPrologResultPool, 
                KS_ScheduledPrologEval.FilterByPrologResult(ApplTest_Prolog, true));

            m_uniformRandomSelector = new KS_ScheduledUniformDistributionSelector(Blackboard, 
                FilteredPrologResultPool, 
                UniformlySelectedOutputPool, 1);

            m_choicePresenter = new KS_ScheduledChoicePresenter(Blackboard, 
                UniformlySelectedOutputPool);

           m_filterPoolCleaner = new KS_ScheduledFilterPoolCleaner(Blackboard,
                new string[]
                {
                    IDSelectedPool, // Output pool for ID selector
                    PrologEvaledPool, // Output pool for prolog eval
                    FilteredPrologResultPool, // Output pool for filter that filters by prolog result
                    UniformlySelectedOutputPool // Final output pool (written into by UniformDistributionSelector)
                });

            Controller = new ScheduledSequenceController();
            Controller.AddKnowledgeSource(m_IDSelector);
            Controller.AddKnowledgeSource(m_prologEval);
            Controller.AddKnowledgeSource(m_filterByPrologResult);
            Controller.AddKnowledgeSource(m_uniformRandomSelector);
            Controller.AddKnowledgeSource(m_choicePresenter);
            Controller.AddKnowledgeSource(m_filterPoolCleaner);

            // Put request for starting content unit in blackboard
            Unit req = new Unit();
            req.AddComponent(new KC_IDSelectionRequest("start", true));
            req.SetActiveRequest(true);
            Blackboard.AddUnit(req);
        }
    }
}
