using System;
using CSA.KnowledgeSources;
using CSA.Demo;

namespace ConsoleChoice
{
    class Program
    {
        static void ReactiveConsoleChoice()
        {
            Console.WriteLine("Starting ReactiveConsoleChoice");

            Demo1_Reactive demo = new Demo1_Reactive();

            demo.AddChoicePresenterHandler(EventHandler_ConsoleChoice.DisplayConsoleChoiceReactive);

            while (demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }
        }

        static void ScheduledConsoleChoice()
        {
            Console.WriteLine("Starting ScheduledConsoleChoice");

            Demo1_Scheduled demo = new Demo1_Scheduled();

            demo.AddChoicePresenterHandler(EventHandler_ConsoleChoice.DisplayConsoleChoiceScheduled);

            while(demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Call with argument 'reactive' or 'scheduled'");
            }

            switch (args[0])
            {
                case "reactive":
                    ReactiveConsoleChoice();
                    break;
                case "scheduled":
                    ScheduledConsoleChoice();
                    break;
                default:
                    throw new ArgumentException("Call with argument 'reactive' or 'scheduled'");
            }
         }
    }
}
