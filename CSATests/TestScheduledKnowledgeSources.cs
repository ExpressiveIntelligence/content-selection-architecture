using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;
using CSA.Core;
using Prolog;

namespace CSA.Tests
{
    public class TestScheduledKnowledgeSources
    {
        private readonly ITestOutputHelper output;

        // fixme: change this to test KnowledgeComponent-based ScheduledFilerSelector
        public static IEnumerable<object[]> Data_TestExecute_ScheduledFilterSelector()
        {
            string inputPool = "inputPool1";
            string outputPool = "outputPool1";

            IBlackboard blackboard = new Blackboard();

            ContentUnit cu1 = new ContentUnit();
            ContentUnit cu2 = new ContentUnit();
            ContentUnit cu3 = new ContentUnit();
            ContentUnit cu4 = new ContentUnit();

            cu1.Metadata[ContentPool] = inputPool;
            cu2.Metadata[ContentPool] = inputPool;

            cu2.Metadata["Test"] = 1;
            cu3.Metadata["Test"] = 1;
            cu4.Metadata["Test"] = 2;

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KS_ScheduledFilterSelector: the filter selector to test            
             * ContentUnit[]: array of CUs to add to the blackboard
             * ContentUnit[]: CUs which should be copied to the output pool        
             */

            return new List<object[]>
            {
                // Filter with default filter condition
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu1, cu2, cu3, cu4 } }, 

