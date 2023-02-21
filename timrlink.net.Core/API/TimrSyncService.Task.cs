using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace timrlink.net.Core.API
{
    public partial class Task
    {
        public Task Clone()
        {
            return (Task) MemberwiseClone();
        }
    }
}
