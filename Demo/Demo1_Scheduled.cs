using System;
using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
using CSA.Controllers;
using static CSA.KnowledgeSources.KSProps;
using static CSA.Demo.ContentUnitSetupForDemos;

namespace CSA.Demo
{
    public class Demo1_Scheduled
    {
        const string FinalOutputPool = "FinalOutputPool";

        public IBlackboard Blackboard { get; }
        private readonly IScheduledKnowledgeSource m_IDSelector;
        private readonly IScheduledKnowledgeSource m_uniformRandomSelector;
        private readonly KS_ReactiveChoicePresenter m_KSChoicePresenter;
        public IScheduledController Controller { get; }

        public Demo1_Scheduled()
        {
            Blackboard = new Blackboard();
            m_IDSelector = new KS_ScheduledIDSelector(Blackboard);
            m_uniformRandomSelector = new KS_ScheduledUniformDistributionSelector(Blackboard, KS_ScheduledIDSelector.DefaultOutputPoolName, FinalOutputPool, 1);

        }
    }
}
