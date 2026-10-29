using System;
using UnityEngine;

namespace Cibio
{
    [ExecuteAlways]
    public sealed class WorldLabels : MonoBehaviour
    {
        [Serializable] public struct Label
        {
            public string text;
            public Vector3 position;
            public float size;
            public Color color;
        }
        public Label[] labels;
        private void OnEnable()
        {
            if (transform.childCount > 0 || labels == null) return;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (Label label in labels)
            {
                var item = new GameObject("Lettering");
                item.transform.SetParent(transform, false);
                item.transform.position = label.position;
                var text = item.AddComponent<TextMesh>();
                text.font = font; text.fontSize = 90;
                text.characterSize = label.size / 9f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.text = label.text; text.color = label.color;
                item.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
        }
    }
}
