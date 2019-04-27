using System;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.KnowledgeUnits
{
    /*
     * "Tag" interface for ITargetUnitID. Extension methods for ITargetUnitID defined below. 
     */
    public interface ITargetUnitID : IUnit
    {
    }

    public static class ITargetUnitID_Extensions
    {
        public static string GetUnitID(this ITargetUnitID unit)
        {
            // Throws an exception if the ITargetUnitID has not been inited (and thus there is no ID slot).
            return (string)unit.Slots[ITargetUnitID_ID];
        }

        /*
         * ITargetUnitID is immutable. InitUnitID enforces this immutability. 
         */
        public static void InitUnitID(this ITargetUnitID unit, string ID)
        {
            if (!unit.Slots.TryGetValue(ITargetUnitID_Inited, out _))
            {
                // Has not been previously inited.
                unit.Slots[ITargetUnitID_ID] = ID;
                unit.Slots[ITargetUnitID_Inited] = true; 
            }
            else
            {
                // Trying to init a previously inited ITargetUnitID - throw an exception.
                throw new InvalidOperationException("Attempt to Init an already Inited ITargetUnitID");
            }
        }
    }
}
