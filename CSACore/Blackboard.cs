using System.Collections.Generic;
using System.Diagnostics;

namespace CSACore
{
    public class Blackboard : IBlackboard
    {
        private readonly IDictionary<string, ISet<IUnit>> dict = new Dictionary<string, ISet<IUnit>>();

        private readonly IDictionary<IUnit, ISet<(IUnit Node, string LinkType)>> links = new Dictionary<IUnit, ISet<(IUnit Node, string LinkType)>>();

        // Adds a knoweldge unit to the blackboard.
        // Duplicate units (by reference) not allowed on the blackboard. Using set semantics for Units sharing the same type. 
        public void AddUnit(IUnit unit)
        {

            if (LookupUnits(unit, out ISet<IUnit> units))
            {
                units.Add(unit);

            }
            else
            {
                ISet<IUnit> newUnits = new HashSet<IUnit>
                {
                    unit
                };
                dict.Add(GetUnitTypeName(unit), newUnits);
            }

        }

        // Removes a knowledge unit from the blackboard. 
        public bool DeleteUnit(IUnit unit)
        {
            if (LookupUnits(unit, out ISet<IUnit> units))
            {
                if (units.Remove(unit))
                {
                    if (units.Count == 0)
                    {
                        dict.Remove(GetUnitTypeName(unit));
                    }

                    var linksToRemove = LookupLinks(unit);
                    IUnit node1 = unit;
                    foreach((IUnit node2, string linkType) in linksToRemove)
                    {
                        RemoveLink(node1, node2, linkType);
                    }

                    return true; 
                }

            }
            return false; 
        }

        // Returns a set of knowledge units on the blackboard matching the unit type. 
        public ISet<IUnit> LookupUnits(string unitType)
        {
            return dict.TryGetValue(unitType, out ISet<IUnit> units) ?  new HashSet<IUnit>(units) : new HashSet<IUnit>();
        }

        // Returns true if the argument unit is on the blackboard. 
        public bool ContainsUnit(IUnit unit)
        {
            return LookupUnits(unit, out ISet<IUnit> units) && units.Contains(unit);
        }

        protected string GetUnitTypeName(IUnit u)
        {
            return u.GetType().FullName;
        }

        private bool LookupUnits(IUnit unit, out ISet<IUnit> units)
        {
            string typeName = GetUnitTypeName(unit);
            units = null;

            return dict.TryGetValue(typeName, out units);
        }

        // Adds an undirected link between unit1 and unit2 with link linkType. Returns true if both unit1 and unit2 exist on the blackboard so the
        // link can be added, false otherwise. 
        // fixme: consider making linkTypes full-fledged classes that can store additional information
        // fixme: add a link direction so that links can be directed as well as undirected
        public bool AddLink(IUnit unit1, IUnit unit2, string linkType)
        {
            if (ContainsUnit(unit1) && ContainsUnit(unit2))
            {
                ISet<(IUnit, string)> linkSet;
                if (links.TryGetValue(unit1, out linkSet))
                {
                    linkSet.Add((unit2, linkType)); 
                }
                else
                {
                    linkSet = new HashSet<(IUnit, string)>();
                    linkSet.Add((unit2, linkType));
                    links.Add(unit1, linkSet);
                }

                if (links.TryGetValue(unit2, out linkSet))
                {
                    linkSet.Add((unit1, linkType));
                }
                else
                {
                    linkSet = new HashSet<(IUnit, string)>();
                    linkSet.Add((unit1, linkType));
                    links.Add(unit2, linkSet);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        // Removes the undirected link linkType between unit1 and unit2. Returns true if the link exists and was removed, false if the link doesn't exist. 
        public bool RemoveLink(IUnit unit1, IUnit unit2, string linkType)
        {
            ISet<(IUnit, string)> linkSet1;
            ISet<(IUnit, string)> linkSet2;

            bool bValue1 = links.TryGetValue(unit1, out linkSet1);
            bool bValue2 = links.TryGetValue(unit2, out linkSet2);

            if (bValue1 && bValue2)
            {
                // Both unit1 and unit2 are the endpoint of links. 

                // Remove the appropriate node from each set. 
                bool bRemove1 = linkSet1.Remove((unit2, linkType));
                bool bRemove2 = linkSet2.Remove((unit1, linkType));

                // Assert that b1 and b2 have the same value. Either both link sets include the other node with this linkType or neither includes the other node with this linkType; 
                Debug.Assert(bRemove1 == bRemove2);

                // Returns if a link was removed, false otherwise. 
                return bRemove1;
            }
            else
            {
                // One of the two nodes is not the endpoint of any links

                if (bValue1 && !bValue2)
                {
                    // Only the first node is the endpoint of any links. 
                    // Assert that its link set must not contain the second node with this linkType.
                    Debug.Assert(!linkSet1.Contains((unit2, linkType)));
                }
                else if (!bValue1 && bValue2)
                {
                    // Only the second node is the endpoint of any links. 
                    // Assert that its link set must not contain the second node with this linkType.
                    Debug.Assert(!linkSet2.Contains((unit1, linkType)));
                }

                // Nothing removed. 
                return false; 
            }
        }

        // Removes all knowledge units and links on the blackboard. 
        public void Clear()
        {
            dict.Clear();
            links.Clear();
        }

        // Returns a set of the links for which the argument unit is an endpoint. 
        // If the unit argument has no links or is not in the blackboard, returns the empty set. 
        // Note that using LookupLinks alone, it is not possible to differentiate between the cases of unit on the blackboard with no links and unit not on the blackboard. 
        public ISet<(IUnit Node, string LinkType)> LookupLinks(IUnit unit)
        {
            return links.TryGetValue(unit, out ISet<(IUnit, string)> linkSet) ? new HashSet<(IUnit, string)>(linkSet) : new HashSet<(IUnit, string)>(); 
        }

        // fixme: add support for hierarchical blackboards and spaces with special indexing (efficient lookup of units by properties rather than just class)
    }
}
