using System;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    /*
     * Shared interface for both reactive and scheduled choice presenters.
     */
    [Obsolete("Use KnowledgeComponent-based version of this interface.")]
    public interface IChoicePresenter_Old
    {
        void SelectChoice(ContentUnit[] choices, uint choiceMade);
    }
}
