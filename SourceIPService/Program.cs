using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace SourceIPService
{
    class Program
    {
        static void Main(string[] args)
        {

            RegisterService();
            //RunInConsole();
        }

        static void RegisterService()
        {
            ServiceBase svc = new Service();
            ServiceBase.Run(svc);
        }

    }
}
