using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    [System.AttributeUsage(AttributeTargets.Property,Inherited = false, AllowMultiple = true)]
    public class IOAttribute : Attribute
    {
        public IOAttribute()
        {
           
        }
    }
}
