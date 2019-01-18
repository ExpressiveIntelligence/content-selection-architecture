using CSACore;

namespace KnowledgeUnits
{
    public class U_IDQuery : Unit
    {
        public new static string TypeName { get; } = new U_IDQuery("").GetType().FullName;

        public string ContentUnitID { get; }

        public U_IDQuery(string ID)
        {
            ContentUnitID = ID;
        }
    }
}
