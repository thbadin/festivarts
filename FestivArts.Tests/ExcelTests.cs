using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FestivArts.Utils;
using FestivArts.Models.Entity;
using ClosedXML.Excel;
using System.IO;
using System.Collections.Generic;

namespace FestivArts.Tests
{
    [TestClass]
    public class ExcelTests
    {
        [TestCleanup]
        public void CleanUp()
        { 
                 //Clean
            using (var ctx = new FestivArtsContext())
            {
                foreach (var p in ctx.Plannings.Where(s => s.Nom.StartsWith("TUImportExport")))
                {
                    ctx.Affectations.RemoveRange(p.Affectations);
                    ctx.Plannings.Remove(p);
                }
                ctx.SaveChanges();
            }
        }

        [TestMethod]
        public void ImportExport()
        {

            //Creation des données
            using (var ctx = new FestivArtsContext()) 
            {
                Planning p = new Planning() { Date = DateTime.Now, Nom = "TUImportExport" };
                Planning p2 = new Planning() { Date = DateTime.Now, Nom = "TUImportExportResult" };
                ctx.Plannings.Add(p); 
                ctx.Plannings.Add(p2);
                ctx.SaveChanges();
                int i = 0, j = 0;
                foreach (var c in ctx.Creneaux) 
                {
                    foreach (var b in ctx.Benevoles)
                    {
                        var a = new Affectation() { BenevoleId = b.Id, PlanningId = p.Id, CreneauId = c.Id };

                        if (j < c.NbBenevoleMax)
                        {
                            ctx.Affectations.Add(a);
                        }
                        else
                            break;
                        j++;
                    }
                    i++;
                    j = 0;
                }
                ctx.SaveChanges();


                Assert.AreEqual(ctx.Creneaux.Sum(s => s.NbBenevoleMax), p.Affectations.Count);
            }


            //Génération exel
            var workbooks =  new  Dictionary<int,Stream>();
            using (var ctx = new FestivArtsContext())
            {
                Planning p = ctx.Plannings.First(s => s.Nom == "TUImportExport");

                foreach (var jour in ctx.JourEvenements)
                {
                    workbooks.Add(jour.Id, new MemoryStream());
                    var wb = new XLWorkbook();
                    ExcelUtils.FillPlanning(wb, ctx, jour, p);
                    wb.SaveAs(workbooks[jour.Id]);
                    workbooks[jour.Id].Position = 0;
                }
            
            }


            foreach (var k in workbooks.Keys)
            {
                //Import
                using (var ctx = new FestivArtsContext())
                {
                    Planning p = ctx.Plannings.First(s => s.Nom == "TUImportExportResult");

                    var wb = new XLWorkbook(workbooks[k]);
                    ImportExcelUtil.ImportPlanning(p, ctx, wb);
                  
                }

            }

            //Assert
            using (var ctx = new FestivArtsContext())
            {
                Planning p = ctx.Plannings.Include("Affectations.Creneau.CreneauDef").First(s => s.Nom == "TUImportExport");
                Planning pres = ctx.Plannings.Include("Affectations.Creneau.CreneauDef").First(s => s.Nom == "TUImportExportResult");
                Dictionary<int, List<Affectation>> affectationByDay = new Dictionary<int, List<Affectation>>();
                Dictionary<int, List<Affectation>> affectationResultByDay = new Dictionary<int, List<Affectation>>();
                foreach (int i in ctx.JourEvenements.Select(s => s.Id)) 
                {
                    affectationByDay.Add(i, new List<Affectation>());
                    affectationResultByDay.Add(i, new List<Affectation>());
                }

                p.Affectations.ForEach(s => affectationByDay[s.Creneau.CreneauDef.JourId].Add(s));
                pres.Affectations.ForEach(s => affectationResultByDay[s.Creneau.CreneauDef.JourId].Add(s));

                foreach (var j in ctx.JourEvenements)
                {
                    try
                    {
                        Assert.AreEqual(affectationByDay[j.Id].Count, affectationResultByDay[j.Id].Count);
                    }
                    catch (AssertFailedException)
                    {
                        
                        var wb = new XLWorkbook(workbooks[j.Id]);
                        wb.SaveAs(@"C:\Users\Titho\Desktop\testResult\" + j.Nom + ".xlsx");
                        wb = new XLWorkbook();
                        ExcelUtils.FillPlanning(wb, ctx, j, pres);
                        wb.SaveAs(@"C:\Users\Titho\Desktop\testResult\" + j.Nom + "_res.xlsx");

                        throw;
                    }
                }

                Assert.AreEqual(p.Affectations.Count, pres.Affectations.Count);
                foreach (var a in p.Affectations)
                {
                    Assert.AreEqual(1, pres.Affectations.Count(s => s.BenevoleId == a.BenevoleId && s.CreneauId == a.CreneauId));

                
                }

            }

       

        }
    }
}
