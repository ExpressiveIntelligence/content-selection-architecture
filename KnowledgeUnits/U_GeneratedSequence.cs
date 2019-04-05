using CSA.Core;

namespace CSA.KnowledgeUnits
{
    public class U_GeneratedSequence : Unit
    {
        public IUnit[] Sequence { get; }

        public U_GeneratedSequence(IUnit[] sequence)
        {
            Sequence = sequence;
        }
    }
}
