using System;

namespace CSA.Core
{
    public static class LinkTypes
    {
        // Link between a Unit and the discrete choices offered (also represented as Units)
        public const string L_Choice = "L_Choice";

        // Link between a ContentUnit and the copy that is a SelectedContentUnit.
        [Obsolete("Replace with L_SeletedUnit")]
        public const string L_SelectedContentUnit = "L_SelectedContentUnit";

        // Link between a Unit and a copy of it that has been filtered by some KS_ScheduledFilterSelector
        public const string L_SelectedUnit = "L_SelectedUnit";

        // fixme: potentially remove since tree links are represented by a KC_TreeNode rather than as links on the blackboard. 
        public const string L_Tree = "L_Tree"; // Link between nodes in a tree
    }
}
