using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Cibio.RPG
{
    public static class RpgIdentity19
    {
        public const int MinLength=2,MaxLength=18;
        public const string Hint="De 2 a 18 caracteres. Letras, números, espaços, hífen ou apóstrofo.";
        public static bool TryName(string input,out string name)
        {
            name="";if(string.IsNullOrWhiteSpace(input))return false;
            string text;try{text=input.Normalize(NormalizationForm.FormC);}catch(ArgumentException){return false;}
            var result=new StringBuilder();bool space=false;
            foreach(char c in text)
            {
                if(char.IsWhiteSpace(c)){space=result.Length>0;continue;}
                var category=char.GetUnicodeCategory(c);
                if(!char.IsLetterOrDigit(c)&&category!=UnicodeCategory.NonSpacingMark&&c!='-'&&c!='\'')return false;
                if(space){result.Append(' ');space=false;}result.Append(c);
            }
            var value=result.ToString();int count=new StringInfo(value).LengthInTextElements;
            if(count<MinLength||count>MaxLength)return false;
            bool hasLetterOrNumber=false;foreach(char c in value)hasLetterOrNumber|=char.IsLetterOrDigit(c);
            if(!hasLetterOrNumber)return false;name=value;return true;
        }
        public static string Display(RpgProgress progress)=>TryName(progress.playerName,out var name)?name:"Brasileiro";
    }
}
