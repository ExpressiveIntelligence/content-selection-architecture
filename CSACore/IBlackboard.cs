using System.Collections.Generic;

namespace CSACore
{
    public interface IBlackboard
    {
        // Adds a knoweldge unit to the blackboard.
        void AddUnit(IUnit u);

        // Removes a knowledge unit from the blackboard. 
        // fixme: when deleting a unit, make sure any links associated with the unit are deleted. 
        bool DeleteUnit(IUnit u);

        // Returns a set of knowledge units on the blackboard matching the unit type. 
        ISet<IUnit> LookupUnits(string unitType);

        // Returns true if the argument unit is on the blackboard. 
        bool ContainsUnit(IUnit u);

        // Adds an undirected link between unit1 and unit2 with link linkType. Returns true if both unit1 and unit2 exist on the blackboard so the
        // link can be added, false otherwise. 
        // fixme: consider making linkTypes full-fledged classes that can store additional information
        // fixme: add a link direction so that links can be directed as well as undirected
        bool AddLink(IUnit unit1, IUnit unit2, string linkType);

        // Removes the undirected link linkType between unit1 and unit2. Returns true if the link exists and was removed, false if the link doesn't exist. 
        bool RemoveLink(IUnit unit1, IUnit unit2, string linkType);

        // Returns a set of the links for which the argument unit is an endpoint. 
        // If the unit argument has no links or is not in the blackboard, returns the empty set. 
        // Note that using LookupLinks alone, it is not possible to differentiate between the cases of unit on the blackboard with no links and unit not on the blackboard. 
        ISet<(IUnit Node, string LinkType)> LookupLinks(IUnit unit);

        // Removes all knowledge units and links on the blackboard. 
        void Clear();

        // True if the blackboard has been changed since the last call to ResetChanged()
        bool Changed { get; }

        // Resets whether the blackboard has been changed to false. Returns the current changed status before the reset.
        bool ResetChanged();
    }
}