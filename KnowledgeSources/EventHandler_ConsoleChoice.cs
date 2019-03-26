using System;
using System.Diagnostics;
using CSA.KnowledgeSources;

namespace CSA.KnowledgeSources
{
    public static class EventHandler_ConsoleChoice
    {
        public static void DisplayConsoleChoice(object sender, KS_ScheduledChoicePresenter.PresenterExecuteEventArgs eventArgs)
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
    }
}
