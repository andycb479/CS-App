using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_APP.DataLayer.Models
{
     public class Audit
     {
          public string Type { get; set; }
          public string Description { get; set; }
          public string Info { get; set; }
          public string Solution { get; set; }
          public string SeeAlso { get; set; }
          public AuditValueType ValueType { get; set; }
          public string ValueData { get; set; }
          public string RegKey { get; set; }
          public string RegItem { get; set; }
          public string RegOption { get; set; }
          public string Reference { get; set; }

     }
}
