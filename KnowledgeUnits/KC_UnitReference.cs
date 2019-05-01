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
            if (unit.HasComponent<KC_UnitReference>())
            {
                return unit.GetComponent<KC_UnitReference>().Name;
            }
            throw new InvalidOperationException("GetReferenceName() called on Unit without a KC_UnitReference componenent.");
        }

        public static void SetReferenceName(this Unit unit, string name)
        {
            if (unit.HasComponent<KC_UnitReference>())
            {
                unit.GetComponent<KC_UnitReference>().Name = name;
            }
            throw new InvalidOperationException("SetReferenceName() called on Unit without a KC_UnitReference componenent.");
        }

        public static bool ReferenceNameEquals(this Unit unit, string refName)
        {
            return unit.StringValueEquals(refName);
        }

        public static Unit GetUnitReference(this Unit unit)
        {
            if (unit.HasComponent<KC_UnitReference>())
            {
                return unit.GetComponent<KC_UnitReference>().Reference;
            }
            throw new InvalidOperationException("GetUnitReference() called on Unit without a KC_UnitReference componenent.");
        }

        public static string SetUnitReference(this Unit unit, Unit reference)
        {
            if (unit.HasComponent<KC_UnitReference>())
            {
                unit.GetComponent<KC_UnitReference>().Reference = reference;
            }
            throw new InvalidOperationException("SetUnitReference() called on Unit without a KC_UnitReference componenent.");
        }
    }
}
