using System.Text;

namespace CSA.Core
{
    /*
     * KnoweldgeComponent for representing sequences of units.
     *
     * fixme: Currently only used by KS_ScheduledLiearizeTreeLeaves. Consider merging this with KC_Decomposition. 
     */
    public class KC_Sequence : KC_UnitList
    {
        /*
         * fixme: this is the distinguishing property, but still need to figure out how I'm going to
         * serialize and deserialize Unit references.
         */
        public Unit[] Sequence
        {
            get => UnitList;

            set => UnitList = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(Sequence: ");
            if (ReadOnly)
            {
                sb.AppendLine("(readonly)");
            }
            else
            {
                sb.AppendLine("");
            }
            foreach (var unit in Sequence)
            {
                sb.AppendLine("   " + unit);
            }
            return sb.ToString();
        }

        public override object Clone() => new KC_Sequence(this);

        public KC_Sequence()
        {
        }

        public KC_Sequence(Unit[] sequence) : base(sequence)
        {
        }

        public KC_Sequence(Unit[] sequence, bool readOnly) : base(sequence, readOnly)
        {
        }

        protected KC_Sequence(KC_Sequence toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_Sequence_Extensions
    {
        public static Unit[] GetSequence(this Unit unit)
        {
            return unit.GetUnitList<KC_Sequence>();
        }

        public static void SetSequence(this Unit unit, Unit[] sequence)
        {
            unit.SetUnitList<KC_Sequence>(sequence);
        }
    }

}
