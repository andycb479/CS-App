using CS_APP.DataLayer.Repository;

namespace CS_APP.DataLayer.Models
{
     public class AuditPolicy : Entity
     {
          public string Name { get; set; }
          public byte[] File { get; set; }

     }
}
