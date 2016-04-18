using ClosedXML.Excel;
using FestivArts.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FestivArts.Models.Entity
{

    public partial class Creneau 
    {
        public override string ToString()
        {
            return this.CreneauDef.ToString() + " " + this.Tache.Nom;
        }
    }

    public partial class Affectation 
    {
        public AffectationStatusEnum Status
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.Creneau.ToString() + " : " + this.Benevole.ToString();
        }
    }

    public class JourEvenementMetadata 
    { 
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm}", ApplyFormatInEditMode = true) ]
        public DateTime DateDebut;

        
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm}", ApplyFormatInEditMode = true) ]
        public DateTime DateFin;
    }

    [MetadataType(typeof(JourEvenementMetadata))]
    public  partial class JourEvenement
    {

        public override string ToString()
        {
            return this.Nom;
        }
    }


    public partial class Tache
    {


        public int GetMaxBenevoleByDay( int jourId)
        {
            return this.Creneaux.Where( s => s.CreneauDef.JourId == jourId).Max(s => s.NbBenevoleMax); 
        }

        public int GetMaxBenevoleAffecté(int planningId)
        {
           return this.Creneaux.Max(s => s.Affectations.Count( t => t.PlanningId == planningId)); 
        }

        private Dictionary<string, int> besoinByDay;
        public Dictionary<string, int> BesoinByDay{
            get
            {
                if (besoinByDay != null)
                    return besoinByDay;

                besoinByDay = new Dictionary<string, int>();
                foreach (var c in this.Creneaux)
                {
                    if (c.NbBenevoleMax > 0)
                    {
                        if ( ! besoinByDay.ContainsKey(c.CreneauDef.JourEvenement.Nom))
                        {
                            besoinByDay.Add(c.CreneauDef.JourEvenement.Nom, 1);
                        }
                        else
                        {
                            besoinByDay[c.CreneauDef.JourEvenement.Nom]++;
                        }
                    }
                }
                return besoinByDay;
            }

        }
    }

    public partial class TypeTache
    {
        public TypeTacheEnum Type 
        {
            get { return (TypeTacheEnum)Id; }
        }
    }

    public partial class Conf 
    {
        private static Conf _currentConf;
        public static Conf CurrentConf 
        {
            get 
            {  
                if(_currentConf == null)
                {
                    using(var ctx = new FestivArtsContext())
                    {
                        _currentConf = ctx.Confs.FirstOrDefault(s => s.IsCurrent);
                    }
                }
                return _currentConf;
                    
            }
        }
    } 

    public partial class CreneauDef
    {
        public DateTime Debut
        {
            get 
            {
                return this.JourEvenement.DateDebut.AddMinutes(this.NoCreneau * Conf.CurrentConf.DureeCreneauMinute);
            }
        }
        public DateTime Fin
        {
            get
            {
                return this.Debut.AddMinutes( Conf.CurrentConf.DureeCreneauMinute);
            }
        }

        public override string ToString()
        {
            return this.JourEvenement.ToString() + " #" + this.NoCreneau;
        }
    }

    public partial class Benevole 
    {

        public string DispoJourStr
        {
            get;
            private set;
        }

        public string LastVeilleCreneaux
        {
            get;
            private set;
        }

        public void FillDispoJour(int jourId)
        {
            List<string> disp = new List<string>();
            string prevCren = "";
            int prevNo = -1;
            foreach (var d in this.Dispoes.Where(s => s.CreneauDef.JourId == jourId).OrderBy(s => s.CreneauDef.NoCreneau))
            {
                if (d.EstDispo && prevNo < 0)
                {
                    prevNo = d.CreneauDef.NoCreneau;
                    prevCren = d.CreneauDef.Debut.ToString("HH:mm") + "-";
                } 
                else if (! d.EstDispo && prevNo >= 0)
                {
                    prevNo = -1;
                    prevCren += d.CreneauDef.Fin.ToString("HH:mm");
                    disp.Add(prevCren);
                    prevCren = "";
                }
            }

            if (prevNo >= 0)
            {
                prevCren += this.Dispoes.Where(s => s.CreneauDef.JourId == jourId).OrderByDescending( s => s.CreneauDef.NoCreneau).First().CreneauDef.Fin.ToString("HH:mm");
                disp.Add(prevCren);
            }
            DispoJourStr = string.Join(" ", disp);

            LastVeilleCreneaux = "";
            using (var ctx = new FestivArtsContext())
            {
                var jourJ = ctx.JourEvenements.First(t => t.Id == jourId);
                JourEvenement prevJ = ctx.JourEvenements.Where(s => s.Ordre < jourJ.Ordre).OrderByDescending(s => s.Ordre).FirstOrDefault();
            
                if(prevJ != null)
                {

                    var pc = this.Affectations.Where(s => s.Creneau.CreneauDef.JourId == prevJ.Id);
                    if (pc.Count() > 0)
                    {
                        int maxCren = pc.Max(s => s.Creneau.CreneauDef.NoCreneau);
                        if( maxCren > 0)
                        {
                            CreneauDef d = ctx.CreneauDefs.First(s => s.NoCreneau == maxCren && s.JourId == prevJ.Id);
                            LastVeilleCreneaux = d.Fin.ToString("HH:mm");
                        }
                    }
                }
            }
            


        }

        public  string PrenomN
        {
            get { return this.Id +" - " +this.Prenom + " " + this.Nom.Substring(0, 1); }
        }

        public override string ToString()
        {
            return PrenomN;
        }

        public string GetExcelKey(AffectationStatusEnum status)
        {
            var str = this.Id + " ";
            switch (status)
            {
                case AffectationStatusEnum.Duplique:
                    str += "(D) ";
                    break;
                case AffectationStatusEnum.NonDisponible:
                    str += "(ND) ";
                    break;
                case AffectationStatusEnum.NonSouhaite:
                    str += "(NS) ";
                    break;
                case AffectationStatusEnum.Unknown:
                    str += "(UKN) ";
                    break;
            }
                str += this.Prenom+" " +this.Nom.Substring(0,1);
                return str;
        }


        public XLColor GetBenevoleColor()
        {
            int val = this.PrenomN.GetHashCode();

            switch (val % 6)
            {
                case 0:
                    return XLColor.DarkBlue;
                case 1:
                    return XLColor.DarkBrown;
                case 2:
                    return XLColor.DarkOrange;
                case 3:
                    return XLColor.DarkRed;
                case 4:
                    return XLColor.DarkGreen;
                case 5:
                    return XLColor.DarkMagenta;
               
            } 
            return XLColor.Black;
        }


        public int DispoCount 
        {
            get { return Dispoes.Count(s => s.EstDispo); }
        }
        public int ModifCount
        {
            get { return Dispoes.Count(s => s.ModifManuel); }
        }

        public Dictionary<int, Dictionary<int, Dispo>> DisposByDays
        {
            get;
            set;
        }



        public void FillDispoByDayFromDb() 
        {

            DisposByDays = new Dictionary<int, Dictionary<int, Dispo>>();   
            foreach (var d in Dispoes) 
            {
                if (!DisposByDays.ContainsKey(d.CreneauDef.JourId))
                    DisposByDays.Add(d.CreneauDef.JourId, new Dictionary<int, Dispo>());

                DisposByDays[d.CreneauDef.JourId].Add(d.CreneauDef.NoCreneau, d);

            }
                
         }

       
    }

    public partial class Dispo 
    {
     public override string ToString()
        {
            return "[" + this.CreneauDef.JourId + "][" + this.CreneauDef.NoCreneau + "] : " + (this.EstDispo ? "Dispo" : "Non");
        }
    }
}