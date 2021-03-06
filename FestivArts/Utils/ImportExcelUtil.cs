﻿using ClosedXML.Excel;
using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Regex regex = new Regex("^([0-9]+)");
            var aAjouter = new Dictionary<int, HashSet<int>>();
         

            for (int l = 0; l < maxB; l++)
            {
                IXLRow r = sheet.Row(line);
                int i = ExcelUtils.FIRST_PLAN_COLUMN;
                foreach (CreneauDef d in jour.CreneauDefs.OrderBy(s => s.NoCreneau))
                {
                    var cell = r.Cell(i);
                    string benStr = cell.Value.ToString();

                    if (!string.IsNullOrWhiteSpace(benStr))
                    {
                        var m = regex.Match(benStr.Trim());
                        if (m.Success )
                        {
                            int id = int.Parse(m.Groups[0].Captures[0].Value);
                            var b = ctx.Benevoles.FirstOrDefault(s => s.Id == id);
                            if (b == null)
                                throw new ImportException(string.Format("Cell ({0}) Tache {1} : n° de bénévole introuvable : {2}'", cell.Address.ToStringRelative(true), t.Nom, benStr));
                            var c = t.Creneaux.FirstOrDefault(s => s.CreneauDefId == d.Id);
                            if (c == null)
                                throw new ImportException(string.Format("Cell ({0}) Tache {1} : creneau introuvable. Creneau def {2}'", cell.Address.ToStringRelative(true), t.Nom, d.Id));

                            if(! aAjouter.ContainsKey(b.Id))
                            {
                                aAjouter.Add(b.Id, new HashSet<int>());
                            }
                            if(!aAjouter[b.Id].Contains(c.Id))
                            {
                                aAjouter[b.Id].Add(c.Id);
                            }
                        }
                        else
                        {
                            throw new ImportException(string.Format("Cell ({0}) ne correspond pas a un n° de bénévole : {1}'", cell.Address.ToStringRelative(true),  benStr));
                        }
                    }
                    i++;
                }
                line++;
            }
            if (aAjouter.Count > 0)
            {
                    
                    var anciensA = ctx.Affectations.Where(s => s.Creneau.CreneauDef.JourId == jour.Id &&
                        s.PlanningId == p.Id && s.Creneau.TacheId == t.Id).ToList();
                foreach(Affectation a in anciensA)
                {
                    if(aAjouter.ContainsKey(a.BenevoleId) && aAjouter[a.BenevoleId].Contains(a.CreneauId))
                    {
                        //L'affectation existait déjà, on ne fait rien
                        aAjouter[a.BenevoleId].Remove(a.CreneauId);
                    }else
                    {
                        //N'existe plus, on la supprime;
                        ctx.Affectations.Remove(a);
                    }
                }

                foreach(var kv in aAjouter.Where( s => s.Value.Count > 0 ).ToList())
                {
                    foreach(var v in kv.Value)
                    {
                        Affectation a = new Affectation()
                        {
                            BenevoleId = kv.Key,
                            CreneauId = v,
                            PlanningId = p.Id
                        };
                        ctx.Affectations.Add(a);
                    }
                }
                ctx.SaveChanges();
            }
        }
    }
}