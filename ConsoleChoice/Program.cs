using System;
using CSA.KnowledgeSources;
using CSA.Demo;

namespace ConsoleChoice
{
    class Program
    {
        static void ReactiveConsoleChoice()
        {
            Console.WriteLine("Starting reactive demo1 (simple choice-based).");

            Demo1_Reactive demo = new Demo1_Reactive();

            demo.AddChoicePresenterHandler(EventHandler_ConsoleChoice.DisplayConsoleChoice);

            while (demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }
        }

        static void ScheduledConsoleChoice()
        {
            Console.WriteLine("Starting scheduled demo1 (simple choice-based).");

            Demo1_Scheduled demo = new Demo1_Scheduled();

            demo.AddChoicePresenterHandler(EventHandler_ConsoleChoice.DisplayConsoleChoice);

            while(demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }
        }

        static void ScheduledConsoleChoice_PrologApplTest()
        {
            Console.WriteLine("Starting demo2 (choice-based with prolog applicability tests).");

                
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Call with argument 'reactive' or 'scheduled'");
            }

            switch (args[0])
            {
                case "reactive_demo1":
                    ReactiveConsoleChoice();
                    break;
                case "scheduled_demo1":
                    ScheduledConsoleChoice();
                    break;
                default:
                    throw new ArgumentException("Unrecognized argument: " + args[0]);
            }
         }
    }
}
