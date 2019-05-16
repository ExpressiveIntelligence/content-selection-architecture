using System;
using System.Text;
using CSA.Core;
using Prolog;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing a Unity Prolog knowledge base. 
     */
    public class KC_PrologKB : KC_ReadOnly
    {
        /*
         * fixme: will need to store the knowledge base on a GameObject, so will need constructors that accept a GameObject
         * or construct a GameObject. Need to look at Ian's Unity demo to better understand how prolog is embedded.         
         */
        private KnowledgeBase m_KB;

        public KnowledgeBase PrologKB
        {
            get
            {
                return m_KB;
            }

            set
            {
                if (!ReadOnly)
                {
                    m_KB = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the PrologKB properity of a readOnly KC_PrologKB");
                }

            }
        }

        // fixme: replace with Name property on KnowledgeComponent (all KnowledgeComponents can be named). 
        public string KBName { get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(PrologKB: " + KBName);
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

        public override object Clone() => new KC_PrologKB(this);

        /*
         * Given a path to a file containing a prolog program, consult the file to assert all facts and rules in the KB.
         */
        public void Consult(string path)
        {
            PrologKB.Consult(path);
        }

        /*
         * Given a prolog fact or rule, assert it in the KB.
         */
        public void Assert(string assertion)
        {
            assertion = assertion.TrimEnd('.'); // Trim final '.' since we'll be wrapping this in "rectract(...)."
            string assertionToAdd = "assert(" + assertion + ").";
            IsTrueParsed(assertionToAdd); // Evaluate the assert() expression against the KB.
        }

        /*
         * Given a prolog fact or rule, retract it from the KB. 
         */
        public void Retract(string assertion)
        {
            assertion = assertion.TrimEnd('.');
            string assertionToRetract = "retract(" + assertion + ").";
            IsTrueParsed(assertionToRetract); // Evaluate the retract() expression against the KB.
        }

        /*
         * Given a prolog expression, evaluate it against the KB. 
         */
        public bool IsTrueParsed(string query)
        {
            return PrologKB.IsTrue(ISOPrologReader.Read(query));
        }

        /* 
         * Returns null if the query doesn't succeed, and a binding (which will be a Symbol or a Structure) if it does. If the variable on the left hand
         * side of the colon expression doesn't appear in the query, the Symbol will be a gensym.      
         * If the query isn't a colon expression, will return the bool true to indicate query success, and null to indicate failure. 
         */
        public object SolveForParsed(string query)
        {
            object parsedQuery = ISOPrologReader.Read(query);

            if (parsedQuery is Structure colonExpression && colonExpression.IsFunctor(Symbol.Colon, 2))
            {
                // The parsedQuery is a colon expression. Use KB.solveFor to generate a binding.
                return PrologKB.SolveFor((LogicVariable)colonExpression.Argument(0), colonExpression.Argument(1), null, false);
            }
            else
            {
                // Not a colon expression. Use IsTrue to evaluate if the query is true or not. 
                return IsTrueParsed(query) ? (object)true : null;
            }
        }

        public KC_PrologKB(string kbName)
        {
            m_KB = new KnowledgeBase(kbName, null);
            KBName = kbName;
        }

        public KC_PrologKB(string kbName, bool readOnly) : base(readOnly)
        {
            m_KB = new KnowledgeBase(kbName, null);
            KBName = kbName;
        }

        /*
         * Create a shallow copy of the KC_PrologKB.
         */
        public KC_PrologKB(KC_PrologKB toCopy) : base(toCopy)
        {
            m_KB = toCopy.m_KB;
            KBName = toCopy.KBName;
        }
    }

    public static class KC_PrologKB_Extensions
    {
        public static KnowledgeBase GetPrologKB(this Unit unit)
        {
            if (unit.HasComponent<KC_PrologKB>())
            {
                return unit.GetComponent<KC_PrologKB>().PrologKB;
            }
            throw new InvalidOperationException("GetPrologKB called on unit that does not have a KC_PrologKB component.");
        }

        public static void SetPrologKB(this Unit unit, KnowledgeBase prologKB) 
        {
            if (unit.HasComponent<KC_PrologKB>())
            {
                unit.GetComponent<KC_PrologKB>().PrologKB = prologKB;
            }
            else
            {
                throw new InvalidOperationException("SetPrologKB called on unit that does not have a KC_PrologKB component.");
            }
        }

        public static string GetKBName(this Unit unit)
        {
            if (unit.HasComponent<KC_PrologKB>())
            {
                return unit.GetComponent<KC_PrologKB>().KBName;
            }
            else
            {
                throw new InvalidOperationException("GetKBName called on unit that does not have a KC_PrologKB component.");
            }
        }
    }

}
