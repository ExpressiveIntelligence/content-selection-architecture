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

            Demo1 demo = new Demo1();

            demo.AddChoicePresenterHandler(EventHandler_ConsoleChoice.DisplayConsoleChoice);

            while(demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }
        }
    }
}
