namespace CSA.KnowledgeUnits
{
    public static class CUSlots
    {
        // Metadata slots
        // fixme: consider using a prefix or different static classes to differentiate between metadata slots and content slots.
        public const string ContentUnitID = "ContentUnitID";
        public const string SelectedContentUnit = "SelectedContentUnit";
        public const string TargetContentUnitID = "TargetContentUnitID";
        public const string ApplTest_Prolog = "ApplTest_Prolog";
        public const string ApplTestResult = "ApplTestResult";
        public const string ApplTestBindings_Prolog = "ApplTestBindings_Prolog";
        public const string ContentPool = "ContentPool";
        public const string FactDeleteList_Prolog = "FactDeleteList_Prolog";
        public const string FactAddList_Prolog = "FactAddList_Prolog";
        public const string Specificity = "Specificity";
        public const string GrammarRuleRHS = "GrammarRuleRHS"; // fixme: may want to generalize this to something like TreeChildren for general tree expansion 

        /*
         * Slot name definitions for tag interfaces. 
         */

        /*
         * ITreeNode slots
         */
        public const string ITreeNode_Parent = "ITreeNode_Parent";
        public const string ITreeNode_Children = "ITreeNode_Children";

        /*
         * ITargetUnitID slots
         */
        public const string ITargetUnitID_ID = "ITargetUnitID_ID";
        public const string ITargetUnitID_Inited = "ITargetUnitID_Inited";

        /*
         * IText slots
         */
        public const string IText_String = "IText_String";
        public const string IText_Inited = "IText_Inited";

        /*
         * IDecomposition slots
         */

        public const string IDecomposition_UnitList = "IDecomposition_UnitList";

        /*
         * IChoiceOptions slots  
         */
        public const string IChoiceOptions_UnitList = "IChoiceOptions_UnitList";

        // fixme: haven't experimented yet with specifying specific knowledge bases on a unit-by-unit bases for evaluating prolog queries
        //public const string KnowledgeBaseName_Prolog = "KnowledgeBaseName_Prolog";

        // Content slots
        // fixme: consider using a prefix or different static classes to differentiate between metadata slots and content slots.
        public const string Text = "Text";
    }
}
