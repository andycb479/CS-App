using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CS_App
{

     public partial class AuditStatus : Window
     {
          private readonly List<Dictionary<string, string>> _audits;
          private readonly bool[] _passedAudits;
          private readonly string[] _failedAuditsResponse;

          public AuditStatus(List<Dictionary<string, string>> audits)
          {
               _audits = audits;
               InitializeComponent();
               _passedAudits = new bool[_audits.Count];
               _failedAuditsResponse = new string[_audits.Count];
               CheckAudits();
               DisplayAudits(_audits);


          }

          private void CheckAudits()
          {
               var registrySetting = _audits.Where(x => x["type"] == "REGISTRY_SETTING").ToList();
               var otherSettings = _audits.Except(registrySetting).ToList();

               RegistryKey registryLocalMachine = Registry.LocalMachine;
               RegistryKey registryCurrentUser = Registry.Users;

               for (int i = 0; i < _audits.Count; i++)
               {
                    if (!IsAuditRegistrySetting(_audits[i])) { _passedAudits[i] = true; continue; }

                    var audit = _audits[i];

                    if (!audit["reg_key"].Contains("HKLM")) continue;

                    var systemValue =
                         Registry.GetValue(audit["reg_key"].Replace("HKLM", registryLocalMachine.Name),
                              audit["reg_item"], null);
                    var auditValue = audit["value_data"];

                    if (systemValue is string stringValue && stringValue == auditValue)
                    {
                         _passedAudits[i] = true;
                    }
                    else if (systemValue is int intValue)
                    {
                         int.TryParse(auditValue, out int auditIntValue);
                         if (intValue == auditIntValue)
                         {
                              _passedAudits[i] = true;
                         }
                    }
                    else if (audit.ContainsKey("check_type"))
                    {
                         _passedAudits[i] = true;
                    }

                    if (!_passedAudits[i])
                    {
                         _failedAuditsResponse[i] = $"Expected {auditValue} got {systemValue ?? "null"}";
                    }


               }

               Console.WriteLine(_passedAudits);

          }

          private bool IsAuditRegistrySetting(Dictionary<string, string> audit)
          {
               return audit["type"] == "REGISTRY_SETTING";
          }

          private void DisplayAudits(List<Dictionary<string, string>> renderAudits)
          {
               int passedCount = _passedAudits.Count(x => x);
               int failedCount = _passedAudits.Length - passedCount;

               FailedCountText.Text = failedCount.ToString();
               PassedCount.Text = passedCount.ToString();

               for (int i = 0; i < renderAudits.Count; i++)
               {
                    var expander = new Expander()
                    {
                         IsExpanded = false,
                         Name = "audit" + i,
                         Header = renderAudits[i]["description"]
                    };

                    // var newstackPanel = new StackPanel { Name = "AuditStackPanel" + i };
                    // foreach (KeyValuePair<string, string> value in renderAudits[i])
                    // {
                    //      if (value.Key == "description")
                    //      {
                    //           expander.Header = value.Value;
                    //      }
                    //
                    //      var textBlock = new TextBlock
                    //      {
                    //           FontSize = 14,
                    //           TextWrapping = TextWrapping.Wrap,
                    //           Inlines = { new Bold(new Run($"{myTI.ToTitleCase(value.Key)}: ")), new Run(value.Value), }
                    //
                    //      };
                    //
                    //
                    //      newstackPanel.Children.Add(textBlock);
                    // }

                    // expander.Content = newstackPanel;


                    var newDockPanel = new DockPanel()
                    {
                         Name = "AuditDockPanel",
                         HorizontalAlignment = HorizontalAlignment.Left,
                         VerticalAlignment = VerticalAlignment.Stretch,
                         LastChildFill = true,
                    };

                    var textBlock = new TextBlock()
                    {
                         FontSize = 14,
                         TextWrapping = TextWrapping.Wrap,
                    };

                    if (_passedAudits[i])
                    {
                         textBlock.Foreground = Brushes.Chartreuse;
                         textBlock.Text = "Passed";
                    }
                    else
                    {
                         textBlock.Foreground = Brushes.Brown;
                         textBlock.Text = "Failed";
                         expander.Content = new TextBlock()
                         {
                              Text = _failedAuditsResponse[i]
                         };
                    }

                    newDockPanel.Children.Add(expander);
                    newDockPanel.Children.Add(textBlock);

                    AuditCheckList.Items.Add(newDockPanel);
               }

          }

     }

     public enum State
     {
          Passed, Warning, Failed
     }

}
