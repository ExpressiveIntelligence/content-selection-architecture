using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
using CSA.Controllers;
using static CSA.KnowledgeSources.KSProps;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.Demo
{
    /* 
     * Contains static methods for initializing data on the blackboard for the demos. 
     */
    public static class ContentUnitSetupForDemos
    {
        /* 
         * Adds ContentUnits to blackboard for demo1, a simple choice-based demo.
         */
        public static void Demo1_DefineCUs(IBlackboard blackboard)
        {
            ContentUnit start = new ContentUnit();
            start.Metadata[ContentUnitID] = "start";
            start.Content[Text] = "You stand before an old wooden door.";

            ContentUnit start_Choice1 = new ContentUnit();
            start_Choice1.Metadata[TargetContentUnitID] = "open door";
            start_Choice1.Content[Text] = "Open the door";

            ContentUnit start_Choice2 = new ContentUnit();
            start_Choice2.Metadata[TargetContentUnitID] = "wait";
            start_Choice2.Content[Text] = "Wait";

            ContentUnit openDoor = new ContentUnit();
            openDoor.Metadata[ContentUnitID] = "open door";
            openDoor.Content[Text] = "Opening the door, you step forth into adventure.";

            ContentUnit waited = new ContentUnit();
            waited.Metadata[ContentUnitID] = "wait";
            waited.Content[Text] = "You wait fruitlessly in front the door.";

            ContentUnit waitedChoice1 = new ContentUnit();
            waitedChoice1.Metadata[TargetContentUnitID] = "waited again";
            waitedChoice1.Content[Text] = "Wait again";

            ContentUnit waitedChoice2 = new ContentUnit();
            waitedChoice2.Metadata[TargetContentUnitID] = "open door after waiting";
            waitedChoice2.Content[Text] = "Finally open the door";

            ContentUnit waitedAgain = new ContentUnit();
            waitedAgain.Metadata[ContentUnitID] = "waited again";
            waitedAgain.Content[Text] = "Sunk in a deep malaise, you wait the rest of your life.";

            ContentUnit openDoorAfterWaiting = new ContentUnit();
            openDoorAfterWaiting.Metadata[ContentUnitID] = "open door after waiting";
            openDoorAfterWaiting.Content[Text] = "Breaking through your reservations, you step forward into a life of adventure.";

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

        /*
         * Adds ContentUnits to blackboard for demo2, a choice-based demo with prolog applicability tests for choice setups. 
         */
        public static void Demo2_DefineCUs(IBlackboard blackboard)
        {
            ContentUnit start = new ContentUnit();
            start.Metadata[ContentUnitID] = "start";
            start.Metadata[ApplTest_Prolog] = "true.";
            start.Content[Text] = "An old gentleman stands before you. ";

            ContentUnit choice_AskForHelp = new ContentUnit();
            choice_AskForHelp.Metadata[TargetContentUnitID] = "ask for help";
            choice_AskForHelp.Content[Text] = "Ask for help";

            ContentUnit choice_introduceYourself = new ContentUnit();
            choice_introduceYourself.Metadata[TargetContentUnitID] = "introduce yourself";
            choice_introduceYourself.Metadata[FactAddList_Prolog] = new string[] { "introducedYourself" };
            choice_introduceYourself.Content[Text] = "Introduce yourself";

            ContentUnit askForHelp1 = new ContentUnit();
            askForHelp1.Metadata[ContentUnitID] = "ask for help";
            askForHelp1.Metadata[ApplTest_Prolog] = "introducedYourself.";
            askForHelp1.Content[Text] = "'Since you introduced yourself so nicely, I'm happy to help!'";

            ContentUnit askForHelp2 = new ContentUnit();
            askForHelp2.Metadata[ContentUnitID] = "ask for help";
            askForHelp2.Metadata[ApplTest_Prolog] = "\\+ introducedYourself.";
            askForHelp2.Content[Text] = "Eyeing you suspiciously the old man replies 'Who are you?'";

            ContentUnit introduceYourself = new ContentUnit();
            introduceYourself.Metadata[ContentUnitID] = "introduce yourself";
            introduceYourself.Metadata[ApplTest_Prolog] = "true.";
            introduceYourself.Content[Text] = "'Very nice to meet you!'";

            blackboard.AddUnit(start);
            blackboard.AddUnit(choice_AskForHelp);
            blackboard.AddUnit(choice_introduceYourself);
            blackboard.AddUnit(askForHelp1);
            blackboard.AddUnit(askForHelp2);
            blackboard.AddUnit(introduceYourself);

            blackboard.AddLink(start, choice_AskForHelp, LinkTypes.L_Choice);
            blackboard.AddLink(start, choice_introduceYourself, LinkTypes.L_Choice);
            blackboard.AddLink(askForHelp2, choice_AskForHelp, LinkTypes.L_Choice);
            blackboard.AddLink(askForHelp2, choice_introduceYourself, LinkTypes.L_Choice);
            blackboard.AddLink(introduceYourself, choice_AskForHelp, LinkTypes.L_Choice);

            U_PrologKB prologKB = new U_PrologKB("Global");

            /*
             * fixme: is there a better way to define a predicate for prolog than asserting and retracting it?
             */
            prologKB.Assert("introducedYourself.");
            prologKB.Retract("introducedYourself.");

            blackboard.AddUnit(prologKB);
        }
    }
}
