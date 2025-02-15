﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCService
{
    public interface ITomIOCServiceCollection
    {
        void AddTransient(Type serviceType, Type implementationType, string shortName);
        T GetService<T>();
        T GetService<T>(string shortName);

    }
}
