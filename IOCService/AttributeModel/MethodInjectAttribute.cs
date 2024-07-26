using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCService
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.All)]
    public class MethodInjectAttribute : Attribute
    {
    }
}
