using System.Collections.Generic;

namespace CSACore
{
    public interface IBlackboard
    {
        void AddUnit(IUnit u);

        bool DeleteUnit(IUnit u);

        ISet<IUnit> LookupUnits(string unitType);

        bool ContainsUnit(IUnit u);
     }
}