                // Filter with input pool and output pool using input pool selection
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] {cu1, cu2} }, 

                // Filter with output pool and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool, TestFilter), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu2, cu3 } },

                // Filter with input and output pools and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool, TestFilter), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu2 } },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool), new ContentUnit[0], new ContentUnit[0] },

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu3, cu4 },
                    new ContentUnit[0] },

                // Nothing in the input pool and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool, TestFilter), new ContentUnit[] { cu3, cu4 },
                    new ContentUnit[0] },

             };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledFilerSelector
        static bool TestFilter(ContentUnit cu)
        {
            return cu.HasMetadataSlot("Test") && (int)cu.Metadata["Test"] == 1 ? true : false;
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledFilerSelector
        /* Given a set of links which go from a unit in an input pool to a unit in an output pool, check that the L_SelectedContentUnit link is 
         * set up correctly and that the linked CU is in the correct output pool. 
         */
        private static void TestFilterLinks(ISet<(IUnit, string, LinkDirection)> links, string outputPool)
        {
            int count = links.Count;
            Assert.Equal(1, count);
            (IUnit cu, string linkType, LinkDirection dir) = links.First();
            Assert.Equal(LinkTypes.L_SelectedContentUnit, linkType);
            Assert.Equal(LinkDirection.End, dir);
            ContentUnit cuCopy = cu as ContentUnit;
            Assert.True(cuCopy.HasMetadataSlot(ContentPool));
            Assert.Equal(outputPool, cuCopy.Metadata[ContentPool]);
        }

        // fixme: delete this when all test methods have been converted to KnowledgeComponent-based knowledge sources. 
        static private void TestNumberOfCUsInOutputPool(int desiredNumberOfCUs, IBlackboard blackboard, string outputPool)
        {
            var CUs = from cu in blackboard.LookupUnits<ContentUnit>()
                      where cu.HasMetadataSlot(ContentPool)
                      where cu.Metadata[ContentPool].Equals(outputPool)
                      select cu;

            Assert.Equal(desiredNumberOfCUs, CUs.Count());
        }

        static private void TestNumberOfUnitsInOutputPool(int desiredNumberOfUnits, IBlackboard blackboard, string outputPool)
        {
            var units = from unit in blackboard.LookupUnits<Unit>()
                        where unit.HasComponent<KC_ContentPool>() && unit.ContentPoolEquals(outputPool)
                        select unit;

            Assert.Equal(desiredNumberOfUnits, units.Count());
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledFilerSelector
        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledFilterSelector))]
        public void TestExecute_ScheduledFilterSelector(IBlackboard blackboard, KS_ScheduledFilterSelector filterSelector, ContentUnit[] unitsToAdd,
            ContentUnit[] filteredUnits)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the units to the blackboard
            foreach (var cu in unitsToAdd)
            {
                blackboard.AddUnit(cu);
            }


            // Executed the filter selector
            filterSelector.Execute();

            string outputPool = filterSelector.OutputPool;

            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var cu in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(cu);
                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            TestNumberOfCUsInOutputPool(filteredUnits.Length, blackboard, outputPool);

        }

        // fixme: change this to test KnowledgeComponent-based ScheduledIDSelector
        public static IEnumerable<object[]> Data_TestExecute_ScheduledIDSelector()
        {
            string inputPool = "inputPool1";
            string outputPool = "outputPool1";

            IBlackboard blackboard = new Blackboard();

            ContentUnit cu1 = new ContentUnit();
            ContentUnit cu2 = new ContentUnit();
            ContentUnit cu3 = new ContentUnit();
            ContentUnit cu4 = new ContentUnit();

            cu1.Metadata[ContentPool] = inputPool;
            cu2.Metadata[ContentPool] = inputPool;

            cu2.Metadata["Test"] = 1;
            cu3.Metadata["Test"] = 1;
            cu4.Metadata["Test"] = 2;

            cu1.Metadata[ContentUnitID] = "ID1";
            cu2.Metadata[ContentUnitID] = "ID2";
            cu3.Metadata[ContentUnitID] = "ID1";
            cu4.Metadata[ContentUnitID] = "ID3";

            U_IDSelectRequest idReq1 = new U_IDSelectRequest("ID1");
            U_IDSelectRequest idReq2 = new U_IDSelectRequest("ID2");
            U_IDSelectRequest idReq3 = new U_IDSelectRequest("ID3");

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KS_ScheduledFilterSelector: the filter selector to test            
             * ContentUnit[]: array of CUs to add to the blackboard
             * U_IDSelectRequest[]: array of U_IDSelectRequests to add to the blackboard            
             * ContentUnit[]: CUs which should be copied to the output pool           
             */

            return new List<object[]>
            {
                // Filter with default filter condition
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[] { cu1, cu3, } }, 

                // Filter with input pool and output pool using input pool selection
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[] {cu1 } }, 

                // Filter with output pool and specified filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool, TestFilter), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[] { cu3 } },

                // Filter with input and output pools and specified filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool, TestFilter),
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new U_IDSelectRequest[] { idReq1 }, new ContentUnit[0] },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool), new ContentUnit[0], new U_IDSelectRequest[0],
                    new ContentUnit[0]},

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[0] },

                // Multiple U_IDRequests
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool, TestFilter),
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new U_IDSelectRequest[] { idReq1, idReq2, idReq3}, new ContentUnit[] { cu2 } },

             };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledIDSelector
        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledIDSelector))]
        public void TestExecute_ScheduledIDSelector(IBlackboard blackboard, KS_ScheduledFilterSelector filterSelector, ContentUnit[] unitsToAdd,
            U_IDSelectRequest[] reqsToAdd, ContentUnit[] filteredUnits)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var cu in unitsToAdd)
            {
                blackboard.AddUnit(cu);
            }

            // Add the requests to the blackboad
            foreach (var req in reqsToAdd)
            {
                blackboard.AddUnit(req);
            }

            // Executed the filter selector
            filterSelector.Execute();

            string outputPool = filterSelector.OutputPool;

            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var cu in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(cu);
                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            TestNumberOfCUsInOutputPool(filteredUnits.Length, blackboard, outputPool);

            // Grab all of the reqs on the blackboard and verify that there are none (should have all been deleted).
            ISet<U_IDSelectRequest> reqs = blackboard.LookupUnits<U_IDSelectRequest>();
            Assert.False(reqs.Any());
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledUniformDistributionSelector
        public static IEnumerable<object[]> Data_TestExecute_ScheduledUniformDistributionSelector()
        {
            string inputPool = "inputPool1";
            string outputPool = "outputPool1";
            int seed = 1;

            IBlackboard blackboard = new Blackboard();

            ContentUnit cu1 = new ContentUnit();
            ContentUnit cu2 = new ContentUnit();
            ContentUnit cu3 = new ContentUnit();
            ContentUnit cu4 = new ContentUnit();

            cu1.Metadata[ContentPool] = inputPool;
            cu2.Metadata[ContentPool] = inputPool;
            cu3.Metadata[ContentPool] = inputPool;

            cu2.Metadata["Test"] = 1;
            cu3.Metadata["Test"] = 1;
            cu4.Metadata["Test"] = 2;

            cu1.Metadata[ContentUnitID] = "ID1";
            cu2.Metadata[ContentUnitID] = "ID2";
            cu3.Metadata[ContentUnitID] = "ID1";
            cu4.Metadata[ContentUnitID] = "ID3";

            /* Structure of object[]: 
            * IBlackboard: blackboard, 
            * KS_ScheduledFilterSelector: the filter selector to test            
            * ContentUnit[]: array of CUs to add to the blackboard
            * ContentUnit[]: units to select among (that pass inputPool and testfilter filters)              
            */

            return new List<object[]>
            {
                // Filter with default filter condition and output pool using seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, seed), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] {cu1, cu2, cu3, cu4 } }, 

                // Filter with specified output pool, using seed of 1, requesting 1 CU
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, outputPool, seed),
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new ContentUnit[] { cu1, cu2, cu3, cu4 } }, 

                // Filter with specified input and output pools, specified number to select and seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, 2, seed),
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new ContentUnit[] { cu1, cu2, cu3 } },

                // Filter with specified input and output pools, specified filter, specified number to select and seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, TestFilter, 1, seed),
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new ContentUnit[] { cu2, cu3 } },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, 5), new ContentUnit[0],
                    new ContentUnit[0] },

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, 1, 1),
                    new ContentUnit[] { cu4 }, new ContentUnit[0] },

             };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledUniformDistributionSelector
        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledUniformDistributionSelector))]
        public void TestExecute_ScheduledUniformDistributionSelector(IBlackboard blackboard, KS_ScheduledUniformDistributionSelector filterSelector,
            ContentUnit[] unitsToAdd, ContentUnit[] unitsToSelectFrom)
        {
            int seed = filterSelector.Seed;
            uint numberToSelect = filterSelector.NumberToSelect;
            string outputPool = filterSelector.OutputPool;

            System.Random random = new System.Random(seed);

            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var cu in unitsToAdd)
            {
                blackboard.AddUnit(cu);
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
            TestNumberOfCUsInOutputPool((int)Math.Min(numberToSelect, unitsToSelectFrom.Length), blackboard, outputPool);
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledPoolCleaner
        public static IEnumerable<object[]> Data_TestExecute_ScheduledPoolCleaner()
        {
            string pool1 = "pool1";
            string pool2 = "pool2";
            string pool3 = "pool3";

            IBlackboard blackboard = new Blackboard();

            ContentUnit cu1 = new ContentUnit();
            ContentUnit cu2 = new ContentUnit();
            ContentUnit cu3 = new ContentUnit();
            ContentUnit cu4 = new ContentUnit();
            ContentUnit cu5 = new ContentUnit();

            cu1.Metadata[ContentPool] = pool1;
            cu2.Metadata[ContentPool] = pool1;
            cu3.Metadata[ContentPool] = pool2;
            cu4.Metadata[ContentPool] = pool3;

            /* Structure of object[]: 
            * IBlackboard: blackboard, 
            * KS_ScheduledFilterPoolCleaner: the knowledge source to test            
            * ContentUnit[]: array of CUs to add to the blackboard
            * ContentUnit[]: the content units that should be left on the blackboard           
            */

            return new List<object[]>
            {
                // One pool to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1 }),
                    new ContentUnit[] { cu1, cu2, cu3, cu4, cu5}, new ContentUnit[] { cu3, cu4, cu5 } }, 

                // Two pools to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1, pool2 }),
                    new ContentUnit[] { cu1, cu2, cu3, cu4, cu5}, new ContentUnit[] { cu4, cu5 } }, 

                // Three pools to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1, pool2, pool3 }),
                    new ContentUnit[] { cu1, cu2, cu3, cu4, cu5}, new ContentUnit[] { cu5 } },

                // Empty pool to clean
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1 }),
                    new ContentUnit[] { cu3, cu4, cu5}, new ContentUnit[] { cu3, cu4, cu5 } }, 

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[] { pool1, pool2, pool3 }),
                    new ContentUnit[0], new ContentUnit[0] },

                // No filter pools specified in constructor
                new object[] { blackboard, new KS_ScheduledFilterPoolCleaner(blackboard, new string[0]),
                    new ContentUnit[] { cu1, cu2, cu3, cu4, cu5}, new ContentUnit[] { cu1, cu2, cu3, cu4, cu5 } },

             };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledPoolCleaner
        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledPoolCleaner))]
        public void TestExecute_ScheduledPoolCleaner(IBlackboard blackboard, KS_ScheduledFilterPoolCleaner cleaner,
            ContentUnit[] unitsToAdd, ContentUnit[] unitsRemaining)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var cu in unitsToAdd)
            {
                blackboard.AddUnit(cu);
            }

            // Executed the cleaner
            cleaner.Execute();

            // Check that only the remaining units are on the blackboard 
            ISet<ContentUnit> cuSet = blackboard.LookupUnits<ContentUnit>();
            Assert.True(cuSet.SetEquals(unitsRemaining));
        }

        // fixme: add tests for the handlers defined in EventHandlers_ChoicePresenter 
        // fixme: change this to test KnowledgeComponent-based ScheduledChoicePresenter (once it has been defined)
        public static IEnumerable<object[]> Data_TestExecute_ScheduledChoicePresenter()
        {
            IBlackboard blackboard = new Blackboard();

            ContentUnit originalCU = new ContentUnit();
            originalCU.Metadata[ContentUnitID] = "foo";
            originalCU.Content[Text] = "Here is a node with choices";

            ContentUnit selectedCU = new ContentUnit(originalCU);
            selectedCU.Metadata[ContentPool] = KS_ScheduledChoicePresenter.DefaultChoicePresenterInputPool;

            ContentUnit choice1 = new ContentUnit();
            choice1.Metadata[TargetContentUnitID] = "bar";
            choice1.Content[Text] = "Choice 1";

            ContentUnit choice2 = new ContentUnit();
            choice2.Metadata[TargetContentUnitID] = "baz";
            choice2.Content[Text] = "Choice 2";

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KSScheduledChoicePresenter: the choice presenter to test            
             * ContentUnit: the selected CU,
             * ContentUnit: the original CU (selected CU is an copy of this),
             * ContentUnit[]: array of choices (ContentUnits) 
             */

            return new List<object[]>
            {
                // Selected and original CU, no choices
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), selectedCU, originalCU,  new ContentUnit[] { } }, 

                // Selected and original CU, one choice
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), selectedCU, originalCU, new ContentUnit[] { choice1 } },

                // Selected and original CU, two choices
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), selectedCU, originalCU, new ContentUnit[] { choice1, choice2} },

                // empty blackboard
                new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), null, null, new ContentUnit[0] },

                // no selected CU
                 new object[] { blackboard, new KS_ScheduledChoicePresenter(blackboard), null, originalCU, new ContentUnit[] { choice1, choice2} },
             };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledChoicePresenter (once it has been defined)
        private EventHandler<PresenterExecuteEventArgs>
            GenerateEventHandler(ContentUnit selectedCU, ContentUnit[] choices, IBlackboard blackboard)
        {
            return (object sender, PresenterExecuteEventArgs eventArgs) =>
            {
                var presenterEventArgs = eventArgs as PresenterExecuteEventArgs;

                if (selectedCU != null)
                {
                    Assert.Equal(selectedCU.Content[Text], presenterEventArgs.TextToDisplay);
                    int numOfChoices = choices.Length;
                    Assert.Equal(numOfChoices, presenterEventArgs.Choices.Length);

                    foreach (ContentUnit choice in choices)
                    {
                        Assert.True(Array.Exists(presenterEventArgs.ChoicesToDisplay, element => element.Equals(choice.Content[Text])));
                    }
                }
                else
                {
                    Assert.Equal("", presenterEventArgs.TextToDisplay);
                }

                // Iterate through each of the choices selecting it and confirming that the correct U_IDSelectRequest is added. 
                IChoicePresenter cp = (IChoicePresenter)sender;
                for (uint i = 0; i < presenterEventArgs.ChoicesToDisplay.Length; i++)
                {
                    cp.SelectChoice(presenterEventArgs.Choices, i);
                    U_IDSelectRequest idSelectRequest = blackboard.LookupSingleton<U_IDSelectRequest>();
                    Assert.True(idSelectRequest.TargetContentUnitID.Equals(choices[i].Metadata[TargetContentUnitID]));
                    blackboard.RemoveUnit(idSelectRequest); // Remove the U_IDSelect request before the next iteration. 
                }

            };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledChoicePresenter (once it has been defined)
        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledChoicePresenter))]
        public void TestExecute_ScheduledChoicePresenter(IBlackboard blackboard, KS_ScheduledChoicePresenter ks, ContentUnit selectedCU,
            ContentUnit originalCU, ContentUnit[] choices)
        {
            System.Diagnostics.Debug.Assert((selectedCU != null && originalCU != null) || (selectedCU == null));

            blackboard.Clear();

            if (selectedCU != null)
            {
                blackboard.AddUnit(selectedCU);
            }

            foreach (ContentUnit choice in choices)
            {
                blackboard.AddUnit(choice);
            }

            if (originalCU != null)
            {
                blackboard.AddUnit(originalCU);
                foreach (ContentUnit choice in choices)
                {
                    blackboard.AddLink(originalCU, choice, LinkTypes.L_Choice);
                }
            }

            if (originalCU != null && selectedCU != null)
            {
                blackboard.AddLink(originalCU, selectedCU, LinkTypes.L_SelectedContentUnit, true);
            }

            /* 
             * Add the event handler which tests whether the correct event args are being passed and that the KS_ScheduledChoicePresenter.SelectChoice()
             * is adding the correct U_IDSelectRequest. 
             */
            ks.PresenterExecute += GenerateEventHandler(selectedCU, choices, blackboard);

            // Execute the choice presenter
            ks.Execute();
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledPrologEval (once it has been defined)
        public static IEnumerable<object[]> Data_TestExecute_ScheduledPrologEval()
        {
            string inputPool = "inputPool1";
            string outputPool = "outputPool1";

            IBlackboard blackboard = new Blackboard();

            ContentUnit cu1 = new ContentUnit();
            ContentUnit cu2 = new ContentUnit();
            ContentUnit cu3 = new ContentUnit();
            ContentUnit cu4 = new ContentUnit();

            cu1.Metadata[ContentPool] = inputPool;
            cu2.Metadata[ContentPool] = inputPool;
            // cu3 and cu4 are in the global content pool (no content pool specified) 

            cu1.Metadata[ContentUnitID] = "ID1";
            cu2.Metadata[ContentUnitID] = "ID2";
            cu3.Metadata[ContentUnitID] = "ID3";
            cu4.Metadata[ContentUnitID] = "ID4";

            // Prolog applicability tests
            cu1.Metadata[ApplTest_Prolog] = "frustrated.";
            cu2.Metadata[ApplTest_Prolog] = "Character:frustrated(Character).";
            cu3.Metadata[ApplTest_Prolog] = "frustrated.";
            // cu4 doesn't have the ApplTest_Prolog slot defined, so it shouldn't be filtered by KS_ScheduledPrologEval

            U_PrologEvalRequest prologReq1 = new U_PrologEvalRequest(ApplTest_Prolog, ApplTestResult, ApplTestBindings_Prolog);

            U_PrologKB prologKB = new U_PrologKB("Global");

            /*
             * For some reason the unit testing infrastructure is making a folder way down in the bin directory the current folder. So using a relative 
             * file to the source folder for CATests.
             */
            prologKB.Consult("../../../PrologTest.prolog");

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KS_ScheduledPrologEval[]: the ScheduledPrologEval to test            
             * ContentUnit[]: array of CUs to add to the blackboard
             * U_PrologEvalRequest[]: array of U_PrologEvalRequests to add to the blackboard   
             * U_PrologKB: the prolog KB to add to the blackboard
             * ContentUnit[]: Content units on which the prolog applicability test was evaluated.
             * string[]: Array of assertions to make in prolog KB.            
             * (ContentUnit, bool)[]: Array of evaluation results for ApplTestResult
             * (ContentUnit, object)[]: Array of bindings for ApplTestBindigns_Prolog   
             */

            return new List<object[]>
            {
                // No specific input pool (global), no queries with bindings, assertion that makes queries true. 
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool), new ContentUnit[] { cu1, cu3, cu4 },
                    new U_PrologEvalRequest[] { prologReq1 }, prologKB, new ContentUnit[] { cu1, cu3 }, new string[] { "dissed(character1, me)." },
                    new (ContentUnit, bool)[] { (cu1, true), (cu3, true) },
                    new (ContentUnit, object)[0] },

                // No specific input pool (global), no queries with bindings, no assertion to make queries true.  
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool), new ContentUnit[] { cu1, cu3, cu4 },
                    new U_PrologEvalRequest[] { prologReq1 }, prologKB, new ContentUnit[] { cu1, cu3 }, new string[0],
                    new (ContentUnit, bool)[] { (cu1, false), (cu3, false) },
                    new (ContentUnit, object)[0] },

                // No specific input pool (global), query with binding, assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool), new ContentUnit[] { cu2 },
                    new U_PrologEvalRequest[] { prologReq1 }, prologKB, new ContentUnit[] { cu2 }, new string[] { "dissed(character1, me)." },
                    new (ContentUnit, bool)[] { (cu2, true) },
                    new (ContentUnit, object)[] { (cu2, Symbol.Intern("character1")) } },

                // No specific input pool (global), query with binding, no assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool), new ContentUnit[] { cu2 },
                    new U_PrologEvalRequest[] { prologReq1 }, prologKB, new ContentUnit[] { cu2 }, new string[0],
                    new (ContentUnit, bool)[] { (cu2, false) },
                    new (ContentUnit, object)[0] }, 

                // No specific input pool (global), some queries with bindings, some without, assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_PrologEvalRequest[] { prologReq1 }, prologKB, new ContentUnit[] { cu1, cu2, cu3 }, new string[] { "dissed(character1, me)." },
                    new (ContentUnit, bool)[] { (cu1, true), (cu2, true), (cu3, true) },
                    new (ContentUnit, object)[] { (cu1, new LogicVariable("V1")), (cu2, Symbol.Intern("character1")), (cu3, new LogicVariable("V2")) } },

                // No prolog eval request 
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_PrologEvalRequest[0], prologKB, new ContentUnit[0], new string[] { "dissed(character1, me)." },
                    new (ContentUnit, bool)[0],
                    new (ContentUnit, object)[0] },

                // No content units on blackboard
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, outputPool), new ContentUnit[0],
                    new U_PrologEvalRequest[] { prologReq1 }, prologKB, new ContentUnit[0], new string[] { "dissed(character1, me)." },
                    new (ContentUnit, bool)[0],
                    new (ContentUnit, object)[0] },

                // Specifying input pool, some queries with bindings, some without, assertion to make queries true.
                new object[] { blackboard, new KS_ScheduledPrologEval(blackboard, inputPool, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_PrologEvalRequest[] { prologReq1 }, prologKB, new ContentUnit[] { cu1, cu2 }, new string[] { "dissed(character1, me)." },
                    new (ContentUnit, bool)[] { (cu1, true), (cu2, true) },
                    new (ContentUnit, object)[] { (cu1, new LogicVariable("V1")), (cu2, Symbol.Intern("character1")) } },

            };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledPrologEval (once it has been defined)
        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledPrologEval))]
        public void TestExecute_ScheduledPrologEval(IBlackboard blackboard, KS_ScheduledPrologEval prologEval, ContentUnit[] unitsToAdd,
            U_PrologEvalRequest[] reqsToAdd, U_PrologKB prologKB, ContentUnit[] evaluatedUnits, string[] assertionsToAdd, (ContentUnit cu, bool result)[] evalResults,
            (ContentUnit cu, object binding)[] bindings)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var cu in unitsToAdd)
            {
                blackboard.AddUnit(cu);
            }

            blackboard.AddUnit(prologKB);

            if (assertionsToAdd.Length > 0)
            {
                foreach (string assertion in assertionsToAdd)
                {
                    output.WriteLine("Adding assertion: " + assertion);
                    prologKB.Assert(assertion);
                }
            }

            // Add the requests to the blackboad
            foreach (var req in reqsToAdd)
            {
                blackboard.AddUnit(req);
            }

