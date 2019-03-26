using System;
using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
using CSA.Controllers;
using static CSA.Demo.ContentUnitSetupForDemos;

namespace CSA.Demo
{
    public class Demo2
    {
        private const string UniformSelectionOutputPool = KS_ScheduledChoicePresenter.DefaultChoicePresenterInputPool;
        private const string FilteredPrologResultPool = "FilteredPrologResultPool";

        public IBlackboard Blackboard { get; }
        public IScheduledController Controller { get; }
        
        private readonly KS_ScheduledIDSelector m_IDSelector;
        private readonly KS_ScheduledPrologEval m_prologEval;
        private readonly KS_ScheduledFilterSelector m_filterByPrologResult;
        private readonly KS_ScheduledUniformDistributionSelector m_uniformRandomSelector;
        private readonly KS_ScheduledChoicePresenter m_choicePresenter;
        private readonly KS_ScheduledFilterPoolCleaner m_filterPoolCleaner;

        public void AddChoicePresenterHandler(EventHandler<KS_ScheduledChoicePresenter.PresenterExecuteEventArgs> handler)
        {
            m_choicePresenter.PresenterExecute += handler;
        }

        public Demo2()
        {
            Blackboard = new Blackboard();
            Demo2_DefineCUs(Blackboard);

            m_IDSelector = new KS_ScheduledIDSelector(Blackboard);

            m_prologEval = new KS_ScheduledPrologEval(Blackboard, KS_ScheduledIDSelector.DefaultOutputPoolName, KS_ScheduledPrologEval.DefaultOutputPoolName);

            m_filterByPrologResult = new KS_ScheduledFilterSelector(Blackboard, KS_ScheduledPrologEval.DefaultOutputPoolName, 
                FilteredPrologResultPool, KS_ScheduledPrologEval.FilterByPrologResult(true));

            m_uniformRandomSelector = new KS_ScheduledUniformDistributionSelector(Blackboard, FilteredPrologResultPool, UniformSelectionOutputPool, 1);

            m_choicePresenter = new KS_ScheduledChoicePresenter(Blackboard, UniformSelectionOutputPool);

            m_filterPoolCleaner = new KS_ScheduledFilterPoolCleaner(Blackboard,
                new string[] 
                { 
                    KS_ScheduledIDSelector.DefaultOutputPoolName, // Output pool for ID selector
                    KS_ScheduledPrologEval.DefaultOutputPoolName, // Output pool for prolog eval
                    FilteredPrologResultPool, // Output pool for filter that filters by prolog result
                    UniformSelectionOutputPool // Final output pool (written into by UniformDistributionSelector)
                });

            Controller = new ScheduledSequenceController();
            Controller.AddKnowledgeSource(m_IDSelector);
            Controller.AddKnowledgeSource(m_prologEval);
            Controller.AddKnowledgeSource(m_filterByPrologResult);
            Controller.AddKnowledgeSource(m_uniformRandomSelector);
            Controller.AddKnowledgeSource(m_choicePresenter);
            Controller.AddKnowledgeSource(m_filterPoolCleaner);

            // Put request for starting content unit in blackboard
            Blackboard.AddUnit(new U_IDSelectRequest("start"));
        }
    }
}
