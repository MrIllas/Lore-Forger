using System;
using System.Collections.Generic;
using UnityEngine;


namespace NovaDot.LoreForger.Data.Save
{
    using Enumerations;

    [Serializable]
    public class LFNodeSaveData 
    {
        [field: SerializeField] public string ID { get; set; } //

        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; } // [field: SerializeField]

        [field: SerializeField] public List<LFChoiceSaveData> Choices { get; set; } //

        [field: SerializeField] public string GroupID { get; set; } //

        [field: SerializeField] public LFDialogueType DialogueType {  get; set; } //

        [field: SerializeField] public Vector2 Position { get; set; } //
    }
}