using System;
using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
using CSA.Controllers;
using static CSA.Demo.ContentUnitSetupForDemos;

namespace CSA.Demo
{
    public class Demo1_Scheduled
    {
        private const string FinalOutputPool = KS_ScheduledChoicePresenter.DefaultChoicePresenterInputPool;

        public IBlackboard Blackboard { get; }
        public IScheduledController Controller { get; }

        private readonly KS_ScheduledIDSelector m_IDSelector;
        private readonly KS_ScheduledUniformDistributionSelector m_uniformRandomSelector;
        private readonly KS_ScheduledChoicePresenter m_choicePresenter;
        private readonly KS_ScheduledFilterPoolCleaner m_filterPoolCleaner;

        public void AddChoicePresenterHandler(EventHandler<KS_ScheduledChoicePresenter.PresenterExecuteEventArgs> handler)
        {
            m_choicePresenter.PresenterExecute += handler;
        }

        public Demo1_Scheduled()
        {
            Blackboard = new Blackboard();
            Demo1_DefineCUs(Blackboard);

            m_IDSelector = new KS_ScheduledIDSelector(Blackboard);
            m_uniformRandomSelector = new KS_ScheduledUniformDistributionSelector(Blackboard, KS_ScheduledIDSelector.DefaultOutputPoolName, FinalOutputPool, 1);
            m_choicePresenter = new KS_ScheduledChoicePresenter(Blackboard, FinalOutputPool);
            m_filterPoolCleaner = new KS_ScheduledFilterPoolCleaner(Blackboard, 
                new string[] { KS_ScheduledIDSelector.DefaultOutputPoolName, FinalOutputPool});

            Controller = new ScheduledSequenceController();
            Controller.AddKnowledgeSource(m_IDSelector);
            Controller.AddKnowledgeSource(m_uniformRandomSelector);
            Controller.AddKnowledgeSource(m_choicePresenter);
            Controller.AddKnowledgeSource(m_filterPoolCleaner);

            // Put request for starting content unit in blackboard
            Blackboard.AddUnit(new U_IDSelectRequest("start"));
        }
    }
}
