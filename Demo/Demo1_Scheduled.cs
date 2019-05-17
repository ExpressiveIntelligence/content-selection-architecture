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
        private const string FinalOutputPool = KS_KC_ScheduledChoicePresenter.DefaultChoicePresenterInputPool;

        public IBlackboard Blackboard { get; }
        public ScheduledSequenceController Controller { get; }

        private readonly KS_KC_ScheduledIDSelector m_IDSelector;
        private readonly KS_KC_ScheduledUniformDistributionSelector m_uniformRandomSelector;
        private readonly KS_KC_ScheduledChoicePresenter m_choicePresenter;
        private readonly KS_KC_ScheduledFilterPoolCleaner m_filterPoolCleaner;

        public void AddChoicePresenterHandler(EventHandler<KC_PresenterExecuteEventArgs> handler)
        {
            m_choicePresenter.PresenterExecute += handler;
        }

        public Demo1_Scheduled()
        {
            Blackboard = new Blackboard();
            Demo1_KC_DefineUnits(Blackboard);

            m_IDSelector = new KS_KC_ScheduledIDSelector(Blackboard);
            m_uniformRandomSelector = new KS_KC_ScheduledUniformDistributionSelector(Blackboard, KS_KC_ScheduledIDSelector.DefaultOutputPoolName, FinalOutputPool, 1);
            m_choicePresenter = new KS_KC_ScheduledChoicePresenter(Blackboard, FinalOutputPool);
            m_filterPoolCleaner = new KS_KC_ScheduledFilterPoolCleaner(Blackboard, 
                new string[] { KS_KC_ScheduledIDSelector.DefaultOutputPoolName, FinalOutputPool});

            Controller = new ScheduledSequenceController();
            Controller.AddKnowledgeSource(m_IDSelector);
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
