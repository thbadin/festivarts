using FestivArts.Models.Entity;
using FestivArts.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FestivArts.Utils
{
    public static class AffectationUtils
    {
        public static void FillAffectationStatus(FestivArtsContext ctx, IEnumerable<Affectation> affectations) 
        {
            foreach (var a in affectations)
                a.Status = AffectationStatusEnum.Unknown;

            Dictionary<int,HashSet<int>> dispoes = new Dictionary<int,HashSet<int>>();
            foreach(var d in ctx.Dispoes)
            {
                if(! dispoes.ContainsKey( d.BenevoleId))
                    dispoes[d.BenevoleId] = new HashSet<int>();
                var s = dispoes[d.BenevoleId];
                s.Add(d.CreneauDefId);
            }

            Dictionary<int, Dictionary<int, Preference>> prefs = new Dictionary<int, Dictionary<int, Preference>>();
            foreach (var p in ctx.Preferences) 
            {
                if (!prefs.ContainsKey(p.BenevoleId))
                    prefs.Add(p.BenevoleId, new Dictionary<int, Preference>());

                prefs[p.BenevoleId].Add(p.TypeTacheId, p);
            }

            Dictionary<int, Dictionary<int, Affectation>> dupli = new Dictionary<int, Dictionary<int, Affectation>>();
            foreach (var a in affectations)
            {

                //Etape 1 : erreur
                if (!dispoes.ContainsKey(a.BenevoleId))
                {
                    a.Status = AffectationStatusEnum.NonDisponible;
                }
                else 
                {
                    var s = dispoes[a.BenevoleId];
                    if (!s.Contains(a.Creneau.CreneauDefId)) 
                    {
                        a.Status = AffectationStatusEnum.NonDisponible;
                    }
                    else 
                    {
                        if (! dupli.ContainsKey(a.Creneau.CreneauDefId))
                        {
                                dupli.Add(a.Creneau.CreneauDefId, new Dictionary<int,Affectation>());
                        }
                        var duplic = dupli[a.Creneau.CreneauDefId];
                        if( duplic.ContainsKey(a.BenevoleId))
                        {

                            duplic[a.BenevoleId].Status = AffectationStatusEnum.Duplique;
                            a.Status = AffectationStatusEnum.Duplique;
                        }
                        else
                        {
                            duplic.Add(a.BenevoleId, a);
                        }
                    }
                }


                //Etape 2 : appreciation
                if (a.Status == AffectationStatusEnum.Unknown )
                {
                    if(prefs.ContainsKey(a.BenevoleId) ){
                        a.Status = AffectationStatusEnum.Correct;
                        if (prefs[a.BenevoleId].ContainsKey(a.Creneau.Tache.TypeTacheId))
                        {
                            var p = prefs[a.BenevoleId][a.Creneau.Tache.TypeTacheId];
                            if (p.Valeur > 0)
                                a.Status = AffectationStatusEnum.Souhaite;
                            else if (p.Valeur < 0)
                                a.Status = AffectationStatusEnum.NonSouhaite;
                        }
                    }
                }

            }


        }
    }
}