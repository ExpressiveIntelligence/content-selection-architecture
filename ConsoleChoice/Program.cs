﻿using System;
using CSA.KnowledgeSources;
using CSA.Demo;

namespace ConsoleDemo
{
    class Program
    {
        private static void ReactiveConsoleChoice()
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

        private static void ScheduledConsoleChoice()
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

        private static void ScheduledConsoleChoice_PrologApplTest()
        {
            Console.WriteLine("Starting demo2 (choice-based with prolog applicability tests).");

            Demo2 demo = new Demo2();

            demo.AddChoicePresenterHandler(EventHandlers_ChoicePresenter.Execute_DisplayConsoleChoice);
            demo.AddSelectChoicePresenterHandler(EventHandlers_ChoicePresenter.SelectChoice_PrologKBChanges);

            while (demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
                // Console.WriteLine(demo.Blackboard.NumberOfUnits<U_PrologEvalRequest>());
            }
        }

        private static void SimpleScheduledCFGExpansion()
        {
            Console.WriteLine("Starting demo3 (simple CFG expansion).");

            Demo3 demo = new Demo3();

            while (demo.Blackboard.Changed)
            {
                demo.Blackboard.ResetChanged();
                demo.Controller.Execute();
            }

        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Call with an argument to specify which demo to run.");
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
                case "demo3":
                    SimpleScheduledCFGExpansion();
                    break;
                default:
                    throw new ArgumentException("Unrecognized argument: " + args[0]);
            }
         }
    }
}
