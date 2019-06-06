using System;
using System.Diagnostics;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using Prolog;
using CSA.Core;
using CSA.KnowledgeUnits;
using CSA.KnowledgeSources;
#pragma warning disable CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.CUSlots;
#pragma warning restore CS0618 // Type or member is obsolete

/* 
 * A test rig for testing integration of UnityProlog (https://github.com/ianhorswill/UnityProlog) with CSA.
 * */
namespace TestScratchpad
{
    static class Program
    {
        public static void Main(string[] args)
        {
            //CreatePrologKBAFromFileAndQuery();
            //AssertingAFact();
            // TestExpandoObjectWithInterface();
            // TestVariableBinding();
            // QueryWithNoArgument();
            //KnowledgeBase kb = new KnowledgeBase("global", null);
            //PrintTypeVariable<KnowledgeBase>(kb);
            // TestIncrement();   
            // TestBoolConstant();
            // TestContainingUnit();
            // ScheduledChoicePresenterTestHarness();
            ScheduledPrologEvalTestHarness();
        }

        // Testing whether KnowledgeComponent.ContainingUnit is refering to the correct ContiningUnit after a copy
        private static void TestContainingUnit()
        {
            Unit unit1 = new Unit();
            unit1.AddComponent(new KC_UnitID("foo", true));

            System.Diagnostics.Debug.Assert(unit1.GetComponent<KC_UnitID>().ContainingUnit == unit1);

            Unit unit2 = new Unit(unit1);

            System.Diagnostics.Debug.Assert(unit2.GetComponent<KC_UnitID>().ContainingUnit == unit2);
            System.Diagnostics.Debug.Assert(unit2.GetReadOnly<KC_UnitID>());
            System.Diagnostics.Debug.Assert(unit2.UnitIDEquals("foo"));
        }

        //Testing whether true and false are handled as prolog queries
        private static void TestBoolConstant()
        {
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTest.prolog");
            kb.IsTrueWrite("true.");
            kb.IsTrueWrite("false.");
        }

        /*
         * fixme: Queries which involve chaining bindings generate an error. It looks like in the guts of SolveFor() there 
         * are recursive calls which call GameObject.SolveFor(). So using this basic feature of prolog requires 
         * the UnityProlog KB to be embeded in a game object. To debug this I should just develop a TextChoices unity 
         * demo with the KB stored on a GameObject.        
         */
        private static void TestIncrement()
        {
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTestWithError.prolog");
            kb.IsTrueWrite("triesGreaterThan(1).");
            //kb.IsTrueWrite("tries(X), X > 1.");
            //kb.IsTrueWrite("assert(triedDoor).");
            //kb.IsTrueWrite("incrementTries.");
            //kb.IsTrueWrite("tries(X), X > 1.");
            //kb.IsTrueWrite("assert(triedDoor).");
            //kb.IsTrueWrite("tries(x), X > 1.");
        }


        // Not related to test prolog. Scratch test of getting the name of the type referenced by a type variable. 
        private static void PrintTypeVariable<T>(T t)
        {
            Console.WriteLine(typeof(T));
            Console.WriteLine(t.GetType().FullName);

        }
        private static void CreatePrologKBAFromFileAndQuery()
        {
            // fixme: KnowledgeBase wants a reference to a game object. This is because GameObjects can form a tree of knowledge bases. 
            // But I don't need this. Ideally we'd refactor prolog so that there is a lower abstract level that makes no reference to Unity classes.
            // Alternatively, a KB could be created on the Unity side and passed into the CSA. 
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTest.prolog");
            var query = ISOPrologReader.Read("mortal(socratese).");
            if (kb.IsTrue(query))
            {
                Console.WriteLine("Socratese is mortal.");
            }
            else
            {
                Console.WriteLine("Something is wrong with logic!");
            }

        }

        /* 
         * Test that SolveFor() can be used to bind variables.
         * Findings:
         * If the variable is bound to a simple value it's a Symbol.
         * If the variable is bound to a composite value (like a functor) it's a Structure.
         * If the query doesn't succeed the returned variable is null.
         * If the query succeeds but the variable isn't bound in the query, the returned variable is a gensym.
         * If you need to return multiple values with a query, you can create a predicate on the prolog side which binds multiple values in a functor and unify that with the return variable. 
         */
        private static void TestVariableBinding()
        {
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTest.prolog");
            var x = kb.SolveForParsed("X:test(X).");
            Console.WriteLine("The value of x is " + x);
            Console.WriteLine("The type of x is " + x.GetType());
            //Console.WriteLine("The type of x is " + ((Structure)x).Argument(1));
        }

