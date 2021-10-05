using CS_APP.Core;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CS_App
{
     public partial class MainWindow : Window
     {
          private string _path;
          private string _fileName;
          private List<Dictionary<string, string>> _allAuditsList;
          private List<Dictionary<string, string>> _currentAuditList;
          private bool[] _selectedAudits;

          public MainWindow()
          {
               InitializeComponent();
               _currentAuditList = new List<Dictionary<string, string>>();
               _allAuditsList = new List<Dictionary<string, string>>();

          }

          private async void ImportPolicy(object sender, RoutedEventArgs e)
          {

               OpenFileDialog openFileDialog = new OpenFileDialog() { };
               if (openFileDialog.ShowDialog() == true)
               {
                    _path = openFileDialog.FileName;
                    _fileName = openFileDialog.SafeFileName;
               }


               _allAuditsList = await new AuditFile(_path).Parse();
               _currentAuditList = _allAuditsList.ToList();
               _selectedAudits = new bool[_allAuditsList.Count];
               SelectAllCheckBox.Visibility = Visibility.Visible;

               DisplayAudits(_allAuditsList);
          }

          public void SaveAudits(object sender, RoutedEventArgs e)
          {

               string path = $@"C:\Users\User\source\repos\CSApp\CS_App\DataBase\{_fileName}.json";

               //Serialization
               using (StreamWriter file = File.CreateText(path))
               {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, _currentAuditList);
               }

               MessageBox.Show($"Audits saved as {_fileName}.json in database.");
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
                    _allAuditsList =
                         (List<Dictionary<string, string>>)serializer.Deserialize(file,
                              typeof(List<Dictionary<string, string>>));
                    _selectedAudits = new bool[_allAuditsList.Count];

                    _currentAuditList = _allAuditsList.ToList();

                    SelectAllCheckBox.Visibility = Visibility.Visible;
               }

               DisplayAudits(_allAuditsList);
          }

          private void CheckBoxHandler(object sender, RoutedEventArgs routedEventArgs)
          {
               if (sender is CheckBox checkBox)
               {
                    string strCheckBoxIndex = checkBox.Name.Replace("checkBox", "");
                    int checkBoxIndex = int.Parse(strCheckBoxIndex);
                    _selectedAudits[checkBoxIndex] = checkBox.IsChecked.Value;
               }
          }

          private void DisplayAudits(List<Dictionary<string, string>> renderAudits)
          {
               if (renderAudits.Count == 0)
               {
                    SelectAllCheckBox.Visibility = Visibility.Hidden;
               }

               SelectAllCheckBox.IsChecked = false;
               SelectAllCheckBox.Content = "Select All";

               auditsList.Items.Clear();

               TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

               for (int i = 0; i < renderAudits.Count; i++)
               {

                    var expander = new Expander()
                    {
                         IsExpanded = false,
                         Name = "audit" + i,
                         MaxWidth = 700
                    };

                    var newstackPanel = new StackPanel { Name = "AuditStackPanel" + i };
                    foreach (KeyValuePair<string, string> value in renderAudits[i])
                    {
                         if (value.Key == "description")
                         {
                              expander.Header = value.Value;
                         }

                         var textBlock = new TextBlock
                         {
                              FontSize = 14,
                              TextWrapping = TextWrapping.Wrap,
                              Inlines = { new Bold(new Run($"{myTI.ToTitleCase(value.Key)}: ")), new Run(value.Value), }

                         };


                         newstackPanel.Children.Add(textBlock);
                    }

                    expander.Content = newstackPanel;

                    var checkBox = new CheckBox
                    {
                         Name = "checkBox" + i,
                         IsChecked = false,
                    };

                    checkBox.Click += CheckBoxHandler;

                    var newDockPanel = new DockPanel()
                    {
                         Name = "AuditDockPanel",
                    };

                    newDockPanel.Children.Add(checkBox);
                    newDockPanel.Children.Add(expander);

                    auditsList.Items.Add(newDockPanel);
               }

               auditsCount.Text = "Total Audits: " + renderAudits.Count;
          }

          private void ExportSelected(object sender, RoutedEventArgs e)
          {

               if (_allAuditsList.Count == 0)
               {
                    return;
               }

               var isAnySelected = _selectedAudits.Any(x => x);

               if (!isAnySelected)
               {
                    MessageBox.Show("First select some options.");
                    return;
               }

               var dialog = new MyDialog();
               string fileName = "Undefined";
               if (dialog.ShowDialog() == true)
               {
                    fileName = dialog.ResponseText;
               }

               string path = $@"C:\Users\User\source\repos\CSApp\CS_App\DataBase\{fileName}.json";

               //Serialization
               using (StreamWriter file = File.CreateText(path))
               {
                    JsonSerializer serializer = new JsonSerializer();
                    var selectedAudits = _currentAuditList.Where((x, index) => _selectedAudits[index]).ToList();
                    serializer.Serialize(file, selectedAudits);
               }

               MessageBox.Show($"Audits saved as {fileName}.json in database.");
          }

          private void SelectAllHandler(object sender, RoutedEventArgs e)
          {
               if (sender is CheckBox checkBox)
               {
                    if (checkBox.IsChecked.Value)
                    {
                         checkBox.Content = "Deselect All";
                         _selectedAudits = Enumerable.Repeat(true, _selectedAudits.Length).ToArray();

                         foreach (var dockPanel in auditsList.Items)
                         {
                              if (dockPanel is DockPanel currentDock)
                              {
                                   foreach (var child in currentDock.Children)
                                   {
                                        if (child is CheckBox currentCheckBox)
                                        {
                                             currentCheckBox.IsChecked = true;
                                        }
                                   }
                              }
                         }

                    }
                    else
                    {
                         checkBox.Content = "Select All";
                         foreach (var dockPanel in auditsList.Items)
                         {
                              if (dockPanel is DockPanel currentDock)
                              {
                                   foreach (var child in currentDock.Children)
                                   {
                                        if (child is CheckBox currentCheckBox)
                                        {
                                             currentCheckBox.IsChecked = false;
                                        }
                                   }
                              }
                         }
                         _selectedAudits = Enumerable.Repeat(false, _selectedAudits.Length).ToArray();
                    }
               }
          }

          private void SearchHandler(object sender, RoutedEventArgs e)
          {

               _currentAuditList.Clear();

               if (_allAuditsList.Count == 0)
               {
                    MessageBox.Show("Load some audits first.");
                    return;
               }

               foreach (var dictionary in _allAuditsList)
               {
                    var containsWord = dictionary.Values.Select(x => x.ToLower().Contains(searchBox.Text.ToLower())).Any(x => x);

                    if (containsWord)
                    {
                         _currentAuditList.Add(dictionary);
                    }
               }

               _selectedAudits = new bool[_currentAuditList.Count];

               DisplayAudits(_currentAuditList);
          }

          private void AuditWithSelected(object sender, RoutedEventArgs e)
          {
               var selectedAudits = _currentAuditList.Where((x, index) => _selectedAudits[index]).ToList();
               var auditStatus = new AuditStatus(selectedAudits);

               auditStatus.Show();
          }

          private void AuditWithAll(object sender, RoutedEventArgs e)
          {
               var auditStatus = new AuditStatus(_currentAuditList);
               auditStatus.Show();

          }
     }
}
