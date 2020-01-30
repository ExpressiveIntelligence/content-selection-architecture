using System;
using System.Diagnostics;
using System.Linq;
using CSA.KnowledgeSources;
using CSA.Demo;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace ConsoleDemo
{
    class Program
    {
        [Obsolete("Reactive KSs are obsolete so ReactiveConsoleChoice has not been updated to use KnowledgeComponents.")]
        private static void ReactiveConsoleChoice()
        {
            Console.WriteLine("Starting reactive demo1 (simple choice-based).");

            Demo1_Reactive demo = new Demo1_Reactive();

            demo.AddChoicePresenterHandler(EventHandlers_Old_ChoicePresenter.Execute_DisplayConsoleChoice);

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
            }
        }

        private static void SimpleScheduledCFGExpansion(string[] args)
        {
            Console.WriteLine("Starting demo3 (simple CFG expansion).");

            int numberOfExpansions = Int32.Parse(args[1]);

            for (int i = 0; i < numberOfExpansions; i++)
            {
                Demo3 demo = new Demo3();
                uint unitCount1 = demo.Blackboard.NumberOfUnits();
                Console.WriteLine("Number of units before execution = " + unitCount1);

                while (demo.Blackboard.Changed)
                {
                    demo.Blackboard.ResetChanged();
                    demo.GenerateTree.Execute();
                }

                /*
                 * For now not placing these in a separate controller or worrying about how to generalize if-then logic in controllers. 
                 * May just want to keep it this way, where raw c# is used for more complicated controller logic. 
                 * fixme: This would be easy to specify in a priority-based scheme: LinearizeTreeLeaves, CleanTree and the logic to print the linearized leaves would all 
                 * be lower priority than the knowledge sources that build the tree. So they would only fire when the tree is complete. 
                 * Even without waiting for reactive KSs, this could be done with scheduled KSs that are sorted by priority with a loop that goes through each of the 
                 * KSs executing the first one whose precondition is satisfied then starting over again from the highest priority. 
                 */
                demo.LinearizeTreeLeaves.Execute();
                demo.CleanTree.Execute();

                var sequenceQuery = from unit in demo.Blackboard.LookupUnits<Unit>()
                                    where unit.HasComponent<KC_Sequence>()
                                    select unit;

                Debug.Assert(sequenceQuery.Count() == 1);
                Unit sequence = sequenceQuery.First();
                foreach(Unit unit in sequence.GetSequence())
                {
                    Console.Write(unit.GetText() + " ");
                }
                Console.WriteLine();

                uint unitCount2 = demo.Blackboard.NumberOfUnits();
                
                Console.WriteLine("Number of units after execution = " + unitCount2);
                Console.WriteLine();
            }
        }

        private static void SimpleEnsemble()
        {
            Console.WriteLine("Starting demoBeth (simple ensemble).");

            DemoEnsembleLite demo = new DemoEnsembleLite();
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
#pragma warning disable CS0618 // Type or member is obsolete 
                    ReactiveConsoleChoice();
#pragma warning restore CS0618 // Type or member is obsolete
                    break;
                case "scheduled_demo1":
                    ScheduledConsoleChoice();
                    break;
                case "demo2":
                    ScheduledConsoleChoice_PrologApplTest();
                    break;
                case "demo3":
                    SimpleScheduledCFGExpansion(args);
                    break;
                case "demoBeth":
                    SimpleEnsemble();
                    break;
                default:
                    throw new ArgumentException("Unrecognized argument: " + args[0]);
            }
         }
    }
}
