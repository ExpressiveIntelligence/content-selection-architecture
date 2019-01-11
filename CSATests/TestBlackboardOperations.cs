using System.Collections.Generic;
using CSACore;
using Xunit;

namespace CSATests
{
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
        public void TestLookupAfterDeleteIsNull()
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

            blackboard.DeleteUnit(u1);
            Assert.Null(blackboard.LookupUnits(type1));

            blackboard.DeleteUnit(u2);
            Assert.Null(blackboard.LookupUnits(type2));

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
        public void Temp()
        {
            /* 
            dynamic o = new System.Dynamic.ExpandoObject();
            o.Foo = 3;
            int baz = o.Baz;
            Assert.NotNull(o.Bar); 
            */

             var dict = new Dictionary<string, int>();
            dict["foo"] = 1;
            dict["bar"] = 2;

            Assert.Equal(1, dict["foo"]);
            Assert.Equal(2, dict["bar"]);
            Assert.Equal(3, dict["baz"]);

        }
    }
}
