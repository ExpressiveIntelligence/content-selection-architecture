using System.Collections.Generic;

#if UNIT_TEST
using Xunit.Abstractions;
#endif

namespace CSA.KnowledgeSources
{
    public interface IScheduledKnowledgeSource
    {
        IDictionary<string, object> Properties { get; }

        /*
         * Executes the scheduled knowledge source. This consists of calling the precondition in order to bind any 
         * data to operate on and then executing each of the resulting activations.         
         */
        void Execute();

#if UNIT_TEST
        ITestOutputHelper XunitOutput { get; set; }
#endif

    }
}