#if UNIT_TEST
            prologEval.XunitOutput = output;
#endif

            // Executed the filter selector
            prologEval.Execute();

            // Remove any assertions that were added for this test case
            if (assertionsToAdd.Length > 0)
            {
                foreach (string assertion in assertionsToAdd)
                {
                    output.WriteLine("Removing assertion: " + assertion);
                    prologKB.Retract(assertion);
                }
            }

            string outputPool = prologEval.OutputPool;

            // Iterate through each of the units which should have been evaluated and see if there's a copy of them in the output pool.
            // Also test that the evaluation results and bindings are correct for each evaluated unit.
            foreach (var contentUnit in evaluatedUnits)
            {
                output.WriteLine("Testing results and bindings for ContentUnitID: " + contentUnit.Metadata[ContentUnitID]);

                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(contentUnit);
                TestFilterLinks(s, outputPool);
                if (evalResults.Length > 0)
                {
                    (ContentUnit cu, bool result) =
                        Array.Find(evalResults, resultTuple => resultTuple.cu == contentUnit);
                    Assert.NotNull(cu);
                    (IUnit resultUnit, _, _) = s.First();
                    ContentUnit resultCU = resultUnit as ContentUnit;
                    Assert.True(resultCU.HasMetadataSlot(ApplTestResult), "ContentUnit missing ApplTestResult");
                    Assert.True(resultCU.Metadata[ApplTestResult].Equals(result),
                        "Prolog evaluation result not correct: " + resultCU.Metadata[ApplTestResult] + " != " + result);
                }

                if (bindings.Length > 0)
                {
                    (ContentUnit cu, object binding) =
                        Array.Find(bindings, bindingTuple => bindingTuple.cu == contentUnit);
                    Assert.NotNull(cu);
                    (IUnit resultUnit, _, _) = s.First();
                    ContentUnit resultCU = resultUnit as ContentUnit;
                    Assert.True(resultCU.HasMetadataSlot(ApplTestBindings_Prolog), "ContentUnit missing ApplTestBindings_Prolog");
                    var checkBinding = Term.Unify(resultCU.Metadata[ApplTestBindings_Prolog], binding);
                    output.WriteLine("checkBinding.Next(): " + checkBinding.GetEnumerator().MoveNext());
                    Assert.True(checkBinding.GetEnumerator().MoveNext(),
                        "Variable binding not correct: " + resultCU.Metadata[ApplTestBindings_Prolog] + " != " + binding);
                }
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as evaluatedUnits
            TestNumberOfCUsInOutputPool(evaluatedUnits.Length, blackboard, outputPool);


            // Grab all of the reqs on the blackboard and verify that there are none (should have all been deleted).
            ISet<U_PrologEvalRequest> reqs = blackboard.LookupUnits<U_PrologEvalRequest>();
            Assert.False(reqs.Any());
        }

        // fixme: add test for KS_ScheduledExecute

        // fixme: change this to test KnowledgeComponent-based ScheduledTierSelector
        /* 
         * Data and test methods for KS_ScheduledTierSelector
         */
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

            // fixme: commented out until I finish making the change 
            /*
            units[0].Metadata[Specificity] = 5;
            units[0].Metadata[ContentUnitID] = "ID0";

            units[1].Metadata[Specificity] = 3;
            units[1].Metadata[ContentUnitID] = "ID1";

            units[2].Metadata[Specificity] = 5;
            units[2].Metadata[ContentUnitID] = "ID2";

            units[3].Metadata[Specificity] = 3;
            units[3].Metadata[ContentUnitID] = "ID3";

            units[4].Metadata[Specificity] = 6;
            units[4].Metadata[ContentUnitID] = "ID4";

            units[5].Metadata[Specificity] = "xylophone";
            units[6].Metadata[Specificity] = "carburator";
            units[7].Metadata[Specificity] = "xylophone";
            units[8].Metadata[Specificity] = "carburator";
            units[9].Metadata[Specificity] = "Zebra";

            units[10].Metadata[Specificity] = 5.3;
            units[11].Metadata[Specificity] = 3.1;
            units[12].Metadata[Specificity] = 5.3;
            units[13].Metadata[Specificity] = 3.1;
            units[14].Metadata[Specificity] = 6.7;
            */
            /* Structure of object[]: 
            * IBlackboard: blackboard, 
            * KS_ScheduledTierSelector: the knowledge source to test            
            * ContentUnit[]: array of CUs to add to the blackboard
            * ContentUnit[]: the content units that should be in the output pool  
            */

            return new List<object[]>
            {
                // fixme: commented out until I finish making the change
                /*
                // No input pool, default output pool, integer specificity
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, Specificity),
                    units.Take(5), new ContentUnit[] { units[4] } }, 

                // No input pool, specified output pool, integer specificy
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, "output1", Specificity),
                    units.Take(5), new ContentUnit[] { units[4] } }, 

                // Input pool, specified output pool, integer specificity
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, pool1, "output1", Specificity),
                    units.Take(5), new ContentUnit[] { units[0], units[2] } },

                // No input pool, specified output pool, filter condition, integer specificity
                new object[] { blackboard,
                    new KS_KC_ScheduledTierSelector(blackboard, "output1", (cu) => ((IComparable)cu.Metadata[Specificity]).CompareTo(6) < 0, Specificity),
                    units.Take(5), new ContentUnit[] { units[0], units[2] } },
                              
                // Input pool, specified output pool, filter condition, integer specificity
                new object[] { blackboard,
                    new KS_KC_ScheduledTierSelector(blackboard, pool1, "output1", (cu) => ((IComparable)cu.Metadata[Specificity]).CompareTo(5) < 0, Specificity),
                    units.Take(5), new ContentUnit[] { units[1] } },

                // No input pool, default output pool, string specificity
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, Specificity),
                    units.Skip(5).Take(5), new ContentUnit[] { units[9] } }, 

                // No input pool, default output pool, float specificity
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, Specificity),
                    units.Skip(10), new ContentUnit[] { units[14] } }, 

                // Empty blackboard
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, Specificity),
                    new ContentUnit[0], new ContentUnit[0] }, 

                // Empty pool
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, pool1, "output1", Specificity),
                    units.Skip(3).Take(2), new ContentUnit[0] },

                // Non-existant tier slot
                new object[] { blackboard, new KS_KC_ScheduledTierSelector(blackboard, "ThisSlotDoesNotExist"),
                    units.Take(5), new ContentUnit[0] },
                    */
             };
        }

        // fixme: change this to test KnowledgeComponent-based ScheduledTierSelector
        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledTierSelector))]
        public void TestExecute_ScheduledTierSelector(IBlackboard blackboard, KS_ScheduledTierSelector tierSelector,
            Unit[] unitsToAdd, Unit[] filteredUnits)
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

                output.WriteLine("Original ContentUnit");
                output.WriteLine(unit.ToString());

                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            TestNumberOfUnitsInOutputPool(filteredUnits.Length, blackboard, outputPool);
        }

        public TestScheduledKnowledgeSources(ITestOutputHelper output)
        {
            this.output = output;
        }
    }
}
