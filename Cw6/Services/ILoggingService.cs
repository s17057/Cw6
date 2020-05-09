using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw6.Services
{
    public interface ILoggingService
    {
        public void Log(string method,string path, string query, string bodyString);
    }
}
