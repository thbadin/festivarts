using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FestivArts.ViewModels
{
    public class ImportViewModel :   IValidatableObject 

    {

        [Key]
        public int Id { get; set; }

        public int PlanningId { get; set; }


        public List<JourEvenement> Jours { get; set; }

        public int LastPlanningId { get; set; }

        [DisplayName("Nouveau planning")]
        public string NewNom { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var list = new List<ValidationResult>();
            if (PlanningId == 0 && string.IsNullOrWhiteSpace(NewNom)) 
            {
                list.Add(new ValidationResult("Planning ou Nouveau nom obligatoire", new string[] {"PlanningId", "NewNom"}));
            }
            return list;
        }
    }
}