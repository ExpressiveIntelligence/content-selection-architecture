﻿using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;
using static CSA.KnowledgeUnits.KUProps;
using Xunit;
using CSA.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSA.Tests
{
    public class TestKnowledgeSources
    {
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
            ISet<IUnit> cuSet = blackboard.LookupUnits(ContentUnit.TypeName);
            Assert.Equal(4, cuSet.Count);

            // Query for selected content units
            var selectedList = from cu in blackboard.LookupUnits(ContentUnit.TypeName)
                               where ((ContentUnit)cu).HasMetadataSlot(SelectedContentUnit)
                               select cu;

            // One content unit has been selected.
            int size = selectedList.Count();
            Assert.Equal(1, size);

            // The right content unit has been selected. 
            Assert.Equal("foo", ((ContentUnit)selectedList.ElementAt(0)).Metadata[ContentUnitID]);

            // The query has been deleted. 
            ISet<IUnit> querySet = blackboard.LookupUnits(U_IDSelectRequest.TypeName);
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
            ISet<IUnit> cuSet = blackboard.LookupUnits(ContentUnit.TypeName);
            Assert.Equal(3, cuSet.Count);

            // Query for selected content units
            var selectedList = from cu in blackboard.LookupUnits(ContentUnit.TypeName)
                               where ((ContentUnit)cu).HasMetadataSlot(SelectedContentUnit)
                               select cu;

            // No content unit selected (since "qux" matches no ID.
            int size = selectedList.Count();
            Assert.Equal(0, size);

            // The query has been deleted. 
            ISet<IUnit> querySet = blackboard.LookupUnits(U_IDSelectRequest.TypeName);
            Assert.Equal(0, querySet.Count);
        }

        public static IEnumerable<object[]> Data_TestExecute_PublicChoicePresenter()
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
        [MemberData(nameof(Data_TestExecute_PublicChoicePresenter))]
        public void TestExecute_PublicChoicePresenter(IBlackboard blackboard, ContentUnit selectedCU, ContentUnit originalCU, ContentUnit[] choices)
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
            var KSAs = ks.Precondition();

            int count = KSAs.Count();
            Assert.Equal(1, count);

            // Execute the activated choice presenter
            KSAs.ElementAt(0).Execute();

            // fixme: the tests below depend on the choice info being stored on the ks. When this is eventually changed to arguments passed into the event handler, will need to use an event 
            // handler to make these tests. 
            Assert.Equal(selectedCU.Content[Text], ks.TextToDisplay);

            int numOfChoices = choices.Length;
            Assert.Equal(numOfChoices, ks.ChoicesToDisplay.Length);

            foreach (ContentUnit choice in choices)
            {
                Assert.True(Array.Exists(ks.ChoicesToDisplay, element => element.Equals(choice.Content[Text])));
            }

            // fixme: add a test for KS_ChoicePresenter.SelectChoice()

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
             * string: name of the output pool            
             */

            return new List<object[]>
            {
                // Filter with default filter condition
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, outputPool }, 

                // Filter with input pool and output pool using input pool selection
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] {cu1, cu2}, outputPool }, 

                // Filter with output pool and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool, TestFilter), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu2, cu3 }, outputPool },

                // Filter with input and output pools and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool, TestFilter), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu2 }, outputPool },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, outputPool), new ContentUnit[0], new ContentUnit[0], outputPool},

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu3, cu4 },
                    new ContentUnit[0], outputPool },

                // Nothing in the input pool and specified filter
                new object[] { blackboard, new KS_ScheduledFilterSelector(blackboard, inputPool, outputPool, TestFilter), new ContentUnit[] { cu3, cu4 },
                    new ContentUnit[0], outputPool },

             };
        }

        static bool TestFilter(ContentUnit cu)
        {
            return cu.HasMetadataSlot("Test") && (int)cu.Metadata["Test"] == 1 ? true : false;
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledFilterSelector))]
        public void TestExecute_ScheduledFilterSelector(IBlackboard blackboard, KS_ScheduledFilterSelector filterSelector, ContentUnit[] unitsToAdd,
            ContentUnit[] filteredUnits, string outputPool)
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


            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var cu in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(cu);
                int count = s.Count();
                Assert.Equal(1, count);
                ContentUnit cuCopy = s.First().Item1 as ContentUnit;
                Assert.True(cuCopy.HasMetadataSlot(ContentPool));
                Assert.Equal(outputPool, cuCopy.Metadata[ContentPool]);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            var CUs = from cu in blackboard.LookupUnits(ContentUnit.TypeName)
                      let cuCast = cu as ContentUnit
                      where cuCast.HasMetadataSlot(ContentPool)
                      where cuCast.Metadata[ContentPool].Equals(outputPool)
                      select cuCast;

            Assert.Equal(filteredUnits.Length, CUs.Count());

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
             * string: name of the output pool            
             */

            return new List<object[]>
            {
                // Filter with default filter condition
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[] { cu1, cu3, }, outputPool }, 

                // Filter with input pool and output pool using input pool selection
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[] {cu1 }, outputPool }, 

                // Filter with output pool and specified filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool, TestFilter), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[] { cu3 }, outputPool },

                // Filter with input and output pools and specified filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool, TestFilter), 
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new U_IDSelectRequest[] { idReq1 }, new ContentUnit[0], outputPool },

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, outputPool), new ContentUnit[0], new U_IDSelectRequest[0], 
                    new ContentUnit[0], outputPool},

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool), new ContentUnit[] { cu3, cu4 },
                    new U_IDSelectRequest[] { idReq1 }, new ContentUnit[0], outputPool },

                // Multiple U_IDRequests
                new object[] { blackboard, new KS_ScheduledIDSelector(blackboard, inputPool, outputPool, TestFilter), 
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new U_IDSelectRequest[] { idReq1, idReq2, idReq3}, new ContentUnit[] { cu2 }, outputPool },

             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledIDSelector))]
        public void TestExecute_ScheduledIDSelector(IBlackboard blackboard, KS_ScheduledFilterSelector filterSelector, ContentUnit[] unitsToAdd,
            U_IDSelectRequest[] reqsToAdd, ContentUnit[] filteredUnits, string outputPool)
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

            // Iterate through each of the units which should have passed the filter and see if there's a copy of them in the output pool.
            foreach (var cu in filteredUnits)
            {
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(cu);
                int count = s.Count();
                Assert.Equal(1, count);
                ContentUnit cuCopy = s.First().Item1 as ContentUnit;
                Assert.True(cuCopy.HasMetadataSlot(ContentPool));
                Assert.Equal(outputPool, cuCopy.Metadata[ContentPool]);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as filteredUnits
            var CUs = from cu in blackboard.LookupUnits(ContentUnit.TypeName)
                      let cuCast = cu as ContentUnit
                      where cuCast.HasMetadataSlot(ContentPool)
                      where cuCast.Metadata[ContentPool].Equals(outputPool)
                      select cuCast;

            Assert.Equal(filteredUnits.Length, CUs.Count());

            // Grab all of the reqs on the blackboard and verify that there are none (should have all been deleted).
            ISet<IUnit> reqs = blackboard.LookupUnits(U_IDSelectRequest.TypeName);
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
            * string: name of the output pool     
            * int: the number of CUs to select           
            * int: the seed for the random number generator
            */

            return new List<object[]>
            {
                // Filter with default filter condition and output pool using seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, seed), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] {cu1, cu2, cu3, cu4 }, KS_ScheduledUniformDistributionSelector.DefaultOutputPoolName, 1, seed }, 

                // Filter with specified output pool, using seed of 1, requesting 1 CU
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, outputPool, seed), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, outputPool, 1, seed }, 

                // Filter with specified input and output pools, specified number to select and seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, 2, seed), new ContentUnit[] { cu1, cu2, cu3, cu4 },
                    new ContentUnit[] { cu1, cu2, cu3 }, outputPool, 2, seed },

                // Filter with specified input and output pools, specified filter, specified number to select and seed of 1
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, TestFilter, 1, seed),
                    new ContentUnit[] { cu1, cu2, cu3, cu4 }, new ContentUnit[] { cu2, cu3 }, outputPool, 1, seed},

                // Empty blackboard
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, 5), new ContentUnit[0],
                    new ContentUnit[0], outputPool, 5, seed},

                // Nothing in the input pool and no filter
                new object[] { blackboard, new KS_ScheduledUniformDistributionSelector(blackboard, inputPool, outputPool, 1, 1), 
                    new ContentUnit[] { cu4 }, new ContentUnit[0], outputPool, 1, seed },

             };
        }

        [Theory]
        [MemberData(nameof(Data_TestExecute_ScheduledUniformDistributionSelector))]
        public void TestExecute_ScheduledUniformDistributionSelector(IBlackboard blackboard, KS_ScheduledFilterSelector filterSelector, 
            ContentUnit[] unitsToAdd, ContentUnit[] unitsToSelectFrom, string outputPool, int numberToSelect, int seed)
        {
            Random random = new Random(seed);
             
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

            for(int i = 0; i < Math.Min(numberToSelect, unitsToSelectFrom.Length); i++)
            {
                int r = i + random.Next(unitsToSelectFrom.Length - i);
                ISet<(IUnit, string, LinkDirection)> s = blackboard.LookupLinks(unitsToSelectFrom[r]);
                int count = s.Count();
                Assert.Equal(1, count);
                ContentUnit cuCopy = s.First().Item1 as ContentUnit;
                Assert.True(cuCopy.HasMetadataSlot(ContentPool));
                Assert.Equal(outputPool, cuCopy.Metadata[ContentPool]);
            }

            // Grab all the content units in the output pool and verify that there's the same number of them as numberToSelect
            var CUs = from cu in blackboard.LookupUnits(ContentUnit.TypeName)
                      let cuCast = cu as ContentUnit
                      where cuCast.HasMetadataSlot(ContentPool)
                      where cuCast.Metadata[ContentPool].Equals(outputPool)
                      select cuCast;

            Assert.Equal(Math.Min(numberToSelect, unitsToSelectFrom.Length), CUs.Count());
        }

    }
}
