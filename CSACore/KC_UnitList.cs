using System;
namespace CSA.Core
{
    public abstract class KC_UnitList : KC_ReadOnly
    {
        private Unit[] m_unitList; 

        public Unit[] UnitList
        {
            get
            {
                return m_unitList;
            }

            set
            {
                if (!ReadOnly)
                {
                    m_unitList = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the UnitList properity of a readOnly KC_UnitList");
                }
            }
        }

        protected KC_UnitList()
        {
            m_unitList = null;
        }

        protected KC_UnitList(Unit[] unitList)
        {
            m_unitList = unitList;
        }

        protected KC_UnitList(Unit[] unitList, bool readOnly) : base(readOnly)
        {
            m_unitList = unitList;
        }

        /*
         * Makes a shallow copy of the array. So one can reassign specific array indices without changing the original array, but changes to a Unit within the array 
         * will change the Unit in the original array. 
         */
        protected KC_UnitList(KC_UnitList toCopy) : base(toCopy)
        {
            m_unitList = (Unit[])toCopy.UnitList.Clone();
        }
    }

    public static class KC_UnitList_Extensions
    {
        public static Unit[] GetUnitList<T>(this Unit unit) where T : KC_UnitList
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().UnitList;
            }
            throw new InvalidOperationException("GetUnitList() called on Unit without a KC_UnitList componenent.");
        }

        public static void SetUnitList<T>(this Unit unit, Unit[] unitList) where T : KC_UnitList
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().UnitList = unitList;
            }
            else
            {
                throw new InvalidOperationException("SetUnitList() called on Unit without a KC_UnitList componenent.");
            }
        }
    }
}
