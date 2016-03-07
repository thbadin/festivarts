using FestivArts.Models.Entity;
using FestivArts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace FestivArts.Controllers
{
    public class CreneauController : Controller
    {
        private FestivArtsContext db = new FestivArtsContext();


        // GET: Creneau
        [HttpPost]
        public ActionResult Edit(int id, CreneauDefViewModel vm)
        {
          Tache tache = db.Taches.Include("Creneaux.CreneauDef").First(s => s.Id == id);
          var creneauxDef = db.CreneauDefs.ToList();
          var dico = new Dictionary<int, JourEvenement>();
          foreach (var j in db.JourEvenements)
          {
              dico.Add(j.Id, j);
          }
          ViewBag.Jours = dico;
            if (ModelState.IsValid)
            {
                Regex r = new Regex(@"\[([0-9]+)\]");

                foreach (var key in Request.Form.AllKeys.Where(k => k.Contains("Creneaux") && k.Contains("NbBenevoleMin")))
                {
                    var m = r.Match(key);
                    int jourId = int.Parse(m.Groups[0].Captures[0].Value.Trim(new char[] { '[', ']' }));
                    m = m.NextMatch();
                    int noCreneau = int.Parse(m.Groups[0].Captures[0].Value.Trim(new char[] { '[', ']' }));

                    var min = int.Parse(Request.Form[key].ToString());
                    var max = int.Parse(Request.Form[key.Replace("NbBenevoleMin","NbBenevoleMax")].ToString());
                    var cre = tache.Creneaux.FirstOrDefault(s => s.CreneauDef.JourId == jourId && s.CreneauDef.NoCreneau == noCreneau);

                    if (min == 0 && cre != null) 
                    {
                        db.Creneaux.Remove(cre);
                    }
                    else if( min > 0)
                    {
                        if (max <= min)
                            max = min;
                        if(cre == null)
                        {
                            cre = new Creneau()
                            {
                                TacheId = tache.Id,
                                CreneauDefId = creneauxDef.First( s => s.NoCreneau == noCreneau && s.JourId == jourId).Id,
                                
                            };
                            db.Creneaux.Add(cre);
                        }

                        cre.NbBenevoleMax = max;
                        cre.NbBenevoleMin = min;
                    }
                   
                }
                db.SaveChanges();

                CreneauDefViewModel newVm = new CreneauDefViewModel(db.Taches.Include("Creneaux.CreneauDef").First(s => s.Id == id), db.JourEvenements.Include("CreneauDefs"));
             
                return View(newVm);
            }
            return View(vm);

        }

        // GET: Creneau
        public ActionResult Edit(int id)
        {
            
            CreneauDefViewModel vm = new CreneauDefViewModel(db.Taches.Include("Creneaux.CreneauDef").First( s =>  s.Id == id), db.JourEvenements.Include("CreneauDefs"));
            var dico = new Dictionary<int, JourEvenement>();
            foreach (var j in db.JourEvenements)
            {
                dico.Add(j.Id, j);
            }
            ViewBag.Jours = dico;
            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}