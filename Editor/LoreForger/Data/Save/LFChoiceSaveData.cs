using System;
using UnityEngine;

namespace NovaDot.LoreForger.Data.Save
{
    [Serializable]
    public class LFChoiceSaveData
    {
        [field: SerializeField] public string Text {  get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}
