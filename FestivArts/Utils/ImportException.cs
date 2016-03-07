using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FestivArts.Utils
{
    public class ImportException : Exception
    {
        public ImportException(string msg) : base(msg) { }
    }
}