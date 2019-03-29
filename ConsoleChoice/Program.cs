using System;
using CSA.KnowledgeSources;
using CSA.Demo;
using CSA.KnowledgeUnits;

namespace ConsoleChoice
{
    class Program
    {
        static void ReactiveConsoleChoice()
        {
            Console.WriteLine("Starting reactive demo1 (simple choice-based).");

            Demo1_Reactive demo = new Demo1_Reactive();

            demo.AddChoicePresenterHandler(EventHandlers_ChoicePresenter.Execute_DisplayConsoleChoice);

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

            demo.AddChoicePresenterHandler(EventHandlers_ChoicePresenter.Execute_DisplayConsoleChoice);

            while(demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }
        }

        static void ScheduledConsoleChoice_PrologApplTest()
        {
            Console.WriteLine("Starting demo2 (choice-based with prolog applicability tests).");

            Demo2 demo = new Demo2();

            demo.AddChoicePresenterHandler(EventHandlers_ChoicePresenter.Execute_DisplayConsoleChoice);
            demo.AddSelectChoicePresenterHandler(EventHandlers_ChoicePresenter.SelectChoice_PrologKBChanges);

            while(demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
                // Console.WriteLine(demo.Blackboard.NumberOfUnits<U_PrologEvalRequest>());
            }
        }

        public static void Main(string[] args)
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
                case "demo2":
                    ScheduledConsoleChoice_PrologApplTest();
                    break;
                default:
                    throw new ArgumentException("Unrecognized argument: " + args[0]);
            }
         }
    }
}
