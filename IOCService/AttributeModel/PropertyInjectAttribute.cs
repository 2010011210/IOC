using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCService
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyInjectAttribute : Attribute
    {

    }
}
