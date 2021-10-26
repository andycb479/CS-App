using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CS_APP.Core;
using Microsoft.Win32;


namespace CS_App
{
     public partial class AuditStatus : Window
     {
          private readonly List<Dictionary<string, string>> _audits;
          private readonly bool[] _passedAudits;
          private readonly string[] _failedAuditsResponse;
          private readonly RegistryKey _registryLocalMachine = Registry.LocalMachine;
          private readonly RegistryKey _registryCurrentUser = Registry.Users;
          private readonly IniFile _localSecurityPolicies;

          public AuditStatus(List<Dictionary<string, string>> audits)
          {
               _audits = audits;
               InitializeComponent();
               _passedAudits = new bool[_audits.Count];
               _failedAuditsResponse = new string[_audits.Count];
               _localSecurityPolicies = ExportLocalSecurityPolicy();
               CheckAudits();
               DisplayAudits(_audits);
          }

          private void CheckAudits()
          {
               for (var i = 0; i < _audits.Count; i++)
               {
                    var audit = _audits[i];

                    if (IsAuditRegistrySetting(audit))
                         CheckRegistrySetting(audit, i);
                    else if (IsAuditUserRightSetting(audit))
                         CheckUserRightSetting(audit, i);
                    else
                         _passedAudits[i] = true;
               }
          }

          private bool IsAuditRegistrySetting(Dictionary<string, string> audit)
          {
               return audit["type"] == "REGISTRY_SETTING";
          }

          private bool IsAuditUserRightSetting(Dictionary<string, string> audit)
          {
               return audit["type"] == "USER_RIGHTS_POLICY";
          }

          private void CheckRegistrySetting(Dictionary<string, string> audit, int auditIndex)
          {
               _localSecurityPolicies.Sections.TryGetValue("Registry Values", out var registryValues);

               foreach (var registryPair in registryValues)
               {
                    if (!registryPair.Key.Contains(audit["reg_item"])) continue;
                    if (!registryPair.Value.Contains(audit["value_data"])) continue;
                    _passedAudits[auditIndex] = true;
                    return;
               }

               var auditValue = audit["value_data"];

               var registryPath = audit["reg_key"]
                    .Replace(audit["reg_key"]
                              .Contains("HKLM")
                              ? "HKLM"
                              : "HKU",
                         audit["reg_key"]
                              .Contains("HKLM")
                              ? _registryLocalMachine.Name
                              : _registryCurrentUser.Name);

               if (audit["reg_key"].Contains("Internet Settings"))
               {
                    registryPath = registryPath.Replace("\\Policies", "");
               }

               var systemValue = Registry.GetValue(registryPath, audit["reg_item"], null);

               if (systemValue is string stringValue && stringValue == auditValue)
               {
                    _passedAudits[auditIndex] = true;
               }
               else if (systemValue is int intValue)
               {
                    int.TryParse(auditValue, out var auditIntValue);
                    if (intValue == auditIntValue) _passedAudits[auditIndex] = true;
               }
               else if (audit.ContainsKey("check_type"))
               {
                    var rgx = new Regex(auditValue);
                    _passedAudits[auditIndex] = rgx.IsMatch((string)systemValue ?? string.Empty);
               }

               if (!_passedAudits[auditIndex])
                    _failedAuditsResponse[auditIndex] = $"Expected {auditValue} got {systemValue ?? "null"}";
          }

          private void CheckUserRightSetting(Dictionary<string, string> audit, int auditIndex)
          {
               _localSecurityPolicies.Sections.TryGetValue("Privilege Rights", out var userRights);
               var auditRightType = audit["right_type"];
               userRights.TryGetValue(auditRightType, out var users);
               if (users == null && audit["value_data"] == "")
               {
                    _passedAudits[auditIndex] = true;
                    return;
               }

               if (users != null)
               {
                    users = users.Replace("*", "");
                    var userListSid = users.Split(",");
                    var userListNames = userListSid.Select(GetAccountTypeBySid).ToList();
                    foreach (var user in userListNames.Where(user => audit["value_data"].Contains(user)))
                         _passedAudits[auditIndex] = true;
               }
          }

