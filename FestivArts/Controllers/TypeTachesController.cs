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
    public class TypeTachesController : Controller
    {
        private FestivArtsContext db = new FestivArtsContext();

        // GET: TypeTaches
        public ActionResult Index()
        {
            return View(db.TypeTaches.OrderBy(s => s.Ordre).ToList());
        }

        // GET: TypeTaches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TypeTache typeTache = db.TypeTaches.Find(id);
            if (typeTache == null)
            {
                return HttpNotFound();
            }
            return View(typeTache);
        }

        // GET: TypeTaches/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TypeTaches/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nom,Description")] TypeTache typeTache)
        {
            if (ModelState.IsValid)
            {
                db.TypeTaches.Add(typeTache);
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            return View(typeTache);
        }

        // GET: TypeTaches/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TypeTache typeTache = db.TypeTaches.Find(id);
            if (typeTache == null)
            {
                return HttpNotFound();
            }
            return View(typeTache);
        }

        // POST: TypeTaches/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nom,Description")] TypeTache typeTache)
        {
            if (ModelState.IsValid)
            {
                db.Entry(typeTache).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(typeTache);
        }

        // GET: TypeTaches/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TypeTache typeTache = db.TypeTaches.Find(id);
            if (typeTache == null)
            {
                return HttpNotFound();
            }
            return View(typeTache);
        }

        // POST: TypeTaches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TypeTache typeTache = db.TypeTaches.Find(id);
            db.TypeTaches.Remove(typeTache);
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
