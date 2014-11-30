using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;
using System.ComponentModel;
using System.ServiceProcess;

namespace SourceIPService
{
    // the RunInstaller attribute tells installutil to call this class, enumerate it's installers 
    // collection, and call the Install methods of all the objects in the collection
    [RunInstaller(true)]
    public class SourceIPInstaller : Installer
    {
        public SourceIPInstaller()
        {
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
            processInstaller.Account = ServiceAccount.LocalSystem;
            processInstaller.Username = null;
            processInstaller.Password = null;

            ServiceInstaller serviceInstaller = new ServiceInstaller();
            serviceInstaller.ServiceName = "SourceIPService";
            serviceInstaller.DisplayName = "SourceIPService";
            serviceInstaller.Description = @"HTTP Service to return IP Address of caller";
            serviceInstaller.StartType = ServiceStartMode.Manual;

            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
