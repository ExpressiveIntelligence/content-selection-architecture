using CSA.Core;
using static CSA.Demo.ContentUnitSetupForDemos;

namespace CSA.Demo
{
    public class DemoEnsembleLite
    {
        public IBlackboard Blackboard { get; }

        public DemoEnsembleLite()
        {
            Blackboard = new Blackboard();
            DemoEnsemble_DefineUnits(Blackboard);

        }
    }
}
