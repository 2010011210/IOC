using IOCDemoProject.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCDemoProject.Model
{
    public class Power : IPower
    {
        public  IMobilePhone _mobilePhone { get; set; }
        public IAndriod _andriod { get; set; }


        //public Power()
        //{

        //}

        public Power(IMobilePhone mobilePhone) 
        {
            this._mobilePhone = mobilePhone;
        }

        public Power(IMobilePhone mobilePhone, IAndriod andridPhone)
        {
            this._mobilePhone = mobilePhone;
            this._andriod = andridPhone;
        }
    }
}
