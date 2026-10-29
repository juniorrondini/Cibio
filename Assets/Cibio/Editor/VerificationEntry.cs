using UnityEditor;
using UnityEditor.SceneManagement;

namespace Cibio.Editor
{
    public static class VerificationEntry
    {
        public static void Run()
        {
            PrototypeTools.CapturePreview();
            EditorSceneManager.OpenScene(PrototypeTools.ScenePath);
            SessionState.SetBool("Cibio.Verify", true);
            EditorApplication.isPlaying = true;
        }
    }
}
