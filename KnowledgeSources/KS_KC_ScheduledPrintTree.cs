using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledPrintTree : ScheduledKnowledgeSource
    {
        // Name for precondition variable binding
        // fixme: replace the whole dictionary structure thing with typed tuples to pass precondition bindings. 
        public const string TreeRoot = "TreeRoot";

        protected override IDictionary<string, object>[] Precondition()
        {
            var treeRoot = from Unit node in m_blackboard.LookupUnits<Unit>()
                           where node.HasComponent<KC_TreeNode>() && node.IsTreeRoot()
                           // fixme: hardcoding in contentpool name just for testing
                           where node.HasComponent<KC_ContentPool>() && node.ContentPoolEquals("GrammarRulePool")
                           select node; 

            if (treeRoot.Any())
            {
                Debug.Assert(treeRoot.Count() == 1);
                IDictionary<string, object>[] bindings = new Dictionary<string, object>[1];
                bindings[0] = new Dictionary<string, object>
                {
                    [TreeRoot] = treeRoot.First()
                };
                return bindings;
            }
            return m_emptyBindings; 
        }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            Unit rootNode = (Unit)boundVars[TreeRoot];
            Console.WriteLine(rootNode);
            foreach(var child in rootNode.GetTreeChildren())
            {
                PrintTreeHelper(1, child.ContainingUnit);
            }
        }

        private static void PrintTreeHelper(uint indent, Unit node)
        {
            Console.WriteLine(IndentLines(indent, node.ToString()));
            foreach (var child in node.GetTreeChildren())
            {
                PrintTreeHelper(indent + 1, child.ContainingUnit);
            }

        }

        private static string IndentLines(uint indent, string s)
        {
            StringReader reader = new StringReader(s);
            StringBuilder sb = new StringBuilder(100);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Indent(indent, sb);
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        private static void Indent(uint indent, StringBuilder sb)
        {
            for (uint i = 0; i < indent; i++)
            {
                sb.Append("  ");
            }
        }

        public KS_KC_ScheduledPrintTree(IBlackboard blackboard) : base(blackboard)
        {
        }
    }
}
