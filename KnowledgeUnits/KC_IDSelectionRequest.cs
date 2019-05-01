using System;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing an ID selection request. 
     */
    public class KC_IDSelectionRequest : KC_ImmutableString
    {
        public bool ActiveRequest { get; set; }

        public string TargetUnitID
        {
            get => StringValue;

            set => StringValue = value;
        }

        public KC_IDSelectionRequest()
        {
            ActiveRequest = false;
        }

        public KC_IDSelectionRequest(string targetID) : base(targetID)
        {
            ActiveRequest = false;
        }

        public KC_IDSelectionRequest(string targetID, bool immutable) : base(targetID, immutable)
        {
            ActiveRequest = false;
        }
    }

    public static class KC_IDSelectionRequest_Extensions
    {
        public static bool GetActiveRequest(this Unit unit)
        {
            if (unit.HasComponent<KC_IDSelectionRequest>())
            {
                return unit.GetComponent<KC_IDSelectionRequest>().ActiveRequest;
            }
            throw new InvalidOperationException("GetActiveRequest() called on Unit without a KC_IDSelectionRequest componenent.");
        }

        public static void SetActiveRequest(this Unit unit, bool active)
        {
            if (unit.HasComponent<KC_IDSelectionRequest>())
            {
                unit.GetComponent<KC_IDSelectionRequest>().ActiveRequest = active;
            }
            throw new InvalidOperationException("SetActiveRequest() called on Unit without a KC_IDSelectionRequest componenent.");
        }

        public static string GetTargetUnitID(this Unit unit)
        {
            return unit.GetStringValue();
        }

        public static void SetTargetUnitID(this Unit unit, string id)
        {
            unit.SetStringValue(id);
        }

        public static bool TargetUnitIDEquals(this Unit unit, string unitID)
        {
            return unit.StringValueEquals(unitID);
        }
    }
}
