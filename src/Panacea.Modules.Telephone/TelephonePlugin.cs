using Panacea.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone
{
    class TelephonePlugin : IPlugin
    {
        public Task BeginInit()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            
        }

        public Task EndInit()
        {
            return Task.CompletedTask;
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }
}
