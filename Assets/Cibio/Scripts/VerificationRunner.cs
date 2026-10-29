#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cibio
{
    // Only runs when explicitly launched by the editor verification command.
    public sealed class VerificationRunner : MonoBehaviour
    {
        private PlayerMotor player;
        private readonly List<string> results = new List<string>();
        private int failures;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Launch()
        {
            if (!SessionState.GetBool("Cibio.Verify", false)) return;
            SessionState.SetBool("Cibio.Verify", false);
            new GameObject("Cibio verification").AddComponent<VerificationRunner>();
        }

        private IEnumerator Start()
        {
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0;
            player = FindAnyObjectByType<PlayerMotor>(); player.AutomatedInput = true;
            yield return new WaitForSeconds(.5f);
            Check(player.Grounded, "Personagem pousa no piso");
            Check(Mathf.Abs(player.transform.position.z) < .001f, "Movimento preso ao plano 2.5D");

            Warp(new Vector3(-5, .05f, 0)); yield return new WaitForSeconds(.2f);
            player.TestInput = new MotionInput { horizontal = 1 };
            yield return new WaitForSeconds(.6f);
            Check(player.transform.position.x > -2.5f, "Caminhada desloca o personagem com colisao");
            Check(Mathf.Abs(player.Motion.HorizontalSpeed - player.settings.walkSpeed) < .1f, "Velocidade de caminhada");
            player.TestInput = default; yield return new WaitForSeconds(.3f);
            Check(Mathf.Abs(player.Motion.HorizontalSpeed) < .01f, "Frenagem ao soltar direcao");

            Warp(new Vector3(-5, .05f, 0)); yield return new WaitForSeconds(.2f);
            player.TestInput = new MotionInput { horizontal = 1, sprint = true };
            yield return new WaitForSeconds(.6f);
            Check(player.transform.position.x > -1.4f, "Corrida ganha distancia");
            Check(Mathf.Abs(player.Motion.HorizontalSpeed - player.settings.runSpeed) < .1f, "Velocidade de corrida");

            Warp(new Vector3(-4, .05f, 0)); yield return new WaitForSeconds(.2f);
            player.TestInput = new MotionInput { jumpPressed = true, jumpHeld = true };
            yield return null;
            player.TestInput = new MotionInput { jumpHeld = true };
            float fullPeak = 0;
            float end = Time.time + 1.3f;
            while (Time.time < end) { fullPeak = Mathf.Max(fullPeak, player.transform.position.y); yield return null; }
            Check(fullPeak > 2.25f && fullPeak < 2.8f, "Altura do pulo longo: " + fullPeak.ToString("0.00") + " m");
            Check(player.Grounded, "Personagem aterrissa apos pular");

            Warp(new Vector3(-4, .05f, 0)); yield return new WaitForSeconds(.2f);
            player.TestInput = new MotionInput { jumpPressed = true, jumpHeld = true };
            yield return null;
            player.TestInput = new MotionInput { jumpHeld = true };
            yield return new WaitForSeconds(.08f);
            player.TestInput = default;
            float shortPeak = 0;
            end = Time.time + 1f;
            while (Time.time < end) { shortPeak = Mathf.Max(shortPeak, player.transform.position.y); yield return null; }
            Check(shortPeak > .6f && shortPeak < fullPeak * .7f, "Pulo curto ao soltar comando: " + shortPeak.ToString("0.00") + " m");

            Warp(new Vector3(-4, .13f, 0));
            player.TestInput = new MotionInput { jumpPressed = true, jumpHeld = true };
            yield return null;
            player.TestInput = new MotionInput { jumpHeld = true };
            yield return new WaitForSeconds(.35f);
            Check(player.transform.position.y > .6f, "Buffer aceita pulo antes de tocar o piso");

            Warp(new Vector3(-8.5f, .05f, 0)); yield return new WaitForSeconds(.2f);
            player.TestInput = new MotionInput { horizontal = -1 };
            end = Time.time + 1f;
            while (player.Grounded && Time.time < end) yield return null;
            yield return new WaitForSeconds(.035f);
            player.TestInput = new MotionInput { jumpPressed = true, jumpHeld = true };
            yield return null;
            player.TestInput = new MotionInput { jumpHeld = true };
            yield return new WaitForSeconds(.08f);
            Check(player.Motion.VerticalSpeed > 1f, "Tolerancia de pulo depois da borda");

            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.transform.position = new Vector3(-4, 2.8f, 0); ceiling.transform.localScale = new Vector3(3, .3f, 3);
            Warp(new Vector3(-4, .05f, 0)); yield return new WaitForSeconds(.2f);
            player.TestInput = new MotionInput { jumpPressed = true, jumpHeld = true };
            yield return null; player.TestInput = new MotionInput { jumpHeld = true };
            yield return new WaitForSeconds(.2f);
            Check(player.Motion.VerticalSpeed <= 0f && player.transform.position.y < .7f, "Colisao com teto interrompe subida");
            Destroy(ceiling);

            Warp(new Vector3(19, .05f, 0)); yield return new WaitForSeconds(.3f);
            Check(player.CheckpointName == "Ponto de respiro", "Checkpoint atualiza o retorno");
            int respawns = player.Respawns;
            Warp(new Vector3(25, -8, 0)); yield return new WaitForSeconds(.3f);
            Check(player.Respawns > respawns && Mathf.Abs(player.transform.position.x - 19) < .1f, "Queda retorna ao checkpoint");
            var camera = FindAnyObjectByType<SideCamera>();
            Check(Vector3.Distance(camera.transform.position, player.transform.position + Vector3.up * 1.65f + camera.offset) < .5f,
                "Camera reposiciona no retorno");

            // Return to the initial framing for a real Play Mode capture.
            Warp(new Vector3(0, .05f, 0)); yield return new WaitForSeconds(.5f);
            camera.Snap();
            CapturePlayView(camera.GetComponent<Camera>());
            yield return new WaitForSeconds(.3f);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(Application.dataPath), "Verification.txt"), results);
            Debug.Log("CIBIO_VERIFY: " + results.Count + " verificacoes, " + failures + " falhas.");
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }

        private void Warp(Vector3 position)
        {
            player.TestInput = default;
            var controller = player.GetComponent<CharacterController>(); controller.enabled = false;
            player.transform.position = position; controller.enabled = true; player.Motion.Reset();
            Physics.SyncTransforms(); FindAnyObjectByType<SideCamera>().Snap();
        }
        private static void CapturePlayView(Camera camera)
        {
            // Batch mode has no Game View backbuffer; render the real play camera explicitly.
            var render = new RenderTexture(1600, 900, 24);
            var previous = RenderTexture.active;
            camera.targetTexture = render; camera.Render(); RenderTexture.active = render;
            var texture = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); texture.Apply();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath), "Preview-PlayMode.png"), texture.EncodeToPNG());
            RenderTexture.active = previous; camera.targetTexture = null;
            Destroy(texture); Destroy(render);
        }
        private void Check(bool value, string text)
        {
            results.Add((value ? "PASS " : "FAIL ") + text);
            if (!value) failures++;
        }
    }
}
#endif
