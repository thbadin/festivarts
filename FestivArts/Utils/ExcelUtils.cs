using ClosedXML.Excel;
using FestivArts.Models.Entity;
using FestivArts.Models.Enums;
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


        public static void FillPlanning(XLWorkbook book, FestivArtsContext ctx, JourEvenement jour, Planning p, bool readableExport = false) 
        {
            foreach (TypeTache t in ctx.TypeTaches.Include("Taches.Creneaux.CreneauDef").Where(s => s.Taches.Max(t => t.Creneaux.Count(u => u.CreneauDef.JourId == jour.Id)) > 0))
            {
                var nom = t.Nom.Replace("/", "");
                if (nom.Length > 20)
                    nom = nom.Substring(0, 20);
                var worksheet = book.Worksheets.Add(t.Id + "-" + jour.Id + " " + nom);
                worksheet.Column(1).Hide();
                worksheet.Column(2).Width = 40;
                List<Affectation> affectations;
                List<Benevole> benevoles;
                if (p == null)
                {
                    affectations = new List<Affectation>();
                    benevoles = new List<Benevole>();
                }
                else
                {
                    affectations = p.Affectations.ToList();
                    AffectationUtils.FillAffectationStatus(ctx, affectations);

                    benevoles = affectations.Where(s => s.Creneau.CreneauDef.JourId == jour.Id && s.Creneau.Tache.TypeTacheId == t.Id).Select(s => s.Benevole).Distinct().ToList();
                }

                FillNewTypeTache(worksheet, t, jour, affectations, benevoles, readableExport, p);
            }
        }

        public static void FillNewPlanning(XLWorkbook book,FestivArtsContext ctx,  JourEvenement jour,bool readableExport = false)
        {
            FillPlanning(book, ctx, jour, null, readableExport);
        }


        private static void FillNewTypeTache(IXLWorksheet sheet, TypeTache tache, JourEvenement jour,IEnumerable<Affectation> affectations, IEnumerable<Benevole> benevoles, bool readableExport = false, Planning p = null) 
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
                FillNewTache(sheet, t, jour, ref i, p, affectations, readableExport);
                i += 2;
            }
            if (readableExport)
            {
                var r = sheet.Row(i);
                r.Cell(2).Value = "Prenom";
                r.Cell(2).Style.Font.Bold = true;
                r.Cell(3).Value = "Nom";
                r.Cell(3).Style.Font.Bold = true;
                r.Cell(5).Value = "Tel";
                r.Cell(5).Style.Font.Bold = true;
                r.Cell(7).Value = "Permis";
                r.Cell(7).Style.Font.Bold = true;
                i++;
                foreach (Benevole b in benevoles.OrderBy(s => s.Prenom))
                {
                    r = sheet.Row(i);
                    r.Cell(2).Value = b.Prenom;
                    r.Cell(3).Value = b.Nom;
                    r.Cell(5).Value = "'" + b.Tel;
                    r.Cell(7).Value = b.Permis ? "Oui" : "Non";

                    i++;
                }
            }
        }



        private static void FIllNewRow(IXLRow r, JourEvenement jour, Tache tache, Planning p, bool isFirst, bool isLast, int tacheLineCount, IEnumerable<Affectation> affectations, bool readableExport) 
        {
            Dictionary<int, Creneau> crenauxNeeded = new Dictionary<int, Creneau>();
            tache.Creneaux.Where( s => s.CreneauDef.JourId == jour.Id).ForEach(s => crenauxNeeded.Add(s.CreneauDef.NoCreneau, s));
            int i = FIRST_PLAN_COLUMN;

            foreach (CreneauDef d in jour.CreneauDefs.OrderBy( s => s.NoCreneau))
            {
                
                List<Affectation> affs = affectations.Where(s => s.Creneau.CreneauDefId == d.Id && s.Creneau.TacheId == tache.Id).OrderBy( s => s.Benevole.Prenom).ToList();


                IXLCell c = r.Cell(i);
                if (i == FIRST_PLAN_COLUMN)
                    c.Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                if (i == FIRST_PLAN_COLUMN + jour.CreneauDefs.Count())
                    c.Style.Border.RightBorder = XLBorderStyleValues.Thick;


                if(affs.Count > tacheLineCount)
                {
                    var a = affs[tacheLineCount];

                    if (! readableExport)
                    {

                        c.Value = a.Benevole.GetExcelKey(a.Status);

                        if (a.Benevole.Permis)
                            c.Style.Font.Underline = XLFontUnderlineValues.Double;
                        if (a.Status == AffectationStatusEnum.NonDisponible || a.Status == AffectationStatusEnum.Duplique || a.Status == AffectationStatusEnum.Unknown)
                        {
                            c.Style.Font.FontColor = XLColor.Red;
                            c.Style.Font.Bold = true;
                        }
                        else if (a.Status == AffectationStatusEnum.NonSouhaite)
                        {
                            c.Style.Font.FontColor = XLColor.DarkOrange;
                            c.Style.Font.Bold = true;
                        }
                        else if (a.Status == AffectationStatusEnum.Souhaite)
                        {
                            c.Style.Font.FontColor = XLColor.DarkGreen;
                            c.Style.Font.Bold = true;
                        }
                    }
                    else
                    {
                        c.Value = a.Benevole.Prenom + " " + a.Benevole.Nom.Substring(0,1);
                        c.Style.Font.FontColor = a.Benevole.GetBenevoleColor();
                    }
                    
                }

                if (isFirst)
                    c.Style.Border.TopBorder = XLBorderStyleValues.Thick;
                if (isLast)
                    c.Style.Border.BottomBorder = XLBorderStyleValues.Thick;

                if (!crenauxNeeded.ContainsKey(d.NoCreneau))
                {
                    c.Style.Fill.BackgroundColor = XLColor.Black;
                    c.Style.Fill.PatternBackgroundColor = XLColor.White;
                    c.Style.Fill.PatternType = XLFillPatternValues.DarkDown;
                }
                else 
                {
                    if (tacheLineCount >= crenauxNeeded[d.NoCreneau].NbBenevoleMax)
                    {
                        c.Style.Fill.BackgroundColor = XLColor.Black;
                        c.Style.Fill.PatternBackgroundColor = XLColor.White;
                        c.Style.Fill.PatternType = XLFillPatternValues.DarkDown;
                    }
                    else if (tacheLineCount >= crenauxNeeded[d.NoCreneau].NbBenevoleMin)
                    {
                        c.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                }
                i++;
            }
        }


        private static void FillNewTache(IXLWorksheet sheet, Tache tache, JourEvenement jour, ref int line, Planning p, IEnumerable<Affectation> affectations, bool readableExport) 
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
                FIllNewRow(r, jour, tache, p, i == 0, i == maxB - 1, i, affectations,  readableExport);
                line++;
            }
        }
    }
}