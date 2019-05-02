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
        public static bool GetImmutable<T>(this Unit unit) where T : KC_Immutable
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().Immutable;
            }
            throw new InvalidOperationException("GetImmutable<T> called on unit that does not have a component with KC_Immutable parent.");
        }

        public static void SetImmutable<T>(this Unit unit, bool immutable) where T : KC_Immutable
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().Immutable = immutable;
            }
            else
            {
                throw new InvalidOperationException("SetImmutable<T> called on unit that does not have a component with KC_Immutable parent.");
            }
        }
    }
}
