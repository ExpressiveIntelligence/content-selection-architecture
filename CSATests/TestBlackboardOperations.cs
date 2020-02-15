using System.Collections.Generic;
using CSA.Core;
using static CSA.Core.LinkTypes;
using Xunit;

namespace CSA.Tests
{
    // fixme: consolidate tests into theories so more test cases can be quickly added. 
    public class TestBlackboardOperations
    {

        // fixme: Blackboard.AddUnit now returns a bool. Add this to tests. 

        [Fact]
        public void TestAddContains()
        {
            IBlackboard blackboard = new Blackboard();
            Unit u1 = new Unit();
            u1.AddComponent(new KC_UnitID("one", true));

            Unit u2 = new Unit();
            u2.AddComponent(new KC_UnitID("two", true));

            Unit u3 = new Unit();
            u3.AddComponent(new KC_UnitID("three", true));

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
            Unit u1 = new Unit();
            u1.AddComponent(new KC_UnitID("one", true));

            Unit u2 = new Unit();
            u2.AddComponent(new KC_UnitID("two", true));

            Unit u3 = new Unit();
            u3.AddComponent(new KC_UnitID("three", true));

            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);
            blackboard.AddUnit(u3);
            Assert.True(blackboard.ContainsUnit(u1));
            Assert.True(blackboard.ContainsUnit(u2));
            Assert.True(blackboard.ContainsUnit(u3));
            blackboard.RemoveUnit(u3);
            Assert.False(blackboard.ContainsUnit(u3));
            Assert.True(blackboard.ContainsUnit(u2));
            Assert.True(blackboard.ContainsUnit(u1));
            blackboard.RemoveUnit(u2);
            Assert.False(blackboard.ContainsUnit(u3));
            Assert.False(blackboard.ContainsUnit(u2));
            Assert.True(blackboard.ContainsUnit(u1));
            blackboard.RemoveUnit(u1);
            Assert.False(blackboard.ContainsUnit(u3));
            Assert.False(blackboard.ContainsUnit(u2));
            Assert.False(blackboard.ContainsUnit(u1));
        }

        [Fact]
        public void TestLookupNotNull()
        {
            IBlackboard blackboard = new Blackboard();

            Unit u1 = new Unit();
            u1.AddComponent(new KC_UnitID("one", true));

            blackboard.AddUnit(u1);

            Assert.NotNull(blackboard.LookupUnits<Unit>());
        }

        [Fact]
        public void TestLookupCount()
        {
            IBlackboard blackboard = new Blackboard();

            Unit u1 = new Unit();
            u1.AddComponent(new KC_UnitID("one", true));

            Unit u2 = new Unit();
            u2.AddComponent(new KC_UnitID("two", true));

            Unit u3 = new Unit();
            u3.AddComponent(new KC_UnitID("three", true));

            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);
            blackboard.AddUnit(u3);

            ISet<Unit> set1 = blackboard.LookupUnits<Unit>();

