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
        Color NameColor20(string hex)
        {
            var c=RpgArt.Hex(hex);return QualitySettings.activeColorSpace==ColorSpace.Linear?c.linear:c;
        }
        bool NameButton20(Rect rect,string label,bool primary,bool enabled=true)
        {
            bool hover=rect.Contains(Event.current.mousePosition);var light=RpgArt.Hex("F0E9D9");
            if(primary)Fill(rect,NameColor20(enabled?(hover?"F5D880":"E6BE60"):"37372E"));
            Text(rect,label,15,primary?RpgArt.Hex(enabled?"191B16":"B7B59F"):light,true,TextAnchor.MiddleCenter);
            var old=GUI.enabled;GUI.enabled=old&&enabled;bool clicked=GUI.Button(rect,"",GUIStyle.none);GUI.enabled=old;return clicked;
        }
        void NamePanel()
        {
            var light=RpgArt.Hex("F0E9D9");var soft=RpgArt.Hex("BBB8A8");var yellow=RpgArt.Hex("E6BE60");var red=RpgArt.Hex("EB6447");
            var background=RpgArt.Texture("Title_Revolta20");GUI.DrawTexture(new Rect(0,0,1440,810),background,ScaleMode.ScaleAndCrop);
            var shade=NameColor20("111410");shade.a=.90f;Fill(new Rect(0,0,684,810),shade);
            Text(new Rect(72,35,170,50),"CIBIO",36,light,true);Text(new Rect(74,90,470,20),"BRASIL NO VERMELHO  /  CRIAÇÃO DO PERSONAGEM",10,yellow,true);
            Fill(new Rect(72,135,42,4),red);
            Text(new Rect(68,163,590,150),"SEU NOME.\nSUA REVOLTA.",59,light,true);
            Text(new Rect(74,330,550,53),JourneyContinues?"Sua história continua. Mantenha seu nome ou escolha como o bairro vai reconhecer você.":"O salário acaba. Os boletos continuam.\nDê um nome a quem vai cobrar essa conta.",16,soft);
            bool valid=RpgIdentity19.TryName(NameDraft,out _);int count=new System.Globalization.StringInfo(NameDraft).LengthInTextElements;
            Text(new Rect(74,409,410,20),"NOME DO PERSONAGEM",11,soft,true);Text(new Rect(535,409,98,20),count+" / 18",11,count>18?red:soft,false,TextAnchor.MiddleRight);
            Fill(new Rect(72,439,562,64),NameColor20("20271F"));Fill(new Rect(72,501,562,2),nameError.Length>0?red:valid?yellow:NameColor20("626C58"));
            if(nameField==null)
            {
                nameField=new GUIStyle(GUI.skin.textField){font=semibold,fontSize=24,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(0,0,0,0),border=new RectOffset(0,0,0,0)};
                nameField.normal.background=nameField.focused.background=nameField.hover.background=nameField.active.background=null;
                nameField.normal.textColor=nameField.focused.textColor=nameField.hover.textColor=nameField.active.textColor=light;
            }
            GUI.SetNextControlName("cibio-player-name");var edited=GUI.TextField(new Rect(89,446,528,49),NameDraft,40,nameField);
            if(edited!=NameDraft){NameDraft=edited;nameError="";}
            if(focusName&&Event.current.type==EventType.Repaint){GUI.FocusControl("cibio-player-name");focusName=false;}
            if(NameDraft.Length==0&&GUI.GetNameOfFocusedControl()!="cibio-player-name")Text(new Rect(89,459,510,30),"Como o bairro vai chamar você?",17,soft);
            Text(new Rect(74,522,556,39),nameError.Length>0?nameError:RpgIdentity19.Hint,12,nameError.Length>0?red:soft);
            if(NameButton20(new Rect(72,583,562,60),JourneyContinues?"CONTINUAR MINHA HISTÓRIA":"ENTRAR NO BAIRRO",true,valid))ConfirmJourney();
            if(NameButton20(new Rect(72,662,138,33),"Voltar  /  Esc",false))CancelJourney();
            Fill(new Rect(72,716,562,1),NameColor20("3A4134"));
            var texture=RpgArt.Texture("Hero_idle");if(texture)GUI.DrawTextureWithTexCoords(new Rect(72,733,43,58),texture,new Rect((Mathf.FloorToInt(Time.unscaledTime*2)%2)/2f,.25f,.5f,.25f));
            Text(new Rect(133,737,470,21),"O BRASILEIRO É VOCÊ.",11,yellow,true);Text(new Rect(133,762,480,20),"Nome e progresso ficam salvos neste dispositivo.",11,soft);
            Text(new Rect(1010,751,354,20),"01  /  BAIRRO DO TRABALHADOR",10,light,true,TextAnchor.MiddleRight);
            if(Event.current.type==EventType.KeyDown&&(Event.current.keyCode==KeyCode.Return||Event.current.keyCode==KeyCode.KeypadEnter)){ConfirmJourney();Event.current.Use();}
        }
    }
}