        /*
         * Demonstrates assertions, and that queries are correctly solved after appropriate assertion of facts and rules.
         */
        private static void AssertingAFact()
        {
            KnowledgeBase kb = new KnowledgeBase("global", null);
            kb.IsTrueWrite("assert(person(plato)).");
            kb.IsTrueWrite("person(socratese).");
            kb.IsTrueWrite("assert((mortal(X) :- person(X))).");
            kb.IsTrueWrite("mortal(socratese).");
            kb.IsTrueWrite("assert(person(socratese)).");
            kb.IsTrueWrite("person(socratese).");
            kb.IsTrueWrite("mortal(socratese).");
            kb.IsTrueWrite("mortal(plato), person(socratese).");
        }

        /*
         * Testing queries that have no arguments. 
         */
        private static void QueryWithNoArgument()
        {
            KnowledgeBase kb = new KnowledgeBase("global", null);
            kb.IsTrueWrite("assert((test :- foo)).");
            kb.IsTrueWrite("assert(foo).");
            kb.IsTrueWrite("retract(foo).");
            kb.IsTrueWrite("test.");
        }

        /*
         * Experiments with the following configuration:
         * Global Prolog KB stored on a knowledge unit.
         * Prolog query applicability tests associated with content units.
         */
        private static void AddPrologQueriesToContentUnits()
        {
            IBlackboard blackboard = new Blackboard();
#pragma warning disable CS0618 // Type or member is obsolete
            ContentUnit cu = new ContentUnit();
#pragma warning restore CS0618 // Type or member is obsolete

            // cu.Metadata[] = 
        }

        private static void IsTrueWrite(this KnowledgeBase kb, string query)
        {
            if (kb.IsTrueParsed(query))
            {
                Console.WriteLine($"{query} is true");

            }
            else
            {
                Console.WriteLine($"{query} is false");
            }
        }

        public static bool IsTrueParsed(this KnowledgeBase kb, string query)
        {
            return kb.IsTrue(ISOPrologReader.Read(query));
        }

        public static object SolveForParsed(this KnowledgeBase kb, string variableAndConstraint)
        {
            if (!(ISOPrologReader.Read(variableAndConstraint) is Structure colonExpression) || !colonExpression.IsFunctor(Symbol.Colon, 2))
                throw new ArgumentException("Argument to SolveFor(string) must be of the form Var:Goal.");
            return kb.SolveFor((LogicVariable)colonExpression.Argument(0), colonExpression.Argument(1), null, false);

        }

        /*
         * Unit test code for KC_ScheduledPrologEval so that I can use the debugger
         */
        #region ScheduledPrologEval Unit Tests

        private static void TestFilterLinks(ISet<(IUnit, string, LinkDirection)> links, string outputPool)
        {
            int count = links.Count;
            Debug.Assert(1 == count);
            (IUnit unit, string linkType, LinkDirection dir) = links.First();
            Debug.Assert(LinkTypes.L_SelectedUnit.Equals(linkType));
            Debug.Assert(LinkDirection.End == dir);
            Unit unitCast = unit as Unit; // fixme: only needing to cast because I'm being inconsistent with whether Unit or IUnit is what I'm targeting. 
            Debug.Assert(unitCast.HasComponent<KC_ContentPool>());
            Debug.Assert(unitCast.ContentPoolEquals(outputPool));
        }

        private static void TestNumberOfUnitsInOutputPool(int desiredNumberOfUnits, IBlackboard blackboard, string outputPool)
        {
            var units = from unit in blackboard.LookupUnits<Unit>()
                        where unit.HasComponent<KC_ContentPool>() && unit.ContentPoolEquals(outputPool)
                        select unit;

            Debug.Assert(desiredNumberOfUnits == units.Count());
        }

