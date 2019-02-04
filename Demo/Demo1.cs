using System;
using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
using CSA.Controllers;

namespace CSA.Demo
{
    public class Demo1
    {
        public IBlackboard Blackboard { get; }
        private IKnowledgeSource m_IDSelector;
        private KS_ChoicePresenter m_KSChoicePresenter;
        public IController Controller { get; }

        public void AddChoicePresenterHandler(EventHandler handler)
        {
            m_KSChoicePresenter.PresenterExecute += handler;
        }

        public Demo1()
        {
        
            Blackboard = new Blackboard();

            // Set up the ContentUnits on the blackboard
            Demo1_DefineCUs(Blackboard);

            // Set up the knowledge sources
            m_IDSelector = new KS_IDSelector(Blackboard);

            // fixme: Need to come up with interfaces for presenters, but won't know what the general presenter framework looks like until I've written more of them. 
            m_KSChoicePresenter = new KS_ChoicePresenter(Blackboard);
            m_IDSelector.Properties[KS_PropertyNames.Priority] = 20;
            m_KSChoicePresenter.Properties[KS_PropertyNames.Priority] = 10;

            // Set up the controller
            Controller = new PriorityController();
            Controller.AddKnowledgeSource(m_IDSelector);
            Controller.AddKnowledgeSource(m_KSChoicePresenter);

            // Put request for starting content unit in blackboard
            Blackboard.AddUnit(new U_IDQuery("start"));
        }

        private static void Demo1_DefineCUs(IBlackboard blackboard)
        {
            ContentUnit start = new ContentUnit();
            start.Metadata[CU_SlotNames.ContentUnitID] = "start";
            start.Content[CU_SlotNames.Text] = "You stand before an old wooden door.";

            ContentUnit start_Choice1 = new ContentUnit();
            start_Choice1.Metadata[CU_SlotNames.TargetContentUnitID] = "open door";
            start_Choice1.Content[CU_SlotNames.Text] = "Open the door";

            ContentUnit start_Choice2 = new ContentUnit();
            start_Choice2.Metadata[CU_SlotNames.TargetContentUnitID] = "wait";
            start_Choice2.Content[CU_SlotNames.Text] = "Wait";

            ContentUnit openDoor = new ContentUnit();
            openDoor.Metadata[CU_SlotNames.ContentUnitID] = "open door";
            openDoor.Content[CU_SlotNames.Text] = "Opening the door, you step forth into adventure.";

            ContentUnit waited = new ContentUnit();
            waited.Metadata[CU_SlotNames.ContentUnitID] = "wait";
            waited.Content[CU_SlotNames.Text] = "You wait fruitlessly in front the door.";

            ContentUnit waitedChoice1 = new ContentUnit();
            waitedChoice1.Metadata[CU_SlotNames.TargetContentUnitID] = "waited again";
            waitedChoice1.Content[CU_SlotNames.Text] = "Wait again";

            ContentUnit waitedChoice2 = new ContentUnit();
            waitedChoice2.Metadata[CU_SlotNames.TargetContentUnitID] = "open door after waiting";
            waitedChoice2.Content[CU_SlotNames.Text] = "Finally open the door";

            ContentUnit waitedAgain = new ContentUnit();
            waitedAgain.Metadata[CU_SlotNames.ContentUnitID] = "waited again";
            waitedAgain.Content[CU_SlotNames.Text] = "Sunk in a deep malaise, you wait the rest of your life.";

            ContentUnit openDoorAfterWaiting = new ContentUnit();
            openDoorAfterWaiting.Metadata[CU_SlotNames.ContentUnitID] = "open door after waiting";
            openDoorAfterWaiting.Content[CU_SlotNames.Text] = "Breaking through your reservations, you step forward into a life of adventure.";

            blackboard.AddUnit(start);
            blackboard.AddUnit(start_Choice1);
            blackboard.AddUnit(start_Choice2);
            blackboard.AddUnit(openDoor);
            blackboard.AddUnit(waited);
            blackboard.AddUnit(waitedChoice1);
            blackboard.AddUnit(waitedChoice2);
            blackboard.AddUnit(waitedAgain);
            blackboard.AddUnit(openDoorAfterWaiting);

            blackboard.AddLink(start, start_Choice1, LinkTypes.L_Choice);
            blackboard.AddLink(start, start_Choice2, LinkTypes.L_Choice);
            blackboard.AddLink(waited, waitedChoice1, LinkTypes.L_Choice);
            blackboard.AddLink(waited, waitedChoice2, LinkTypes.L_Choice);

        }
    }
}
