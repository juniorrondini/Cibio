using UnityEngine;

namespace Cibio.City
{
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public sealed class CityColorGrade : MonoBehaviour
    {
        public Material grading;
        [Range(.5f, 2)] public float exposure = 1;
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (grading) { grading.SetFloat("_Exposure", exposure); Graphics.Blit(source, destination, grading); }
            else Graphics.Blit(source, destination);
        }
    }
}
