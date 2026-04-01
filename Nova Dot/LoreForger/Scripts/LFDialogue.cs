using UnityEngine;

namespace NovaDot.LoreForger
{
    using SO;
    public class LFDialogue : MonoBehaviour
    {
        /* Dialogue Scriptable Objects */
        [SerializeField] private LFDialogueContainerSO dialogueContainer;
        [SerializeField] private LFDialogueGroupSO dialogueGroup;
        [SerializeField] private LFDialogueSO dialogue;

        /* Filters */
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;
    }
}