        public static IEnumerable<object[]> Data_TestExecute_ScheduledPrologEval()
        {
            string inputPool = "inputPool1";
            string outputPool = "outputPool1";

            IBlackboard blackboard = new Blackboard();

            Unit unit1 = new Unit();
            Unit unit2 = new Unit();
            Unit unit3 = new Unit();
            Unit unit4 = new Unit();

            unit1.AddComponent(new KC_ContentPool(inputPool, true));
            unit2.AddComponent(new KC_ContentPool(inputPool, true));
            // unit3 and unit4 are in the global content pool (no content pool specified) 

            unit1.AddComponent(new KC_UnitID("ID1", true));
            unit2.AddComponent(new KC_UnitID("ID2", true));
            unit3.AddComponent(new KC_UnitID("ID3", true));
            unit4.AddComponent(new KC_UnitID("ID4", true));

            // Prolog applicability tests
            unit1.AddComponent(new KC_PrologExpression(ApplTest_Prolog, "frustrated.", true));
            unit2.AddComponent(new KC_PrologExpression(ApplTest_Prolog, "Character:frustrated(Character).", true));
            unit3.AddComponent(new KC_PrologExpression(ApplTest_Prolog, "frustrated.", true));
            // unit4 doesn't have a prolog expression defined, so it shouldn't be filtered by KS_ScheduledPrologEval

            Unit prologKB = new Unit();
            prologKB.AddComponent(new KC_PrologKB("Global", true));

            /*
             * For some reason the unit testing infrastructure is making a folder way down in the bin directory the current folder. So using a relative 
             * file to the source folder for CATests.
             */
            prologKB.GetComponent<KC_PrologKB>().Consult("../../../UnitTestProlog.prolog");

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KS_ScheduledPrologEval[]: the ScheduledPrologEval to test            
             * Unit[]: array of CUs to add to the blackboard
             * Unit: the prolog KB to add to the blackboard
             * Unit[]: Content units on which the prolog applicability test was evaluated.
             * string[]: Array of assertions to make in prolog KB.            
             * (Unit, bool)[]: Array of evaluation results
             * (Unit, object)[]: Array of bindings   
             */

            return new List<object[]>
            {
                // No specific input pool (global), no queries with bindings, assertion that makes queries true. 
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool, ApplTest_Prolog), new Unit[] { unit1, unit3, unit4 },
                    prologKB, new Unit[] { unit1, unit3 }, new string[] { "dissed(character1, me)." },
                    new (Unit, bool)[] { (unit1, true), (unit3, true) },
                    new (Unit, object)[0] },

