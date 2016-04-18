using ClosedXML.Excel;
using FestivArts.Models.Entity;
using FestivArts.Utils;
using FestivArts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FestivArts.Controllers
{
    public class ImportController : Controller
    {

        private FestivArtsContext db = new FestivArtsContext();

        // GET: Import
        public ActionResult Index()
        {

            if (db.Plannings.Count() > 0)
            {
                ViewBag.Plannings = new SelectList(db.Plannings.OrderByDescending(s => s.Date), "Id", "Nom", 0);
            }
            return View();
        }


        [HttpPost]
        public ActionResult Index(ImportViewModel vm)
        {
            if (ModelState.IsValid && Request.Files.Count == 1) 
            {
                using(var workbook = new XLWorkbook(Request.Files[0].InputStream) )
                {
                    Planning p;
                    if (!string.IsNullOrWhiteSpace(vm.NewNom)) 
                    {
                        p = new Planning()
                        {
                            Date = DateTime.Now,
                            Nom = vm.NewNom
                        };
                        db.Plannings.Add(p);
                        db.SaveChanges();
                    }
                    else 
                    {
                        p = db.Plannings.First(s => s.Id == vm.PlanningId);
                    }

                    try   
                    {
                        ImportExcelUtil.ImportPlanning(p, db, workbook);
                        vm.LastPlanningId = p.Id;
                    }
                    catch (ImportException ex) 
                    {
                        ModelState.AddModelError("NewNom", ex.Message);
                    }
                }
            }
            vm.Jours = db.JourEvenements.ToList();
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