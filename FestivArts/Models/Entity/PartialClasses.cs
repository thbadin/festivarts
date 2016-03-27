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

        public override string ToString()
        {
            return this.Prenom + " " + this.Nom;
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
                case AffectationStatusEnum.Souhaite:
                    str += "(S) ";
                    break;
                case AffectationStatusEnum.Correct:
                    str += "(C) ";
                    break;
            }
                str += this.Prenom + " " + this.Nom;
                return str;
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