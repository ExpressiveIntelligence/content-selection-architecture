using System.Text;

namespace CSA.Core
{
    public class KC_Decomposition : KC_UnitList
    {
        public Unit[] Decomposition
        {
            get => UnitList;

            set => UnitList = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(Decomposition: ");
            if (ReadOnly)
            {
                sb.AppendLine("(readonly)");
            }
            else
            {
                sb.AppendLine("");
            }
            foreach (var unit in Decomposition)
            {
                sb.AppendLine("   " + unit);
            }
            return sb.ToString();
        }

        public override object Clone() => new KC_Decomposition(this);

        public KC_Decomposition()
        {
        }

        public KC_Decomposition(Unit[] decomposition) : base(decomposition)
        {
        }

        public KC_Decomposition(Unit[] decomposition, bool readOnly) : base(decomposition, readOnly)
        {
        }

        protected KC_Decomposition(KC_Decomposition toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_Decomposition_Extensions
    {
        public static Unit[] GetDecomposition(this Unit unit)
        {
            return unit.GetUnitList<KC_Decomposition>();
        }

        public static void SetDecomposition(this Unit unit, Unit[] decomposition)
        {
            unit.SetUnitList<KC_Decomposition>(decomposition);
        }
    }
}