          private void DisplayAudits(List<Dictionary<string, string>> renderAudits)
          {
               var passedCount = _passedAudits.Count(x => x);
               var failedCount = _passedAudits.Length - passedCount;

               FailedCountText.Text = failedCount.ToString();
               PassedCount.Text = passedCount.ToString();

               for (var i = 0; i < renderAudits.Count; i++)
               {
                    var expander = new Expander
                    {
                         IsExpanded = false,
                         Name = "audit" + i,
                         Header = renderAudits[i]["description"]
                    };
                    var newDockPanel = new DockPanel
                    {
                         Name = "AuditDockPanel",
                         HorizontalAlignment = HorizontalAlignment.Left,
                         VerticalAlignment = VerticalAlignment.Stretch,
                         LastChildFill = true
                    };
                    var textBlock = new TextBlock
                    {
                         FontSize = 14,
                         TextWrapping = TextWrapping.Wrap
                    };
                    var fixButton = new Button
                    {
                         Content = "Fix",
                         IsEnabled = false,
                         Name = "fixButton" + i,
                         Margin = new Thickness(10, 0, 0, 0)

                    };
                    newDockPanel.Children.Add(expander);
                    newDockPanel.Children.Add(textBlock);
                    if (_passedAudits[i])
                    {
                         textBlock.Foreground = Brushes.Chartreuse;
                         textBlock.Text = "Passed";
                    }
                    else
                    {
                         textBlock.Foreground = Brushes.Brown;
                         textBlock.Text = "Failed";
                         expander.Content = new TextBlock
                         {
                              Text = _failedAuditsResponse[i]
                         };
                         fixButton.IsEnabled = true;
                         newDockPanel.Children.Add(fixButton);
                         fixButton.Click += HandleFix;
                    }

                    AuditCheckList.Items.Add(newDockPanel);
               }
          }

          private void HandleFix(object sender, RoutedEventArgs routedEventArgs)
          {
               if (sender is Button fixButton)
               {
                    string strFixButtonIndex = fixButton.Name.Replace("fixButton", "");
                    int auditIndex = int.Parse(strFixButtonIndex);
                    var audit = _audits[auditIndex];
                    fixButton.IsEnabled = false;

                    if (IsAuditRegistrySetting(audit))
                    {
                         var registryPath = audit["reg_key"]
                              .Replace(audit["reg_key"]
                                        .Contains("HKLM")
                                        ? "HKLM"
                                        : "HKU",
                                   audit["reg_key"]
                                        .Contains("HKLM")
                                        ? _registryLocalMachine.Name
                                        : _registryCurrentUser.Name);

                         var auditValue = audit["value_data"];
                         int.TryParse(auditValue, out var auditIntValue);
                         try
                         {
                              Registry.SetValue(registryPath, audit["reg_item"], auditIntValue);
                              MessageBox.Show($"Registry Setting was modified successfully.");

                         }
                         catch (Exception e)
                         {
                              MessageBox.Show(e.Message);
                         }
                    }
               }
          }


          private IniFile ExportLocalSecurityPolicy()
          {
               var tempFile = Path.GetTempFileName();

               var p = new Process
               {
                    StartInfo =
                    {
                         FileName =
                              Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\secedit.exe"),
                         Arguments = $@"/export /cfg ""{tempFile}"" /quiet",
                         CreateNoWindow = true,
                         UseShellExecute = false
                    }
               };
               p.Start();
               p.WaitForExit();

               var file = IniFile.Load(tempFile);
               return file;
          }

          private string GetAccountTypeBySid(string sid)
          {
               var sidPattern = @"^S-\d-\d+-(\d+-){1,14}\d+$";
               if (!Regex.IsMatch(sid, sidPattern)) return sid;
               return new SecurityIdentifier(sid)
                    .Translate(typeof(NTAccount)).ToString().Replace("BUILTIN", "")
                    .Replace("\\", "");
          }
     }
}