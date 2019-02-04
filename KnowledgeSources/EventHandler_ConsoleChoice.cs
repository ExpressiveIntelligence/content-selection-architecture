using System;
using System.Diagnostics;

namespace CSA.KnowledgeSources
{
    public static class EventHandler_ConsoleChoice
    {
        public static void DisplayConsoleChoice(object sender, EventArgs e)
        {
            KS_ChoicePresenter cp = (KS_ChoicePresenter)sender;
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
