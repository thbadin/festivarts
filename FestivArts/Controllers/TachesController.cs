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
    public class TachesController : Controller
    {
        private FestivArtsContext db = new FestivArtsContext();

        // GET: Taches
        public ActionResult Index()
        {
            var taches = db.Taches.OrderBy(s => s.Ordre).Include(t => t.Creneaux);
            return View(taches.ToList());
        }

        // GET: Taches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tache tache = db.Taches.Find(id);
            if (tache == null)
            {
                return HttpNotFound();
            }
            return View(tache);
        }

        // GET: Taches/Create
        public ActionResult Create()
        {
            ViewBag.Id = new SelectList(db.Creneaux, "Id", "Id");
            ViewBag.TypesTaches = new SelectList(db.TypeTaches.OrderBy( s => s.Ordre), "Id", "Nom", 0);
            return View();
        }

        // POST: Taches/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nom,Description,TypeTacheId")] Tache tache)
        {
            if (ModelState.IsValid)
            {
                db.Taches.Add(tache);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            ViewBag.Id = new SelectList(db.Creneaux, "Id", "Id", tache.Id);
            ViewBag.TypesTaches = new SelectList(db.TypeTaches.OrderBy(s => s.Ordre), "Id", "Nom", tache.TypeTache.Id);
            return View(tache);
        }

        // GET: Taches/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tache tache = db.Taches.Find(id);
            if (tache == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id = new SelectList(db.Creneaux, "Id", "Id", tache.Id);
            ViewBag.TypesTaches = new SelectList(db.TypeTaches.OrderBy(s => s.Ordre), "Id", "Nom", tache.TypeTache.Id);
            return View(tache);
        }

        // POST: Taches/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nom,Description,TypeTacheId")] Tache tache)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tache).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id = new SelectList(db.Creneaux, "Id", "Id", tache.Id);
            ViewBag.TypesTaches = new SelectList(db.TypeTaches.OrderBy(s => s.Ordre), "Id", "Nom", tache.TypeTache.Id);
            return View(tache);
        }

        // GET: Taches/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tache tache = db.Taches.Find(id);
            if (tache == null)
            {
                return HttpNotFound();
            }
            return View(tache);
        }

        // POST: Taches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tache tache = db.Taches.Find(id);
            db.Taches.Remove(tache);
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
