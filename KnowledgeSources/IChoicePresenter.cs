using CSA.Core;

namespace CSA.KnowledgeSources
{
    /*
     * Shared interface for both reactive and scheduled choice presenters.
     */
    public interface IChoicePresenter
    {
        void SelectChoice(ContentUnit[] choices, uint choiceMade);
    }
}
