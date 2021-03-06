﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FestivArts.Models.Entity;
using System.Text.RegularExpressions;
using FestivArts.ViewModels;

namespace FestivArts.Controllers
{
    public class BenevolesController : Controller
    {
        private FestivArtsContext db = new FestivArtsContext();

        // GET: Benevoles
        public ActionResult Index()
        {
            return View(db.Benevoles.Include("Dispoes").ToList());
        }

        public ActionResult Create()
        {
            var vm = new BenevoleCreateViewModel();
            vm.Jours = db.JourEvenements.Select(j => new BenevoleJourCreateNewModel() { Id = j.Id, Nom = j.Nom }).ToList();
            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(BenevoleCreateViewModel vm)
        {

            Benevole b = new Benevole()
            {
                Nom = vm.Nom,
                Prenom = vm.Prenom,
                Permis = vm.Permis,
                Commentaire = vm.Commentaire,
                Email = vm.Email,
                Tel = vm.Tel
            };
            db.Benevoles.Add(b);

            foreach(CreneauDef c in db.CreneauDefs)
            {
                Dispo d = new Dispo()
                {
                    CreneauDef = c,
                    EstDispo = vm.Jours.Any(j => j.IsSelected && c.JourEvenement.Id == j.Id),
                    ModifManuel = false
                };
                b.Dispoes.Add(d);
            }
            db.SaveChanges();
            return RedirectToAction("Index");

        }


        public ActionResult Find( )
        {
            return View(new RechercheBenevoleViewModels(db));
        }

        [HttpPost]
        public ActionResult Find(RechercheBenevoleViewModels vm)
        {
            return View( new RechercheBenevoleViewModels(db, vm.SelectedJour) );
        }


        // GET: Benevoles/Details/5
        public ActionResult Details(int? id)
        {
            
            Benevole benevole = db.Benevoles.Find(id);
            if (benevole == null)
            {
                return HttpNotFound();
            }
            return View(benevole);
        }


        // GET: Benevoles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Benevole benevole = db.Benevoles.Include("Dispoes.CreneauDef.JourEvenement").First( s => s.Id == id);
            if (benevole == null)
            {
                return HttpNotFound();
            }
            benevole.FillDispoByDayFromDb();
            var dico = new Dictionary<int, JourEvenement>();
            var coms = new Dictionary<int, string>();

            ViewBag.Jours = db.JourEvenements.OrderBy(j => j.Ordre).ToList();
            foreach (var j in ViewBag.Jours)
            {
                var strDispo = benevole.CommentaireDispoes.FirstOrDefault(s => s.JourId == j.Id);
                coms.Add(j.Id, strDispo == null ? "" : strDispo.Commentaire);
            }
            ViewBag.Coms = coms;

            var nextBenevole = db.Benevoles.OrderBy(s => s.Id).FirstOrDefault(s => s.Id > benevole.Id);
            if (nextBenevole == null)
                nextBenevole = db.Benevoles.OrderBy(s => s.Id).First();
            ViewBag.NextBenId = nextBenevole.Id;
            return View(benevole);
        }

        // POST: Benevoles/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "DisposByDays")] Benevole b)
        {




            Benevole benevole = db.Benevoles.Include("Dispoes.CreneauDef.JourEvenement").First(s => s.Id == id);
            if (ModelState.IsValid)
            {
                Regex r = new Regex(@"\[([0-9]+)\]");

                foreach (var key in Request.Form.AllKeys.Where(k => k.Contains("DisposByDays")))
                {
                    var m = r.Match(key);
                    int jourId = int.Parse(m.Groups[0].Captures[0].Value.Trim(new char[] { '[', ']' }));
                    m = m.NextMatch();
                    int noCreneau = int.Parse(m.Groups[0].Captures[0].Value.Trim(new char[] { '[', ']' }));

                    var val = Request.Form[key].ToString().ToLower();
                    bool isDispo = val.Contains("true");

                    Dispo d = benevole.Dispoes.First(s => s.CreneauDef.NoCreneau == noCreneau && s.CreneauDef.JourId == jourId);
                    if (d.EstDispo != isDispo) 
                    {
                        d.EstDispo = isDispo;
                        d.ModifManuel = true;
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            benevole.FillDispoByDayFromDb();
            var dico = new Dictionary<int, JourEvenement>();
            var coms = new Dictionary<int, string>();
            foreach (var j in db.JourEvenements)
            {
                var strDispo = benevole.CommentaireDispoes.FirstOrDefault( s => s.JourId == j.Id);
                dico.Add(j.Id,j);
                coms.Add(j.Id, strDispo == null ? "" : strDispo.Commentaire );
            }
            ViewBag.Jours = dico;
            ViewBag.Coms = coms;
            var nextBenevole = db.Benevoles.OrderBy(s => s.Id).First(s => s.Id > benevole.Id);
            if (nextBenevole == null)
                nextBenevole = db.Benevoles.OrderBy(s => s.Id).First();
            ViewBag.NextBenId = nextBenevole.Id;
            return View(benevole);
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
