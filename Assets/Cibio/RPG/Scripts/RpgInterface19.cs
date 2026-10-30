using UnityEngine;

namespace Cibio.RPG
{
    public sealed partial class RpgInterface
    {
        public string NameDraft{get;set;}="";
        public bool JourneyContinues{get;private set;}
        string nameError="";bool focusName;GUIStyle nameField;
        int NameHudSize(string name)
        {
            float width=Style(15,paper,true).CalcSize(new GUIContent(name)).x;
            return Mathf.Clamp(Mathf.FloorToInt(15*Mathf.Min(1,150/Mathf.Max(1,width))),8,15);
        }
        public bool BeginJourney(bool continueGame)
        {
            if(game.mode!=0)return false;JourneyContinues=continueGame&&game.HasSave;
            var saved=JourneyContinues?RpgGame.Load():null;
            NameDraft=saved!=null&&RpgIdentity19.TryName(saved.playerName,out var existing)?existing:"";
            nameError="";focusName=true;game.mode=6;return true;
        }
        public bool ConfirmJourney()
        {
            if(game.mode!=6)return false;
            if(!RpgIdentity19.TryName(NameDraft,out var name)){nameError=RpgIdentity19.Hint;return false;}
            // StartGame loads/resets only after the player confirms. Cancelling never touches a save.
            game.StartGame(JourneyContinues,name);return true;
        }
        public void CancelJourney(){if(game.mode==6){game.mode=0;nameError="";}}
        void NamePanel()
        {
            var background=RpgArt.Texture("Title_Vila13");float zoom=1.025f+Mathf.Sin(Time.unscaledTime*.13f)*.005f;
            GUI.DrawTexture(new Rect(720-720*zoom,405-405*zoom,1440*zoom,810*zoom),background,ScaleMode.ScaleAndCrop);
            Fill(new Rect(0,0,1440,810),new Color(.017f,.025f,.028f,.83f));
            Card(new Rect(297,119,846,572),.98f);Fill(new Rect(337,159,3,36),gold);
            Text(new Rect(354,157,650,22),"BRASIL NO VERMELHO  /  SEU PERSONAGEM",11,gold,true);
            Text(new Rect(337,207,754,51),"Toda resistência tem um nome.",30,paper,true);
            Text(new Rect(339,267,710,43),JourneyContinues?"Seu progresso está guardado. Escolha como o bairro vai conhecer você.":"Você é o Brasileiro. Dê um nome ao trabalhador que vai enfrentar o Companheiro.",15,muted);
            Round(new Rect(339,338,126,170),new Color(.075f,.105f,.105f),6);
            var texture=RpgArt.Texture("Hero_idle");if(texture)GUI.DrawTextureWithTexCoords(new Rect(349,348,106,150),texture,new Rect((Mathf.FloorToInt(Time.unscaledTime*2)%2)/2f,.25f,.5f,.25f));
            Text(new Rect(490,337,566,24),"NOME DO PERSONAGEM",11,muted,true);
            Round(new Rect(489,371,564,63),new Color(.2f,.29f,.27f),6);Round(new Rect(491,373,560,59),new Color(.035f,.055f,.055f),5);
            if(nameField==null)nameField=new GUIStyle(GUI.skin.textField){font=semibold,fontSize=23,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(16,16,0,0),border=new RectOffset(0,0,0,0),normal={textColor=paper,background=Texture2D.blackTexture},focused={textColor=paper,background=Texture2D.blackTexture}};
            GUI.SetNextControlName("cibio-player-name");NameDraft=GUI.TextField(new Rect(496,379,550,47),NameDraft,40,nameField);
            if(focusName&&Event.current.type==EventType.Repaint){GUI.FocusControl("cibio-player-name");focusName=false;}
            Text(new Rect(491,447,560,43),nameError.Length>0?nameError:RpgIdentity19.Hint,12,nameError.Length>0?new Color(.96f,.58f,.5f):muted);
            Text(new Rect(339,540,714,32),"O nome aparece no HUD e fica salvo neste dispositivo.",12,muted);
            if(Button(new Rect(339,605,170,44),"Voltar  Esc"))CancelJourney();
            if(Button(new Rect(751,605,302,44),JourneyContinues?"Continuar jornada":"Entrar no bairro",true))ConfirmJourney();
            if(Event.current.type==EventType.KeyDown&&(Event.current.keyCode==KeyCode.Return||Event.current.keyCode==KeyCode.KeypadEnter)){ConfirmJourney();Event.current.Use();}
        }
    }
}
