using System;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    [Obsolete("Use KC_IDSelectionRequest KnowledgeComponent.")]
    public class U_IDSelectRequest : Unit
    {
        public string TargetContentUnitID { get; }

        public U_IDSelectRequest(string ID)
        {
            TargetContentUnitID = ID;
        }
    }
}
