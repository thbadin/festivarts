using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FestivArts.Models.Entity;

namespace FestivArts.Controllers
{
    public class JourEvenementsController : Controller
    {
        private FestivArtsContext db = new FestivArtsContext();

        // GET: JourEvenements
        public ActionResult Index()
        {
            return View(db.JourEvenements.OrderBy(j => j.Ordre).ToList());
        }

        // GET: JourEvenements/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JourEvenement jourEvenement = db.JourEvenements.Find(id);
            if (jourEvenement == null)
            {
                return HttpNotFound();
            }
            return View(jourEvenement);
        }

        // GET: JourEvenements/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: JourEvenements/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nom,Description,Ordre,DateDebut,DateFin")] JourEvenement jourEvenement)
        {
            if (ModelState.IsValid)
            {
                db.JourEvenements.Add(jourEvenement);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jourEvenement);
        }

        // GET: JourEvenements/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JourEvenement jourEvenement = db.JourEvenements.Find(id);
            if (jourEvenement == null)
            {
                return HttpNotFound();
            }
            return View(jourEvenement);
        }

        // POST: JourEvenements/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nom,Description,Ordre,DateDebut,DateFin")] JourEvenement jourEvenement)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jourEvenement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jourEvenement);
        }

        // GET: JourEvenements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JourEvenement jourEvenement = db.JourEvenements.Find(id);
            if (jourEvenement == null)
            {
                return HttpNotFound();
            }
            return View(jourEvenement);
        }

        // POST: JourEvenements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JourEvenement jourEvenement = db.JourEvenements.Find(id);
            db.JourEvenements.Remove(jourEvenement);
            db.SaveChanges();
            return RedirectToAction("Index");
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
