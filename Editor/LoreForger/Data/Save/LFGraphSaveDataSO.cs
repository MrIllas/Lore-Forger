using System.Collections.Generic;
using UnityEngine;

namespace NovaDot.LoreForger.Data.Save
{
    public class LFGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }

        [field: SerializeField] public List<LFGroupSaveData> Groups { get; set; } //
        [field: SerializeField] public List<LFNodeSaveData> Nodes { get; set; } //

        [field: SerializeField] public List<string> OldGroupNames { get; set; } //
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; } //

        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; } //

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<LFGroupSaveData>();
            Nodes = new List<LFNodeSaveData>();
        }
    }
}
