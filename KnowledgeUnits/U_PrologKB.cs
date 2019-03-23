using CSA.Core;
using Prolog;

namespace CSA.KnowledgeUnits
{
    public class U_PrologKB : Unit
    {
        public KnowledgeBase KB { get; }
        public string KBName { get; }

        public U_PrologKB(string kbName)
        {
            KB = new KnowledgeBase(kbName, null);
            KBName = kbName;
        }

        public void Consult(string path)
        {
            KB.Consult(path);
        }

        public void Assert(string assertion)
        {
            assertion = assertion.TrimEnd('.'); // Trim final '.' since we'll be wrapping this in "rectract(...)."
            string assertionToAdd = "assert(" + assertion + ").";
            IsTrueParsed(assertionToAdd);
        }

        public void Retract(string assertion)
        {
            assertion = assertion.TrimEnd('.');
            string assertionToRetract = "retract(" + assertion + ").";
            IsTrueParsed(assertionToRetract);
        }

        public bool IsTrueParsed(string query)
        {
            return KB.IsTrue(ISOPrologReader.Read(query));
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
                return KB.SolveFor((LogicVariable)colonExpression.Argument(0), colonExpression.Argument(1), null, false);
            }
            else
            {
                 // Not a colon expression. Use IsTrue to evaluate if the query is true or not. 
                return IsTrueParsed(query) ? (object)true : null;
            }

        }
    }
}
