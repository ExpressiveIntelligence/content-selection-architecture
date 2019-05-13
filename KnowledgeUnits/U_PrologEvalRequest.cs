using System;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    [Obsolete("Create a KC_PrologEvalRequest KnowledgeComponent.")]
    public class U_PrologEvalRequest : Unit
    {
        // fixme: to get prolog working at first not implementing pools but will come back to this
        public string InputPool { get; }
        public string OutputPool { get; }

        // Name of the slot containing the prolog query to evaluate
        public string PrologQuerySlotName { get; }

        // Name of the slot that the result of evaluating the query will be written to.
        // fixme: for now just returning a boolean result. Need to figure out how to return bound variables. 
        public string PrologResultSlotName { get; }

        public string PrologBoundVarSlotName { get; }

        public U_PrologEvalRequest(string prologQuerySlotName, string prologResultSlotName)
        {
            PrologQuerySlotName = prologQuerySlotName;
            PrologResultSlotName = prologResultSlotName;
            PrologBoundVarSlotName = null;
        }

        public U_PrologEvalRequest(string prologQuerySlotName, string prologResultSlotName, string prologBoundVarSlotName)
        {
            PrologQuerySlotName = prologQuerySlotName;
            PrologResultSlotName = prologResultSlotName;
            PrologBoundVarSlotName = prologBoundVarSlotName;
        }

    }
}
