using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBOT.Models
{
    class CommandServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return this;
        }
    }
}
