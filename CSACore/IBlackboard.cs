using System.Collections.Generic;

namespace CSA.Core
{
    public enum LinkDirection
    {
        Start,
        End,
        Undirected
    };

    public interface IBlackboard
    {

        // Adds a knoweldge unit to the blackboard.
        void AddUnit(IUnit u);

        // Removes a knowledge unit from the blackboard. 
        // fixme: when deleting a unit, make sure any links associated with the unit are deleted. 
        bool RemoveUnit(IUnit u);

        /*
         * Looks up and returns all the units of type T on the blackboard.
         */
        ISet<T> LookupUnits<T>() where T : IUnit;

        /*
         * For singleton units on the blackboard, looks up and returns the singleton of type T. If there is no unit of the type unitType on the 
         * blackboard, returns null. If there is more than one unit of the type, unitType on the blackboard, throws an error. 
         */
        T LookupSingleton<T>() where T : IUnit;

        // Returns true if the argument unit is on the blackboard. 
        bool ContainsUnit(IUnit u);

        // fixme: Consider adding a ContainsAny() method
        // bool ContainsAny<T>();

        // Adds an undirected link between unit1 and unit2 with link linkType. Returns true if both unit1 and unit2 exist on the blackboard so the
        // link can be added, false otherwise. If directed == true, the link is a directed link going from the first unit to the second. 
        // fixme: consider making linkTypes full-fledged classes that can store additional information
        bool AddLink(IUnit unit1, IUnit unit2, string linkType, bool directed = false);

        // Removes the undirected link linkType between unit1 and unit2. Returns true if the link exists and was removed, false if the link doesn't exist. 
        bool RemoveLink(IUnit unit1, IUnit unit2, string linkType, bool directed = false);

        // Returns a set of the links for which the argument unit is an endpoint. 
        // If the unit argument has no links or is not in the blackboard, returns the empty set. 
        // Note that using LookupLinks alone, it is not possible to differentiate between the cases of unit on the blackboard with no links and unit not on the blackboard. 
        ISet<(IUnit Node, string LinkType, LinkDirection Direction)> LookupLinks(IUnit unit);

        // Removes all knowledge units and links on the blackboard. 
        void Clear();

        // True if the blackboard has been changed since the last call to ResetChanged()
        bool Changed { get; }

        // Resets whether the blackboard has been changed to false. Returns the current changed status before the reset.
        bool ResetChanged();

        /*
         * The methods below support debugging. 
         */

        // Returns the number of units of a given type stored in the blackboard.
        uint NumberOfUnits<T>() where T : IUnit;
    }
}