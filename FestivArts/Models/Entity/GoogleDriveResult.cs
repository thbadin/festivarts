using FestivArts.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace FestivArts.Models.Entity
{
    public class GoogleDriveResult
    {
         
        public string Nom { private get; set; }

        public string FormatedNom
        {
            get 
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Nom.Trim());
            }
        }

        public string Prenom { private get; set; }

        public string FormatedPrenom
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Prenom.Trim());
            }
        }

        public string Telephone { private get; set; }

        public string FormatedTel
        {
            get
            {
                var sb = new StringBuilder();
                foreach (char c in Telephone.Trim().ToCharArray())
                {
                    if (Char.IsDigit(c))
                    {
                        sb.Append(c);
                    }
                }
                string telChiffre;
                if (sb.Length == 9)
                    telChiffre = "0" + sb.ToString();
                else
                    telChiffre = sb.ToString();
                return  telChiffre.Substring(0, 2) + " " + telChiffre.Substring(2, 2) + " " + telChiffre.Substring(4, 2) + " " + telChiffre.Substring(6, 2) + " " + telChiffre.Substring(8, 2);
            }
        }

        private void parsePrefString(Dictionary<TypeTacheEnum, int> dic, string str, int mult = 1) 
        { 
            str = str.Trim().ToLower();
            if (str.Contains("sécurité"))
                dic[TypeTacheEnum.Securite] += (1 * mult);
            if (str.Contains("bar"))
                dic[TypeTacheEnum.Bar] += (1 * mult);
            if (str.Contains("place"))
                dic[TypeTacheEnum.Place] += (1 * mult);
            if (str.Contains("catering"))
                dic[TypeTacheEnum.Catering] += (1 * mult);
            if (str.Contains("logistique"))
                dic[TypeTacheEnum.Logistique] += (1 * mult);
            if (str.Contains("eco"))
                dic[TypeTacheEnum.Ecocom] += (1 * mult);
        }

        public Dictionary<TypeTacheEnum, int> Prefs 
        {
            get 
            {
                var dic =new  Dictionary<TypeTacheEnum, int>();
                foreach (TypeTacheEnum t in Enum.GetValues(typeof(TypeTacheEnum)))
                {
                    dic.Add(t, 0);
                }
                parsePrefString(dic, Preference);
                parsePrefString(dic, NonPreference, -1);
                return dic;
            }
        }

        public string Mail { private get; set; }

        public string FormatedMail
        {
            get
            {
                return Mail.Trim().ToLower();
            }
        }

        public string DispoMercredi { get; set; }
        public string CommentaireMercredi { get; set; }
        public string DispoJeudi { get; set; }
        public string CommentaireJeudi { get; set; }
        public string DispoVendredi { get; set; }
        public string CommentaireVendredi { get; set; }
        public string DispoSamedi { get; set; }
        public string CommentaireSamedi { get; set; }
        public string DispoDimanche { get; set; }
        public string CommentaireDimanche { get; set; }
        public string Preference{ get; set; }
        public string NonPreference{ get; set; }
        public string Permis { private get; set; }
        public string PrecisionGeneral {  get; set; }
        public string Majeur { private get; set; }

        public bool APermis 
        {
            get { return Permis.Trim().ToLower() == "oui"; }
        }
        public bool EstMajeur
        {
            get { return Majeur.Trim().ToLower() == "oui"; }
        }
        

        public string DispoLundi { get; set; }

        public string CommentaireLundi { get; set; }

        public override string ToString()
        {
            return Nom + " " + Prenom;
        }
    }
}