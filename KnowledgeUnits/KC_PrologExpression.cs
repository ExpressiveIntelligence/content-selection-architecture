using System;
using System.Text;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing a prolog expression. 
     */
    public class KC_PrologExpression : KC_ReadOnlyString
    {
        public string PrologExp
        {
            get => StringValue;

            set => StringValue = value;
        }

        public string PrologExpName { get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendFormat("Name: {0}, PrologExp: {1}", PrologExpName, PrologExp);
            if (ReadOnly)
            {
                sb.Append(", readonly)");
            }
            else
            {
                sb.Append(")");
            }
            return sb.ToString();
        }

        public override object Clone() => new KC_PrologExpression(this);

        /*
         * fixme: add a method to validate prolog expressions. 
         */

        public KC_PrologExpression(string name)
        {
            PrologExpName = name;
        }

        public KC_PrologExpression(string name, string prologExp) : base(prologExp)
        {
            PrologExpName = name;
        }

        public KC_PrologExpression(string name, string prologExp, bool readOnly) : base(prologExp, readOnly)
        {
            PrologExpName = name;
        }

        protected KC_PrologExpression(KC_PrologExpression toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_PrologExpression_Extensions
    {
        public static string GetPrologExpName(this Unit unit)
        {
            if (unit.HasComponent<KC_PrologExpression>())
            {
                return unit.GetComponent<KC_PrologExpression>().PrologExpName;
            }
            throw new InvalidOperationException("GetPrologExpName called on unit that does not have a KC_PrologExpression component.");
        }

        public static string GetPrologExp(this Unit unit)
        {
            if (unit.HasComponent<KC_PrologExpression>())
            {
                return unit.GetComponent<KC_PrologExpression>().PrologExp;
            }
            throw new InvalidOperationException("GetPrologExp called on unit that does not have a KC_PrologExpression component.");
        }

        public static string SetPrologExp(this Unit unit, string prologExp)
        {
            if (unit.HasComponent<KC_PrologExpression>())
            {
                unit.GetComponent<KC_PrologExpression>().PrologExp = prologExp;
            }
            throw new InvalidOperationException("SetPrologExp called on unit that does not have a KC_PrologExpression component.");
        }
    }
}
