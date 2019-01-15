using KnowledgeSources;
using KnowledgeUnits;
using Xunit;
using CSACore;
using System.Collections.Generic;
using System.Linq;

namespace CSATests
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

        [Fact]
        public void TestPrecondition_KS_IDSelector_NoMatch_EmptyBlackboard()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            IList<KnowledgeSource> list = ks.Precondition();
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void TestPrecondition_KS_IDSelector_NoMatch_NonmatchingKU()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            blackboard.AddUnit(new TestUnit1("foo")); 
            IList<KnowledgeSource> list = ks.Precondition();
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void TestPrecondition_KS_IDSelector_NoMatch_KS_IDSelector_PreconditionMatched()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            U_IDQuery ku = new U_IDQuery("foo");
            ku.Properties[U_PropertyNames.KSPreconditionMatched] = null;
            blackboard.AddUnit(ku);
            IList<KnowledgeSource> list = ks.Precondition();
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void TestPrecondition_KS_IDSelector_Match()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            U_IDQuery ku = new U_IDQuery("foo");
            blackboard.AddUnit(ku);
            IList<KnowledgeSource> list = ks.Precondition();
            Assert.Equal(1, list.Count);
            Assert.True(ku.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched));
        }

        [Fact]
        public void TestPrecondition_KS_IDSelector_RunTwice()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            U_IDQuery ku = new U_IDQuery("foo");
            blackboard.AddUnit(ku);
            IList<KnowledgeSource> list1 = ks.Precondition();
            IList<KnowledgeSource> list2 = ks.Precondition();
            Assert.Equal(0, list2.Count);
        }

        [Fact]
        public void TestExecutable_False()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            Assert.False(ks.Executable);
        }

        [Fact]
        public void TestExecutable_True()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            U_IDQuery ku = new U_IDQuery("foo");
            blackboard.AddUnit(ku);
            IList<KnowledgeSource> list = ks.Precondition();
            Assert.True(list[0].Executable);
        }

        [Fact]
        public void TestPrecondition_KS_IDSelector_ObviationCondition_False()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            U_IDQuery ku = new U_IDQuery("foo");
            blackboard.AddUnit(ku);
            IList<KnowledgeSource> list = ks.Precondition();
            Assert.False(list[0].EvaluateObviationCondition());
        }

        [Fact]
        public void TestPrecondition_KS_IDSelector_ObviationCondition_True()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            U_IDQuery ku = new U_IDQuery("foo");
            blackboard.AddUnit(ku);
            IList<KnowledgeSource> list = ks.Precondition();
            blackboard.DeleteUnit(ku);
            Assert.True(list[0].EvaluateObviationCondition());
        }

        [Fact]
        public void TestPrecondition_KS_IDSelector_Precondition_MultipleMatches()
        {
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            List<U_IDQuery> kuList = new List<U_IDQuery>
            {
                new U_IDQuery("foo"),
                new U_IDQuery("bar"),
                new U_IDQuery("baz")
            };

            foreach(IUnit u in kuList)
            {
                blackboard.AddUnit(u);
            }
            IList<KnowledgeSource> list = ks.Precondition();
            Assert.Equal(3, list.Count);
            foreach(U_IDQuery u in kuList)
            {
                Assert.True(u.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched));
            }
        }

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

            foreach(IUnit u in cuList)
            {
                blackboard.AddUnit(u);
            }

            IList<KnowledgeSource> list = ks.Precondition();
            Assert.Equal(1, list.Count);
            list[0].Execute();
            Assert.False(ks.Executable);

            // Four content units total (the original three plus a new selected one)
            ISet<IUnit> cuSet = blackboard.LookupUnits(ContentUnit.CU_Name);
            Assert.Equal(4, cuSet.Count);

            // Query for selected content units
            var selectedList = from cu in blackboard.LookupUnits(ContentUnit.CU_Name)
                               where ((ContentUnit)cu).HasMetadataSlot(CU_SlotNames.SelectedContentUnit)
                               select cu;

            // One content unit has been selected.
            int size = selectedList.Count();
            Assert.Equal(1, size);

            // The right content unit has been selected. 
            Assert.Equal("foo", ((ContentUnit)selectedList.ElementAt(0)).Metadata[CU_SlotNames.ContentUnitID]);

            // The query has been deleted. 
            ISet<IUnit> querySet = blackboard.LookupUnits(KU_Names.U_IDQuery);
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

            IList<KnowledgeSource> list = ks.Precondition();
            Assert.Equal(1, list.Count);
            list[0].Execute();
            Assert.False(ks.Executable);

            // Three content units total (no selected unit)
            ISet<IUnit> cuSet = blackboard.LookupUnits(ContentUnit.CU_Name);
            Assert.Equal(3, cuSet.Count);

            // Query for selected content units
            var selectedList = from cu in blackboard.LookupUnits(ContentUnit.CU_Name)
                               where ((ContentUnit)cu).HasMetadataSlot(CU_SlotNames.SelectedContentUnit)
                               select cu;

            // No content unit selected (since "qux" matches no ID.
            int size = selectedList.Count();
            Assert.Equal(0, size);

            // The query has been deleted. 
            ISet<IUnit> querySet = blackboard.LookupUnits(KU_Names.U_IDQuery);
            Assert.Equal(0, querySet.Count);
        }
    }
}
