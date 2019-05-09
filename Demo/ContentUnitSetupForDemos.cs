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

        public static Unit Demo3_1_DefineUnits(IBlackboard blackboard, string grammarPool)
        {
            // Start symbol
            Unit startSymbol = new Unit();
            startSymbol.AddComponent(new KC_IDSelectionRequest("Start", true));
            startSymbol.AddComponent(new KC_TreeNode(null));
            startSymbol.AddComponent(new KC_ContentPool(grammarPool, true));
            startSymbol.AddComponent(new KC_Order(0, true));

            // OrderCounter
            Unit orderCounter = new Unit();
            orderCounter.AddComponent(new KC_OrderCounter(0));

            /*
             * Grammar rules
             */

            // Start rule
            Unit startRule = new Unit();
            startRule.AddComponent(new KC_UnitID("Start", true));
            startRule.AddComponent(new KC_ContentPool(grammarPool, true));

            // Define the RHS of start rule
            Unit[] startRHS = { new Unit(), new Unit() };
            startRHS[0].AddComponent(new KC_Text("Hello", true));
            startRHS[1].AddComponent(new KC_IDSelectionRequest("Place", true));

            startRule.AddComponent(new KC_Decomposition(startRHS, true));

            // Place rule
            Unit placeRule1 = new Unit();
            placeRule1.AddComponent(new KC_UnitID("Place", true));
            placeRule1.AddComponent(new KC_ContentPool(grammarPool, true));

            // Define RHS for placeRule1
            Unit[] placeRule1RHS = { new Unit() };
            placeRule1RHS[0].AddComponent(new KC_Text("world", true));

            placeRule1.AddComponent(new KC_Decomposition(placeRule1RHS, true));

            Unit placeRule2 = new Unit();
            placeRule2.AddComponent(new KC_UnitID("Place", true));
            placeRule2.AddComponent(new KC_ContentPool(grammarPool, true));

            // Define RHS for placeRule2
            Unit[] placeRule2RHS = { new Unit() };
            placeRule2RHS[0].AddComponent(new KC_Text("universe", true));

            placeRule2.AddComponent(new KC_Decomposition(placeRule2RHS, true));

            blackboard.AddUnit(startSymbol);
            blackboard.AddUnit(orderCounter);
            blackboard.AddUnit(startRule);
            blackboard.AddUnit(placeRule1);
            blackboard.AddUnit(placeRule2);

            return startSymbol;
        }

        /*
         * Adds ContentUnits to blackboard for demo3, a simple context-free-grammar demo, no rule specificities,
         * no pushing or poping. Uses this grammar from the 12B assignment. 
         * animal:cat,emu,okapi
         * emotion:happy,sad,elated,curious,sleepy
         * color:red,green,blue
         * name:emily,luis,otavio,anna,charlie
         * character:#name# the #adjective# #animal#
         * place:school,the beach,the zoo,Burning Man
         * adjective:#color#,#emotion#,
         * origin:once #character# and #character# went to #place#
         */
        public static Unit Demo3_2_DefineUnits(IBlackboard blackboard, string grammarPool)
        {
            // Start symbol
            Unit startSymbol = new Unit();
            startSymbol.AddComponent(new KC_IDSelectionRequest("Origin", true));
            startSymbol.AddComponent(new KC_TreeNode(null));
            startSymbol.AddComponent(new KC_ContentPool(grammarPool, true));
            startSymbol.AddComponent(new KC_Order(0, true));

            // OrderCounter
            Unit orderCounter = new Unit();
            orderCounter.AddComponent(new KC_OrderCounter(0));

            /*
             * Grammar rules
             */

            /* Origin rule
             * Origin --> Once #Character# and #Character# went to #Place#
             */
            Unit startRule = MakeRuleUnit("Origin", grammarPool, blackboard);

            // Define the RHS of Origin rule
            Unit[] startRHS = MakeUnitArray(6);
            startRHS[0].AddComponent(new KC_Text("Once", true));
            startRHS[1].AddComponent(new KC_IDSelectionRequest("Character", true));
            startRHS[2].AddComponent(new KC_Text("and", true));
            startRHS[3].AddComponent(new KC_IDSelectionRequest("Character", true));
            startRHS[4].AddComponent(new KC_Text("went to", true));
            startRHS[5].AddComponent(new KC_IDSelectionRequest("Place", true));
            startRule.AddComponent(new KC_Decomposition(startRHS, true));

            /*
             * Adjective rules
             * Adjective --> #Color#
             * Adjective --> #Emotion#
             */
            Unit adjectiveRule1 = MakeRuleUnit("Adjective", grammarPool, blackboard);

            // Define RHS of Adjective1 rule
            Unit[] adjectiveRule1RHS = MakeUnitArray(1);
            adjectiveRule1RHS[0].AddComponent(new KC_IDSelectionRequest("Color", true));
            adjectiveRule1.AddComponent(new KC_Decomposition(adjectiveRule1RHS, true));

            Unit adjectiveRule2 = MakeRuleUnit("Adjective", grammarPool, blackboard);

            // Define RHS of Adjective2 rule
            Unit[] adjectiveRule2RHS = MakeUnitArray(1);
            adjectiveRule2RHS[0].AddComponent(new KC_IDSelectionRequest("Emotion", true));
            adjectiveRule2.AddComponent(new KC_Decomposition(adjectiveRule2RHS, true));

            /*
             * Place rules 
             * Place --> school
             * Place --> the beach
             * Place --> the zoo
             * Place --> Burning Man
             */
            string[] terminals1 = { "school", "the beach", "the zoo", "Burning Man" };
            MakeTerminalSingletons("Place", grammarPool, terminals1, blackboard);

            /*
             * Character rule
             * Character --> #name# the #adjective# #animal#
             */
            Unit characterRule = MakeRuleUnit("Character", grammarPool, blackboard);

            // Define RHS of Character rule
            Unit[] characterRuleRHS = MakeUnitArray(4);
            characterRuleRHS[0].AddComponent(new KC_IDSelectionRequest("Name", true));
            characterRuleRHS[1].AddComponent(new KC_Text("the", true));
            characterRuleRHS[2].AddComponent(new KC_IDSelectionRequest("Adjective", true));
            characterRuleRHS[3].AddComponent(new KC_IDSelectionRequest("Animal", true));
            characterRule.AddComponent(new KC_Decomposition(characterRuleRHS, true));

            /*
             * Name rules
             * Name --> emily 
             * Name --> luis
             * Name --> otavio
             * Name --> anna
             * Name --> charlie            
             */
            string[] terminals2 = { "emily", "luis", "otavio", "anna", "charlie" };
            MakeTerminalSingletons("Name", grammarPool, terminals2, blackboard);

            /*
             * Color rules
             * Color --> red
             * Color --> green
             * Color --> blue           
             */
            string[] terminals3 = { "red", "green", "blue" };
            MakeTerminalSingletons("Color", grammarPool, terminals3, blackboard);

            /*
             * Emotion rules
             * Emotion --> happy
             * Emotion --> sad
             * Emotion --> curious
             * Emotion --> sleepy          
             */
            string[] terminals4 = { "happy", "sad", "curious", "sleepy" };
            MakeTerminalSingletons("Emotion", grammarPool, terminals4, blackboard);

            /*
             * Animal rules
             * Animal --> cat
             * Animal --> emu
             * Animal --> okapi                       
             */
            string[] terminals5 = { "cat", "emu", "okapi" };
            MakeTerminalSingletons("Animal", grammarPool, terminals5, blackboard);

            blackboard.AddUnit(startSymbol);
            blackboard.AddUnit(orderCounter);
            return startSymbol;
        }


        private static Unit MakeRuleUnit(string ruleID, string pool, IBlackboard blackboard)
        {
            Unit u = new Unit();
            u.AddComponent(new KC_UnitID(ruleID, true));
            u.AddComponent(new KC_ContentPool(pool, true));
            blackboard.AddUnit(u);
            return u;
        }

        private static void MakeTerminalSingletons(string ruleID, string pool, string[] terminals, IBlackboard blackboard)
        {
            foreach(string terminal in terminals)
            {
                Unit rule = MakeRuleUnit(ruleID, pool, blackboard);
                Unit[] ruleRHS = MakeUnitArray(1);
                ruleRHS[0].AddComponent(new KC_Text(terminal, true));
                rule.AddComponent(new KC_Decomposition(ruleRHS, true));
            }
        }

        private static Unit[] MakeUnitArray(uint len)
        {
            Unit[] units = new Unit[len];
            for(int i =0; i < len; i++)
            {
                units[i] = new Unit();
            }
            return units;
        }
    }
}