                // No specific input pool (global), no queries with bindings, no assertion to make queries true.  
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool, ApplTest_Prolog), new Unit[] { unit1, unit3, unit4 },
                    prologKB, new Unit[] { unit1, unit3 }, new string[0],
                    new (Unit, bool)[] { (unit1, false), (unit3, false) },
                    new (Unit, object)[0] },

                // No specific input pool (global), query with binding, assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool, ApplTest_Prolog), new Unit[] { unit2 },
                    prologKB, new Unit[] { unit2 }, new string[] { "dissed(character1, me)." },
                    new (Unit, bool)[] { (unit2, true) },
                    new (Unit, object)[] { (unit2, Symbol.Intern("character1")) } },

                // No specific input pool (global), query with binding, no assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool, ApplTest_Prolog), new Unit[] { unit2 },
                    prologKB, new Unit[] { unit2 }, new string[0],
                    new (Unit, bool)[] { (unit2, false) },
                    new (Unit, object)[0] }, 

                // No specific input pool (global), some queries with bindings, some without, assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool, ApplTest_Prolog), new Unit[] { unit1, unit2, unit3, unit4 },
                    prologKB, new Unit[] { unit1, unit2, unit3 }, new string[] { "dissed(character1, me)." },
                    new (Unit, bool)[] { (unit1, true), (unit2, true), (unit3, true) },
                    new (Unit, object)[] { (unit1, new LogicVariable("V1")), (unit2, Symbol.Intern("character1")), (unit3, new LogicVariable("V2")) } },

                // No content units on blackboard
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool, ApplTest_Prolog), new Unit[0],
                    prologKB, new Unit[0], new string[] { "dissed(character1, me)." },
                    new (Unit, bool)[0],
                    new (Unit, object)[0] },

                // Specifying input pool, some queries with bindings, some without, assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, inputPool, outputPool, ApplTest_Prolog), new Unit[] { unit1, unit2, unit3, unit4 },
                    prologKB, new Unit[] { unit1, unit2 }, new string[] { "dissed(character1, me)." },
                    new (Unit, bool)[] { (unit1, true), (unit2, true) },
                    new (Unit, object)[] { (unit1, new LogicVariable("V1")), (unit2, Symbol.Intern("character1")) } },

            };
        }

        public static void ScheduledPrologEvalTestHarness()
        {
            var tests = Data_TestExecute_ScheduledPrologEval();
            foreach (var test in tests)
            {
                IBlackboard blackboard = (IBlackboard)test[0];
                KS_ScheduledPrologEval pe = (KS_ScheduledPrologEval)test[1];
                Unit[] unitsToAdd = (Unit[])test[2];
                Unit prologKB = (Unit)test[3];
                Unit[] evaledUnits = (Unit[])test[4];
                string[] assertions = (string[])test[5];
                (Unit, bool)[] evalResults = ((Unit, bool)[])test[6];
                (Unit, object)[] bindings = ((Unit, object)[])test[7];
                TestExecute_ScheduledPrologEval(
                    blackboard,
                    pe,
                    unitsToAdd,
                    prologKB,
                    evaledUnits,
                    assertions,
                    evalResults,
                    bindings);
            }
        }

        public static void TestExecute_ScheduledPrologEval(IBlackboard blackboard, KS_ScheduledPrologEval prologEval, Unit[] unitsToAdd,
            Unit prologKB, Unit[] evaluatedUnits, string[] assertionsToAdd, (Unit u, bool result)[] evalResults,
            (Unit u, object binding)[] bindings)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the units to the blackboard
            foreach (var unit in unitsToAdd)
            {
                blackboard.AddUnit(unit);
            }

            blackboard.AddUnit(prologKB);

            if (assertionsToAdd.Length > 0)
            {
                foreach (string assertion in assertionsToAdd)
                {
                     prologKB.GetComponent<KC_PrologKB>().Assert(assertion);
                }
            }

            // Execute the filter selector
            prologEval.Execute();

            // Remove any assertions that were added for this test case
            if (assertionsToAdd.Length > 0)
            {
                foreach (string assertion in assertionsToAdd)
                {
                    prologKB.GetComponent<KC_PrologKB>().Retract(assertion);
                }
            }

            string outputPool = prologEval.OutputPool;

            // Iterate through each of the units which should have been evaluated and see if there's a copy of them in the output pool.
            // Also test that the evaluation results and bindings are correct for each evaluated unit.
            foreach (var unit in evaluatedUnits)
            {

                /*
                 * Lookup links from this unit and verify that:
                 * 1) there is only one link, 
                 * 2) the one link is of type L_SelectedUnit
                 * 3) the direction is LinkDirection.End (the linked unit is the end of the link)
                 * 4) the linked unit is in the correct output pool               
                 */
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(unit);
                TestFilterLinks(s, outputPool);

                // Get the result unit and cast as a Unit. 
                (IUnit resultIUnit, _, _) = s.First();
                Unit resultUnit = resultIUnit as Unit;

                if (evalResults.Length > 0)
                {
                    (Unit u, bool result) =
                        Array.Find(evalResults, resultTuple => resultTuple.u == unit);
                    Debug.Assert(u != null);
                    Debug.Assert(result == resultUnit.GetPrologExpEvalResult());
                }

                if (bindings.Length > 0)
                {
                    (Unit u, object binding) =
                        Array.Find(bindings, bindingTuple => bindingTuple.u == unit);
                    Debug.Assert(u != null);

                    IEnumerable<bool> checkBinding = Term.Unify(resultUnit.GetPrologExpBindings(), binding);
                    Debug.Assert(checkBinding.GetEnumerator().MoveNext(),
                        $"Variable binding not correct: {resultUnit.GetPrologExpBindings()} != {binding}");
                }

                // Verify that each evaluated unit is linked to a unit containing a matching, evaluated KC_EvaluatablePrologExpression.
                KC_EvaluatablePrologExpression evaledPrologExp = resultUnit.GetComponent<KC_EvaluatablePrologExpression>();
                Debug.Assert(unit.GetPrologExpName<KC_PrologExpression>().Equals(evaledPrologExp.PrologExpName));
                Debug.Assert(evaledPrologExp.Evaluated);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as evaluatedUnits
            TestNumberOfUnitsInOutputPool(evaluatedUnits.Length, blackboard, outputPool);
        }

        #endregion


        /*
         * Unit test code for KS_ScheduledChoicePresenter so that I can use the debugger. 
         */
        #region ScheduledChoicePresenter Unit Tests

        private static EventHandler<PresenterExecuteEventArgs>
            GenerateEventHandler(Unit selectedUnit, Unit[] choices, IBlackboard blackboard)
        {
            return (object sender, PresenterExecuteEventArgs eventArgs) =>
            {
                if (selectedUnit != null)
                {
                    Debug.Assert(selectedUnit.TextEquals(eventArgs.TextToDisplay));
                    int numOfChoices = choices.Length;
                    Debug.Assert(numOfChoices == eventArgs.Choices.Length);

                    foreach (Unit choice in choices)
                    {
                        Debug.Assert(Array.Exists(eventArgs.ChoicesToDisplay, element => element.Equals(choice.GetText())));
                    }
                }
                else
                {
                    Debug.Assert(eventArgs.TextToDisplay.Equals(""));
                }

                // Iterate through each of the choices selecting it and confirming that the KC_IDSelectionRequest is activated. 
                IChoicePresenter cp = (IChoicePresenter)sender;
                for (uint i = 0; i < eventArgs.ChoicesToDisplay.Length; i++)
                {
                    cp.SelectChoice(eventArgs.Choices, i);
                    Debug.Assert(eventArgs.Choices[i].GetActiveRequest());
                    eventArgs.Choices[i].SetActiveRequest(false); // Deactivate the KC_IDSelectionRequest.
                }
            };
        }

        public static IEnumerable<object[]> Data_TestExecute_ScheduledChoicePresenter()
        {
            IBlackboard blackboard = new Blackboard();

            Unit originalUnit = new Unit();
            originalUnit.AddComponent(new KC_UnitID("foo", true));
            originalUnit.AddComponent(new KC_Text("Here is a node with choices", true));

            Unit selectedUnit = new Unit(originalUnit);
            selectedUnit.AddComponent(new KC_ContentPool(KS_ScheduledChoicePresenter.DefaultChoicePresenterInputPool, true));

            Unit choice1 = new Unit();
            choice1.AddComponent(new KC_IDSelectionRequest("bar", true));
            choice1.AddComponent(new KC_Text("Choice 1", true));

            Unit choice2 = new Unit();
            choice2.AddComponent(new KC_IDSelectionRequest("baz", true));
            choice2.AddComponent(new KC_Text("Choice 2", true));

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KS_ScheduledChoicePresenter: the choice presenter to test            
             * Unit: the selected CU,
             * Unit: the original CU (selected CU is an copy of this),
             * Unit[]: array of choices 
             */

            return new List<object[]>
            {
                // Selected and original CU, no choices
                // new object[] { blackboard, new KS_KC_ScheduledChoicePresenter(blackboard), selectedUnit, originalUnit,  new Unit[] { } }, 

                // Selected and original CU, one choice
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), selectedUnit, originalUnit, new Unit[] { choice1 } },

                //// Selected and original CU, two choices
                //new object[] { blackboard, new KS_KC_ScheduledChoicePresenter(blackboard), selectedUnit, originalUnit, new Unit[] { choice1, choice2} },

                //// empty blackboard
                //new object[] { blackboard, new KS_KC_ScheduledChoicePresenter(blackboard), null, null, new Unit[0] },

                //// no selected CU
                 //new object[] { blackboard, new KS_KC_ScheduledChoicePresenter(blackboard), null, originalUnit, new Unit[] { choice1, choice2} },
             };
        }

        public static void ScheduledChoicePresenterTestHarness()
        {
            var tests = Data_TestExecute_ScheduledChoicePresenter();
            foreach(var test in tests)
            {
                IBlackboard blackboard = (IBlackboard)test[0];
                KS_ScheduledChoicePresenter cp = (KS_ScheduledChoicePresenter)test[1];
                Unit selectedUnit = (Unit)test[2];
                Unit originalUnit = (Unit)test[3];
                Unit[] choices = (Unit[])test[4];
                TestExecute_ScheduledChoicePresenter(blackboard, cp, selectedUnit, originalUnit, choices);
            }
        }

        public static void TestExecute_ScheduledChoicePresenter(IBlackboard blackboard, KS_ScheduledChoicePresenter ks, Unit selectedUnit,
            Unit originalUnit, Unit[] choices)
        {
            Debug.Assert((selectedUnit != null && originalUnit != null) || (selectedUnit == null));

            blackboard.Clear();

            // If there's a selectedUnit, add it to the blackboard. 
            if (selectedUnit != null)
            {
                blackboard.AddUnit(selectedUnit);
            }

            // Add any choices to the blackboard. 
            foreach (Unit choice in choices)
            {
                blackboard.AddUnit(choice);
            }

            // If there is an originalUnit, add links between the originalUnit and the choices. 
            if (originalUnit != null)
            {
                blackboard.AddUnit(originalUnit);
                foreach (Unit choice in choices)
                {
                    blackboard.AddLink(originalUnit, choice, LinkTypes.L_Choice);
                }
            }

            // If there's both an original unit and a selected unit, add a link between them. 
            if (originalUnit != null && selectedUnit != null)
            {
                blackboard.AddLink(originalUnit, selectedUnit, LinkTypes.L_SelectedUnit, true);
            }

            /* 
             * Add the event handler which tests whether the correct event args are being passed and that the KS_ScheduledChoicePresenter.SelectChoice()
             * is activating the KC_IDSelectionRequest on the choice. 
             */
            ks.PresenterExecute += GenerateEventHandler(selectedUnit, choices, blackboard);

            // Execute the choice presenter
            ks.Execute();
        }
        #endregion

        /*
         * Test whether an interface can be used to make intellisence work with an ExpandoObject. 
         */
        #region ExpandObject + Interface Experiment
        public static void TestExpandoObjectWithInterface()
        {
            dynamic test = new ExpandoObject();
            // fixme: can't cast test to ITestInterface, even though it's dynamic. At runtime sees that test is ExpandObject and can't find conversion
            ITestInterface testCast = (ITestInterface)test;


            test.Foo = 3;
            test.Bar = new object();
            Console.WriteLine(test.Foo);
        }

        public interface ITestInterface
        {
            int Foo { get; set; }
            object Bar { get; set; }
        }
        #endregion

        /*
         * Test using object composition to provide typed access to metadata and content slots.
         */
        #region Object Composition Experiment
        /* public interface IMetadata_ID
        {
            string ContentUnitID { get; set; }
        }

        public interface IMetadata_TargetID
        {
            string TargetContentUnitID { get; set; }
        }

        public interface IMetadata_ApplicabilityTestResult
        {
            bool ApplTestResult { get; set; }
        }

        public interface IMetadata_UnityPrologApplTest
        {
            string ApplTest_UnityProlog { get; set; }
            object ApplTestBindings_UnityProlog { get; set; } 
        }

        public class Metadata_ID : IMetadata_ID
        {
            public string ContentUnitID { get; set; }
        }

        public class Metatdata_TargetID : IMetadata_TargetID
        {
            public string TargetContentUnitID { get; set; }
        }

        public class Metadata_ApplicabilityTestResult : IMetadata_ApplicabilityTestResult
        {
            public bool ApplTestResult { get; set; }
        }

        // Boo. This approach is going to end up with the problem of defining a combinatorial collection of classes which I want to avoid. 
        public class ContentUnit_ID_ApplTestResult : IMetadata_ID, IMetadata_ApplicabilityTestResult
        {
            private readonly Metadata_ID m_id = new Metadata_ID();
            private readonly Metadata_ApplicabilityTestResult m_applResult = new Metadata_ApplicabilityTestResult();

            public string ContentUnitID { get => m_id.ContentUnitID; set => m_id.ContentUnitID = value; }
            public bool ApplTestResult { get => m_applResult.ApplTestResult; set => m_applResult.ApplTestResult = value; }
        } */
        #endregion

        /*
         * Let's explore now whether extension methods combined with using a dictionary will do what we need
         */
        #region Tag Interface Experiment
        /*
         * IMetadata must have a Metadata dictionary. This is implemented by ContentUnit
         */
        public interface IMetadata
        {
            IDictionary<string, object> Metadata { get; }
        }

        /*
         * An IMetadata_ID has a string ID. Interface implemented by extension methods (trick for multiple inheritance).
         */
        public interface IMetadata_ID : IMetadata
        {
        }

        public static string GetID(this IMetadata_ID contentUnit)
        {
            return (string)contentUnit.Metadata[ContentUnitID];
        }

        public static void SetID(this IMetadata_ID contentUnit, string id)
        {
            contentUnit.Metadata[ContentUnitID] = id;
        }

        /*
         * An IMetatadata_TargetID has a string target ID. Interface implemented by extension methods (trick for multiple inheritance).
         */
        public interface IMetadata_TargetID : IMetadata 
        {
        }

        public static string GetTargetID(this IMetadata_TargetID contentUnit)
        {
            return (string)contentUnit.Metadata[TargetContentUnitID];
        }

        public static void SetTargetID(this IMetadata_TargetID contentUnit, string id)
        {
            contentUnit.Metadata[TargetContentUnitID] = id;
        }

        /*
         * An IMetadata_ApplicabilityTestResult has a boolean applicability test result (appl test might be prolog, or simpler if test). 
         * Interface implemented by extension methods (trick for multiple inheritance).       
         */
        public interface IMetadata_ApplicabilityTestResult : IMetadata
        {
        }

        public static bool GetApplTestResult(this IMetadata_ApplicabilityTestResult contentUnit)
        {
            return (bool)contentUnit.Metadata[ApplTestResult];
        }

        public static void SetApplTestResult(this IMetadata_ApplicabilityTestResult contentUnit, bool result)
        {
            contentUnit.Metadata[ApplTestResult] = result;
        }

        /*
         * An IMetadata_UnityPrologApplTest has a string applicability test (will be parsed into a query by UnityProlog) and a object representing a variable binding.
         * Interface implemented by extension methods (trick for multiple inheritance).        
         */
        public interface IMetadata_UnityPrologApplTest : IMetadata
        {
        }

        public static string GetApplTest_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit)
        {
            return (string)contentUnit.Metadata[ApplTest_Prolog];
        }

        public static void SetApplTest_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit, string test)
        {
            contentUnit.Metadata[ApplTest_Prolog] = test;
        }

        public static object GetApplTestBindings_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit)
        {
            return contentUnit.Metadata[ApplTestBindings_Prolog];
        }

        public static void SetApplTestBindings_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit, object binding)
        {
            contentUnit.Metadata[ApplTestBindings_Prolog] = binding;
        }

        /*
         * Defining a ContentUnit with an ID, applicability test result, and prolog applicability test. 
         */
        /* public class ContentUnit_ID_Prolog : ContentUnit, IMetadata_ID, IMetadata_ApplicabilityTestResult, IMetadata_UnityPrologApplTest
        {
            public ContentUnit_ID_Prolog() : base()
            {
            }

            public ContentUnit_ID_Prolog(ContentUnit_ID_Prolog contentUnit) : base(contentUnit)
            { 
            }
        }*/

        /*
         * Defining a ContentUnit with a target ID
         */
        /*public class ContentUnit_Choice_TargetID : ContentUnit, IMetadata_TargetID
        {
            public ContentUnit_Choice_TargetID() : base()
            {
            }

            public ContentUnit_Choice_TargetID(ContentUnit_Choice_TargetID contentUnit) : base(contentUnit)
            {
            }
        }*/

        /*
         * Yay, it looks like this approach to faking multiple inheritance for content units is going to work! 
         * But, instead, I decide to use a component-based approach. This allows me to dynamically define the slots on a Unit without 
         * having to statically declare a type for each combination of tag interfaces that I use.  
         */
        /*public static void MultipleInheritanceTest()
        {
            ContentUnit_ID_Prolog contentUnit = new ContentUnit_ID_Prolog();

            contentUnit.SetID("start");
            contentUnit.SetApplTest_UnityProlog("available.");

            string id = contentUnit.GetID();

            IMetadata_ID contentUnit2 = new ContentUnit_ID_Prolog();
            contentUnit2.SetID("foo");

            ContentUnit_ID_Prolog contentUnit3 = new ContentUnit_ID_Prolog(contentUnit);
        }*/
        #endregion

    }
}
