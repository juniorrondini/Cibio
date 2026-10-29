using UnityEngine;

namespace Cibio.City
{
    [ExecuteAlways,RequireComponent(typeof(TextMesh))]
    public sealed class WorldLettering : MonoBehaviour
    {
        public Shader depthShader;
        private static Material worldFont;
        private TextMesh text;
        private void OnEnable()
        {
            text=GetComponent<TextMesh>();Font.textureRebuilt+=Refresh;
            text.font.RequestCharactersInTexture(text.text,text.fontSize,text.fontStyle);Refresh(text.font);
        }
        private void OnDisable(){Font.textureRebuilt-=Refresh;}
        private void Refresh(Font font)
        {
            if(!text||font!=text.font)return;
            if(!worldFont)worldFont=new Material(depthShader?depthShader:Shader.Find("Cibio/World Lettering")){name="Depth-tested font",hideFlags=HideFlags.HideAndDontSave};
            worldFont.mainTexture=font.material.mainTexture;GetComponent<Renderer>().sharedMaterial=worldFont;
        }
    }
}
