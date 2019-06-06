using System;
using System.Diagnostics;
using System.Linq;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public static class EventHandlers_ChoicePresenter
    {
        public static void Execute_DisplayConsoleChoice(object sender, KC_PresenterExecuteEventArgs eventArgs)
        {
            I_KC_ChoicePresenter cp = (I_KC_ChoicePresenter)sender;
            Console.WriteLine(eventArgs.TextToDisplay);

            if (eventArgs.ChoicesToDisplay.Length > 0)
            {
                Debug.Assert(eventArgs.ChoicesToDisplay.Length < 10);

                for (int i = 0; i < eventArgs.ChoicesToDisplay.Length; i++)
                {
                    Console.Write($"{i}. ");
                    Console.WriteLine(eventArgs.ChoicesToDisplay[i]);
                }

                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey(true);
                }
                while (!char.IsDigit(keyInfo.KeyChar));

                // Activate the KC_IDSelectionRequest stored on the selected choice unit.  
                uint choiceMade = uint.Parse(keyInfo.KeyChar.ToString());
                cp.SelectChoice(eventArgs.Choices, choiceMade);
            }
        }

        public static void SelectChoice_PrologKBChanges(object sender, KC_SelectChoiceEventArgs eventArgs)
        {
            Unit selectedChoice = eventArgs.SelectedChoice;

            var prologKBQuery = from unit in eventArgs.Blackboard.LookupUnits<Unit>()
                                where unit.HasComponent<KC_PrologKB>()
                                select unit;

            // Currently only support one global KB. 
            Debug.Assert(prologKBQuery.Count() == 1);

            KC_PrologKB prologKB = prologKBQuery.First().GetComponent<KC_PrologKB>();

            // If there are any facts to retract, retract them
            if (selectedChoice.HasComponent<KC_PrologFactDeleteList>())
            {
                selectedChoice.GetComponent<KC_PrologFactDeleteList>().DeleteFacts(prologKB);
            }

            // If there are any facts to add, add them 
            if (selectedChoice.HasComponent<KC_PrologFactAddList>())
            {
                selectedChoice.GetComponent<KC_PrologFactAddList>().AddFacts(prologKB);
            }
        }
    }

    /* 
     * The EventArgs class definition for passing text to display and choice information to the display callback. 
     */
    public class KC_PresenterExecuteEventArgs : EventArgs
    {
        public string TextToDisplay { get; }
        public string[] ChoicesToDisplay { get; }
        public Unit[] Choices { get; }

        public KC_PresenterExecuteEventArgs(string textToDisplay, string[] choicesToDisplay, Unit[] choices)
        {
            TextToDisplay = textToDisplay;
            ChoicesToDisplay = choicesToDisplay;
            Choices = choices;
        }
    }

    /*
     * The EventArgs class definition for performing any processing on the selected choice. This can be used, for example, to process the FactAddList
     * and FactDeleteList slots on the selected choice.         
     */
    public class KC_SelectChoiceEventArgs : EventArgs
    {
        public Unit SelectedChoice { get; }
        public IBlackboard Blackboard { get; }

        public KC_SelectChoiceEventArgs(Unit selectedChoice, IBlackboard blackboard)
        {
            SelectedChoice = selectedChoice;
            Blackboard = blackboard;
        }
    }
}
