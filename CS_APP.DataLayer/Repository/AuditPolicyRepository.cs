using CS_APP.DataLayer.Models;

namespace CS_APP.DataLayer.Repository
{
     public class AuditPolicyRepository : Repository<AuditPolicy>, IAuditPolicyRepository
     {
          public AuditPolicyRepository(ApplicationContext context) :
               base(context)
          {
          }
     }
}
