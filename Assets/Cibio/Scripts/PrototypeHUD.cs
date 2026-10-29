using UnityEngine;

namespace Cibio
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        public PlayerMotor player;
        private GUIStyle title, small, body, big;
        private readonly Color ink = new Color(.06f, .14f, .14f);
        private readonly Color paper = new Color(.97f, .95f, .88f);
        private readonly Color yellow = new Color(1f, .81f, .20f);
        private void OnGUI()
        {
            if (player == null) return;
            if (title == null)
            {
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                title = Style(font, 26, FontStyle.Bold, paper);
                small = Style(font, 12, FontStyle.Bold, paper);
                body = Style(font, 16, FontStyle.Normal, paper);
                big = Style(font, 38, FontStyle.Bold, yellow);
            }
            Matrix4x4 saved = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 810f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1440 * scale) / 2f,
                (Screen.height - 810 * scale) / 2f), Quaternion.identity, Vector3.one * scale);
            Box(new Rect(32, 30, 415, 97), ink);
            Box(new Rect(32, 30, 5, 97), yellow);
            GUI.Label(new Rect(54, 43, 375, 20), "CIBIO  /  PROTÓTIPO 01", small);
            GUI.Label(new Rect(54, 66, 375, 36), "O Brasil virou uma Vuvuzela", title);
            Box(new Rect(1100, 30, 308, 74), ink);
            GUI.Label(new Rect(1120, 43, 265, 22), "MOVIMENTO + CÂMERA   /   2.5D", small);
            Box(new Rect(1120, 78, 267, 4), new Color(.25f, .35f, .34f));
            Box(new Rect(1120, 78, 267 * Mathf.Clamp01(player.FurthestX / 52f), 4), yellow);
            Box(new Rect(32, 728, 1377, 52), ink);
            GUI.Label(new Rect(54, 742, 1000, 24), "A / D  mover       SHIFT  correr       ESPAÇO  pular (segure para ir mais alto)       R  recomeçar       ESC  pausar", body);
            GUI.Label(new Rect(1130, 743, 260, 22), player.Grounded ? "NO CHÃO  •  " + Mathf.Abs(player.Motion.HorizontalSpeed).ToString("0.0") + " m/s" : "NO AR", small);
            if (player.Paused || player.Completed)
            {
                Box(new Rect(390, 282, 660, 215), ink);
                GUI.Label(new Rect(430, 314, 590, 55), player.Paused ? "RESPIRO MERECIDO." : "UM PULO DE CADA VEZ.", big);
                GUI.Label(new Rect(430, 386, 590, 54), player.Paused ? "ESC para continuar.\nR retoma do último ponto de respiro." : "Percurso de teste concluído.\nMovimentação, personagem e câmera: primeira base.", body);
            }
            GUI.matrix = saved;
        }
        private static GUIStyle Style(Font font, int size, FontStyle weight, Color color)
        { var style = new GUIStyle { font = font, fontSize = size, fontStyle = weight }; style.normal.textColor = color; return style; }
        private static void Box(Rect rect, Color color)
        { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
    }
}
