using System;
using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
using CSA.Controllers;
using static CSA.KnowledgeSources.KSProps;
using static CSA.Demo.ContentUnitSetupForDemos;

namespace CSA.Demo
{
    public class Demo1_Reactive
    {
        public IBlackboard Blackboard { get; }
        private readonly IReactiveKnowledgeSource m_IDSelector;
        private readonly KS_ReactiveChoicePresenter m_KSChoicePresenter;
        public IReactiveController Controller { get; }

        public void AddChoicePresenterHandler(EventHandler handler)
        {
            m_KSChoicePresenter.PresenterExecute += handler;
        }

        public Demo1_Reactive()
        {
        
            Blackboard = new Blackboard();

            // Set up the ContentUnits on the blackboard
            Demo1_DefineCUs(Blackboard);

            // Set up the knowledge sources
            m_IDSelector = new KS_ReactiveIDSelector(Blackboard);

            // fixme: Need to come up with interfaces for presenters, but won't know what the general presenter framework looks like until I've written more of them. 
            m_KSChoicePresenter = new KS_ReactiveChoicePresenter(Blackboard);
            m_IDSelector.Properties[Priority] = 20;
            m_KSChoicePresenter.Properties[Priority] = 10;

            // Set up the controller
            Controller = new ReactivePriorityController();
            Controller.AddKnowledgeSource(m_IDSelector);
            Controller.AddKnowledgeSource(m_KSChoicePresenter);

            // Put request for starting content unit in blackboard
            Blackboard.AddUnit(new U_IDSelectRequest("start"));
        }
    }
}
