using CSA.Core;

namespace CSA.KnowledgeSources
{
    /*
     * Shared interface for both reactive and scheduled choice presenters.
     */
    public interface I_KC_ChoicePresenter
    {
        void SelectChoice(Unit[] choices, uint choiceMade);
    }
}
