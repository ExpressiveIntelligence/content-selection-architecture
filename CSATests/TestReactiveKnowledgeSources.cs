using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
#pragma warning disable CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.CUSlots;
#pragma warning restore CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.KUProps;
using CSA.Core;

namespace CSA.Tests
{
    [Obsolete("When reactive KSs are finally implemented again, implement real tests for them.")]
    public class TestReactiveKnowledgeSources
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
                new object[] { blackboard, new KS_Old_ReactiveIDSelector(blackboard), new IUnit[] { }, false, 0 }, 

                // KS_IDSelector, non-matching unit
                new object[] { blackboard, new KS_Old_ReactiveIDSelector(blackboard), new IUnit[] { new TestUnit1("foo") }, false, 0 }, 

                // KS_IDSelector, matching unit, previously matched
                new object[] { blackboard, new KS_Old_ReactiveIDSelector(blackboard), new IUnit[] { new U_IDSelectRequest("foo") }, true, 0 }, 

                // KS_IDSelector, matching unit, not previously matched
                new object[] { blackboard, new KS_Old_ReactiveIDSelector(blackboard), new IUnit[] { new U_IDSelectRequest("foo") }, false, 1 }, 

                // KS_IDSelector, multiple matching units, not previously matched
                new object[]
                {
                    blackboard, new KS_Old_ReactiveIDSelector(blackboard), new IUnit[]
                    {
                        new U_IDSelectRequest("foo"),
                        new U_IDSelectRequest("bar"),
                        new U_IDSelectRequest("baz")
                    },
                    false, 3
                },

                // ChoicePresenter, empty blackboard
                new object[] { blackboard, new KS_Old_ReactiveChoicePresenter(blackboard), new IUnit[] { }, false, 0 }, 

                // ConsoleChoiceSelector, non-matching unit
                new object[] { blackboard, new KS_Old_ReactiveChoicePresenter(blackboard), new IUnit[] { new TestUnit1("foo") }, false, 0 }, 

                // ChoicePresenter, matching unit, previously matched
                new object[] { blackboard, new KS_Old_ReactiveChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) }, true, 0 }, 

                // ChoicePresenter, matching unit, not previously matched
                new object[] { blackboard, new KS_Old_ReactiveChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) }, false, 1}, 

                // ChoicePresenter, multiple matching units, not previously matched
                new object[]
                {
                    blackboard, new KS_Old_ReactiveChoicePresenter(blackboard), new IUnit[]
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
                    unitToAdd.Slots[KSPreconditionMatched] = new HashSet<ReactiveKnowledgeSource> { ks };
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
                    Assert.True(u.Slots.ContainsKey(KSPreconditionMatched));
                    bool containsKS = ((HashSet<ReactiveKnowledgeSource>)u.Slots[KSPreconditionMatched]).Contains(ks);
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
                new object[] { blackboard, new KS_Old_ReactiveIDSelector(blackboard), new IUnit[] { new U_IDSelectRequest("foo") } }, 

                // KS_IDSelector, multiple matching units
                new object[]
                {
                    blackboard, new KS_Old_ReactiveIDSelector(blackboard), new IUnit[]
                    {
                        new U_IDSelectRequest("foo"),
                        new U_IDSelectRequest("bar"),
                        new U_IDSelectRequest("baz")
                    }
                 },

                // ChoicePresenter, one matching unit
                new object[] { blackboard, new KS_Old_ReactiveChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) } }, 

                // ChoicePresenter, multiple matching units
                new object[]
                {
                    blackboard, new KS_Old_ReactiveChoicePresenter(blackboard), new IUnit[]
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
            KS_Old_ReactiveIDSelector ks = new KS_Old_ReactiveIDSelector(blackboard);
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
            KS_Old_ReactiveIDSelector ks = new KS_Old_ReactiveIDSelector(blackboard);
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

        private EventHandler<PresenterExecuteEventArgs> GenerateEventHandler(ContentUnit selectedCU, ContentUnit[] choices, IBlackboard blackboard)
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

            KS_Old_ReactiveChoicePresenter ks = new KS_Old_ReactiveChoicePresenter(blackboard);
            ks.PresenterExecute += GenerateEventHandler(selectedCU, choices, blackboard);

            var KSAs = ks.Precondition();

            int count = KSAs.Count();
            Assert.Equal(1, count);

            // Execute the activated choice presenter
            KSAs.ElementAt(0).Execute();
        }

        public TestReactiveKnowledgeSources(ITestOutputHelper output)
        {
            this.output = output;
        }

    }
}
