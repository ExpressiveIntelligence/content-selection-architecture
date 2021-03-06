﻿using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledPrintTree : ScheduledKnowledgeSource
    {
        // Name for precondition variable binding
        public const int TreeRoot = 0; 

        protected override object[][] Precondition()
        {
            var treeRoot = from Unit node in m_blackboard.LookupUnits<Unit>()
                           where node.HasComponent<KC_TreeNode>() && node.IsTreeRoot()
                           // fixme: hardcoding in contentpool name just for testing
                           where node.HasComponent<KC_ContentPool>() && node.ContentPoolEquals("GrammarRulePool")
                           select node; 

            if (treeRoot.Any())
            {
                Debug.Assert(treeRoot.Count() == 1);
                object[][] bindings = new object[1][];
                bindings[TreeRoot][0] = new object[] { treeRoot.First() };

                return bindings;
            }
            return m_emptyBindings; 
        }

        protected override void Execute(object[] boundVars)
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

        public KS_ScheduledPrintTree(IBlackboard blackboard) : base(blackboard)
        {
        }
    }
}
