using System;
using System.Collections.Generic;
using System.IO;

namespace CS_APP.Core.Models
{
     public class IniFile
     {
          public static IniFile Load(string filename)
          {
               var result = new IniFile();
               result.Sections = new Dictionary<string, IniSection>();
               var section = new IniSection(String.Empty);
               result.Sections.Add(section.Name, section);

               foreach (var line in File.ReadAllLines(filename))
               {
                    var trimedLine = line.Trim();
                    switch (line[0])
                    {
                         case ';':
                              continue;
                         case '[':
                              section = new IniSection(trimedLine.Substring(1, trimedLine.Length - 2));
                              result.Sections.Add(section.Name, section);
                              break;
                         default:
                              var parts = trimedLine.Split('=');
                              if (parts.Length > 1)
                              {
                                   section.Add(parts[0].Trim(), parts[1].Trim());
                              }
                              break;
                    }
               }

               return result;
          }

          public IDictionary<string, IniSection> Sections { get; private set; }
     }
}