            Assert.Equal(3, set1.Count);
        }

        [Fact]
        public void TestLookupCountAfterDelete()
        {
            IBlackboard blackboard = new Blackboard();

            Unit u1 = new Unit();
            u1.AddComponent(new KC_UnitID("one", true));

            Unit u2 = new Unit();
            u2.AddComponent(new KC_UnitID("two", true));

            blackboard.AddUnit(u1);
            blackboard.AddUnit(u2);

            Assert.Equal(2, blackboard.LookupUnits<Unit>().Count);
 
            blackboard.RemoveUnit(u1);
            Assert.Equal(1, blackboard.LookupUnits<Unit>().Count);
 
            blackboard.RemoveUnit(u2);
            Assert.Equal(0, blackboard.LookupUnits<Unit>().Count);
         }
         
        [Fact]
        // Deleting and adding elements to a returned set shouldn't change the set in the dictionary
        public void TestManipulatingSet()
        {
            IBlackboard blackboard = new Blackboard();

            Unit u1 = new Unit();
            u1.AddComponent(new KC_UnitID("one", true));

            Unit u2 = new Unit();
            u2.AddComponent(new KC_UnitID("two", true));

            blackboard.AddUnit(u1);
            ISet<Unit> set1 = blackboard.LookupUnits<Unit>();
            Assert.Equal(1, set1.Count);

            set1.Add(u2);
            ISet<Unit> set2 = blackboard.LookupUnits<Unit>();
            Assert.Equal(1, set2.Count);

            set2.Remove(u1);
            ISet<Unit> set3 = blackboard.LookupUnits<Unit>();
            Assert.Equal(1, set3.Count);
        }

        // fixme: add tests for directed links. 

        public static IEnumerable<object[]> Data_TestLink_Blackboard()
        {
            IBlackboard blackboard = new Blackboard();

            IUnit[] units = {
                new Unit(),
                new Unit(),
                new Unit(),
                new Unit()
            };

            /* Structure of object[]: 
             * IBlackboard: blackboard,            
             * IUnit[]: array of units to add
             * (Unit, Unit, string, bool, bool)[]: array of links to add plus retval   
             * IUnit[]: array of units to delete
             * (Unit, Unit, string, bool, bool): array of links to delete plus retval 
             * IUnit[]: array of units remaining
             * (Unit, Unit, string, bool)[]: array of links remaining
             */

            return new List<object[]>
            {
                // Empty blackboard, no operations
                new object[] { blackboard, new IUnit[0], new (IUnit, IUnit, string, bool, bool)[0], new IUnit[0], new (IUnit, IUnit, string, bool, bool)[0],
                    new IUnit[0], new (IUnit, IUnit, string, bool)[0] },

                // Empty blackboard, link operations with false retval
                new object[] { blackboard, new IUnit[0],
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], L_SelectedUnit, true, false),
                        (units[1], units[2], L_SelectedUnit, false, false)
                    },
                    new IUnit[0],
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], L_SelectedUnit, true, false),
                        (units[1], units[2], L_SelectedUnit, false, false)
                    },
                    new IUnit[0], new (IUnit, IUnit, string, bool)[0] }, 

                // Blackboard with tree with three nodes. Removal of one leaf should not remove the other two nodes or their link
                new object[] { blackboard,
                    new IUnit[] { units[0], units[1], units[2] },
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], L_Choice, true, true),
                        (units[0], units[2], L_Choice, false, true)
                    },
                    new IUnit[] { units[2] },
                    new (IUnit, IUnit, string, bool, bool)[0],
                    new IUnit[] { units[0], units[1] },
                    new (IUnit, IUnit, string, bool)[]
                    {
                       (units[0], units[1], L_Choice, true),
                    }
                },

                // Blackboard with tree with four nodes. Removal of the root node removes two of the links
                new object[] { blackboard,
                    new IUnit[] { units[0], units[1], units[2], units[3] },
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], L_Choice, true, true),
                        (units[0], units[2], L_Choice, true, true),
                        (units[1], units[3], L_SelectedUnit, true, true)
                    },
                    new IUnit[] { units[0] },
                    new (IUnit, IUnit, string, bool, bool)[0],
                    new IUnit[] { units[1], units[2], units[3] },
                    new (IUnit, IUnit, string, bool)[]
                    {
                        (units[1], units[3], L_SelectedUnit, true)
                    }
                },

                // Blackboard with three nodes all linked to one node. Removal of the one node removes all the links. 
                // Subsequent removal of links should return false.
                new object[] { blackboard,
                    new IUnit[] { units[0], units[1], units[2], units[3] },
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], L_Choice, true, true),
                        (units[0], units[2], L_Choice, true, true),
                        (units[0], units[3], L_Choice, true, true)
                    },
                    new IUnit[] { units[0] },
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], L_Choice, true, false),
                        (units[0], units[2], L_Choice, true, false),
                        (units[0], units[3], L_Choice, true, false)

                    },
                    new IUnit[] { units[1], units[2], units[3] },
                    new (IUnit, IUnit, string, bool)[0]
                },

                // Blackboard with two nodes connected to each other with two directed links. Removing one link should leave the other 
                 new object[] { blackboard,
                    new IUnit[] { units[0], units[1] },
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], "lType1", true, true),
                        (units[1], units[0], "lType1", true, true),
                    },
                    new IUnit[0],
                    new (IUnit, IUnit, string, bool, bool)[]
                    {
                        (units[0], units[1], "lType1", true, true),
                    },
                    new IUnit[] { units[0], units[1] } ,
                    new (IUnit, IUnit, string, bool)[]
                    {
                        (units[1], units[0], "lType1", true)
                    }
                },

            };
        }

        [Theory]
        [MemberData(nameof(Data_TestLink_Blackboard))]
        public void TestLink_Blackboard(IBlackboard blackboard, IUnit[] unitsToAdd, (IUnit, IUnit, string, bool, bool)[] linksToAdd, IUnit[] unitsToRemove,
            (IUnit, IUnit, string, bool, bool)[] linksToRemove, IUnit[] unitsRemaining, (IUnit, IUnit, string, bool)[] linksRemaining)
        {
            // Clear the blackboard of previous tests. 
            blackboard.Clear();

            // Add the units 
            TestUtilities.AddUnits(blackboard, unitsToAdd);

            // Add each of the links 
            foreach ((IUnit unit1, IUnit unit2, string linkType, bool directed, bool retval) in linksToAdd)
            {
                bool added = blackboard.AddLink(unit1, unit2, linkType, directed);
                Assert.True(added == retval);
            }

            // Remove some units. Depending on which units are removed, this may remove some of the links
            TestUtilities.RemoveUnits(blackboard, unitsToRemove);

            // Remove some links. Depending on which links are removed, this may remove some units
            foreach ((IUnit unit1, IUnit unit2, string linkType, bool directed, bool retval) in linksToRemove)
            {
                bool removed = blackboard.RemoveLink(unit1, unit2, linkType, directed);
                Assert.True(removed == retval);
            }

            // Check that each unit that is supposed to be remaining is on the blackboard
            foreach (IUnit unit in unitsRemaining)
            {
                Assert.True(blackboard.ContainsUnit(unit));
            }

            // Check that the total number of units on the blackboard is equal to the number of units that are supposed to be remaining
            Assert.Equal(unitsRemaining.Length, (int)blackboard.NumberOfUnits());

            // Check that each link remaning appears on the blackboard both in unit1 and unit2's link sets. 
            foreach ((IUnit unit1, IUnit unit2, string linkType, bool directed) in linksRemaining)
            {
                ISet<(IUnit, string, LinkDirection)> unit1Links = blackboard.LookupLinks(unit1);
                ISet<(IUnit, string, LinkDirection)> unit2Links = blackboard.LookupLinks(unit2);

                LinkDirection unit1Dir = directed ? LinkDirection.Start : LinkDirection.Undirected;
                LinkDirection unit2Dir = directed ? LinkDirection.End : LinkDirection.Undirected;

                Assert.True(unit1Links.Contains((unit2, linkType, unit2Dir)));
                Assert.True(unit2Links.Contains((unit1, linkType, unit1Dir)));
            }

            // Check that the total number of links on the blackboard is equal to the number of links that are supposed to be remaining
            Assert.Equal(linksRemaining.Length, (int)blackboard.NumberOfLinks());
        }

        [Fact]
        public void TestAddLink_UnitsOnBlackboard()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new Unit(), new Unit(), new Unit() };
            for (int i = 0; i < units.Length; i++)
            {
                units[i].AddComponent(new KC_Order(i));
                blackboard.AddUnit(units[i]);
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
            IUnit[] units = { new Unit(), new Unit(), new Unit() };
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
            IUnit[] units = { new Unit(), new Unit(), new Unit() };
            foreach (var unit in units)
            {
                blackboard.AddUnit(unit);
            }
            Assert.True(blackboard.AddLink(units[0], units[1], "lType1"));
            Assert.True(blackboard.AddLink(units[0], units[2], "lType2"));

            var linkSet1 = blackboard.LookupLinks(units[0]);
            Assert.Equal(2, linkSet1.Count);
            Assert.True(linkSet1.Contains((units[1], "lType1", LinkDirection.Undirected)));
            Assert.True(linkSet1.Contains((units[2], "lType2", LinkDirection.Undirected)));

            var linkSet2 = blackboard.LookupLinks(units[1]);
            Assert.Equal(1, linkSet2.Count);
            Assert.True(linkSet2.Contains((units[0], "lType1", LinkDirection.Undirected)));

            var linkSet3 = blackboard.LookupLinks(units[2]);
            Assert.Equal(1, linkSet3.Count);
            Assert.True(linkSet3.Contains((units[0], "lType2", LinkDirection.Undirected)));
        }

        [Fact]
        public void TestRemoveLink()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new Unit(), new Unit(), new Unit(), new Unit() };
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

            Unit notOnBlackboard = new Unit();
            Assert.False(blackboard.RemoveLink(units[3], notOnBlackboard, "lType1"));
            var linkSet3 = blackboard.LookupLinks(units[3]);
            Assert.Equal(1, linkSet3.Count);
        }

        [Fact]
        public void TestDeleteUnitWithLinks()
        {
            IBlackboard blackboard = new Blackboard();
            IUnit[] units = { new Unit(), new Unit(), new Unit(), new Unit() };
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

            Assert.True(blackboard.RemoveUnit(units[0]));
            linkSet1 = blackboard.LookupLinks(units[0]);
            linkSet2 = blackboard.LookupLinks(units[1]);
            var linkSet3 = blackboard.LookupLinks(units[2]);
            Assert.Equal(0, linkSet1.Count);
            Assert.Equal(0, linkSet2.Count);
            Assert.Equal(1, linkSet3.Count);
        }

        public static IEnumerable<object[]> Data_TestChanged_Blackboard()
        {

            Unit u1 = new Unit();
            u1.AddComponent(new KC_UnitID("one", true));

            Unit u2 = new Unit();
            u2.AddComponent(new KC_UnitID("two", true));

            Unit u3 = new Unit();
            u3.AddComponent(new KC_UnitID("three", true));

            IBlackboard blackboard = new Blackboard();

            /* Structure of object[]: 
            * IBlackboard: blackboard,            
            * IUnit[]: array of units to add
            * bool: changed after unit add           
            * (IUnit node1, IUnit node, string linkType)[]: array of links to add,
            * bool: changed after link add           
            * IUnit[]: array of units to delete
            * bool: changed after unit delete           
            * (IUnit node1, IUnit node, string linkType)[]: array of links to delete,
            * bool: changed after link delete           
            */

            return new List<object[]>
            {
                // Empty blackboard, no assertions and deletions
                new object[] { blackboard, new IUnit[] { }, false, new (IUnit, IUnit, string)[] { }, false, new IUnit[] { }, false, new (IUnit, IUnit, string)[] { }, false }, 

                // Adding a unit
                new object[] { blackboard, new IUnit[] { u1 }, true, new (IUnit, IUnit, string)[] { }, false, new IUnit[] { }, false, new (IUnit, IUnit, string)[] { }, false },

                // Adding and removing a unit
                new object[] { blackboard, new IUnit[] { u1 }, true, new (IUnit, IUnit, string)[] { }, false, new IUnit[] { u1 }, true, new (IUnit, IUnit, string)[] { }, false },

                // Adding a unit and removing a unit not on the blackboard
                new object[] { blackboard, new IUnit[] { u1 }, true, new (IUnit, IUnit, string)[] { }, false, new IUnit[] { u2 }, false, new (IUnit, IUnit, string)[] { }, false },
 
                // Adding two units and adding a link
                new object[] { blackboard, new IUnit[] { u1 }, true, new (IUnit, IUnit, string)[] { }, false, new IUnit[] { u2 }, false, new (IUnit, IUnit, string)[] { }, false },

                // Adding two units and adding a link
                new object[] { blackboard, new IUnit[] { u1, u2 }, true, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, true, new IUnit[] { }, false, new (IUnit, IUnit, string)[] { }, false },

                // Adding two units and removing a link
                new object[] { blackboard, new IUnit[] { u1, u2 }, true, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, true, new IUnit[] { }, false, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, true },

                // Adding two units, adding a link, then removing a non-existant link (no unit)
                new object[] { blackboard, new IUnit[] { u1, u2 }, true, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, true, new IUnit[] { }, false, new (IUnit, IUnit, string)[] { (u1, u3, "l_foo") }, false },

                // Adding two units, removing one of them, then trying to remove the link that used to exist
                new object[] { blackboard, new IUnit[] { u1, u2 }, true, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, true, new IUnit[] { u1 }, true, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, false },

                // Adding three units, adding link, removing unit not participating in link, then removing the link that used to exist
                new object[] { blackboard, new IUnit[] { u1, u2, u3}, true, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, true, new IUnit[] { u3 }, true, new (IUnit, IUnit, string)[] { (u1, u2, "l_foo") }, true },

            };

        }

        [Theory]
        [MemberData(nameof(Data_TestChanged_Blackboard))]
        public void TestChanged_Blackboard(
            IBlackboard blackboard,
            IUnit[] unitsToAdd,
            bool changedAfterUnitAdd,
            (IUnit node1, IUnit node2, string linkType)[] linksToAdd,
            bool changedAfterLinkAdd,
            IUnit[] unitsToDelete,
            bool changedAfterUnitDelete,
            (IUnit node1, IUnit node2, string linkType)[] linksToDelete,
            bool changedAfterLinkDelete)
        {
            blackboard.Clear();

            foreach (IUnit unit in unitsToAdd)
            {
                blackboard.AddUnit(unit);
            }

            Assert.True(blackboard.Changed == changedAfterUnitAdd);
            Assert.True(blackboard.ResetChanged() == changedAfterUnitAdd);
            Assert.False(blackboard.Changed);

            foreach ((IUnit node1, IUnit node2, string linkType) link in linksToAdd)
            {
                blackboard.AddLink(link.node1, link.node2, link.linkType);
            }

            Assert.True(blackboard.Changed == changedAfterLinkAdd);
            Assert.True(blackboard.ResetChanged() == changedAfterLinkAdd);
            Assert.False(blackboard.Changed);

            foreach (IUnit unit in unitsToDelete)
            {
                blackboard.RemoveUnit(unit);
            }

            Assert.True(blackboard.Changed == changedAfterUnitDelete);
            Assert.True(blackboard.ResetChanged() == changedAfterUnitDelete);
            Assert.False(blackboard.Changed);

            foreach ((IUnit node1, IUnit node2, string linkType) link in linksToDelete)
            {
                blackboard.RemoveLink(link.node1, link.node2, link.linkType);
            }

            Assert.True(blackboard.Changed == changedAfterLinkDelete);
            Assert.True(blackboard.ResetChanged() == changedAfterLinkDelete);
            Assert.False(blackboard.Changed);
        }

        // fixme: add test for LookupUnits<T>

        // fixme: add test for LookupSingleton<T>

        // fixme: add test for NumberOfUnits<T>
    }
}
