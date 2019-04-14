using System;

namespace SatCore.Attribute
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class IOAttribute : System.Attribute
    {
        public IOAttribute()
        {

        }
    }
}
