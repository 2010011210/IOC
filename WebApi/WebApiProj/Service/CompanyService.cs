namespace WebApiProj.Service
{
    public class CompanyService : ICompany
    {
        public string GetName(string address)
        {
            return $"地址：{address}:东中聚合有限公司{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
        }
    }
}
