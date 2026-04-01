using System.Collections.Generic;
using UnityEngine;


namespace NovaDot.LoreForger.SO
{
    using Data;
    using Enumerations;

    public class LFDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName {  get; set; }
        [field: SerializeField] public string Text { get; set; } //

        [field: SerializeField] public List<LFDialogueChoiceData> Choices { get; set; } //

        [field: SerializeField] public LFDialogueType DialogueType { get; set; } //

        [field: SerializeField] public bool IsStartingDialogue { get; set; } //

        public void Initialize (string dialogueName, string text, List<LFDialogueChoiceData> choices, LFDialogueType dialogueType, bool isStartingDialogue )
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
        }
    }   
}