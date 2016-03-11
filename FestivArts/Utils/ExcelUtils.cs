using ClosedXML.Excel;
using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace FestivArts.Utils
{
    public class ExcelUtils
    {


        public const int FIRST_PLAN_COLUMN = 3;


        public static void FillPlanning(XLWorkbook book, FestivArtsContext ctx, JourEvenement jour, Planning p) 
        {
            foreach (TypeTache t in ctx.TypeTaches.Include("Taches.Creneaux.CreneauDef").Where(s => s.Taches.Max(t => t.Creneaux.Count(u => u.CreneauDef.JourId == jour.Id)) > 0))
            {
                var worksheet = book.Worksheets.Add(t.Id + "-" + jour.Id + " " + t.Nom + " ");
                worksheet.Column(1).Hide();
                worksheet.Column(2).Width = 40;
                FillNewTypeTache(worksheet, t, jour, p);
            }
        }

        public static void FillNewPlanning(XLWorkbook book,FestivArtsContext ctx,  JourEvenement jour)
        {

            FillPlanning(book, ctx, jour, null);
            
        }


        private static void FillNewTypeTache(IXLWorksheet sheet, TypeTache tache, JourEvenement jour, Planning p = null) 
        {
            IXLCell c = sheet.Cell("A1");
            c.Value = jour.Id;

            c = sheet.Cell("B1");
            c.Value = tache.Nom;
            c.Style.Font.FontSize = 34;
            c.Style.Font.Bold = true;

            c = sheet.Cell("C1");
            c.Value = jour.Nom;
            c.Style.Font.FontSize = 30;
            c.Style.Font.Bold = true;
            int i = 3;
            foreach (Tache t in tache.Taches.Where( s => s.Creneaux.Count( u => u.CreneauDef.JourId == jour.Id) > 0 )) 
            {
                FillNewTache(sheet, t, jour, ref i, p);
                i += 2;
            }
        }



        private static void FIllNewRow(IXLRow r, JourEvenement jour, Tache tache, Planning p, bool isFirst, bool isLast, int tacheLineCount) 
        {
            Dictionary<int, Creneau> crenauxNeeded = new Dictionary<int, Creneau>();
            tache.Creneaux.Where( s => s.CreneauDef.JourId == jour.Id).ForEach(s => crenauxNeeded.Add(s.CreneauDef.NoCreneau, s));
            int i = FIRST_PLAN_COLUMN;

            foreach (CreneauDef d in jour.CreneauDefs.OrderBy( s => s.NoCreneau))
            {
                
                List<Affectation> affs = p.Affectations.Where(s => s.Creneau.CreneauDefId == d.Id && s.Creneau.TacheId == tache.Id).ToList();


                IXLCell c = r.Cell(i);
                if (i == FIRST_PLAN_COLUMN)
                    c.Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                if (i == FIRST_PLAN_COLUMN + jour.CreneauDefs.Count())
                    c.Style.Border.RightBorder = XLBorderStyleValues.Thick;

                if(affs.Count > tacheLineCount)
                {
                    c.Value = affs[tacheLineCount].Benevole.ExcelKey;
                }

                if (isFirst)
                    c.Style.Border.TopBorder = XLBorderStyleValues.Thick;
                if (isLast)
                    c.Style.Border.BottomBorder = XLBorderStyleValues.Thick;

                if (!crenauxNeeded.ContainsKey(d.NoCreneau))
                {
                    c.Style.Fill.BackgroundColor = XLColor.Black;
                }
                else 
                {
                    if (tacheLineCount >= crenauxNeeded[d.NoCreneau].NbBenevoleMax) 
                    {
                        c.Style.Fill.BackgroundColor = XLColor.Black;
                    }
                    else if (tacheLineCount > crenauxNeeded[d.NoCreneau].NbBenevoleMin)
                    {
                        c.Style.Fill.BackgroundColor = XLColor.Gray;
                    }
                }
                i++;
            }

        

        }


        private static void FillNewTache(IXLWorksheet sheet, Tache tache, JourEvenement jour, ref int line, Planning p) 
        {

            IXLCell c = sheet.Cell("A" + line);
            c.Value = tache.Id;


            c = sheet.Cell("B" + line);
            c.Value = tache.Nom;
            c.Style.Font.FontSize = 20;
            c.Style.Font.Bold = true;

            line++;
            IXLRow r = sheet.Row(line);
            int j = FIRST_PLAN_COLUMN;
            foreach (var def in jour.CreneauDefs.OrderBy(s => s.NoCreneau)) 
            {
                c = r.Cell(j);
                c.Value = "'"+def.Debut.ToString("HH:mm",CultureInfo.InvariantCulture);
                j++;
            }
            line++;

            int maxB = tache.GetMaxBenevoleByDay(jour.Id);
            for (int i = 0; i < maxB; i++) 
            {

                r = sheet.Row(line);
                FIllNewRow(r, jour, tache, p, i == 0, i == maxB - 1, i);
                line++;
            }
     





        }
    }
}