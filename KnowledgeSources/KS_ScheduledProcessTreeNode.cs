﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSA.Core;
using static CSA.Core.KCNames;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledProcessTreeNode : KS_ScheduledContentPoolCollector
    {
        // Declare delegate for processing tree nodes
        public delegate void ProcessNode(Unit node, IBlackboard blackboard);

        // Field for storing instantiation of process tree node.
        public ProcessNode m_processNode; 

        protected override void Execute(object[] boundVars)
        {
            var units = (IEnumerable<Unit>)boundVars[FilteredUnits];

            if (units.Count() > 1)
            {
                throw new InvalidOperationException("KS_ProcessTreeNode called with " + units.Count() + " selected tree nodes. There should only be 1.");
            }
            if (units.Any())
            {
                m_processNode(units.First(), m_blackboard);
            }
        }

        /*
         * Useful methods for ProcessNode.
         */
         // fixme: consider pushing processing into a component on Units.
        public static void ActivateIDRequest(Unit node, IBlackboard blackboard)
        {
            Unit origNode = FindOriginalUnit(node, blackboard);
            origNode.SetActiveRequest(true);

            /*
             * Save a reference to the current tree node we're expanding on the blackboard. 
             */
            Unit leafExpansionRef = new Unit();
            leafExpansionRef.AddComponent(new KC_UnitReference(CurrentTreeNodeExpansion, true, origNode));
            blackboard.AddUnit(leafExpansionRef);
        }

        public KS_ScheduledProcessTreeNode(IBlackboard blackboard, ProcessNode processNode) : base(blackboard)
        {
            m_processNode = processNode ?? throw new ArgumentException("Null value for processNode passed into KS_ProcessTreeNode consructor");
        }

        public KS_ScheduledProcessTreeNode(IBlackboard blackboard, string inputPool, ProcessNode processNode) : base(blackboard, inputPool)
        {
            m_processNode = processNode ?? throw new ArgumentException("Null value for processNode passed into KS_ProcessTreeNode consructor");
        }

        public KS_ScheduledProcessTreeNode(IBlackboard blackboard, FilterCondition filter, ProcessNode processNode) : base(blackboard, filter)
        {
            m_processNode = processNode ?? throw new ArgumentException("Null value for processNode passed into KS_ProcessTreeNode consructor");
        }

        /*
         * ScheduledFilterSelector constructed with both an input pool and a filter specified using the conjunction of SelectFromPool and filter 
         * as the FilterConditionDel.         
         */
        public KS_ScheduledProcessTreeNode(IBlackboard blackboard, string inputPool, FilterCondition filter, ProcessNode processNode) : base(blackboard, inputPool, filter)
        {
            m_processNode = processNode ?? throw new ArgumentException("Null value for processNode passed into KS_ProcessTreeNode consructor");
        }
    }
}
