using System;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing an unit ID. 
     */
    public class KC_UnitID : KC_ImmutableString
    {
        public string UnitID
        {
            get => StringValue;

            set => StringValue = value;
        }

        public KC_UnitID()
        {
        }

        public KC_UnitID(string targetID) : base(targetID)
        {
        }

        public KC_UnitID(string targetID, bool immutable) : base(targetID, immutable)
        {
        }
    }

    public static class KC_UnitID_Extensions
    {
        public static string GetUnitID(this Unit unit)
        {
            return unit.GetStringValue();
        }

        public static void SetUnitID(this Unit unit, string unitID)
        {
            unit.SetStringValue(unitID);
        }

        /*
         * fixme: add an equality test method for the UnitID. 
         * bool UnitIDEquals(string unitID)        
         */
        public static bool UnitIDEquals(this Unit unit, string unitID)
        {
            return unit.StringValueEquals(unitID);
        }

    }
}
