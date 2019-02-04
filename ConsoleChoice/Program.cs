using System;
using CSA.Core;
using CSA.KnowledgeSources;
using CSA.Controllers;
using CSA.KnowledgeUnits;

namespace ConsoleChoice
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting ConsoleChoice");

            IBlackboard blackboard = new Blackboard();

            // Set up the ContentUnits on the blackboard
            DefineCUs(blackboard);

            // Set up the knowledge sources
            IKnowledgeSource iDSelector = new KS_IDSelector(blackboard);

            // fixme: Need to come up with interfaces for presenters, but won't know what the general presenter framework looks like until I've written more of them. 
            KS_ChoicePresenter choicePresenter = new KS_ChoicePresenter(blackboard);
            iDSelector.Properties[KS_PropertyNames.Priority] = 20;
            choicePresenter.Properties[KS_PropertyNames.Priority] = 10;
            choicePresenter.PresenterExecute += EventHandler_ConsoleChoice.DisplayConsoleChoice; 
            
            // Set up the controller
            IController priControl = new PriorityController();
            priControl.AddKnowledgeSource(iDSelector);
            priControl.AddKnowledgeSource(choicePresenter);

            // Put request for starting content unit in blackboard
            blackboard.AddUnit(new U_IDQuery("start"));

            // fixme: Currently the top-level loop for blackboard execution is in the calling application. Probably want to move this into the controller or blackboard and make event driven. 
            while(blackboard.Changed)
            {
                blackboard.ResetChanged();
                priControl.Execute();
            }

        }

        private static void DefineCUs(IBlackboard blackboard)
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
