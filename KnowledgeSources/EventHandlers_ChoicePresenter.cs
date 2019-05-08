using System;
using System.Diagnostics;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public static class EventHandlers_ChoicePresenter
    {
        public static void Execute_DisplayConsoleChoice(object sender, PresenterExecuteEventArgs eventArgs)
        {
            IChoicePresenter cp = (IChoicePresenter)sender;
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

                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                uint choiceMade = uint.Parse(keyInfo.KeyChar.ToString());
                cp.SelectChoice(eventArgs.Choices, choiceMade);
            }
        }

        public static void SelectChoice_PrologKBChanges(object sender, SelectChoiceEventArgs eventArgs)
        {
            ContentUnit selectedChoice = eventArgs.SelectedChoice;
            U_PrologKB kb = eventArgs.Blackboard.LookupSingleton<U_PrologKB>();

            // If there are any facts to retract, retract them
            if (selectedChoice.HasMetadataSlot(FactDeleteList_Prolog))
            {
                string[] deleteList = (string[])selectedChoice.Metadata[FactDeleteList_Prolog];
                foreach (string fact in deleteList)
                {
                    kb.Retract(fact);
                }
            }

            // If there are any facts to add, add them 
            if (selectedChoice.HasMetadataSlot(FactAddList_Prolog))
            {
                string[] addList = (string[])selectedChoice.Metadata[FactAddList_Prolog];
                foreach (string fact in addList)
                {
                    kb.Assert(fact);
                }
            }
        }
    }

    /* 
     * The EventArgs class definition for passing text to display and choice information to the display callback. 
     */
    public class PresenterExecuteEventArgs : EventArgs
    {
        public string TextToDisplay { get; }
        public string[] ChoicesToDisplay { get; }
        public ContentUnit[] Choices { get; }

        public PresenterExecuteEventArgs(string textToDisplay, string[] choicesToDisplay, ContentUnit[] choices)
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
    public class SelectChoiceEventArgs : EventArgs
    {
        public ContentUnit SelectedChoice { get; }
        public IBlackboard Blackboard { get; }

        public SelectChoiceEventArgs(ContentUnit selectedChoice, IBlackboard blackboard)
        {
            SelectedChoice = selectedChoice;
            Blackboard = blackboard;
        }
    }
}
