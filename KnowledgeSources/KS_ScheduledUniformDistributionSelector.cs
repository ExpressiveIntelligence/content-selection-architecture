using System;
using System.Collections.Generic;
using System.Linq;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledUniformDistributionSelector : KS_ScheduledFilterSelector
    {
        /*
         * Initializing the enumerator of unique output pool names (static) and the initialization of the DefaultOutputPoolName (instance).
         */
        private static readonly IEnumerator<string> m_OutputPoolNameEnumerator = OutputPoolNameEnumerator("UniformlySelected");
        public override string DefaultOutputPoolName { get; } = GenDefaultOutputPoolName(m_OutputPoolNameEnumerator);

        // Number of items to uniformly select from the input set. 
        public uint NumberToSelect { get; }

        // The seed used for the random number generator
        public int Seed { get; }

        private readonly Random m_random;

        private Unit[] ShuffleUnits(IEnumerable<Unit> units, uint numberToShuffle)
        {
            /*
             * First copy the IEnumerable into an array so I know what I'm working with (if the IEnumerable is a linked list for instance, 
             * then an inplace shuffle will be inefficient). 
             */            
            var shuffledUnits = units.ToArray();

            // Fisher-Yates sorting algorithm
            int n = shuffledUnits.Length;
            // Only need to shuffle the first numberToShuffle elements, since we won't be looking at the rest. 
            for (int i = 0; i < numberToShuffle; i++)
            {
                // Use Next on random with an argument indicating remaining number of units to shuffle (exclusive bound). 
                int r = i + m_random.Next(n - i);
                Unit unit = shuffledUnits[r];
                shuffledUnits[r] = shuffledUnits[i];
                shuffledUnits[i] = unit;
            }

            return shuffledUnits;
        }

        protected override void Execute(object[] boundVars)
        {
            var units = (IEnumerable<Unit>)boundVars[FilteredUnits];

            // If the number of units is <= the number we're supposed to uniformly select, then select all of them
            if (units.Count() <= NumberToSelect)
            {
                base.Execute(boundVars);
            }
            // There are more units to select from than the number we're supposed to select - select from them uniformly
            else
            {
                var shuffledUnits = ShuffleUnits(units, NumberToSelect);
                for (int i = 0; i < NumberToSelect; i++)
                {
                    CopyUnitToOutputPool(shuffledUnits[i]);
                }
            }
        }

        protected Random InitializeRandom(int seed)
        {
            // If the seed has the default value (no seed specified in constructor) use system clock to seed Random. Otherwise use the specified seed.
            return seed == int.MinValue ? new Random() : new Random(seed);
        }

        public KS_ScheduledUniformDistributionSelector(IBlackboard blackboard, int seed = int.MinValue) : base(blackboard)
        {
            m_random = InitializeRandom(seed);
            Seed = seed;
            NumberToSelect = 1;
        }

        public KS_ScheduledUniformDistributionSelector(IBlackboard blackboard, string inputPool, int seed = int.MinValue) :
            base(blackboard, inputPool)
        {
            m_random = InitializeRandom(seed);
            Seed = seed;
            NumberToSelect = 1;
        }

        public KS_ScheduledUniformDistributionSelector(IBlackboard blackboard, string inputPool, string outputPool, uint numberToSelect,
            int seed = int.MinValue) : base(blackboard, inputPool, outputPool)
        {
            m_random = InitializeRandom(seed);
            Seed = seed;
            NumberToSelect = numberToSelect;
        }

        public KS_ScheduledUniformDistributionSelector(IBlackboard blackboard, string inputPool, FilterCondition filter, uint numberToSelect,
            int seed = int.MinValue) : base(blackboard, inputPool, filter)
        {
            m_random = InitializeRandom(seed);
            Seed = seed;
            NumberToSelect = numberToSelect;
        }

        public KS_ScheduledUniformDistributionSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter,
            uint numberToSelect, int seed = int.MinValue) : base(blackboard, inputPool, outputPool, filter)
        {
            m_random = InitializeRandom(seed);
            Seed = seed;
            NumberToSelect = numberToSelect;
        }
    }
}
