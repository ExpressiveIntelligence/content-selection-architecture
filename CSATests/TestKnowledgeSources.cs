using CSA.KnowledgeSources;
using CSA.KnowledgeUnits;
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
            selectedCU.Metadata[CU_SlotNames.SelectedContentUnit] = null;

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
                new object[] { blackboard, new KS_IDSelector(blackboard), new IUnit[] { }, false, 0 }, 

                // KS_IDSelector, non-matching unit
                new object[] { blackboard, new KS_IDSelector(blackboard), new IUnit[] { new TestUnit1("foo") }, false, 0 }, 

                // KS_IDSelector, matching unit, previously matched
                new object[] { blackboard, new KS_IDSelector(blackboard), new IUnit[] { new U_IDQuery("foo") }, true, 0 }, 

                // KS_IDSelector, matching unit, not previously matched
                new object[] { blackboard, new KS_IDSelector(blackboard), new IUnit[] { new U_IDQuery("foo") }, false, 1 }, 

                // KS_IDSelector, multiple matching units, not previously matched
                new object[]
                {
                    blackboard, new KS_IDSelector(blackboard), new IUnit[]
                    {
                        new U_IDQuery("foo"),
                        new U_IDQuery("bar"),
                        new U_IDQuery("baz")
                    },
                    false, 3
                },

                // ChoicePresenter, empty blackboard
                new object[] { blackboard, new KS_ChoicePresenter(blackboard), new IUnit[] { }, false, 0 }, 

                // ConsoleChoiceSelector, non-matching unit
                new object[] { blackboard, new KS_ChoicePresenter(blackboard), new IUnit[] { new TestUnit1("foo") }, false, 0 }, 

                // ChoicePresenter, matching unit, previously matched
                new object[] { blackboard, new KS_ChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) }, true, 0 }, 

                // ChoicePresenter, matching unit, not previously matched
                new object[] { blackboard, new KS_ChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) }, false, 1}, 

                // ChoicePresenter, multiple matching units, not previously matched
                new object[]
                {
                    blackboard, new KS_ChoicePresenter(blackboard), new IUnit[]
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
        public void TestPrecondition(IBlackboard blackboard, KnowledgeSource ks, IUnit[] unitsToAdd, bool previouslyMatched, int numActivatedKSs)
        {
            blackboard.Clear(); // Clear the blackboard so that there aren't KUs laying around from previous tests. 

            // Add the units in unitsToAdd
            foreach (IUnit unitToAdd in unitsToAdd)
            {
                blackboard.AddUnit(unitToAdd);
                // If they should be marked as previously matched, set KSPreconditionMatched
                if (previouslyMatched)
                {
                    unitToAdd.Properties[U_PropertyNames.KSPreconditionMatched] = new HashSet<KnowledgeSource> { ks };
                }
            }

            // Call KnowledgeSource.Precondition() to get the activated KSs.
            IEnumerable<IKnowledgeSourceActivation> KSAs = ks.Precondition();

            // Check that the number of activated KSs equals the number we're expecting
            int count = KSAs.Count();
            Assert.Equal(numActivatedKSs, count);

            // If there were activated KSs, check that the KUs were marked as having been matched.  
            if (count > 0)
            {
                foreach (IUnit u in unitsToAdd)
                {
                    Assert.True(u.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched));
                    bool containsKS = ((HashSet<KnowledgeSource>)u.Properties[U_PropertyNames.KSPreconditionMatched]).Contains(ks);
                    Assert.True(containsKS);
                }
            }

            // Run the preconditions again to verify that on a second running they don't activate any KSs.
            KSAs = ks.Precondition();
            count = KSAs.Count();
            Assert.Equal(0, count);
        }

        // fixme: remove
        /* 
        // Since the Executable property is defined on the abstract class KnowledgeSource, don't need to test this KS by KS (unless a KS overrides the property).
        [Fact]
        public void TestExecutable_False()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            Assert.False(ks.Executable);
        }

        // Since the Executable property is defined on the abstract class KnowledgeSource, don't need to test this KS by KS (unless a KS overrides the property).
        [Fact]
        public void TestExecutable_True()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            U_IDQuery ku = new U_IDQuery("foo");
            blackboard.AddUnit(ku);
            var KSs = ks.Precondition();
            Assert.True(KSs.ElementAt(0).Executable);
        }
        */

        public static IEnumerable<object[]> Data_TestObviationCondition()
        {
            IBlackboard blackboard = new Blackboard();
            ContentUnit selectedCU = new ContentUnit();
            selectedCU.Metadata[CU_SlotNames.SelectedContentUnit] = null;

            /* Structure of object[]: 
             * IBlackboard: blackboard, 
             * KnowledgeSource: KS whose obviation condition to test, 
             * IUnit[]: array of KUs to add, 
             */

            return new List<object[]>
            {
                // KS_IDSelector, one matching unit
                new object[] { blackboard, new KS_IDSelector(blackboard), new IUnit[] { new U_IDQuery("foo") } }, 

                // KS_IDSelector, multiple matching units
                new object[]
                {
                    blackboard, new KS_IDSelector(blackboard), new IUnit[]
                    {
                        new U_IDQuery("foo"),
                        new U_IDQuery("bar"),
                        new U_IDQuery("baz")
                    }
                 },

                // ChoicePresenter, one matching unit
                new object[] { blackboard, new KS_ChoicePresenter(blackboard), new IUnit[] { new ContentUnit(selectedCU) } }, 

                // ChoicePresenter, multiple matching units
                new object[]
                {
                    blackboard, new KS_ChoicePresenter(blackboard), new IUnit[]
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
        public void TestObviationCondition(IBlackboard blackboard, KnowledgeSource ks, IUnit[] unitsToAdd)
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
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            List<U_IDQuery> kuList = new List<U_IDQuery>
            {
                new U_IDQuery("foo"),
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
            cuList[0].Metadata[CU_SlotNames.ContentUnitID] = "foo";
            cuList[1].Metadata[CU_SlotNames.ContentUnitID] = "bar";
            cuList[2].Metadata[CU_SlotNames.ContentUnitID] = "baz";

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
                               where ((ContentUnit)cu).HasMetadataSlot(CU_SlotNames.SelectedContentUnit)
                               select cu;

            // One content unit has been selected.
            int size = selectedList.Count();
            Assert.Equal(1, size);

            // The right content unit has been selected. 
            Assert.Equal("foo", ((ContentUnit)selectedList.ElementAt(0)).Metadata[CU_SlotNames.ContentUnitID]);

            // The query has been deleted. 
            ISet<IUnit> querySet = blackboard.LookupUnits(U_IDQuery.TypeName);
            Assert.Equal(0, querySet.Count);


        }

        [Fact]
        public void TestExecute_KS_IDSelector_NoSelectedUnit()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            List<U_IDQuery> kuList = new List<U_IDQuery>
            {
                new U_IDQuery("qux"),
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
            cuList[0].Metadata[CU_SlotNames.ContentUnitID] = "foo";
            cuList[1].Metadata[CU_SlotNames.ContentUnitID] = "bar";
            cuList[2].Metadata[CU_SlotNames.ContentUnitID] = "baz";

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
                               where ((ContentUnit)cu).HasMetadataSlot(CU_SlotNames.SelectedContentUnit)
                               select cu;

            // No content unit selected (since "qux" matches no ID.
            int size = selectedList.Count();
            Assert.Equal(0, size);

            // The query has been deleted. 
            ISet<IUnit> querySet = blackboard.LookupUnits(U_IDQuery.TypeName);
            Assert.Equal(0, querySet.Count);
        }

        public static IEnumerable<object[]> Data_TestExecute_PublicChoicePresenter()
        {
            IBlackboard blackboard = new Blackboard();

            ContentUnit originalCU = new ContentUnit();
            originalCU.Metadata[CU_SlotNames.ContentUnitID] = "foo";
            originalCU.Content[CU_SlotNames.Text] = "Here is a node with choices"; 

            ContentUnit selectedCU = new ContentUnit(originalCU);
            selectedCU.Metadata[CU_SlotNames.SelectedContentUnit] = null;

            ContentUnit choice1 = new ContentUnit();
            choice1.Metadata[CU_SlotNames.TargetContentUnitID] = "bar";
            choice1.Content[CU_SlotNames.Text] = "Choice 1";

            ContentUnit choice2 = new ContentUnit();
            choice2.Metadata[CU_SlotNames.TargetContentUnitID] = "baz";
            choice2.Content[CU_SlotNames.Text] = "Choice 2";

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

        // fixme: remove
        // This test method tests a subset of TextExecute_ChoicePresenter below
        /* [Theory]
        [MemberData(nameof(Data_TestGetChoices_PublicChoicePresenter))]
        public void TestGetChoices_PublicChoicePresenter(IBlackboard blackboard, ContentUnit selectedCU, ContentUnit originalCU, ContentUnit[] choices)
        {
            blackboard.Clear();
            blackboard.AddUnit(selectedCU);
            blackboard.AddUnit(originalCU);
            blackboard.AddLink(originalCU, selectedCU, LinkTypes.L_SelectedContentUnit);

            foreach(ContentUnit choice in choices)
            {
                blackboard.AddUnit(choice);
                blackboard.AddLink(originalCU, choice, LinkTypes.L_Choice);
            }

            ChoicePresenter_PublicAccessors ks = new ChoicePresenter_PublicAccessors(blackboard);
            var KSs = ks.Precondition();

            int count = KSs.Count();
            Assert.Equal(1, count);

            int numOfChoices = choices.Length;
            var returnedChoices = ((ChoicePresenter_PublicAccessors)KSs.ElementAt(0)).GetChoices();
            int numOfReturnedChoices = returnedChoices.Count();

            Assert.Equal(numOfChoices, numOfReturnedChoices);
            foreach(ContentUnit choice in choices)
            {
                returnedChoices.Contains(choice);
            }
        } */

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

            KS_ChoicePresenter ks = new KS_ChoicePresenter(blackboard);
            var KSAs = ks.Precondition();

            int count = KSAs.Count();
            Assert.Equal(1, count);

            // Execute the activated choice presenter
            KSAs.ElementAt(0).Execute();

            Assert.Equal(selectedCU.Content[CU_SlotNames.Text], ks.TextToDisplay);

            int numOfChoices = choices.Length;
            Assert.Equal(numOfChoices, ks.ChoicesToDisplay.Length);

            foreach (ContentUnit choice in choices)
            {
                Assert.True(Array.Exists(ks.ChoicesToDisplay, element => element.Equals(choice.Content[CU_SlotNames.Text])));
            }

            // fixme: add a test for KS_ChoicePresenter.SelectChoice()

        }
    }
}
