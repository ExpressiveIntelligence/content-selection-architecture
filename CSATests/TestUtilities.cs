using Xunit;

using CSA.Core;

namespace CSA.Tests
{
    public static class TestUtilities
    {
        public static void AddUnits(IBlackboard blackboard, IUnit[] unitsToAdd)
        {
            foreach(IUnit unit in unitsToAdd)
            {
                Assert.True(blackboard.AddUnit(unit));
            }
        }

        public static void RemoveUnits(IBlackboard blackboard, IUnit[] unitsToRemove)
        {
            foreach(IUnit unit in unitsToRemove)
            {
                Assert.True(blackboard.RemoveUnit(unit));
            }
        }
    }
}
