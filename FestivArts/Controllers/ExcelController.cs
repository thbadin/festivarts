using ClosedXML.Excel;
using FestivArts.Models.Entity;
using FestivArts.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FestivArts.Controllers
{
    public class ExcelController : Controller
    {
        // GET: Excel
        public ActionResult New(int id)
        {
            var workbook = new XLWorkbook();
            JourEvenement jour;
            using (var ctx = new FestivArtsContext())
            {
                jour = ctx.JourEvenements.First(s => s.Id == id);
                ExcelUtils.FillNewPlanning(workbook, ctx, jour);
            }
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "newPlanning" + jour.Nom + ".xlsx");

        }

        public ActionResult Last(int id)
        {
            var workbook = new XLWorkbook();
            JourEvenement jour;
            string strPrefixFile = "new";
            using (var ctx = new FestivArtsContext())
            {
                jour = ctx.JourEvenements.First(s => s.Id == id);
                Planning p = ctx.Plannings.Include("Affectations.Benevole").OrderByDescending(s => s.Date).FirstOrDefault();
                if(p != null)
                {
                    if (!string.IsNullOrWhiteSpace(p.Nom))
                        strPrefixFile = p.Nom;
                    else
                        strPrefixFile = p.Date.ToShortDateString();
                }
                 
                ExcelUtils.FillPlanning(workbook, ctx, jour, p);
            }
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", strPrefixFile+"Planning" + jour.Nom + ".xlsx");

        }

    }
}