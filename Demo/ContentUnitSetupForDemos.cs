using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;
using static CSA.KnowledgeUnits.KUProps;

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

        /*
         * Adds ContentUnits to blackboard for demo3, a simple context-free-grammar demo, no rule specificities,
         * no pushing or poping.         
         */

        public static IUnit Demo3_DefineCUs(IBlackboard blackboard, string grammarPool)
        {
            // Start symbol
            Unit startSymbol = new Unit();
            startSymbol.Properties[GrammarNonTerminal] = "Start";
            startSymbol.Properties[WithinTreeLevelCount] = 1;
            startSymbol.Properties[IsTreeNode] = true;
            startSymbol.Properties[IsLeafNode] = true;

            /*
             * Grammar rules
             */

            // Start rule
            ContentUnit startRule = new ContentUnit();
            startRule.Metadata[ContentUnitID] = "Start";
            startRule.Metadata[ContentPool] = grammarPool;

            // Define the RHS of start rule
            Unit[] startRHS = { new Unit(), new Unit() };
            // fixme: replace this with a template in the rule, so only non-terminals are in the tree 
            startRHS[0].Properties[GrammarTerminal] = "Hello"; 

            startRHS[1].Properties[GrammarNonTerminal] = "Place";

            startRule.Metadata[GrammarRuleRHS] = startRHS;

            // Place rule
            ContentUnit placeRule1 = new ContentUnit();
            placeRule1.Metadata[ContentUnitID] = "Place";
            placeRule1.Metadata[ContentPool] = grammarPool;

            // Define RHS for placeRule1
            Unit[] placeRule1RHS = { new Unit() };
            // fixme: replace this with a template in the rule, so only non-terminals are in the tree 
            placeRule1RHS[0].Properties[GrammarTerminal] = "world";

            placeRule1.Metadata[GrammarRuleRHS] = placeRule1RHS;

            ContentUnit placeRule2 = new ContentUnit();
            placeRule2.Metadata[ContentUnitID] = "Place";
            placeRule2.Metadata[ContentPool] = grammarPool;

            // Define RHS for placeRule2
            Unit[] placeRule2RHS = { new Unit() };
            // fixme: replace this with a template in the rule, so only non-terminals are in the tree 
            placeRule2RHS[0].Properties[GrammarTerminal] = "universe";

            placeRule2.Metadata[GrammarRuleRHS] = placeRule2RHS;

            _ = blackboard.AddUnit(startSymbol);
            AddGrammarRule(blackboard, startRule);
            AddGrammarRule(blackboard, placeRule1);
            AddGrammarRule(blackboard, placeRule2);

            return startSymbol;
        }

        private static void AddGrammarRule(IBlackboard blackboard, ContentUnit rule)
        {
            blackboard.AddUnit(rule);
            /* foreach (Unit unit in (Unit[])rule.Metadata[GrammarRuleRHS])
            {
                _ = blackboard.AddUnit(unit);
            } */
        }
    }
}
