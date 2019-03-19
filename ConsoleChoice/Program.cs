using System;
using CSA.KnowledgeSources;
using CSA.Demo;

namespace ConsoleChoice
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting ConsoleChoice");

            Demo1_Reactive demo = new Demo1_Reactive();

            demo.AddChoicePresenterHandler(EventHandler_ConsoleChoice.DisplayConsoleChoice);

            while(demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }
        }
    }
}
