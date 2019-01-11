using CSACore;

namespace KnowledgeUnits
{
    public class U_IDQuery : Unit
    {
        public string ContentUnitID { get; }

        public U_IDQuery(string ID)
        {
            ContentUnitID = ID;
        }
    }
}
