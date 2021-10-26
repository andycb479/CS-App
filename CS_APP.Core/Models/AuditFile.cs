using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CS_APP.Core.Models
{
     public class AuditFile
     {
          private readonly string _filePath;
          private string _content;
          private List<Dictionary<string, string>> _listOfDictionaries;

          public AuditFile(string filePath)
          {
               _filePath = filePath;
               _listOfDictionaries = new List<Dictionary<string, string>>();

          }

          public async Task<List<Dictionary<string, string>>> Parse()
          {
               _content = await File.ReadAllTextAsync(_filePath);

               var pattern = @"type|description|info|solution|see_also|value_type|value_data|right_type|reference|reg_key|reg_item|reg_option|reference|check_type|reg_ignore_hku_users|audit_policy_subcategory";

               var auditStrings = Regex.Matches(_content, @"<custom_item>[\s\S]*?<\/custom_item>");

               var auditsList = new List<string>();

               foreach (Match policy in auditStrings)
               {
                    auditsList.Add(Regex.Replace(policy.Value, @"<custom_item>|</custom_item>", ""));
               }

               foreach (var audit in auditsList)
               {
                    var type = Regex.Split(audit, @"\s+:");

                    List<string> result = new List<string>();

                    foreach (var str in type)
                    {
                         var value = Regex.Match(str, pattern).Value;

                         if (!string.IsNullOrWhiteSpace(value))
                         {
                              var temp = Regex.Split(str, pattern)[0];
                              temp = Regex.Replace(temp, "[\"\n]", string.Empty).Trim();
                              result.Add(temp);
                              result.Add(value);
                         }
                         else
                         {
                              result.Add(Regex.Replace(str, "[\"\n]", string.Empty).Trim());
                         }

                    }

                    var valueDictionary = new Dictionary<string, string>();

                    for (int i = 1; i < result.Count - 1; i++)
                    {
                         if (Regex.IsMatch(result[i], pattern))
                         {
                              valueDictionary[result[i]] = result[i + 1];
                         }
                    }

                    _listOfDictionaries.Add(valueDictionary);
               }

               return _listOfDictionaries;
          }
     }
}
