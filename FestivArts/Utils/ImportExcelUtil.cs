using ClosedXML.Excel;
using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace FestivArts.Utils
{
    public class ImportExcelUtil
    {
        public static void ImportPlanning(Planning p, FestivArtsContext ctx, XLWorkbook book )
        {
            foreach (var sheet in book.Worksheets) 
            {
                int jourId;
                if(!int.TryParse(sheet.Cell("A1").Value.ToString(), out jourId))
                {
                    throw new ImportException("Cellule A1 n'est pas un nombre");
                }
                JourEvenement j = ctx.JourEvenements.Include("CreneauDefs").FirstOrDefault(s => s.Id == jourId);
                if (j == null)
                {
                    throw new ImportException("Cellule A1 n'est pas un jour valide");
                }
                int line = 2;
                while (line < sheet.RowCount() && line < 1000) 
                {
                    IXLRow r = sheet.Row(line);
                    string fc = r.Cell(1).Value.ToString();
                    if (!string.IsNullOrWhiteSpace(fc)) 
                    {
                        int tacheId = 0;
                        if (!int.TryParse(fc, out tacheId))
                        {
                            throw new ImportException("Ligne "+line+" : not a number");
                        }
                        Tache t = ctx.Taches.Include("Creneaux.CreneauDef").FirstOrDefault(s => s.Id == tacheId);
                        if (t == null)
                        {
                            throw new ImportException("Ligne "+line+" : not a valid tache id");
                        }
                        line ++;
                        ImportTache(sheet, ref line, t, j, p, ctx);
                    }
                    line++;


                }


            }


        }

        private static void ImportTache(IXLWorksheet sheet, ref int line, Tache t, JourEvenement jour, Planning p, FestivArtsContext ctx)
        {
            IXLRow timeRow = sheet.Row(line);
            line++;
            int maxB = t.GetMaxBenevoleByDay(jour.Id);
            Regex regex = new Regex("^([0-9])");
            for (int l = 0; l < maxB; l++)
            {

                IXLRow r = sheet.Row(line);
                int i = ExcelUtils.FIRST_PLAN_COLUMN;
                foreach (CreneauDef d in jour.CreneauDefs.OrderBy(s => s.NoCreneau))
                {
                    string benStr  = r.Cell(i).Value.ToString();
                    if (!string.IsNullOrWhiteSpace(benStr)) 
                    {
                        var m = regex.Match(benStr.Trim());
                        if (m.Success) 
                        {
                            int id = int.Parse(m.Groups[0].Captures[0].Value);
                            var b = ctx.Benevoles.FirstOrDefault(s => s.Id == id);
                            if (b == null)
                                throw new ImportException(string.Format("Case({0},{1}) n° de bénévole introuvable : {2}'", line, i, benStr));
                            var c = t.Creneaux.FirstOrDefault( s => s.CreneauDefId == d.Id );
                            if (c == null)
                                throw new ImportException(string.Format("Case({0},{1}) creneau introuvable. Creneau def {2}'", line, i,  d.Id ));

                            Affectation af = new Affectation()
                            {
                                BenevoleId = b.Id,
                                PlanningId = p.Id,
                                CreneauId = c.Id
                            };
                            ctx.Affectations.Add(af);


                        }
                        else 
                        {
                            throw new ImportException(string.Format("Case({0},{1}) ne correspond pas a un n° de bénévole : {2}'", line, i, benStr));
                        }
                    }
                    
                    i++;
                }
                line++;
            }
            ctx.SaveChanges();
        }
    }
}