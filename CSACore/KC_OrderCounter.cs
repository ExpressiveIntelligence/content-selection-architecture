using System.Text;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for a counter. It is currently only used by KS_ScheduledExpandTreeNode to keep track of the order of children.
     * fixme: this probably should be changed to Counter to generalize it. Also, revisit KS_ScheduledExpandTreeNode to see if there
     * is another way to keep track of order besides using this counter.
     */
    public class KC_OrderCounter : KC_Integer
    {
        /*
         * This is the property that uniquely distinguishes KC_OrderCounter for Json deserialization. 
         */
        [DistinguishingProperty]
        public int OrderCounter
        {
            get => IntValue;

            set => IntValue = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(OrderCounter: " + IntValue);
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

        public override object Clone() => new KC_OrderCounter(this);

        public int Inc() => OrderCounter++;

        public int Dec() => OrderCounter--;

        public KC_OrderCounter()
        {
        }

        public KC_OrderCounter(int order) : base(order)
        {
        }

        public KC_OrderCounter(int order, bool readOnly) : base(order, readOnly)
        {
        }

        protected KC_OrderCounter(KC_OrderCounter toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_OrderCounter_Extensions
    {
        public static int GetOrderCounter(this Unit unit)
        {
            return unit.GetIntValue<KC_OrderCounter>();
        }

        public static void SetOrderCounter(this Unit unit, int orderCounter)
        {
            unit.SetIntValue<KC_OrderCounter>(orderCounter);
        }

        public static int IncOrderCounter(this Unit unit)
        {
            return unit.GetComponent<KC_OrderCounter>().Inc();
        }

        public static int DecOrderCounter(this Unit unit)
        {
            return unit.GetComponent<KC_OrderCounter>().Dec();
        }
    }

}
