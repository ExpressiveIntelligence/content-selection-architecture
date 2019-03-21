using System;
using System.Diagnostics;

namespace CSA.KnowledgeSources
{
    public static class EventHandler_ConsoleChoice
    {
        /*
         * fixme: currently have two versions of DisplayConsoleChoice for use with Reactive and Scheduled console choice. They don't share a parent
         * which exposes the methods ChoicesToDisplay and SelectChoice, so can't make this a superclass. Need to refactor so they have a shared parent
         * or, more cleanly, get rid of storing choice info on the knowledge source and pass it purely in the args. 
         */

        public static void DisplayConsoleChoiceReactive(object sender, EventArgs e)
        {
            KS_ReactiveChoicePresenter cp = (KS_ReactiveChoicePresenter)sender;
            Console.WriteLine(cp.TextToDisplay);


            if (cp.ChoicesToDisplay.Length > 0)
            {
                Debug.Assert(cp.ChoicesToDisplay.Length < 10);

                for (int i = 0; i < cp.ChoicesToDisplay.Length; i++)
                {
                    Console.Write($"{i}. ");
                    Console.WriteLine(cp.ChoicesToDisplay[i]);
                }

                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey(true);
                }
                while (!char.IsDigit(keyInfo.KeyChar));

                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                int choiceMade = int.Parse(keyInfo.KeyChar.ToString());
                cp.SelectChoice(choiceMade);
            }
        }

        public static void DisplayConsoleChoiceScheduled(object sender, EventArgs e)
        {
            KS_ScheduledChoicePresenter cp = (KS_ScheduledChoicePresenter)sender;
            Console.WriteLine(cp.TextToDisplay);


            if (cp.ChoicesToDisplay.Length > 0)
            {
                Debug.Assert(cp.ChoicesToDisplay.Length < 10);

                for (int i = 0; i < cp.ChoicesToDisplay.Length; i++)
                {
                    Console.Write($"{i}. ");
                    Console.WriteLine(cp.ChoicesToDisplay[i]);
                }

                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey(true);
                }
                while (!char.IsDigit(keyInfo.KeyChar));

                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                int choiceMade = int.Parse(keyInfo.KeyChar.ToString());
                cp.SelectChoice(choiceMade);
            }

        }
    }
}
