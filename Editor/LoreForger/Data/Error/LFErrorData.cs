using UnityEngine;

namespace NovaDot.LoreForger.Data.Error
{
    public class LFErrorData
    {
        public Color Color { get; set; }

        public LFErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32(
                (byte) Random.Range(65, 256),
                (byte) Random.Range(50, 176),
                (byte) Random.Range(50, 176),
                255
            );
        }
    }
}