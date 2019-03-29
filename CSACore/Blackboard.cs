﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSA.Core
{
    public class Blackboard : IBlackboard
    {
        private readonly IDictionary<string, ISet<IUnit>> dict = new Dictionary<string, ISet<IUnit>>();

        private readonly IDictionary<IUnit, ISet<(IUnit Node, string LinkType, LinkDirection Direction)>> links = 
            new Dictionary<IUnit, ISet<(IUnit Node, string LinkType, LinkDirection Direction)>>();

        protected bool m_changed = false;

        // True if the blackboard has been changed since the last call to ResetChanged()
        public bool Changed => m_changed;

        // Adds a knoweldge unit to the blackboard.
        // Duplicate units (by reference) not allowed on the blackboard. Using set semantics for Units sharing the same type. 
        public void AddUnit(IUnit unit)
        {

            if (LookupUnits(unit, out ISet<IUnit> units))
            {
                if (units.Add(unit))
                {
                    // Blackboard changed only if the unit was successfully added to the set (not a duplicate). 
                    m_changed = true; 
                }
            }
            else
            {
                ISet<IUnit> newUnits = new HashSet<IUnit>
                {
                    unit
                };
                dict.Add(GetUnitTypeName(unit), newUnits);

                // Adding a unit with a new type to the blackboard always changes it
                m_changed = true;
            }
        }

        // Removes a knowledge unit from the blackboard. 
        public bool RemoveUnit(IUnit unit)
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
                    foreach((IUnit node2, string linkType, LinkDirection direction) in linksToRemove)
                    {
                        if (direction == LinkDirection.Undirected)
                        {
                            RemoveLink(node1, node2, linkType, false);
                        }
                        else
                        {
                            RemoveLink(node1, node2, linkType, true);
                        }
                    }

                    // Removing a unit changes the blackboard (but only if the unit was actually on the blackboard) 
                    m_changed = true;

                    return true; 
                }

            }
            return false; 
        }

        // Returns a set of knowledge units on the blackboard matching the unit type. 
        public ISet<T> LookupUnits<T>() where T : IUnit
        {
            return dict.TryGetValue(typeof(T).FullName, out ISet<IUnit> units) ? new HashSet<T>(units.Cast<T>()) : new HashSet<T>();
        }

        /*
         * For singleton units on the blackboard, looks up and returns the singleton of type T. If there is no unit of the type unitType on the 
         * blackboard, returns null. If there is more than one unit of the type, unitType on the blackboard, throws an error. 
         */
        public T LookupSingleton<T>() where T : IUnit
        {
            if (dict.TryGetValue(typeof(T).FullName, out ISet<IUnit> units))
            {
                // Found at least one IUnit of type unitType
                if (units.Count > 1)
                {
                    throw new InvalidOperationException("IBlackboard.LooupSingleton called for unit type with >1 unit instances on the blackboard.");
                }
                return (T)units.First();
            }
            else
            {
                return default(T); // No IUnit of unitType found on the blackboard
            }

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
            units = null;

            return dict.TryGetValue(GetUnitTypeName(unit), out units);
        }

        // Adds an undirected link between unit1 and unit2 with link linkType. Returns true if both unit1 and unit2 exist on the blackboard so the
        // link can be added, false otherwise. 
        // fixme: consider making linkTypes full-fledged classes that can store additional information
        public bool AddLink(IUnit unit1, IUnit unit2, string linkType, bool directed=false)
        {
            if (ContainsUnit(unit1) && ContainsUnit(unit2))
            {
                LinkDirection unit1Dir;
                LinkDirection unit2Dir; 

                if (directed)
                {
                    unit1Dir = LinkDirection.Start;
                    unit2Dir = LinkDirection.End;
                }
                else
                {
                    unit1Dir = unit2Dir = LinkDirection.Undirected;
                }

                if (LookupLinks(unit1).Contains((unit2, linkType, unit2Dir)))
                {
                    // Link is already on the blackboard
                    Debug.Assert(LookupLinks(unit2).Contains((unit1, linkType, unit1Dir)));
                    return false;
                }
                else
                {
                    // Link is not already on the blackboard. 

                    if (links.TryGetValue(unit1, out ISet<(IUnit, string, LinkDirection)> linkSet))
                    {
                        bool bAdd = linkSet.Add((unit2, linkType, unit2Dir));
                        Debug.Assert(bAdd);
                    }
                    else
                    {
                        linkSet = new HashSet<(IUnit, string, LinkDirection)>();
                        bool bAdd = linkSet.Add((unit2, linkType, unit2Dir));
                        Debug.Assert(bAdd);
                        links.Add(unit1, linkSet);
                    }

                    if (links.TryGetValue(unit2, out linkSet))
                    {
                        bool bAdd = linkSet.Add((unit1, linkType, unit1Dir));
                        Debug.Assert(bAdd);
                    }
                    else
                    {
                        linkSet = new HashSet<(IUnit, string, LinkDirection)>();
                        bool bAdd = linkSet.Add((unit1, linkType, unit1Dir));
                        Debug.Assert(bAdd);
                        links.Add(unit2, linkSet);
                    }

                    // The blackboard is changed if we hadd a link
                    m_changed = true;
                    return true;
                }
            }
            else
            {
                // One of the two units is not on the blackboard
                return false;
            }
        }

        // Removes the undirected link linkType between unit1 and unit2. Returns true if the link exists and was removed, false if the link doesn't exist. 
        public bool RemoveLink(IUnit unit1, IUnit unit2, string linkType, bool directed=false)
        {
            ISet<(IUnit, string, LinkDirection)> linkSet1;
            ISet<(IUnit, string, LinkDirection)> linkSet2;

            bool bGet1 = links.TryGetValue(unit1, out linkSet1);
            bool bGet2 = links.TryGetValue(unit2, out linkSet2);

            LinkDirection unit1Dir;
            LinkDirection unit2Dir;

            if (directed)
            {
                unit1Dir = LinkDirection.Start;
                unit2Dir = LinkDirection.End;
            }
            else
            {
                unit1Dir = unit2Dir = LinkDirection.Undirected;
            }

            if (bGet1 && bGet2)
            {
                // Both unit1 and unit2 are the endpoint of links. 

                // Remove the appropriate node from each set. 
                bool bRemove1 = linkSet1.Remove((unit2, linkType, unit2Dir));
                bool bRemove2 = linkSet2.Remove((unit1, linkType, unit1Dir));

                // Assert that b1 and b2 have the same value. Either both link sets include the other node with this linkType or neither includes the other node with this linkType; 
                Debug.Assert(bRemove1 == bRemove2);


                m_changed = bRemove1; // Blackboard changed if a link was removed. 
                return bRemove1; // Returns true if a link was removed, false otherwise. 
            }
            else
            {
                // One of the two nodes is not the endpoint of any links

                if (bGet1 && !bGet2)
                {
                    // Only the first node is the endpoint of any links. 
                    // Assert that its link set must not contain the second node with this linkType.
                    Debug.Assert(!linkSet1.Contains((unit2, linkType, unit2Dir)));
                }
                else if (!bGet1 && bGet2)
                {
                    // Only the second node is the endpoint of any links. 
                    // Assert that its link set must not contain the first node with this linkType.
                    Debug.Assert(!linkSet2.Contains((unit1, linkType, unit1Dir)));
                }

                // Nothing removed. 
                return false; 
            }
        }

        // Removes all knowledge units and links on the blackboard. 
        public void Clear()
        {
            // Clear changes the blackboard if there are any units on the blackboard (if there are no units there can't be any links)
            m_changed = dict.Count > 0;

            dict.Clear();
            links.Clear();
        }

        /*
         * Returns a set of the links for which the argument unit is an endpoint. 
         * If the unit argument has no links or is not in the blackboard, returns the empty set.   
         * Note that using LookupLinks alone, it is not possible to differentiate between the cases of unit on the blackboard with no links and unit 
         * not on the blackboard.        
         */

        public ISet<(IUnit Node, string LinkType, LinkDirection Direction)> LookupLinks(IUnit unit)
        {
            return links.TryGetValue(unit, out ISet<(IUnit, string, LinkDirection)> linkSet) ? new HashSet<(IUnit, string, LinkDirection)>(linkSet) 
                : new HashSet<(IUnit, string, LinkDirection)>(); 
        }

        // Reset whether this blackboard has been changed to false. Returns the current changed status before the reset.  
        public bool ResetChanged()
        {
            bool changed = m_changed;
            m_changed = false;
            return changed;
        }

        /*
         * Below are the definitions of public methods that help support debugging. 
         */

        public uint NumberOfUnits<T>() where T : IUnit
        {
            return (uint)LookupUnits<T>().Count();
        }

        // fixme: add support for hierarchical blackboards and spaces with special indexing (efficient lookup of units by properties rather than just class)
    }
}
