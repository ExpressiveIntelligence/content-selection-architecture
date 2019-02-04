using CSA.Core;

namespace CSA.KnowledgeUnits
{
    public class U_IDQuery : Unit
    {
        public static new string TypeName { get; } = new U_IDQuery("").GetType().FullName;

        public string TargetContentUnitID { get; }

        public U_IDQuery(string ID)
        {
            TargetContentUnitID = ID;
        }
    }
}
