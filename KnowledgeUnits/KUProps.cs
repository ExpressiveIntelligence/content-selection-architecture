namespace CSA.KnowledgeUnits
{
    public static class KUProps
    {
        public const string KSPreconditionMatched = "KSPreconditionMatched";

        public const string IsTreeNode = "IsTreeNode";
        public const string IsLeafNode = "IsLeafNode";
        public const string GrammarTerminal = "GrammarTerminal";
        public const string GrammarNonTerminal = "GrammarNonTerminal";
        public const string WithinTreeLevelCount = "WithinTreeLevelCount";
        public const string CurrentSymbolExpansion = "CurrentSymbolExpansion";

        // fixme: possibly move to enum indicating grammar terminal and non-terminal. But this might change based on the move to multiple-inheritance (extension methods) for ContentUnits
        // public enum GrammarSymbolType { Grammar_NonTerminal, Grammar_Terminal };
    }
}
