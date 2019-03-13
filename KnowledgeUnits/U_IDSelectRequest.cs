using CSA.Core;

namespace CSA.KnowledgeUnits
{
    public class U_IDSelectRequest : Unit
    {
        public static new string TypeName { get; } = new U_IDSelectRequest("").GetType().FullName;

        public string TargetContentUnitID { get; }

        public U_IDSelectRequest(string ID)
        {
            TargetContentUnitID = ID;
        }
    }
}
