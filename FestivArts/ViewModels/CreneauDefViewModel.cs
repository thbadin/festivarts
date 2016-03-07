using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FestivArts.ViewModels
{
    public class CreneauDefViewModel
    {
        [Key]
        public int Id
        {
            get;
            set;
        }

        public CreneauDefViewModel()
        {

        }

        public CreneauDefViewModel(Tache t, IEnumerable<JourEvenement> jours) 
        {
            Tache = t;
            Id = t.Id;
            Creneaux = new Dictionary<int, Dictionary<int,Creneau>>();
            foreach (var j in jours) 
            {
                var l = new Dictionary<int,Creneau>();
                Creneaux.Add(j.Id, l);
                foreach (var d in j.CreneauDefs) 
                {
                    Creneau c = new Creneau() 
                    { 
                        CreneauDefId = d.Id,
                        CreneauDef = d,
                        TacheId = Tache.Id,
                        NbBenevoleMax = 0,
                        NbBenevoleMin = 0
                    };
                    var old = t.Creneaux.FirstOrDefault(s => s.CreneauDef.NoCreneau == d.NoCreneau && s.CreneauDef.JourId == d.JourId);
                    if(old != null)
                    {

                        c.NbBenevoleMin = old.NbBenevoleMin;
                        c.NbBenevoleMax = old.NbBenevoleMax;
                    }
                    l.Add(d.NoCreneau, c);
                }
                
            }
        }

        public Dictionary<int, Dictionary<int,Creneau>> Creneaux { get; set; }

        public Tache Tache { get;  set; }
    }
}