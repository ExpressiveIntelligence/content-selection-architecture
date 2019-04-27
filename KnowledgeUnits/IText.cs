using System;
using System.Collections.Generic;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.KnowledgeUnits
{
    /*
     * "Tag" interface for content IText. Extension methods for IText defined below. 
     */
    public interface IText : IUnit
    {
    }

    public static class IText_Extensions
    {
        public static string GetString(this IText unit)
        {
            try
            {
                return (string)unit.Slots[IText_String];
            }
            catch (KeyNotFoundException)
            {
                // IContent_Text_String slot not created. Return null; 
                return null; 
            }
        }

        public static void SetString(this IText unit, string text)
        {
            unit.Slots[IText_String] = text;
        }

        public static void InitImmutableText(this IText unit, string text)
        {
            if (!unit.Slots.TryGetValue(IText_Inited, out _))
            {
                // Has not been previously inited.
                unit.Slots[IText_String] = text;
                unit.Slots[IText_Inited] = true;
            }
            else
            {
                // Trying to init a previously inited ITargetUnitID - throw an exception.
                throw new InvalidOperationException("Attempt to Init an already Inited ITargetUnitID");
            }

        }
    }

}
