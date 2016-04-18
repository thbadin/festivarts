using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FestivArts.ViewModels
{
    public class RechercheBenevoleViewModels
    {

        public RechercheBenevoleViewModels() { }

        public RechercheBenevoleViewModels(FestivArtsContext ctx, int selectedJour)
        {
            this.SelectedJour = selectedJour;
            Jours = ctx.JourEvenements.Select(s => new SelectListItem() { Text = s.Nom, Value = s.Id.ToString(), Selected = selectedJour == s.Id }).ToList() ;
            Benevoles = ctx.Benevoles.Include("Dispoes.CreneauDef").Include("Affectations.Creneau.CreneauDef").Where(s => s.Dispoes.Count(t => t.EstDispo && t.CreneauDef.JourId == selectedJour) > 0).ToList();

            Benevoles.ForEach(s => s.FillDispoJour(selectedJour));
        }

        public RechercheBenevoleViewModels(FestivArtsContext ctx)
        {
            this.SelectedJour = ctx.JourEvenements.OrderBy(s => s.Ordre).First().Id; ;
            Jours = ctx.JourEvenements.Select(s => new SelectListItem() { Text = s.Nom, Value = s.Id.ToString(), Selected = SelectedJour == s.Id }).ToList();
            Benevoles = ctx.Benevoles.Include("Dispoes.CreneauDef").Include("Affectations.Creneau.CreneauDef").Where(s => s.Dispoes.Count(t => t.EstDispo && t.CreneauDef.JourId == SelectedJour) > 0).ToList();
    
            Benevoles.ForEach(s => s.FillDispoJour(SelectedJour));

        }


        public List<SelectListItem> Jours { get; private set; }
        public int SelectedJour { get; set; }

        public List<Benevole> Benevoles { get; private set; }

    }
}