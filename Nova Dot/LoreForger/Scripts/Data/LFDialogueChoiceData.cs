using System;
using UnityEngine;

namespace NovaDot.LoreForger.Data
{
    using NovaDot.LoreForger.SO;

    [Serializable]
    public class LFDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        public LFDialogueSO NextDialogue { get; set; } //
    }
}