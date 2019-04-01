using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;
using static CSA.KnowledgeUnits.KUProps;
using CSA.Core;
using Prolog;
using UnityEngine;

namespace CSA.Tests
{
    public class TestKnowledgeSources
    {

        private readonly ITestOutputHelper output;

        class TestUnit1 : Unit
        {
            public string S { get; set; }

            public TestUnit1(string init)
            {
                S = init;
            }
        }

        public static IEnumerable<object[]> Data_TestPrecondition()
        {
            IBlackboard blackboard = new Blackboard();
            ContentUnit selectedCU = new ContentUnit();
            selectedCU.Metadata[SelectedContentUnit] = null;

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KnowledgeSource: KS whose precondition to test, 
             * IUnit[]: array of KUs to add, 
             * bool: whether KUs should be marked as previously matched by this KS, 
             * int: number of activated KSs precondition should return,
             */

            return new List<object[]>
            {
                // KS_IDSelector, empty blackboard
                new object[] { blackboard, new KS_ReactiveIDSelector(blackboard), new IUnit[] { }, false, 0 }, 

                // KS_IDSelector, non-matching unit
                new object[] { blackboard, new KS_ReactiveIDSelector(blackboard), new IUnit[] { new TestUnit1("foo") }, false, 0 }, 

                // KS_IDSelector, matching unit, previously matched
                new object[] { blackboard, new KS_ReactiveIDSelector(blackboard), new IUnit[] { new U_IDSelectRequest("foo") }, true, 0 }, 

                // KS_IDSelector, matching unit, not previously matched
                new object[] { blackboard, new KS_ReactiveIDSelector(blackboard), new IUnit[] { new U_IDSelectRequest("foo") }, false, 1 }, 

                // KS_IDSelector, multiple matching units, not previously matched
                new object[]
                {
                    blackboard, new KS_ReactiveIDSelector(blackboard), new IUnit[]
                    {
                        new U_IDSelectRequest("foo"),
                        new U_IDSelectRequest("bar"),
                        new U_IDSelectRequest("baz")
                    },
                    false, 3
                },

                // ChoicePresenter, empty blackboard
                new object[] { blackboard, new KS_ReactiveChoicePresenter(blackboard), new IUnit[] { }, false, 0 }, 

                // ConsoleChoiceSelector, non-matching unit
                new object[] { blackboard, new KS_ReactiveChoicePresenter(blackboard), new IUnit[] { new TestUnit1("foo") }, false, 0 }, 

                // ChoicePresenter, matching unit, previously matched
                new object[] { blackboard, new KS_ReactiveChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) }, true, 0 }, 

