using System;
namespace CSA.Core
{
    /*
     * Abstract KnowledgeComponent for storing an readOnly property. Used as a base class by KnowledgeComponents that 
     * want to have the option of making their contents readOnly.     
     */
    public abstract class KC_ReadOnly : KnowledgeComponent
    {
        private bool m_readOnly;

        public bool ReadOnly
        {
            get
            {
                return m_readOnly;
            }

            set
            {
                if (!m_readOnly)
                {
                    m_readOnly = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the ReadOnly properity of an readOnly KC_ReadOnly");
                }
            }
        }

        protected KC_ReadOnly()
        {
            m_readOnly = false;
        }
         
        protected KC_ReadOnly(bool readOnly)
        {
            m_readOnly = readOnly;
        }

        protected KC_ReadOnly(KC_ReadOnly toCopy)
        {
            m_readOnly = toCopy.m_readOnly;
        }
    }

    public static class KC_ReadOnly_Extensions
    {
        public static bool GetReadOnly<T>(this Unit unit) where T : KC_ReadOnly
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().ReadOnly;
            }
            throw new InvalidOperationException("GetReadOnly<T> called on unit that does not have a component with KC_ReadOnly parent.");
        }
         
        public static void SetReadOnly<T>(this Unit unit, bool readOnly) where T : KC_ReadOnly
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().ReadOnly = readOnly;
            }
            else
            {
                throw new InvalidOperationException("SetReadOnly<T> called on unit that does not have a component with KC_ReadOnly parent.");
            }
        }
    }
}
