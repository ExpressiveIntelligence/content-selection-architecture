using System;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    public class KC_Decomposition : KC_Immutable
    {
        private Unit[] m_decomposition; 

        public Unit[] Decomposition
        {
            get
            {
                // if the KC_Decomposition isn't immutable, just return the decompostion. This means any changes to the individual members of the decomposition are permanent.
                if (!Immutable)
                {
                    return m_decomposition;
                }

                // This KC_Decomposition is immutable, return an array of copies of the Units of the decomposition so that changes to Units aren't permanent. 
                Unit[] decompositionCopy = new Unit[m_decomposition.Length];
                for (int i = 0; i < m_decomposition.Length; i++)
                {
                    decompositionCopy[i] = new Unit(m_decomposition[i]);
                }
                return decompositionCopy;
            }

            set
            {
                if (!Immutable)
                {
                    m_decomposition = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the Decomposition properity of an immutable KC_Decomposition");

                }
            }
        }

        public KC_Decomposition()
        {
            m_decomposition = null;
        }

        public KC_Decomposition(Unit[] decomposition)
        {
            m_decomposition = decomposition;
        }

        public KC_Decomposition(Unit[] decomposition, bool immutable) : base(immutable)
        {
            m_decomposition = decomposition;
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
            throw new InvalidOperationException("SetDecomposition() called on Unit without a KC_Decomposition componenent.");
        }
    }
}