                // ChoicePresenter, matching unit, not previously matched
                new object[] { blackboard, new KS_ReactiveChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) }, false, 1}, 

                // ChoicePresenter, multiple matching units, not previously matched
                new object[]
                {
                    blackboard, new KS_ReactiveChoicePresenter(blackboard), new IUnit[]
                    {
                        new ContentUnit(selectedCU),
                        new ContentUnit(selectedCU),
                        new ContentUnit(selectedCU)
                    },
                    false, 3
                },
            };
        }

        [Theory]
        [MemberData(nameof(Data_TestPrecondition))]
        public void TestPrecondition(IBlackboard blackboard, ReactiveKnowledgeSource ks, IUnit[] unitsToAdd, bool previouslyMatched, int numActivatedKSs)
        {
            blackboard.Clear(); // Clear the blackboard so that there aren't KUs laying around from previous tests. 

            // Add the units in unitsToAdd
            foreach (IUnit unitToAdd in unitsToAdd)
            {
                blackboard.AddUnit(unitToAdd);
                // If they should be marked as previously matched, set KSPreconditionMatched
                if (previouslyMatched)
                {
                    unitToAdd.Properties[KSPreconditionMatched] = new HashSet<ReactiveKnowledgeSource> { ks };
                }
            }

            // Call KnowledgeSource.Precondition() to get the activated KSs.
            IEnumerable<IKnowledgeSourceActivation> KSAs = ks.Precondition();

            // Check that the number of activated KSs equals the number we're expecting
            int count = KSAs.Count();
            Assert.Equal(numActivatedKSs, count);

            /*
             * fixme: need to remove this because we're no longer storing matches on the matching units. Instead store match sets on the KnowledgeSources.
             * For now I've created a separate test for Selector Knowledge Sources (which already implement match set storage) until we get this 
             * resolved for all KnowledgeSources. 
             * 
             * If there were activated KSs, check that the KUs were marked as having been matched.  
             */
            if (count > 0)
            {
                foreach (IUnit u in unitsToAdd)
                {
                    Assert.True(u.Properties.ContainsKey(KSPreconditionMatched));
                    bool containsKS = ((HashSet<ReactiveKnowledgeSource>)u.Properties[KSPreconditionMatched]).Contains(ks);
                    Assert.True(containsKS);
                }
            }

            // Run the preconditions again to verify that on a second running they don't activate any KSs.
            KSAs = ks.Precondition();
            count = KSAs.Count();
            Assert.Equal(0, count);
        }

        // fixme: TestSelectorPrecondition not passing. comment out for now and consider deleting if it doesn't make sense with the switch to ScheduledKnowledgeSources.
        //public static IEnumerable<object[]> Data_TestSelectorPrecondition()
        //{
        //    string inputPoolName = "inputPool";
        //    string outputPoolName = "outputPool";
        //    IBlackboard blackboard = new Blackboard();

        //    ContentUnit cuInputPool = new ContentUnit();
        //    cuInputPool.Metadata[ContentPool] = inputPoolName;

        //    ContentUnit cuDifferentPool = new ContentUnit();
        //    cuDifferentPool.Metadata[ContentPool] = "differentPool";

        //    ContentUnit cuNoPool = new ContentUnit();

        //    /* Structure of object[]: 
        //     * IBlackboard: blackboard, 
        //     * KnowledgeSource: KS whose precondition to test, 
        //     * IUnit[]: array of KUs to add, 
        //     * bool: whether there should be an activation            
        //     */

        //    return new List<object[]>
        //    {
        //        // Empty blackboard
        //        new object[] {blackboard, new KS_ScheduledFilterSelector(blackboard, inputPoolName, outputPoolName), new IUnit[] { }, false },

        //        // Blackboard with content unit not in a pool 
        //        new object[] {blackboard, new KS_ScheduledFilterSelector(blackboard, inputPoolName, outputPoolName), new IUnit[] { cuNoPool }, false },

        //        // Blackboard with content unit in a different pool
        //        new object[] {blackboard, new KS_ScheduledFilterSelector(blackboard, inputPoolName, outputPoolName), new IUnit[] { cuDifferentPool }, false },

        //        // Blackboard with content unit in input pool
        //        new object[] {blackboard, new KS_ScheduledFilterSelector(blackboard, inputPoolName, outputPoolName), new IUnit[] { cuInputPool }, true },

        //    };
        //}

        //[Theory]
        //[MemberData(nameof(Data_TestSelectorPrecondition))]
        //public void TestSelectorPrecondition(IBlackboard blackboard, ReactiveKnowledgeSource ks, IUnit[] unitsToAdd, bool activated)
        //{
        //    // Clear the blackboard so there aren't any KUs lying around. 
        //    blackboard.Clear();

        //    // Add the units in unitsToAdd
        //    foreach (IUnit unitToAdd in unitsToAdd)
        //    {
        //        blackboard.AddUnit(unitToAdd);
        //    }

        //    // Call KnowledgeSource.Precondition() to get the activated KSs.
        //    IEnumerable<IKnowledgeSourceActivation> KSAs = ks.Precondition();


        //    // Check that the number of activated KSs equals the number we're expecting
        //    int count = KSAs.Count();
        //    if (activated)
        //    {
        //        Assert.Equal(1, count);
        //    }
        //    else
        //    {
        //        Assert.Equal(0, count);
        //    }

        //    // Run the preconditions again to verify that on a second running they don't activate any KSs.
        //    KSAs = ks.Precondition();
        //    count = KSAs.Count();
        //    Assert.Equal(0, count);
        //}


        public static IEnumerable<object[]> Data_TestObviationCondition()
        {
            IBlackboard blackboard = new Blackboard();
            ContentUnit selectedCU = new ContentUnit();
            selectedCU.Metadata[SelectedContentUnit] = null;

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KnowledgeSource: KS whose obviation condition to test, 
             * IUnit[]: array of KUs to add, 
             *             
             */

            return new List<object[]>
            {
                // KS_IDSelector, one matching unit
                new object[] { blackboard, new KS_ReactiveIDSelector(blackboard), new IUnit[] { new U_IDSelectRequest("foo") } }, 

                // KS_IDSelector, multiple matching units
                new object[]
                {
                    blackboard, new KS_ReactiveIDSelector(blackboard), new IUnit[]
                    {
                        new U_IDSelectRequest("foo"),
                        new U_IDSelectRequest("bar"),
                        new U_IDSelectRequest("baz")
                    }
                 },

                // ChoicePresenter, one matching unit
                new object[] { blackboard, new KS_ReactiveChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) } }, 

                // ChoicePresenter, multiple matching units
                new object[]
                {
                    blackboard, new KS_ReactiveChoicePresenter(blackboard), new IUnit[]
                    {
                        new ContentUnit(selectedCU),
                        new ContentUnit(selectedCU),
                        new ContentUnit(selectedCU)
                    }
                },
            };
        }

        [Theory]
        [MemberData(nameof(Data_TestObviationCondition))]
        public void TestObviationCondition(IBlackboard blackboard, ReactiveKnowledgeSource ks, IUnit[] unitsToAdd)
        {
            blackboard.Clear(); // Clear the blackboard so that there aren't KUs laying around from previous tests. 

            // Add the units in unitsToAdd
            foreach (IUnit unitToAdd in unitsToAdd)
            {
                blackboard.AddUnit(unitToAdd);
            }

            // Call KnowledgeSource.Precondition() to get the activated KSs.
            IEnumerable<IKnowledgeSourceActivation> KSAs = ks.Precondition();

            // If there are any activated KSs...   
            if (KSAs.Any())
            {
                // First, the obviation condition should evaluate to false since the matching KUs are still on the blackboard.
                foreach (IKnowledgeSourceActivation KSA in KSAs)
                {
                    Assert.False(KSA.EvaluateObviationCondition());
                }

                // Second, remove the units from the blackboard
                foreach (IUnit unitToRemove in unitsToAdd)
                {
                    blackboard.RemoveUnit(unitToRemove);
                }

                // Finally, the obviation condition should now evaluate to true since the matching KUs are no longer on the blackboard. 
                foreach (IKnowledgeSourceActivation KSA in KSAs)
                {
                    Assert.True(KSA.EvaluateObviationCondition());
                }
            }
        }

        // fixme: Eventually see if testing of KnoledgeSource.Execute() can be turned into a theory. 
        [Fact]
        public void TestExecute_KS_IDSelector_SelectedUnit()
        {
            IBlackboard blackboard = new Blackboard();
            KS_ReactiveIDSelector ks = new KS_ReactiveIDSelector(blackboard);
            List<U_IDSelectRequest> kuList = new List<U_IDSelectRequest>
            {
                new U_IDSelectRequest("foo"),
             };

            foreach (IUnit u in kuList)
            {
                blackboard.AddUnit(u);
            }

            List<ContentUnit> cuList = new List<ContentUnit>
            {
                new ContentUnit(),
                new ContentUnit(),
                new ContentUnit()
            };
            cuList[0].Metadata[ContentUnitID] = "foo";
            cuList[1].Metadata[ContentUnitID] = "bar";
            cuList[2].Metadata[ContentUnitID] = "baz";

            foreach (IUnit u in cuList)
            {
                blackboard.AddUnit(u);
            }

            var KSAs = ks.Precondition();
            int count = KSAs.Count();
            Assert.Equal(1, count);
            KSAs.ElementAt(0).Execute();

            // Four content units total (the original three plus a new selected one)
            ISet<ContentUnit> cuSet = blackboard.LookupUnits<ContentUnit>();
            Assert.Equal(4, cuSet.Count);

            // Query for selected content units
            var selectedList = from cu in blackboard.LookupUnits<ContentUnit>()
                               where cu.HasMetadataSlot(SelectedContentUnit)
                               select cu;

            // One content unit has been selected.
            int size = selectedList.Count();
            Assert.Equal(1, size);

            // The right content unit has been selected. 
            Assert.Equal("foo", selectedList.ElementAt(0).Metadata[ContentUnitID]);

            // The query has been deleted. 
            ISet<U_IDSelectRequest> querySet = blackboard.LookupUnits<U_IDSelectRequest>();
            Assert.Equal(0, querySet.Count);


        }

        [Fact]
        public void TestExecute_KS_IDSelector_NoSelectedUnit()
        {
            IBlackboard blackboard = new Blackboard();
            KS_ReactiveIDSelector ks = new KS_ReactiveIDSelector(blackboard);
            List<U_IDSelectRequest> kuList = new List<U_IDSelectRequest>
            {
                new U_IDSelectRequest("qux"),
             };

            foreach (IUnit u in kuList)
            {
                blackboard.AddUnit(u);
            }

            List<ContentUnit> cuList = new List<ContentUnit>
            {
                new ContentUnit(),
                new ContentUnit(),
                new ContentUnit()
            };
            cuList[0].Metadata[ContentUnitID] = "foo";
            cuList[1].Metadata[ContentUnitID] = "bar";
            cuList[2].Metadata[ContentUnitID] = "baz";

            foreach (IUnit u in cuList)
            {
                blackboard.AddUnit(u);
            }

            var KSAs = ks.Precondition();
            int count = KSAs.Count();
            Assert.Equal(1, count);
            KSAs.ElementAt(0).Execute();

            // Three content units total (no selected unit)
            ISet<ContentUnit> cuSet = blackboard.LookupUnits<ContentUnit>();
            Assert.Equal(3, cuSet.Count);

            // Query for selected content units
            var selectedList = from cu in blackboard.LookupUnits<ContentUnit>()
                               where cu.HasMetadataSlot(SelectedContentUnit)
                               select cu;

            // No content unit selected (since "qux" matches no ID.
            int size = selectedList.Count();
            Assert.Equal(0, size);

            // The query has been deleted. 
            ISet<U_IDSelectRequest> querySet = blackboard.LookupUnits<U_IDSelectRequest>();
            Assert.Equal(0, querySet.Count);
        }

        public static IEnumerable<object[]> Data_TestExecute_ReactiveChoicePresenter()
        {
            IBlackboard blackboard = new Blackboard();

            ContentUnit originalCU = new ContentUnit();
            originalCU.Metadata[ContentUnitID] = "foo";
            originalCU.Content[Text] = "Here is a node with choices";

            ContentUnit selectedCU = new ContentUnit(originalCU);
            selectedCU.Metadata[SelectedContentUnit] = null;

            ContentUnit choice1 = new ContentUnit();
            choice1.Metadata[TargetContentUnitID] = "bar";
            choice1.Content[Text] = "Choice 1";

            ContentUnit choice2 = new ContentUnit();
            choice2.Metadata[TargetContentUnitID] = "baz";
            choice2.Content[Text] = "Choice 2";

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * ContentUnit: the selected CU,
             * ContentUnit: the original CU (selected CU is an copy of this),
             * ContentUnit[]: array of choices (ContentUnits) 
             */

            return new List<object[]>
            {
                // Selected and original CU, no choices
                new object[] { blackboard, selectedCU, originalCU,  new ContentUnit[] { } }, 

                // Selected and original CU, one choice
                new object[] { blackboard, selectedCU, originalCU, new ContentUnit[] { choice1 } },

                // Selected and original CU, two choices
                new object[] { blackboard, selectedCU, originalCU, new ContentUnit[] { choice1, choice2} },
             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ReactiveChoicePresenter))]
        public void TestExecute_ReactiveChoicePresenter(IBlackboard blackboard, ContentUnit selectedCU, ContentUnit originalCU, ContentUnit[] choices)
        {
            blackboard.Clear();
            blackboard.AddUnit(selectedCU);
            blackboard.AddUnit(originalCU);
            blackboard.AddLink(originalCU, selectedCU, LinkTypes.L_SelectedContentUnit);

            foreach (ContentUnit choice in choices)
            {
                blackboard.AddUnit(choice);
                blackboard.AddLink(originalCU, choice, LinkTypes.L_Choice);
            }

            KS_ReactiveChoicePresenter ks = new KS_ReactiveChoicePresenter(blackboard);
            ks.PresenterExecute += GenerateEventHandler(selectedCU, choices, blackboard);

            var KSAs = ks.Precondition();

            int count = KSAs.Count();
            Assert.Equal(1, count);

            // Execute the activated choice presenter
            KSAs.ElementAt(0).Execute();
        }

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

        static bool TestFilter(ContentUnit cu)
        {
            return cu.HasMetadataSlot("Test") && (int)cu.Metadata["Test"] == 1 ? true : false;
        }

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

        static private void TestNumberOfCUsInOutputPool(int desiredNumberOfCUs, IBlackboard blackboard, string outputPool)
        {
            var CUs = from cu in blackboard.LookupUnits<ContentUnit>()
                      where cu.HasMetadataSlot(ContentPool)
                      where cu.Metadata[ContentPool].Equals(outputPool)
                      select cu;

            Assert.Equal(desiredNumberOfCUs, CUs.Count());
        }

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

        /* 
         * Data and test methods for KS_ScheduledTierSelector
         */
        public static IEnumerable<object[]> Data_TestExecute_ScheduledTierSelector()
        {
            string pool1 = "pool1";

            IBlackboard blackboard = new Blackboard();

            ContentUnit[] CUs = new ContentUnit[15];

            for (int i = 0; i < CUs.Length; i++)
            {
                CUs[i] = new ContentUnit();
            }

            CUs[0].Metadata[ContentPool] = pool1;
            CUs[1].Metadata[ContentPool] = pool1;
            CUs[2].Metadata[ContentPool] = pool1;

            CUs[0].Metadata[Specificity] = 5;
            CUs[0].Metadata[ContentUnitID] = "ID0";

            CUs[1].Metadata[Specificity] = 3;
            CUs[1].Metadata[ContentUnitID] = "ID1";

            CUs[2].Metadata[Specificity] = 5;
            CUs[2].Metadata[ContentUnitID] = "ID2";

            CUs[3].Metadata[Specificity] = 3;
            CUs[3].Metadata[ContentUnitID] = "ID3";

            CUs[4].Metadata[Specificity] = 6;
            CUs[4].Metadata[ContentUnitID] = "ID4";

            CUs[5].Metadata[Specificity] = "xylophone";
            CUs[6].Metadata[Specificity] = "carburator";
            CUs[7].Metadata[Specificity] = "xylophone";
            CUs[8].Metadata[Specificity] = "carburator";
            CUs[9].Metadata[Specificity] = "Zebra";

            CUs[10].Metadata[Specificity] = 5.3;
            CUs[11].Metadata[Specificity] = 3.1;
            CUs[12].Metadata[Specificity] = 5.3;
            CUs[13].Metadata[Specificity] = 3.1;
            CUs[14].Metadata[Specificity] = 6.7;

            /* Structure of object[]: 
            * IBlackboard: blackboard, 
            * KS_ScheduledTierSelector: the knowledge source to test            
            * ContentUnit[]: array of CUs to add to the blackboard
            * ContentUnit[]: the content units that should be in the output pool  
            */

            return new List<object[]>
            {
                // No input pool, default output pool, integer specificity
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, Specificity),
                    CUs.Take(5), new ContentUnit[] { CUs[4] } }, 

                // No input pool, specified output pool, integer specificy
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, "output1", Specificity),
                    CUs.Take(5), new ContentUnit[] { CUs[4] } }, 

                // Input pool, specified output pool, integer specificity
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, pool1, "output1", Specificity),
                    CUs.Take(5), new ContentUnit[] { CUs[0], CUs[2] } },

                // No input pool, specified output pool, filter condition, integer specificity
                new object[] { blackboard,
                    new KS_ScheduledTierSelector(blackboard, "output1", (cu) => ((IComparable)cu.Metadata[Specificity]).CompareTo(6) < 0, Specificity),
                    CUs.Take(5), new ContentUnit[] { CUs[0], CUs[2] } },
                              
                // Input pool, specified output pool, filter condition, integer specificity
                new object[] { blackboard,
                    new KS_ScheduledTierSelector(blackboard, pool1, "output1", (cu) => ((IComparable)cu.Metadata[Specificity]).CompareTo(5) < 0, Specificity),
                    CUs.Take(5), new ContentUnit[] { CUs[1] } },

                // No input pool, default output pool, string specificity
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, Specificity),
                    CUs.Skip(5).Take(5), new ContentUnit[] { CUs[9] } }, 

                // No input pool, default output pool, float specificity
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, Specificity),
                    CUs.Skip(10), new ContentUnit[] { CUs[14] } }, 

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, Specificity),
                    new ContentUnit[0], new ContentUnit[0] }, 

                // Empty pool
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, pool1, "output1", Specificity),
                    CUs.Skip(3).Take(2), new ContentUnit[0] },

                // Non-existant tier slot
                new object[] { blackboard, new KS_ScheduledTierSelector(blackboard, "ThisSlotDoesNotExist"),
                    CUs.Take(5), new ContentUnit[0] },
             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledTierSelector))]
        public void TestExecute_ScheduledTierSelector(IBlackboard blackboard, KS_ScheduledTierSelector tierSelector,
            ContentUnit[] unitsToAdd, ContentUnit[] filteredUnits)
        {
            // Clear the blackboard of any previous testing state
            blackboard.Clear();

            // Add the content units to the blackboard
            foreach (var cu in unitsToAdd)
            {
                blackboard.AddUnit(cu);
            }

            // Execute the tier selector
            tierSelector.Execute();

            string outputPool = tierSelector.OutputPool;

            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var cu in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(cu);

                output.WriteLine("Original ContentUnit");
                output.WriteLine(cu.ToString());

                TestFilterLinks(s, outputPool);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            TestNumberOfCUsInOutputPool(filteredUnits.Length, blackboard, outputPool);
        }



        public TestKnowledgeSources(ITestOutputHelper output)
        {
            this.output = output;
        }

    }
}
