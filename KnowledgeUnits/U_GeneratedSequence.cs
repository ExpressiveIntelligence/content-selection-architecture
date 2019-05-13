using System;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * U_GeneratedSequence is deprecated. Add a KC_Sequence KnowledgeComponent to a Unit for the same functionality. 
     */
    [Obsolete("U_GeneratedSequence is deprecated. Add a KC_Sequence KnoweldgeComponent to a Unit for the same functionality.")]
    public class U_GeneratedSequence : Unit
    {
        public IUnit[] Sequence { get; }

        public U_GeneratedSequence(IUnit[] sequence)
        {
            Sequence = sequence;
        }
    }
}
