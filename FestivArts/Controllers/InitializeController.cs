using ClosedXML.Excel;
using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace FestivArts.Controllers
{
    public class InitializeController : Controller
    {
        private FestivArtsContext db = new FestivArtsContext();
        // GET: Admin
        public ActionResult GenerateCreneauDef()
        {
            if (db.Creneaux.Count() > 0)
                throw new ArgumentException("Creneau non vide");

            int duree = db.Confs.First(s => s.IsCurrent).DureeCreneauMinute;
            foreach (var j in db.JourEvenements) 
            {
                DateTime cur = j.DateDebut;
                int i = 0;
                while(cur < j.DateFin)
                {
                    CreneauDef d = new CreneauDef()
                    {
                        JourEvenement = j,
                        NoCreneau = i
                    };
                    i++;
                    cur = cur.AddMinutes(duree);
                    db.CreneauDefs.Add(d);
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DeleteCreneauDef()
        {
            db.CreneauDefs.RemoveRange(db.CreneauDefs.ToList());
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public ActionResult ImportBenevole() 
        {
            return View();
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1).ToLower();
        }

        [ActionName("ImportBenevole")]
        [HttpPost]
        public ActionResult ImportBenevolePost()
        {
            var list = new List<GoogleDriveResult>();
            var ist = Request.Files[0].InputStream;
            ist.Position = 0;
            using(var workbook = new XLWorkbook(@"C:\Users\Titho\Downloads\import.xlsx") )
            {
                using (var ws = workbook.Worksheet(1)) 
                {
                    int i = 2;
                    var row = ws.Row(i);
                    while (!row.IsEmpty()) 
                    {
                        int c = 2;
                        GoogleDriveResult g = new GoogleDriveResult() 
                        { 
                            Nom = row.Cell(c++).GetValue<string>(),
                            Prenom = row.Cell(c++).GetValue<string>(),
                            Telephone = row.Cell(c++).GetValue<string>(),
                            Mail = row.Cell(c++).GetValue<string>().Trim().ToLower(),
                            Permis = row.Cell(c++).GetValue<string>(),
                            DispoMercredi = row.Cell(c+=2).GetValue<string>(),
                            CommentaireMercredi = row.Cell(c++).GetValue<string>(),
                            Preference = row.Cell(c+=2).GetValue<string>(),
                            NonPreference = row.Cell(c++).GetValue<string>(),
                            DispoSamedi = row.Cell(c++).GetValue<string>(),
                            CommentaireSamedi = row.Cell(c++).GetValue<string>(),
                            DispoDimanche = row.Cell(c++).GetValue<string>(),
                            CommentaireDimanche = row.Cell(c++).GetValue<string>(),
                            DispoLundi = row.Cell(c++).GetValue<string>(),
                            CommentaireLundi = row.Cell(c++).GetValue<string>(),
                            PrecisionGeneral = row.Cell(c+=2).GetValue<string>(),
                            DispoVendredi = row.Cell(c++).GetValue<string>(),
                            CommentaireVendredi = row.Cell(c++).GetValue<string>(),
                            Majeur = row.Cell(c += 2).GetValue<string>(),
                            DispoJeudi = row.Cell(c++).GetValue<string>(),
                            CommentaireJeudi = row.Cell(c++).GetValue<string>()
                        };
                        string valid = row.Cell(c).GetValue<string>().Trim();
                        if(valid == "1")
                            list.Add(g);
                        i++;
                        row = ws.Row(i);
                    }
                }
            }

            //Creation Bénévoles
            foreach (var g in list) 
            {
                Benevole b = db.Benevoles.FirstOrDefault(s => s.Email.Trim().ToLower() == g.FormatedMail);
                if (b == null) 
                {
                    b = new Benevole();
                    db.Benevoles.Add(b);
                    b.Email = g.FormatedMail;
                }
                b.Nom = UppercaseFirst(g.FormatedNom.Trim());
                b.Prenom = UppercaseFirst(g.FormatedPrenom.Trim());
                b.Tel = g.FormatedTel;
                b.Permis = g.APermis;
                b.Majeur = g.EstMajeur;
            }
            db.SaveChanges();

            //Gestion des preferences
            foreach (var g in list)
            {
                Benevole b = db.Benevoles.Include("Preferences").First(s => s.Email.Trim().ToLower() == g.FormatedMail);
                foreach (var kv in g.Prefs) 
                {
                    Preference p = b.Preferences.FirstOrDefault( s => s.TypeTacheId == ((int) kv.Key) &&  s.BenevoleId == b.Id);
                    if (p == null)
                    {
                        p = new Preference();
                        db.Preferences.Add(p);
                        p.TypeTacheId = (int)kv.Key;
                        p.BenevoleId = b.Id;
                    }
                    p.Valeur = kv.Value;
                    db.SaveChanges();
                }
            }
            db.SaveChanges();


            //Gestion des dispo
            foreach (var g in list)
            {
                Benevole b = db.Benevoles.Include("Dispoes").First(s => s.Email.Trim().ToLower() == g.FormatedMail);
                foreach (var c in db.CreneauDefs.Include("JourEvenement"))
                {
                    Dispo d = b.Dispoes.FirstOrDefault(s => s.CreneauDefId == c.Id && s.BenevoleId == b.Id);
                    if (d == null)
                    {
                        d = new Dispo();
                        db.Dispoes.Add(d);
                        d.CreneauDefId = c.Id;
                        d.BenevoleId = b.Id;
                        d.ModifManuel = false;
                    }

                    if (!d.ModifManuel) 
                    {
                        string dispoTxt;
                        switch(c.JourEvenement.Nom.Trim().ToLower())
                        {
                            case "mercedi":
                                dispoTxt = g.DispoMercredi;
                                break;
                            case "jeudi":
                                dispoTxt = g.DispoJeudi;
                                break;
                            case "vendredi":
                                dispoTxt = g.DispoVendredi;
                                break;
                            case "samedi":
                                dispoTxt = g.DispoSamedi;
                                break;
                            case "dimanche":
                                dispoTxt = g.DispoDimanche;
                                break;
                            default :
                                throw new ArgumentException("c.JourEvenement.Nom");
                        }
                        d.EstDispo = isDispo(dispoTxt, c);
                        
                    }
                }

                db.SaveChanges();
            }

            db.SaveChanges();


            //Gestion des commentaire dispo
            foreach (var g in list)
            {
                Benevole b = db.Benevoles.Include("Dispoes").First(s => s.Email.Trim().ToLower() == g.FormatedMail);
                foreach (var j in db.JourEvenements)
                {
                    CommentaireDispo c = b.CommentaireDispoes.FirstOrDefault(s => s.JourId == j.Id && s.BenevoleId == b.Id);
                    if (c == null)
                    {
                        c = new CommentaireDispo();
                        b.CommentaireDispoes.Add(c);
                        c.JourId = j.Id;
                        c.BenevoleId = b.Id;
                    }
                    string dispoTxt;
                    switch (j.Nom.Trim().ToLower())
                    {
                        case "mercredi":
                            dispoTxt = g.CommentaireMercredi;
                            break;
                        case "jeudi":
                            dispoTxt = g.CommentaireJeudi;
                            break;
                        case "vendredi":
                            dispoTxt = g.CommentaireVendredi;
                            break;       
                        case "samedi":   
                            dispoTxt = g.CommentaireSamedi;
                            break;       
                        case "dimanche": 
                            dispoTxt = g.CommentaireDimanche;
                            break;
                        default:
                            throw new ArgumentException(j.Nom);
                    }
                    c.Commentaire = dispoTxt.Trim();
                }
            }
            db.SaveChanges();
            return View();
        }

        private bool isDispo(string txt, CreneauDef c) 
        {
            txt = txt.ToLower();
            if (c.Debut.Hour >= 7 && c.Fin.Hour <= 13 && c.Fin.Hour >= 7) 
            {
                if (txt.Contains("matin"))
                    return true;
            }
            if (c.Debut.Hour >= 12 && c.Fin.Hour <= 19 && c.Fin.Hour >= 12)
            {
                if (txt.Contains("après"))
                    return true;
            }

            if (c.Debut.Hour >= 18 || c.Fin.Hour <= 2)
            {
                if (txt.Contains("soir"))
                    return true;
            }
            return false;
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