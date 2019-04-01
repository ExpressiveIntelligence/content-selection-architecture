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

        // fixme: haven't experimented yet with specifying specific knowledge bases on a unit-by-unit bases for evaluating prolog queries
        //public const string KnowledgeBaseName_Prolog = "KnowledgeBaseName_Prolog";

        // Content slots
        // fixme: consider using a prefix or different static classes to differentiate between metadata slots and content slots.
        public const string Text = "Text";
    }
}
