using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
using CSA.Core;
using static CSA.KnowledgeUnits.KCNames;
using Prolog;

namespace CSA.Tests
{
    public class TestScheduledKnowledgeSources
    {
        private readonly ITestOutputHelper output;

        /*
         * Utilities used by test methods. 
         */
        #region utilities used by test methods
        static bool TestFilter(Unit unit)
        {
            return unit.HasComponent<KC_Order>() && unit.GetOrder() == 1;
        }

        /* 
         * Given a set of links which go from a unit in an input pool to a unit in an output pool, check that the L_SelectedContentUnit link is 
         * set up correctly and that the linked Unit is in the correct output pool. 
         */
        private static void TestFilterLinks(ISet<(IUnit, string, LinkDirection)> links, string outputPool)
        {
            int count = links.Count;
            Assert.Equal(1, count);
            (IUnit unit, string linkType, LinkDirection dir) = links.First();
            Assert.Equal(LinkTypes.L_SelectedUnit, linkType);
            Assert.Equal(LinkDirection.End, dir);
            Unit unitCast = unit as Unit; // fixme: only needing to cast because I'm being inconsistent with whether Unit or IUnit is what I'm targeting. 
            Assert.True(unitCast.HasComponent<KC_ContentPool>());
            Assert.True(unitCast.ContentPoolEquals(outputPool));
        }

        static private void TestNumberOfUnitsInOutputPool(int desiredNumberOfUnits, IBlackboard blackboard, string outputPool)
        {
            var units = from unit in blackboard.LookupUnits<Unit>()
                        where unit.HasComponent<KC_ContentPool>() && unit.ContentPoolEquals(outputPool)
                        select unit;

            Assert.Equal(desiredNumberOfUnits, units.Count());
        }
        #endregion

        /*
         * Data and test methods for KS_ScheduledFilterSelector
         */
        #region KS_ScheduledFilterSelector tests
        public static IEnumerable<object[]> Data_TestExecute_ScheduledFilterSelector()
        {
            string inputPool = "inputPool1";
            string outputPool = "outputPool1";

            IBlackboard blackboard = new Blackboard();

            var unit1 = new Unit();
            var unit2 = new Unit();
            var unit3 = new Unit();
            var unit4 = new Unit();

            unit1.AddComponent(new KC_ContentPool(inputPool, true));
            unit2.AddComponent(new KC_ContentPool(inputPool, true));

            unit2.AddComponent(new KC_Order(1, true));
            unit3.AddComponent(new KC_Order(1, true));
            unit4.AddComponent(new KC_Order(2, true));

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KS_ScheduledFilterSelector: the filter selector to test            
             * Unit[]: array of CUs to add to the blackboard
             * Unit[]: CUs which should be copied to the output pool        
             */

            return new List<object[]>
            {
                // Filter with default filter condition
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] { unit1, unit2, unit3, unit4 } },

