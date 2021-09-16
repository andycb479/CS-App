using CS_APP.DataLayer.Models;
using CS_APP.DataLayer.Repository;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CS_APP.Core;
using System.Drawing;
using System.Globalization;
using System;
using System.Text;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace CS_App
{
     public partial class MainWindow : Window
     {
          private string _path;
          private string _fileName;
          private List<Dictionary<string, string>> _auditsList;

          public MainWindow()
          {
               InitializeComponent();

          }

          private async void ImportPolicy(object sender, RoutedEventArgs e)
          {

               OpenFileDialog openFileDialog = new OpenFileDialog() { };
               if (openFileDialog.ShowDialog() == true)
               {
                    _path = openFileDialog.FileName;
                    _fileName = openFileDialog.SafeFileName;
               }


               _auditsList = await new AuditFile(_path).Parse();

               DisplayAudits();
          }

          public void SaveAudits(object sender, RoutedEventArgs e)
          {

               string path = $@"C:\Users\User\source\repos\CSApp\CS_App\DataBase\{_fileName}.json";

               //Serialization
               using (StreamWriter file = File.CreateText(path))
               {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, _auditsList);
               }

               MessageBox.Show($"Audits saved as {_fileName} in database.");
          }

          public void OpenJsonAudits(object sender, RoutedEventArgs e)
          {

               OpenFileDialog openFileDialog = new OpenFileDialog() { };
               if (openFileDialog.ShowDialog() == true)
               {
                    _path = openFileDialog.FileName;
                    _fileName = openFileDialog.SafeFileName;
               }

               //Deserialization
               using (StreamReader file =
                    File.OpenText(_path))
               {
                    JsonSerializer serializer = new JsonSerializer();
                    _auditsList =
                         (List<Dictionary<string, string>>)serializer.Deserialize(file,
                              typeof(List<Dictionary<string, string>>));
               }

               DisplayAudits();
          }

          private void DisplayAudits()
          {
               TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

               for (int i = 0; i < _auditsList.Count; i++)
               {

                    var expander = new Expander()
                    {
                         Header = "Audit " + i,
                         IsExpanded = false,
                         Name = "audit" + i,
                         MaxWidth = 700
                    };

                    var newstackPanel = new StackPanel { Name = "AuditStackPanel" + i };
                    foreach (KeyValuePair<string, string> value in _auditsList[i])
                    {
                         var textBlock = new TextBlock
                         {
                              FontSize = 14,
                              TextWrapping = TextWrapping.Wrap,
                              //Text = $"{value.Key}: {value.Value}",
                              Inlines = { new Bold(new Run($"{myTI.ToTitleCase(value.Key)}: ")), new Run(value.Value), }

                         };
                         newstackPanel.Children.Add(textBlock);
                    }

                    expander.Content = newstackPanel;
                    auditsList.Items.Add(expander);
               }

               auditsCount.Text = "Total Audits: " + _auditsList.Count;
          }
     }
}
