using System;
using CSA.Controllers;
using CSA.Core;

namespace CSA.Demo
{
    public class Demo3
    {
        public IBlackboard Blackboard { get; }
        public CFGExpansionController Controller { get; }

        private const string grammarPool = "GrammarRulePool";

        public Demo3()
        {
            Blackboard = new Blackboard();
            Unit expansionTreeRootNode = ContentUnitSetupForDemos.Demo3_DefineCUs(Blackboard, grammarPool);
            Controller = new CFGExpansionController(expansionTreeRootNode, grammarPool, Blackboard);
         }
    }
}
