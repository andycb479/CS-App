using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS_APP.DataLayer.Models
{
     class AuditPolicyConfiguration : IEntityTypeConfiguration<AuditPolicy>
     {
          public void Configure(EntityTypeBuilder<AuditPolicy> builder)
          {

          }
     }
}
