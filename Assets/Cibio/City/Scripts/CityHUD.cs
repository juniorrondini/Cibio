using UnityEngine;

namespace Cibio.City
{
    public sealed class CityHUD : MonoBehaviour
    {
        public CityMotor player;
        private GUIStyle small, title, body, large;
        private readonly Color ink = new Color(.055f, .105f, .10f, .94f), cream = new Color(.96f, .94f, .86f);
        private readonly Color gold = new Color(.96f, .75f, .28f), green = new Color(.25f, .69f, .51f);
        public static bool Hide { get; set; }
        private void OnGUI()
        {
            if (!player || Hide) return;
            if (title == null)
            {
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                small = Style(font, 12, true, cream); title = Style(font, 24, true, cream);
                body = Style(font, 16, false, cream); large = Style(font, 36, true, gold);
            }
            var original = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 810f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1440 * scale) / 2, (Screen.height - 810 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            Panel(new Rect(28, 28, 354, 88), ink); Panel(new Rect(28, 28, 4, 88), gold);
            GUI.Label(new Rect(48, 40, 310, 20), "CIBIO   /   BAIRRO 01", small);
            GUI.Label(new Rect(48, 65, 310, 34), "Praça do Respiro", title);
            var life = player.GetComponent<Vitality>();
            Panel(new Rect(1110, 28, 302, 88), ink);
            GUI.Label(new Rect(1130, 41, 260, 23), "FÔLEGO DO BRASILEIRO", small);
            Panel(new Rect(1130, 76, 260, 12), new Color(.19f, .26f, .23f));
            Panel(new Rect(1130, 76, 260 * life.Current / life.maximum, 12), life.Current > 30 ? green : gold);
            Panel(new Rect(28, 731, 1384, 49), ink);
            GUI.Label(new Rect(48, 745, 1050, 24), "WASD / SETAS  explorar      SHIFT  correr      ESPAÇO  pular      J / CLIQUE  atacar      ESC  pausar", body);
            GUI.Label(new Rect(1180, 746, 210, 22), "DERROTADOS   " + MeleeCombat.DefeatedEnemies.ToString("00"), small);
            if (player.Paused || life.IsDead)
            {
                Panel(new Rect(404, 291, 632, 199), ink);
                GUI.Label(new Rect(440, 320, 560, 50), life.IsDead ? "SEM FÔLEGO." : "UM RESPIRO.", large);
                GUI.Label(new Rect(440, 390, 550, 60), life.IsDead ? "R para voltar à praça e recuperar o fôlego." : "ESC para continuar explorando.\nR para voltar ao começo da praça.", body);
            }
            GUI.matrix = original;
        }
        private static GUIStyle Style(Font font, int size, bool bold, Color color)
        { var style = new GUIStyle { font = font, fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal }; style.normal.textColor = color; return style; }
        private static void Panel(Rect rect, Color color)
        { Color original = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = original; }
    }
}
