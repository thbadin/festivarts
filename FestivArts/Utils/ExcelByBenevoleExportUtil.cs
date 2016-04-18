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
        public static void FillPlanning(XLWorkbook book, FestivArtsContext ctx, Planning p)
        {
            foreach (var j in ctx.JourEvenements.Include("CreneauDefs.Creneaux.Affectations.Benevole"))
            {
                var worksheet = book.Worksheets.Add(j.Nom);
                FillJour(worksheet, ctx, p, j);
                worksheet.Columns().Width = 5;
                worksheet.Column(1).Width = 25;
                worksheet.SheetView.Freeze(2, 1);
            }
        }

        private static void FillJour(IXLWorksheet sheet, FestivArtsContext ctx, Planning p, JourEvenement j)
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
            foreach (var b in ctx.Benevoles.OrderBy( s => s.Prenom))
            {

                var aff = p.Affectations.Where( s => s.BenevoleId == b.Id && s.Creneau.CreneauDef.JourId == j.Id);
                var disp = b.Dispoes.Where(s => s.CreneauDef.JourId == j.Id && s.EstDispo);
                FillBenevole(sheet, ctx, ref i, b, aff, disp, creneauxDef);
                i ++;
            } 
        }
        private static void FillBenevole(IXLWorksheet sheet, FestivArtsContext ctx, ref int row, Benevole b, IEnumerable<Affectation> affectations, IEnumerable<Dispo> dispos, IEnumerable<CreneauDef> creneaux)
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
            c.Value = b.ToString();

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