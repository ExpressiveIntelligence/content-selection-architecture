using CSA.Core;
using Prolog;

namespace CSA.KnowledgeUnits
{
    public class U_PrologKB : Unit
    {
        public static new string TypeName { get; } = new U_PrologKB("").GetType().FullName;

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

        public bool IsTrueParsed(string query)
        {
            return KB.IsTrue(ISOPrologReader.Read(query));
        }
    }
}
