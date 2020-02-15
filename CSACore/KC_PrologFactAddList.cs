using System;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing a list of prolog facts to assert. 
     */
    public class KC_PrologFactAddList : KC_StringArray
    {
        public string[] AddList
        {
            get => StringArray;

            set => StringArray = value;
        }

        public override string ToString()
        {
            return ToString("PrologFactAddList");
        }

        public void AddFacts(KC_PrologKB kb)
        {
            foreach (string fact in AddList)
            {
                kb.Assert(fact);
            }
        }

        // fixme: add something to validate add list 

        public override object Clone() => new KC_PrologFactAddList(this);

        public KC_PrologFactAddList()
        {
        }

        public KC_PrologFactAddList(string[] strings) : base(strings)
        {
        }

        public KC_PrologFactAddList(string[] strings, bool readOnly) : base(strings, readOnly)
        {
        }

        public KC_PrologFactAddList(KC_PrologFactAddList toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_PrologFactAddList_Extensions
    {
        public static string[] GetPrologFactAddList(this Unit unit)
        {
            if (unit.HasComponent<KC_PrologFactAddList>())
            {
                return unit.GetComponent<KC_PrologFactAddList>().AddList;
            }
            throw new InvalidOperationException("GetPrologFactAddList called on unit that does not have a KC_PrologFactAddList component.");
        }

        public static void SetPrologFactAddList(this Unit unit, string[] strings)
        {
            if (unit.HasComponent<KC_PrologFactAddList>())
            {
                unit.GetComponent<KC_PrologFactAddList>().AddList = strings;
            }
            throw new InvalidOperationException("SetPrologFactAddList called on unit that does not have a KC_PrologFactAddList component.");
        }

    }
}
