using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FestivArts.ViewModels
{
    public class BenevoleJourCreateNewModel
    {
        public string Nom { get; set; }

        public int Id { get; set; }

        public bool IsSelected { get; set; }
    }


    public class BenevoleCreateViewModel : Benevole
    {
        public BenevoleCreateViewModel()
        {
            this.Jours = new List<BenevoleJourCreateNewModel>();
        }

        public List<BenevoleJourCreateNewModel> Jours { get; set; }
    }
}