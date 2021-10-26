using System;
using System.Collections.Generic;

namespace CS_APP.Core.Models
{
     public class IniSection : Dictionary<string, string>
     {
          public IniSection(string name) : base(StringComparer.OrdinalIgnoreCase)
          {
               this.Name = name;
          }

          public string Name { get; private set; }
     }
}