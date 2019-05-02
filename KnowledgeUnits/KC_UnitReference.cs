using System;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing an named reference to a knowledge unit. Used to create global variables on the blackboard. 
     */

    public class KC_UnitReference : KC_ImmutableString
    {
        public Unit Reference { get; set; }

        public string Name
        {
            get => StringValue;

            set => StringValue = value;
        }

        public KC_UnitReference()
        {
            Reference = null;
        }

        public KC_UnitReference(string name) : base(name)
        {
            Reference = null;
        }

        public KC_UnitReference(string name, Unit reference) : base(name)
        {
            Reference = reference;
        }

        public KC_UnitReference(string name, bool immutable) : base(name, immutable)
        {
            Reference = null;
        }

        public KC_UnitReference(string name, bool immutable, Unit reference) : base(name, immutable)
        {
            Reference = reference;
        }
    }

    public static class KC_UnitReference_Extensions
    {
        public static string GetReferenceName(this Unit unit)
        {
            return unit.GetStringValue<KC_UnitReference>();
        }

        public static void SetReferenceName(this Unit unit, string name)
        {
            unit.SetStringValue<KC_UnitReference>(name);
        }

        public static bool ReferenceNameEquals(this Unit unit, string refName)
        {
            return unit.StringValueEquals<KC_UnitReference>(refName);
        }

        public static Unit GetUnitReference(this Unit unit)
        {
            if (unit.HasComponent<KC_UnitReference>())
            {
                return unit.GetComponent<KC_UnitReference>().Reference;
            }
            throw new InvalidOperationException("GetUnitReference() called on Unit without a KC_UnitReference componenent.");
        }

        public static void SetUnitReference(this Unit unit, Unit reference)
        {
            if (unit.HasComponent<KC_UnitReference>())
            {
                unit.GetComponent<KC_UnitReference>().Reference = reference;
            }
            else
            {
                throw new InvalidOperationException("SetUnitReference() called on Unit without a KC_UnitReference componenent.");
            }
        }
    }
}
