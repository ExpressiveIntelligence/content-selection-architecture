using System.Collections.Generic;
using CSACore;
using Xunit;

namespace CSATests
{
    // fixme: consolidate tests into theories so more test cases can be quickly added. 
    public class TestBlackboardOperations
    {

        class TestUnit1 : Unit
        {
            public string S { get; set; }

            public TestUnit1(string init)
            {
                S = init;
            }
        }

        class TestUnit2 : Unit
        {

            public int I { get; set; }

            public TestUnit2(int i_init)
            {
                I = i_init;
            }
        }

        [Fact]
        public void TestAddContains()
        {
            IBlackboard blackboard = new Blackboard();
            TestUnit1 u1 = new TestUnit1("one");
            TestUnit1 u2 = new TestUnit1("two");
            TestUnit1 u3 = new TestUnit1("three");
            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);
            blackboard.AddUnit(u3);
            Assert.True(blackboard.ContainsUnit(u1));
            Assert.True(blackboard.ContainsUnit(u2));
            Assert.True(blackboard.ContainsUnit(u3));
        }

        [Fact]
        public void TestAddDeleteContains()
        {
            IBlackboard blackboard = new Blackboard();
            TestUnit1 u1 = new TestUnit1("one");
            TestUnit1 u2 = new TestUnit1("two");
            TestUnit1 u3 = new TestUnit1("three");
            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);
            blackboard.AddUnit(u3);
            Assert.True(blackboard.ContainsUnit(u1));
            Assert.True(blackboard.ContainsUnit(u2));
            Assert.True(blackboard.ContainsUnit(u3));
            blackboard.DeleteUnit(u3);
            Assert.False(blackboard.ContainsUnit(u3));
            Assert.True(blackboard.ContainsUnit(u2));
            Assert.True(blackboard.ContainsUnit(u1));
            blackboard.DeleteUnit(u2);
            Assert.False(blackboard.ContainsUnit(u3));
            Assert.False(blackboard.ContainsUnit(u2));
            Assert.True(blackboard.ContainsUnit(u1));
            blackboard.DeleteUnit(u1);
            Assert.False(blackboard.ContainsUnit(u3));
            Assert.False(blackboard.ContainsUnit(u2));
            Assert.False(blackboard.ContainsUnit(u1));
        }

        [Fact]
        public void TestLookupNotNull()
        {
            IBlackboard blackboard = new Blackboard();

            TestUnit1 u1 = new TestUnit1("one");
            TestUnit2 u2 = new TestUnit2(1);
            string type1 = u1.GetType().FullName;
            string type2 = u2.GetType().FullName;

            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);

            Assert.NotNull(blackboard.LookupUnits(type1));
            Assert.NotNull(blackboard.LookupUnits(type2));
        }

        [Fact]
        public void TestLookupCount()
        {
            IBlackboard blackboard = new Blackboard();

            TestUnit1 u1 = new TestUnit1("one");
            TestUnit1 u2 = new TestUnit1("two");
            TestUnit1 u3 = new TestUnit1("three");

            TestUnit2 u4 = new TestUnit2(1);
            TestUnit2 u5 = new TestUnit2(2);

            string type1 = u1.GetType().FullName;
            string type2 = u4.GetType().FullName;

            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);
            blackboard.AddUnit(u3);
            blackboard.AddUnit(u4);
            blackboard.AddUnit(u5);

            ISet<IUnit> set1 = blackboard.LookupUnits(type1);
            ISet<IUnit> set2 = blackboard.LookupUnits(type2);

            Assert.Equal(3, set1.Count);
            Assert.Equal(2, set2.Count);

        }

        [Fact]
        public void TestLookupCountIsZeroAfterDelete()
        {
            IBlackboard blackboard = new Blackboard();

            TestUnit1 u1 = new TestUnit1("one");
            TestUnit2 u2 = new TestUnit2(1);
            string type1 = u1.GetType().FullName;
            string type2 = u2.GetType().FullName;

            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);

            Assert.Equal(1, blackboard.LookupUnits(type1).Count);
            Assert.Equal(1, blackboard.LookupUnits(type2).Count);

            blackboard.DeleteUnit(u1);
            Assert.Equal(0, blackboard.LookupUnits(type1).Count);
            Assert.Equal(1, blackboard.LookupUnits(type2).Count);
   
            blackboard.DeleteUnit(u2);
            Assert.Equal(0, blackboard.LookupUnits(type1).Count);
            Assert.Equal(0, blackboard.LookupUnits(type2).Count);
        }

        [Fact]
        public void TestLookupCountAfterDelete()
        {
            IBlackboard blackboard = new Blackboard();

            TestUnit1 u1 = new TestUnit1("one");
            TestUnit1 u2 = new TestUnit1("two");
            TestUnit1 u3 = new TestUnit1("three");

            TestUnit2 u4 = new TestUnit2(1);
            TestUnit2 u5 = new TestUnit2(2);

            string type1 = u1.GetType().FullName;
            string type2 = u4.GetType().FullName;

            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);
            blackboard.AddUnit(u3);
            blackboard.AddUnit(u4);
            blackboard.AddUnit(u5);

            ISet<IUnit> set1 = blackboard.LookupUnits(type1);
            ISet<IUnit> set2 = blackboard.LookupUnits(type2);

            Assert.Equal(3, set1.Count);
            Assert.Equal(2, set2.Count);

            blackboard.DeleteUnit(u1);
            blackboard.DeleteUnit(u4);

            set1 = blackboard.LookupUnits(type1);
            set2 = blackboard.LookupUnits(type2);

            Assert.Equal(2, set1.Count);
            Assert.Equal(1, set2.Count);
        }

        [Fact]
        // Deleting and adding elements to a returned set shouldn't change the set in the dictionary
        public void TestManipulatingSet()
        {
            IBlackboard blackboard = new Blackboard();

            TestUnit1 u1 = new TestUnit1("one");
            TestUnit1 u2 = new TestUnit1("two");

            string type1 = u1.GetType().FullName;

            blackboard.AddUnit(u1);
            ISet<IUnit> set1 = blackboard.LookupUnits(type1);
            Assert.Equal(1, set1.Count);

            set1.Add(u2);
            ISet<IUnit> set2 = blackboard.LookupUnits(type1);
            Assert.Equal(1, set2.Count);

            set2.Remove(u1);
            ISet<IUnit> set3 = blackboard.LookupUnits(type1);
            Assert.Equal(1, set3.Count);
        }

        [Fact]
        public void TestAddLink_UnitsOnBlackboard()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new TestUnit1("foo"), new TestUnit1("bar"), new TestUnit1("baz") };
            foreach(var unit in units)
            {
                blackboard.AddUnit(unit);
            }
            Assert.True(blackboard.AddLink(units[0], units[1], "lType1"));
            Assert.True(blackboard.AddLink(units[0], units[2], "lType2"));

            var linkSet1 = blackboard.LookupLinks(units[0]);
            Assert.Equal(2, linkSet1.Count);

            var linkSet2 = blackboard.LookupLinks(units[1]);
            Assert.Equal(1, linkSet2.Count);

            var linkSet3 = blackboard.LookupLinks(units[2]);
            Assert.Equal(1, linkSet3.Count);
        }

        [Fact]
        public void TestAddLink_UnitNotOnBlackboard()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new TestUnit1("foo"), new TestUnit1("bar"), new TestUnit1("baz") };
            blackboard.AddUnit(units[0]);
            blackboard.AddUnit(units[1]);
            Assert.True(blackboard.AddLink(units[0], units[1], "lType1"));
            Assert.False(blackboard.AddLink(units[0], units[2], "lType2"));

            var linkSet1 = blackboard.LookupLinks(units[0]);
            Assert.Equal(1, linkSet1.Count);

            var linkSet2 = blackboard.LookupLinks(units[1]);
            Assert.Equal(1, linkSet2.Count);

            // Looking up links for a unit not on the blackboard. LookupLinks() returns the empty set.
            var linkSet3 = blackboard.LookupLinks(units[2]);
            Assert.Equal(0, linkSet3.Count);

        }

        [Fact]
        public void TestLookupLinks()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new TestUnit1("foo"), new TestUnit1("bar"), new TestUnit1("baz") };
            foreach (var unit in units)
            {
                blackboard.AddUnit(unit);
            }
            Assert.True(blackboard.AddLink(units[0], units[1], "lType1"));
            Assert.True(blackboard.AddLink(units[0], units[2], "lType2"));

            var linkSet1 = blackboard.LookupLinks(units[0]);
            Assert.Equal(2, linkSet1.Count);
            Assert.True(linkSet1.Contains((units[1], "lType1")));
            Assert.True(linkSet1.Contains((units[2], "lType2")));

            var linkSet2 = blackboard.LookupLinks(units[1]);
            Assert.Equal(1, linkSet2.Count);
            Assert.True(linkSet2.Contains((units[0], "lType1")));

            var linkSet3 = blackboard.LookupLinks(units[2]);
            Assert.Equal(1, linkSet3.Count);
            Assert.True(linkSet3.Contains((units[0], "lType2")));
        }

        [Fact]
        public void TestRemoveLink()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new TestUnit1("foo"), new TestUnit1("bar"), new TestUnit1("baz"), new ContentUnit() };
            foreach (var unit in units)
            {
                blackboard.AddUnit(unit);
            }
            Assert.True(blackboard.AddLink(units[0], units[1], "lType1"));
            Assert.True(blackboard.AddLink(units[0], units[2], "lType2"));
            Assert.True(blackboard.AddLink(units[2], units[3], "lType3"));

            var linkSet1 = blackboard.LookupLinks(units[0]);
            var linkSet2 = blackboard.LookupLinks(units[2]);
            Assert.Equal(2, linkSet1.Count);
            Assert.Equal(2, linkSet2.Count);
            Assert.True(blackboard.RemoveLink(units[0], units[2], "lType2"));
            linkSet1 = blackboard.LookupLinks(units[0]);
            linkSet2 = blackboard.LookupLinks(units[2]);
            Assert.Equal(1, linkSet1.Count);
            Assert.Equal(1, linkSet2.Count);

            Assert.False(blackboard.RemoveLink(units[0], units[2], "lType2"));
            linkSet1 = blackboard.LookupLinks(units[0]);
            linkSet2 = blackboard.LookupLinks(units[2]);
            Assert.Equal(1, linkSet1.Count);
            Assert.Equal(1, linkSet2.Count);

            TestUnit1 u = new TestUnit1("NotOnBlackboard");
            Assert.False(blackboard.RemoveLink(units[3], u, "lType1"));
            var linkSet3 = blackboard.LookupLinks(units[3]);
            Assert.Equal(1, linkSet3.Count);
        }

        [Fact]
        public void TestDeleteUnitWithLinks()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new TestUnit1("foo"), new TestUnit1("bar"), new TestUnit1("baz"), new ContentUnit() };
            foreach (var unit in units)
            {
                blackboard.AddUnit(unit);
            }
            Assert.True(blackboard.AddLink(units[0], units[1], "lType1"));
            Assert.True(blackboard.AddLink(units[0], units[2], "lType2"));
            Assert.True(blackboard.AddLink(units[2], units[3], "lType3"));

            var linkSet1 = blackboard.LookupLinks(units[0]);
            var linkSet2 = blackboard.LookupLinks(units[2]);
            Assert.Equal(2, linkSet1.Count);
            Assert.Equal(2, linkSet2.Count);

            Assert.True(blackboard.DeleteUnit(units[0]));
            linkSet1 = blackboard.LookupLinks(units[0]);
            linkSet2 = blackboard.LookupLinks(units[1]);
            var linkSet3 = blackboard.LookupLinks(units[2]);
            Assert.Equal(0, linkSet1.Count); 
            Assert.Equal(0, linkSet2.Count);
            Assert.Equal(1, linkSet3.Count);
        }
    }
}
