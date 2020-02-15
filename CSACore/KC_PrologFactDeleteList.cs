using System;

namespace CSA.Core
{
    /*
     * Knowledge component for storing a list of prolog facts to retract.
     */
    public class KC_PrologFactDeleteList : KC_StringArray
    {
        /*
         * This is the property that uniquely distinguishes KC_PrologFactDeleteList for Json deserialization. 
         */
        [DistinguishingProperty]
        public string[] DeleteList
        {
            get => StringArray;

            set => StringArray = value;
        }

        public override string ToString()
        {
            return ToString("PrologFactDeleteList");
        }

        public void DeleteFacts(KC_PrologKB kb)
        {
            foreach (string fact in DeleteList)
            {
                kb.Retract(fact);
            }
        }

        // fixme: add something to validate add list 

        public override object Clone() => new KC_PrologFactDeleteList(this);

        public KC_PrologFactDeleteList()
        {
        }

        public KC_PrologFactDeleteList(string[] strings) : base(strings)
        {
        }

        public KC_PrologFactDeleteList(string[] strings, bool readOnly) : base(strings, readOnly)
        {
        }

        public KC_PrologFactDeleteList(KC_PrologFactDeleteList toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_PrologFactDeleteList_Extensions
    {
        public static string[] GetPrologFactDeleteList(this Unit unit)
        {
            if (unit.HasComponent<KC_PrologFactDeleteList>())
            {
                return unit.GetComponent<KC_PrologFactDeleteList>().DeleteList;
            }
            throw new InvalidOperationException("GetPrologFactDeleteList called on unit that does not have a KC_PrologFactDeleteList component.");
        }

        public static void SetPrologFactDeleteList(this Unit unit, string[] strings)
        {
            if (unit.HasComponent<KC_PrologFactDeleteList>())
            {
                unit.GetComponent<KC_PrologFactDeleteList>().DeleteList = strings;
            }
            throw new InvalidOperationException("SetPrologFactDeleteList called on unit that does not have a KC_PrologFactDeleteList component.");
        }
    }
}
