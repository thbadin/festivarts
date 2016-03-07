using FestivArts.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FestivArts.DAL
{
    public interface IDal : IDisposable
    {
        FestivArtsContext Context { get; }

    }
}
