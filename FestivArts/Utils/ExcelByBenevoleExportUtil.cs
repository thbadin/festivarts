using ClosedXML.Excel;
using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace FestivArts.Utils
{
    public static class ExcelByBenevoleExportUtil
    {
        public static void FillPlanning(XLWorkbook book, FestivArtsContext ctx, Planning p, bool readable)
        {
            foreach (var j in ctx.JourEvenements.Include("CreneauDefs.Creneaux.Affectations.Benevole"))
            {
                var worksheet = book.Worksheets.Add(j.Nom);
                FillJour(worksheet, ctx, p, j, readable);
                worksheet.Columns().Width = 5;
                worksheet.Column(1).Width = 25;
                worksheet.SheetView.Freeze(2, 1);
            }
        }

        private static void FillJour(IXLWorksheet sheet, FestivArtsContext ctx, Planning p, JourEvenement j, bool readable)
        {
            IXLCell c = sheet.Cell("A1");
            c.Value = "Planning de " + j.Nom;
            c.Style.Font.FontSize = 34;
            c.Style.Font.Bold = true;
          

            var creneauxDef = ctx.CreneauDefs.Where(s => s.JourId == j.Id).OrderBy(s => s.NoCreneau);
            IXLRow r = sheet.Row(2);
            int col = 2;
            foreach (var cd in creneauxDef)
            {
                c = r.Cell(col);
                c.Value = "'" + cd.Debut.ToString("HH:mm", CultureInfo.InvariantCulture);
                col++;
            }

            int i = 3;
            var listeBenevole = ctx.Benevoles.ToList();
            foreach (var b in listeBenevole.OrderBy( s => s.Prenom))
            {

                var aff = p.Affectations.Where( s => s.BenevoleId == b.Id && s.Creneau.CreneauDef.JourId == j.Id);
                var disp = b.Dispoes.Where(s => s.CreneauDef.JourId == j.Id && s.EstDispo);
                FillBenevole(sheet, ctx, ref i, b, aff, disp, creneauxDef, listeBenevole, readable);
                i ++;
            }
            if (!readable)
            {
                i += 1;
                FillCalculManqueDispo(sheet.Row(i), ctx, j);

                i += 1;
                FillNbBenevoleRepas(sheet.Row(i), ctx, j);
                i += 1;
                FillNbBenevoleRepasReel(sheet.Row(i), ctx, j);
            }
        }


        private static int NbBenevoleRepas(FestivArtsContext ctx, JourEvenement j, int debut, int fin)
        {
            Dictionary<int, int> nbBeneParTache = new Dictionary<int, int>();
            var crenDefs = ctx.CreneauDefs.Include("Creneaux").Where(c => c.JourId == j.Id).ToList().Where(c => c.Debut.Hour >= debut && c.Fin.Hour <= fin && c.Fin.DayOfYear == j.DateDebut.DayOfYear).ToList();
            foreach (CreneauDef cd in crenDefs)
            {
                foreach (var cr in cd.Creneaux)
                {
                    if (!nbBeneParTache.ContainsKey(cr.TacheId))
                    {
                        nbBeneParTache.Add(cr.TacheId, 0);
                    }
                    nbBeneParTache[cr.TacheId] = Math.Max(nbBeneParTache[cr.TacheId], cr.NbBenevoleMax);
                }
            }
            return nbBeneParTache.Values.Sum();
        }

        private static int NbBenevoleRepasReel(FestivArtsContext ctx, JourEvenement j, int debut, int fin)
        {
            HashSet<int> benevoles = new HashSet<int>();
            foreach (CreneauDef cd in ctx.CreneauDefs.Include("Creneaux").Where(c => c.JourId == j.Id).ToList().Where(c => c.Debut.Hour >= debut && c.Fin.Hour <= fin && c.Fin.DayOfYear == j.DateDebut.DayOfYear))
            {
                
                foreach (var cr in cd.Creneaux)
                {
                    cr.Affectations.Select(a => a.BenevoleId).ForEach(a => { if (!benevoles.Contains(a)) { benevoles.Add(a); } });
                }
            }
            return benevoles.Count;
        }

        private static void FillNbBenevoleRepasReel(IXLRow row, FestivArtsContext ctx, JourEvenement j)
        {
            IXLCell ce = row.Cell(1);
            ce.Style.Font.Bold = true;
            ce.Value = "Nb  midi (reel): ";

            ce = row.Cell(2);
            ce.Style.Font.Bold = true;
            ce.Value = NbBenevoleRepasReel(ctx, j, 11, 14);

            ce = row.Cell(4);
            ce.Style.Font.Bold = true;
            ce.Value = "Nb  soir (reel): ";

            ce = row.Cell(5);
            ce.Style.Font.Bold = true;
            ce.Value = NbBenevoleRepasReel(ctx, j, 18, 21);


        }

        private static void FillNbBenevoleRepas(IXLRow row, FestivArtsContext ctx, JourEvenement j)
        {
            IXLCell ce = row.Cell(1);
            ce.Style.Font.Bold = true;
            ce.Value = "Nb  midi : ";

            ce = row.Cell(2);
            ce.Style.Font.Bold = true;
            ce.Value = NbBenevoleRepas(ctx,j,11,14);

            ce = row.Cell(4);
            ce.Style.Font.Bold = true;
            ce.Value = "Nb  soir : ";

            ce = row.Cell(5);
            ce.Style.Font.Bold = true;
            ce.Value = NbBenevoleRepas(ctx, j, 18, 21);


        }
        private static void FillCalculManqueDispo(IXLRow row, FestivArtsContext ctx, JourEvenement j)
        {
            IXLCell ce = row.Cell(1);
            ce.Style.Font.Bold = true;
            ce.Value = "Total manque/surplus dispo";

            int i = 2;
            foreach(CreneauDef cd in ctx.CreneauDefs.Include("Dispoes").Include("Creneaux").Where( c => c.JourId == j.Id).ToList())
            {
                int minBen = cd.Creneaux.Sum(cr => cr.NbBenevoleMin);
                int maxBen = cd.Creneaux.Sum(cr => cr.NbBenevoleMax);
                int dispo = cd.Dispoes.Where(d => d.EstDispo).Count();
                IXLCell c = row.Cell(i++);
                c.Value = dispo - minBen;
                c.Style.Font.Bold = true;
                if (dispo  < minBen)
                {
                    c.Style.Font.FontColor = XLColor.Red;
                }else if (dispo   > maxBen)
                {
                    c.Style.Font.FontColor = XLColor.DarkGreen;
                }
                else
                {
                    c.Style.Font.FontColor = XLColor.DarkOrange;
                }
            }
        }

        private static void FillBenevole(IXLWorksheet sheet, FestivArtsContext ctx, ref int row, Benevole b, IEnumerable<Affectation> affectations, IEnumerable<Dispo> dispos, IEnumerable<CreneauDef> creneaux, IEnumerable<Benevole> listeBenevole, bool readable)
        {
            var aff = new Dictionary<int, List<Affectation>>();
            var disp = new Dictionary<int, Dispo>();

            foreach (var a in affectations)
            {
                if (!aff.ContainsKey(a.Creneau.CreneauDefId))
                    aff.Add(a.Creneau.CreneauDefId, new List<Affectation>());

                aff[a.Creneau.CreneauDefId].Add(a);

            }
            dispos.ForEach(s => disp.Add(s.CreneauDefId, s));


            IXLRow r = sheet.Row(row);
            IXLCell c = r.Cell(1);
            c.Value = (readable ? "" : b.Id+" - ") + b.GetPrenomUnique(listeBenevole);

            int i = 2;
            int prevTacheId = -1;
            int firsCell = -1;
            foreach (var cren in creneaux)
            {

                c = r.Cell(i);
                c.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                c.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                c.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                c.Style.Border.LeftBorder = XLBorderStyleValues.Thin;


                if (aff.ContainsKey(cren.Id))
                {

                    if(aff[cren.Id].Count == 1)
                    {
                        //Cas normal 1 affectation
                        if (prevTacheId > 0  && aff[cren.Id][0].Creneau.TacheId == prevTacheId) 
                        {
                            //cas identique : rien a faire
                        }
                        else 
                        {
                            if (prevTacheId > 0) { 
                                if (firsCell != i - 1) 
                                {
                                    sheet.Range(row, firsCell, row, i - 1).Merge();
                                }
                            }
                            firsCell = i;
                            prevTacheId = aff[cren.Id][0].Creneau.TacheId;
                        }
                    }
                    else
                    {
                        if (prevTacheId > 0)
                        {
                            if (firsCell != i - 1)
                            {
         
                                sheet.Range(row, firsCell, row, i - 1).Merge();
                            }
                        }
                        firsCell = -1;
                        prevTacheId = -1;
                    }
                    c.Value = string.Join(",", aff[cren.Id].Select(s => s.Creneau.Tache.Nom));
                    c.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    if (aff[cren.Id].Count > 1)
                    {
                        c.Style.Font.FontColor = XLColor.Red;
                        c.Style.Font.Bold = true;
                    }
                }
                else{
                    if (prevTacheId > 0)
                    {
                        if (firsCell != i - 1)
                        {
                            sheet.Range(row, firsCell, row, i - 1).Merge();
                        }
                    }
                    firsCell = -1;
                    prevTacheId = -1;
                    if( ! disp.ContainsKey( cren.Id))
                    {
                        c.Style.Fill.BackgroundColor = XLColor.Black;
                        c.Style.Fill.PatternBackgroundColor = XLColor.White;
                        c.Style.Fill.PatternType = XLFillPatternValues.DarkDown;
                    }
                }
                i++;
            }
            if (prevTacheId > 0)
            {
                if (firsCell != i - 1)
                {
                    sheet.Range(row, firsCell, row, i - 1).Merge();
                }
            }
        }

    }
}