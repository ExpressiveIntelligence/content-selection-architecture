using System;
using System.Text;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing an ID selection request. 
     */
    public class KC_IDSelectionRequest : KC_ReadOnlyString
    {
        public bool ActiveRequest { get; set; }

        public string TargetUnitID
        {
            get => StringValue;

            set => StringValue = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(TargetUnitID: " + StringValue + ", ActiveRequest: " + ActiveRequest);
            if (ReadOnly)
            {
                sb.Append(", readonly)");
            }
            else
            {
                sb.Append(")");
            }
            return sb.ToString();
        }

        public override object Clone() => new KC_IDSelectionRequest(this);

        public KC_IDSelectionRequest()
        {
            ActiveRequest = false;
        }

        public KC_IDSelectionRequest(string targetID) : base(targetID)
        {
            ActiveRequest = false;
        }

        public KC_IDSelectionRequest(string targetID, bool readOnly) : base(targetID, readOnly)
        {
            ActiveRequest = false;
        }

        protected KC_IDSelectionRequest(KC_IDSelectionRequest toCopy) : base(toCopy)
        {
            ActiveRequest = toCopy.ActiveRequest;
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
            else
            {
                throw new InvalidOperationException("SetActiveRequest() called on Unit without a KC_IDSelectionRequest componenent.");
            }
        }

        public static string GetTargetUnitID(this Unit unit)
        {
            return unit.GetStringValue<KC_IDSelectionRequest>();
        }

        public static void SetTargetUnitID(this Unit unit, string id)
        {
            unit.SetStringValue<KC_IDSelectionRequest>(id);
        }

        public static bool TargetUnitIDEquals(this Unit unit, string unitID)
        {
            return unit.StringValueEquals<KC_IDSelectionRequest>(unitID);
        }
    }
}
