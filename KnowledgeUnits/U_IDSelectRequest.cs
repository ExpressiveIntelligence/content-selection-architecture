using CSA.Core;

namespace CSA.KnowledgeUnits
{
    public class U_IDSelectRequest : Unit
    {
        public string TargetContentUnitID { get; }

        public U_IDSelectRequest(string ID)
        {
            TargetContentUnitID = ID;
        }
    }
}