                // Filter with input pool and output pool using input pool selection
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] {unit1, unit2} }, 

                // Filter with output pool and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool, TestFilter), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] { unit2, unit3 } },

                // Filter with input and output pools and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool, TestFilter), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] { unit2 } },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool), new Unit[0], new Unit[0] },

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool), new Unit[] { unit3, unit4 },
                    new Unit[0] },

                // Nothing in the input pool and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool, TestFilter), new Unit[] { unit3, unit4 },
                    new Unit[0] },
             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledFilterSelector))]
        public void TestExecute_ScheduledFilterSelector(IBlackboard blackboard, KS_ScheduledFilterSelector filterSelector, Unit[] unitsToAdd,
            Unit[] filteredUnits)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the units to the blackboard
            foreach (var unit in unitsToAdd)
            {
                blackboard.AddUnit(unit);
            }


            // Executed the filter selector
            filterSelector.Execute();

            string outputPool = filterSelector.OutputPool;

            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var unit in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(unit);
                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            TestNumberOfUnitsInOutputPool(filteredUnits.Length, blackboard, outputPool);

        }
        #endregion

        /*
         * Data and test methods for KS_ScheduledIDSelector.
         */
        #region KS_ScheduledIDSelector tests
        public static IEnumerable<object[]> Data_TestExecute_ScheduledIDSelector()
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

            unit2.AddComponent(new KC_Order(1, true));
            unit3.AddComponent(new KC_Order(1, true));
            unit4.AddComponent(new KC_Order(2, true));

            unit1.AddComponent(new KC_UnitID("ID1", true));
            unit2.AddComponent(new KC_UnitID("ID2", true));
            unit3.AddComponent(new KC_UnitID("ID1", true));
            unit4.AddComponent(new KC_UnitID("ID3", true));

            Unit idReq1 = new Unit();
            Unit idReq2 = new Unit();
            Unit idReq3 = new Unit();

            idReq1.AddComponent(new KC_IDSelectionRequest("ID1", true));
            idReq2.AddComponent(new KC_IDSelectionRequest("ID2", true));
            idReq3.AddComponent(new KC_IDSelectionRequest("ID3", true));

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KS_ScheduledFilterSelector: the filter selector to test            
             * Unit[]: array of Units to add to the blackboard
             * Unit[]: array of Units with a KC_IDSelectRequest to add to the blackboard            
             * Unit[]: Units which should be copied to the output pool           
             */

            return new List<object[]>
            {
                // Filter with default filter condition
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] { idReq1 }, new Unit[] { unit1, unit3, } }, 

                // Filter with input pool and output pool using input pool selection
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] { idReq1 }, new Unit[] {unit1 } }, 

                // Filter with output pool and specified filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool, TestFilter), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] { idReq1 }, new Unit[] { unit3 } },

                // Filter with input and output pools and specified filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool, TestFilter),
                    new Unit[] { unit1, unit2, unit3, unit4 }, new Unit[] { idReq1 }, new Unit[0] },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool), new Unit[0], new Unit[0],
                    new Unit[0]},

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool), new Unit[] { unit3, unit4 },
                    new Unit[] { idReq1 }, new Unit[0] },

                // Multiple U_IDRequests
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool, TestFilter),
                    new Unit[] { unit1, unit2, unit3, unit4 }, new Unit[] { idReq1, idReq2, idReq3}, new Unit[] { unit2 } },

             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledIDSelector))]
        public void TestExecute_ScheduledIDSelector(IBlackboard blackboard, KS_ScheduledIDSelector filterSelector, Unit[] unitsToAdd,
            Unit[] reqsToAdd, Unit[] filteredUnits)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var unit in unitsToAdd)
            {
                blackboard.AddUnit(unit);
            }

            // Add the requests to the blackboad
            foreach (var req in reqsToAdd)
            {
                blackboard.AddUnit(req);
                req.SetActiveRequest(true);
            }

            // Executed the filter selector
            filterSelector.Execute();

            string outputPool = filterSelector.OutputPool;

            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var unit in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(unit);
                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            TestNumberOfUnitsInOutputPool(filteredUnits.Length, blackboard, outputPool);

            // Grab all of the reqs on the blackboard and verify that none that are still active (all requests should have been set to inactive). 
            var reqs = from unit in blackboard.LookupUnits<Unit>()
                       where unit.HasComponent<KC_IDSelectionRequest>()
                       where unit.GetActiveRequest()
                       select unit;
            Assert.False(reqs.Any());
        }
        #endregion

        /*
         * Data and test methods for KS_ScheduledUniformDistributionSelector.
         */
        #region KS_ScheduledUniformDistributionSelector tests
        public static IEnumerable<object[]> Data_TestExecute_ScheduledUniformDistributionSelector()
        {
            string inputPool = "inputPool1";
            string outputPool = "outputPool1";
            int seed = 1;

            IBlackboard blackboard = new Blackboard();

            Unit unit1 = new Unit();
            Unit unit2 = new Unit();
            Unit unit3 = new Unit();
            Unit unit4 = new Unit();

            unit1.AddComponent(new KC_ContentPool(inputPool, true));
            unit2.AddComponent(new KC_ContentPool(inputPool, true));
            unit3.AddComponent(new KC_ContentPool(inputPool, true));

            unit2.AddComponent(new KC_Order(1, true));
            unit3.AddComponent(new KC_Order(1, true));
            unit4.AddComponent(new KC_Order(2, true));

            unit1.AddComponent(new KC_UnitID("ID1", true));
            unit2.AddComponent(new KC_UnitID("ID2", true));
            unit3.AddComponent(new KC_UnitID("ID1", true));
            unit4.AddComponent(new KC_UnitID("ID3", true));

            /* Structure of object[]: 
            * IBlackboard: blackboard, 
            * KS_ScheduledFilterSelector: the filter selector to test            
            * Unit[]: array of units to add to the blackboard
            * Unit[]: units to select among (that pass inputPool and testfilter filters)              
            */

            return new List<object[]>
            {
                // Filter with default filter condition and output pool using seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, seed), new Unit[] { unit1, unit2, unit3, unit4 },
                    new Unit[] {unit1, unit2, unit3, unit4 } }, 

                // Filter with specified output pool, using seed of 1, requesting 1 CU
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, outputPool, seed),
                    new Unit[] { unit1, unit2, unit3, unit4 }, new Unit[] { unit1, unit2, unit3, unit4 } }, 

                // Filter with specified input and output pools, specified number to select and seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, 2, seed),
                    new Unit[] { unit1, unit2, unit3, unit4 }, new Unit[] { unit1, unit2, unit3 } },

                // Filter with specified input and output pools, specified filter, specified number to select and seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, TestFilter, 1, seed),
                    new Unit[] { unit1, unit2, unit3, unit4 }, new Unit[] { unit2, unit3 } },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, 5), new Unit[0],
                    new Unit[0] },

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, 1, 1),
                    new Unit[] { unit4 }, new Unit[0] },

             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledUniformDistributionSelector))]
        public void TestExecute_ScheduledUniformDistributionSelector(IBlackboard blackboard, KS_ScheduledUniformDistributionSelector filterSelector,
            Unit[] unitsToAdd, Unit[] unitsToSelectFrom)
        {
            int seed = filterSelector.Seed;
            uint numberToSelect = filterSelector.NumberToSelect;
            string outputPool = filterSelector.OutputPool;

            System.Random random = new System.Random(seed);

            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var unit in unitsToAdd)
            {
                blackboard.AddUnit(unit);
            }

            // Executed the filter selector
            filterSelector.Execute();

            // Check that the uniform distriubtion selector selected the correct numberToSelect content units
            for (int i = 0; i < Math.Min(numberToSelect, unitsToSelectFrom.Length); i++)
            {
                int r = i + random.Next(unitsToSelectFrom.Length - i);
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(unitsToSelectFrom[r]);
                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as numberToSelect
            TestNumberOfUnitsInOutputPool((int)Math.Min(numberToSelect, unitsToSelectFrom.Length), blackboard, outputPool);
        }
        #endregion

        /*
         * Data and test methods for KS_ScheduledPoolCleaner.
         */
        #region KS_ScheduledPoolCleaner tests
        public static IEnumerable<object[]> Data_TestExecute_ScheduledPoolCleaner()
        {
            string pool1 = "pool1";
            string pool2 = "pool2";
            string pool3 = "pool3";

            IBlackboard blackboard = new Blackboard();

            Unit unit1 = new Unit();
            Unit unit2 = new Unit();
            Unit unit3 = new Unit();
            Unit unit4 = new Unit();
            Unit unit5 = new Unit();

            unit1.AddComponent(new KC_ContentPool(pool1, true));
            unit2.AddComponent(new KC_ContentPool(pool1, true));
            unit3.AddComponent(new KC_ContentPool(pool2, true));
            unit4.AddComponent(new KC_ContentPool(pool3, true));

            /* Structure of object[]: 
            * IBlackboard: blackboard, 
            * KS_ScheduledFilterPoolCleaner: the knowledge source to test            
            * Unit[]: array of units to add to the blackboard
            */

            return new List<object[]>
            {
                // One pool to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1 }),
                    new Unit[] { unit1, unit2, unit3, unit4, unit5}, new Unit[] { unit3, unit4, unit5 } }, 

                // Two pools to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1, pool2 }),
                    new Unit[] { unit1, unit2, unit3, unit4, unit5}, new Unit[] { unit4, unit5 } }, 

                // Three pools to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1, pool2, pool3 }),
                    new Unit[] { unit1, unit2, unit3, unit4, unit5}, new Unit[] { unit5 } },

                // Empty pool to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1 }),
                    new Unit[] { unit3, unit4, unit5}, new Unit[] { unit3, unit4, unit5 } }, 

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1, pool2, pool3 }),
                    new Unit[0], new Unit[0] },

                // No filter pools specified in constructor
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[0]),
                    new Unit[] { unit1, unit2, unit3, unit4, unit5}, new Unit[] { unit1, unit2, unit3, unit4, unit5 } },

             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledPoolCleaner))]
        public void TestExecute_ScheduledPoolCleaner(IBlackboard blackboard, KS_ScheduledFilterPoolCleaner cleaner,
            Unit[] unitsToAdd, Unit[] unitsRemaining)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var unit in unitsToAdd)
            {
                blackboard.AddUnit(unit);
            }

            // Executed the cleaner
            cleaner.Execute();

            // Check that only the remaining units are on the blackboard 
            ISet<Unit> unitSet = blackboard.LookupUnits<Unit>();
            Assert.True(unitSet.SetEquals(unitsRemaining));
        }
        #endregion

        /*
         * Data and test methods for KS_ScheduledChoicePresenter. 
         */
        #region KS_ScheduledChoicePresenter tests
        // fixme: add tests for the handlers defined in EventHandlers_ChoicePresenter 
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
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), selectedUnit, originalUnit,  new Unit[] { } }, 

                // Selected and original CU, one choice
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), selectedUnit, originalUnit, new Unit[] { choice1 } },

                // Selected and original CU, two choices
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), selectedUnit, originalUnit, new Unit[] { choice1, choice2} },

                // empty blackboard
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), null, null, new Unit[0] },

                // no selected CU
                 new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), null, originalUnit, new Unit[] { choice1, choice2} },
             };
        }

        private EventHandler<KC_PresenterExecuteEventArgs>
            GenerateEventHandler(Unit selectedUnit, Unit[] choices, IBlackboard _)
        {
            return (object sender, KC_PresenterExecuteEventArgs eventArgs) =>
            {
                if (selectedUnit != null)
                {
                    Assert.True(selectedUnit.TextEquals(eventArgs.TextToDisplay));
                    int numOfChoices = choices.Length;
                    Assert.Equal(numOfChoices, eventArgs.Choices.Length);

                    foreach (Unit choice in choices)
                    {
                        Assert.True(Array.Exists(eventArgs.ChoicesToDisplay, element => element.Equals(choice.GetText())));
                    }
                }
                else
                {
                    Assert.Equal("", eventArgs.TextToDisplay);
                }

                // Iterate through each of the choices selecting it and confirming that the KC_IDSelectionRequest is activated. 
                I_KC_ChoicePresenter cp = (I_KC_ChoicePresenter)sender;
                for (uint i = 0; i < eventArgs.ChoicesToDisplay.Length; i++)
                {
                    cp.SelectChoice(eventArgs.Choices, i);
                    Assert.True(eventArgs.Choices[i].GetActiveRequest());
                    eventArgs.Choices[i].SetActiveRequest(false); // Deactivate the KC_IDSelectionRequest.
                }
            };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledChoicePresenter))]
        public void TestExecute_ScheduledChoicePresenter(IBlackboard blackboard, KS_ScheduledChoicePresenter ks, Unit selectedUnit,
            Unit originalUnit, Unit[] choices)
        {
            System.Diagnostics.Debug.Assert((selectedUnit != null && originalUnit != null) || (selectedUnit == null));

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
         * Data and test methods for KS_ScheduledPrologEval.
         */
        #region KS_ScheduledPrologEval tests
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

            // units5 and 6 have a differently named prolog expression
            Unit unit5 = new Unit();
            Unit unit6 = new Unit();
            unit5.AddComponent(new KC_UnitID("ID5", true));
            unit6.AddComponent(new KC_UnitID("ID6", true));
            unit5.AddComponent(new KC_PrologExpression("PrologExpName1", "frustrated.", true));
            unit6.AddComponent(new KC_PrologExpression("PrologExpName1", "Character:frustrated(Character).", true));

            Unit prologKB = new Unit();
            prologKB.AddComponent(new KC_PrologKB("Global", true));

            /*
             * For some reason the unit testing infrastructure is making a folder way down in the bin directory the current folder. So using a relative 
             * file to the source folder for CATests.
             */
            prologKB.GetComponent<KC_PrologKB>().Consult("../../../PrologTest.prolog");

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

                // Global input pool, prolog expressions with different names (not ApplTest_Prolog), so no expressions should be evaluated. 
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool, ApplTest_Prolog), new Unit[] { unit5, unit6 }, 
                    prologKB, new Unit[0], new string[] { "dissed(character1, me)." },
                    new (Unit, bool)[0],
                    new (Unit, object)[0] },
            };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledPrologEval))]
        public void TestExecute_ScheduledPrologEval(IBlackboard blackboard, KS_ScheduledPrologEval prologEval, Unit[] unitsToAdd,
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
                    output.WriteLine("Adding assertion: " + assertion);
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
                    output.WriteLine("Removing assertion: " + assertion);
                    prologKB.GetComponent<KC_PrologKB>().Retract(assertion);
                }
            }

            string outputPool = prologEval.OutputPool;

            // Iterate through each of the units which should have been evaluated and see if there's a copy of them in the output pool.
            // Also test that the evaluation results and bindings are correct for each evaluated unit.
            foreach (var unit in evaluatedUnits)
            {
                output.WriteLine("Testing results and bindings for UnitID: " + unit.GetUnitID());

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
                    Assert.NotNull(u);
                    Assert.Equal(result, resultUnit.GetPrologExpEvalResult()); 
                }

                if (bindings.Length > 0)
                {
                    (Unit u, object binding) =
                        Array.Find(bindings, bindingTuple => bindingTuple.u == unit);
                    Assert.NotNull(u);
 
                    IEnumerable<bool> checkBinding = Term.Unify(resultUnit.GetPrologExpBindings(), binding);
                    output.WriteLine("checkBinding.Next(): " + checkBinding.GetEnumerator().MoveNext());
                    Assert.True(checkBinding.GetEnumerator().MoveNext(),
                        $"Variable binding not correct: {resultUnit.GetPrologExpBindings()} != {binding}");
                }

                // Verify that each evaluated unit is linked to a unit containing a matching, evaluated KC_EvaluatablePrologExpression.
                KC_EvaluatablePrologExpression evaledPrologExp = resultUnit.GetComponent<KC_EvaluatablePrologExpression>();
                Assert.Equal(unit.GetPrologExpName<KC_PrologExpression>(), evaledPrologExp.PrologExpName);
                Assert.True(evaledPrologExp.Evaluated);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as evaluatedUnits
            TestNumberOfUnitsInOutputPool(evaluatedUnits.Length, blackboard, outputPool);
        }
        #endregion

        // fixme: add test for KS_ScheduledExecute

        /* 
         * Data and test methods for KS_ScheduledTierSelector
         */
        #region KS_ScheduledTierSelector tests
        public static IEnumerable<object[]> Data_TestExecute_ScheduledTierSelector()
        {
            string pool1 = "pool1";

            IBlackboard blackboard = new Blackboard();

            Unit[] units = new Unit[15];

            for (int i = 0; i < units.Length; i++)
            {
                units[i] = new Unit();
            }

            units[0].AddComponent(new KC_ContentPool(pool1, true));
            units[1].AddComponent(new KC_ContentPool(pool1, true));
            units[2].AddComponent(new KC_ContentPool(pool1, true));

            units[0].AddComponent(new KC_Order(5, true));
            units[0].AddComponent(new KC_UnitID("ID0", true));

            units[1].AddComponent(new KC_Order(3, true));
            units[1].AddComponent(new KC_UnitID("ID1", true));

            units[2].AddComponent(new KC_Order(5, true));
            units[2].AddComponent(new KC_UnitID("ID2", true));

            units[3].AddComponent(new KC_Order(3, true));
            units[3].AddComponent(new KC_UnitID("ID3", true));

            units[4].AddComponent(new KC_Order(6, true));
            units[4].AddComponent(new KC_UnitID("ID4", true));

            units[5].AddComponent(new KC_Text("xylophone", true));
            units[6].AddComponent(new KC_Text("carburator", true));
            units[7].AddComponent(new KC_Text("xylophone", true));
            units[8].AddComponent(new KC_Text("carburator", true));
            units[9].AddComponent(new KC_Text("Zebra", true));

            units[10].AddComponent(new KC_Utility(5.3, true));
            units[11].AddComponent(new KC_Utility(3.1, true));
            units[12].AddComponent(new KC_Utility(5.3, true));
            units[13].AddComponent(new KC_Utility(3.1, true));
            units[14].AddComponent(new KC_Utility(6.7, true));

            /* Structure of object[]: 
            * IBlackboard: blackboard, 
            * KS_ScheduledTierSelector: the knowledge source to test            
            * Unit[]: array of CUs to add to the blackboard
            * Unit[]: the units that should be in the output pool  
            */

            return new List<object[]>
            {
                /*
                 * Test cases for HighestTierSelector
                 */
                #region HighestTierSelector test cases
                // No input pool, default output pool, KC_Order as tier slot
                new object[] { blackboard,  new KS_ScheduledHighestTierSelector<KC_Order>(blackboard),
                    units.Take(5), new Unit[] { units[4] } }, 

                // No input pool, specified output pool, KC_Order as tier slot
                new object[] { blackboard, new KS_ScheduledHighestTierSelector<KC_Order>(blackboard, "output1"),
                    units.Take(5), new Unit[] { units[4] } }, 

                // Input pool, specified output pool, KC_Order as tier slot
                new object[] { blackboard, new KS_ScheduledHighestTierSelector<KC_Order>(blackboard, pool1, "output1"),
                    units.Take(5), new Unit[] { units[0], units[2] } },

                // No input pool, specified output pool, filter condition, KC_Order as tier slot
                new object[] { blackboard,
                    new KS_ScheduledHighestTierSelector<KC_Order>(blackboard, "output1", (Unit unit) => unit.GetOrder() < 6),
                    units.Take(5), new Unit[] { units[0], units[2] } },
                              
                // Input pool, specified output pool, filter condition, KC_Order as tier slot
                new object[] { blackboard,
                    new KS_ScheduledHighestTierSelector<KC_Order>(blackboard, pool1, "output1", (Unit unit) => unit.GetOrder() < 5),
                    units.Take(5), new Unit[] { units[1] } },

                // No input pool, default output pool, KC_Text as tier slot
                new object[] { blackboard, new KS_ScheduledHighestTierSelector<KC_Text>(blackboard),
                    units.Skip(5).Take(5), new Unit[] { units[9] } }, 

                // No input pool, default output pool, KC_Utility as tier slot
                new object[] { blackboard, new KS_ScheduledHighestTierSelector<KC_Utility>(blackboard),
                    units.Skip(10), new Unit[] { units[14] } }, 

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledHighestTierSelector<KC_Order>(blackboard),
                    new Unit[0], new Unit[0] }, 

                // Empty pool
                new object[] { blackboard, new KS_ScheduledHighestTierSelector<KC_Order>(blackboard, pool1, "output1"),
                    units.Skip(3).Take(2), new Unit[0] },

                // Non-existant KC_UnitID tier slot
                new object[] { blackboard, new KS_ScheduledHighestTierSelector<KC_UnitID>(blackboard),
                    units.Skip(5).Take(5), new Unit[0] },
                #endregion

                /*
                 * Test cases for ScheduledLowestTierSelector
                 */
                #region LowestTierSelector test cases
                // No input pool, default output pool, KC_Order as tier slot
                new object[] { blackboard,  new KS_ScheduledLowestTierSelector<KC_Order>(blackboard),
                    units.Take(5), new Unit[] { units[1], units[3] } }, 

                // No input pool, specified output pool, KC_Order as tier slot
                new object[] { blackboard, new KS_ScheduledLowestTierSelector<KC_Order>(blackboard, "output1"),
                    units.Take(5), new Unit[] { units[1], units[3] } }, 

                // Input pool, specified output pool, KC_Order as tier slot
                new object[] { blackboard, new KS_ScheduledLowestTierSelector<KC_Order>(blackboard, pool1, "output1"),
                    units.Take(5), new Unit[] { units[1] } },

                // No input pool, specified output pool, filter condition, KC_Order as tier slot
                new object[] { blackboard,
                    new KS_ScheduledLowestTierSelector<KC_Order>(blackboard, "output1", (Unit unit) => unit.GetOrder() > 5),
                    units.Take(5), new Unit[] { units[4] } },
                              
                // Input pool, specified output pool, filter condition, KC_Order as tier slot
                new object[] { blackboard,
                    new KS_ScheduledLowestTierSelector<KC_Order>(blackboard, pool1, "output1", (Unit unit) => unit.GetOrder() > 3),
                    units.Take(5), new Unit[] { units[0], units[2] } },

                // No input pool, default output pool, KC_Text as tier slot
                new object[] { blackboard, new KS_ScheduledLowestTierSelector<KC_Text>(blackboard),
                    units.Skip(5).Take(5), new Unit[] { units[6], units[8] } }, 

                // No input pool, default output pool, KC_Utility as tier slot
                new object[] { blackboard, new KS_ScheduledLowestTierSelector<KC_Utility>(blackboard),
                    units.Skip(10), new Unit[] { units[11], units[13] } }, 

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledLowestTierSelector<KC_Order>(blackboard),
                    new Unit[0], new Unit[0] }, 

                // Empty pool
                new object[] { blackboard, new KS_ScheduledLowestTierSelector<KC_Order>(blackboard, pool1, "output1"),
                    units.Skip(3).Take(2), new Unit[0] },

                // Non-existant KC_UnitID tier slot
                new object[] { blackboard, new KS_ScheduledLowestTierSelector<KC_UnitID>(blackboard),
                    units.Skip(5).Take(5), new Unit[0] },
                #endregion

            };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledTierSelector))]
        public void TestExecute_ScheduledTierSelector<T>(IBlackboard blackboard, KS_ScheduledTierSelector<T> tierSelector,
            Unit[] unitsToAdd, Unit[] filteredUnits) where T : KnowledgeComponent, IComparable
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();
            
            // Add the content units to the blackboard
            foreach (var unit in unitsToAdd)
            {
                blackboard.AddUnit(unit);
            }

            // Execute the tier selector
            tierSelector.Execute();

            string outputPool = tierSelector.OutputPool;

            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var unit in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(unit);

                output.WriteLine("Original Unit:");
                output.WriteLine(unit.ToString());

                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            TestNumberOfUnitsInOutputPool(filteredUnits.Length, blackboard, outputPool);
        }
        #endregion

        public TestScheduledKnowledgeSources(ITestOutputHelper output)
        {
            this.output = output;
        }
    }
}
