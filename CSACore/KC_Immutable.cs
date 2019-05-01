using System;
namespace CSA.Core
{
    /*
     * Abstract KnowledgeComponent for storing an immutable property. Used as a base class by KnowledgeComponents that 
     * want to have the option of making their contents immutable.     
     */
    public abstract class KC_Immutable : KnowledgeComponent
    {
        private bool m_immutable;

        public bool Immutable
        {
            get
            {
                return m_immutable;
            }

            set
            {
                if (!m_immutable)
                {
                    m_immutable = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the Immutable properity of an immutable KC_Immutable");
                }
            }
        }

        protected KC_Immutable()
        {
            m_immutable = false;
        }

        protected KC_Immutable(bool immutable)
        {
            m_immutable = immutable;
        }
    }

    public static class KC_Immutable_Extensions
    {
        public static bool GetImmutable(this Unit unit)
        {
            if (unit.HasComponent<KC_Immutable>())
            {
                return unit.GetComponent<KC_Immutable>().Immutable;
            }
            throw new InvalidOperationException("GetImmutable called on unit that does not have a KC_Immutable component.");
        }

        public static void SetImmutable(this Unit unit, bool immutable)
        {
            if (unit.HasComponent<KC_Immutable>())
            {
                unit.GetComponent<KC_Immutable>().Immutable = immutable;
            }
            throw new InvalidOperationException("SetImmutable called on unit that does not have a KC_Immutable component.");
        }
    }
}
