using System;
using UnityEngine;

namespace NovaDot.LoreForger.Data.Save
{
    [Serializable]
    public class LFGroupSaveData
    {
        [field: SerializeField] public string ID { get; set; } //

        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; } //
    }
}
