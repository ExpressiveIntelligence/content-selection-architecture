using System;
using System.Text;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    public class KC_Decomposition : KC_ReadOnly
    {
        private Unit[] m_decomposition;

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
            foreach(var unit in m_decomposition)
            {
                sb.AppendLine("   " + unit);
            }
            return sb.ToString();
        }

        public Unit[] Decomposition
        {
            // Since m_decomposition is an array, it is already immutable. Don't need to test whether component is readOnly or not. 
            // fixme: not doing a deep copy of Units, so they still could be changed. Should implement readOnly on Units as well. 
            get => m_decomposition;

            set
            {
                if (!ReadOnly)
                {
                    m_decomposition = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the Decomposition properity of a readOnly KC_Decomposition");

                }
            }
        }

        public override object Clone() => new KC_Decomposition(this);

        public KC_Decomposition()
        {
            m_decomposition = null;
        }

        public KC_Decomposition(Unit[] decomposition)
        {
            m_decomposition = decomposition;
        }

        public KC_Decomposition(Unit[] decomposition, bool readOnly) : base(readOnly)
        {
            m_decomposition = decomposition;
        }

        protected KC_Decomposition(KC_Decomposition toCopy) : base (toCopy)
        {
            // Since m_decomposition is an array, copying reference to the same array rather than creating an array copy.  
            m_decomposition = toCopy.m_decomposition;
        }
    }

    public static class KC_Decomposition_Extensions
    {
        public static Unit[] GetDecomposition(this Unit unit)
        {
            if (unit.HasComponent<KC_Decomposition>())
            {
                return unit.GetComponent<KC_Decomposition>().Decomposition;
            }
            throw new InvalidOperationException("GetDecomposition() called on Unit without a KC_Decomposition componenent.");
        }

        public static void SetDecomposition(this Unit unit, Unit[] decomposition)
        {
            if (unit.HasComponent<KC_Decomposition>())
            {
                unit.GetComponent<KC_Decomposition>().Decomposition = decomposition;
            }
            else
            {
                throw new InvalidOperationException("SetDecomposition() called on Unit without a KC_Decomposition componenent.");
            }
        }
    }
}
