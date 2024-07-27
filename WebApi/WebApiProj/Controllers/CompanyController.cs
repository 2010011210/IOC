using Microsoft.AspNetCore.Mvc;
using WebApiProj.Service;

namespace WebApiProj.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private ICompany _company;  // 构造函数注入

        public CompanyController(ICompany company) 
        {
            this._company = company;
        }

        [HttpGet]
        public string GetName(string address) 
        {
            return _company.GetName(address);
        }
    }
}
