using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CS_APP.DataLayer.Models;

namespace CS_App
{

     partial class MyDialog : Window
     {
          public MyDialog()
          {
               InitializeComponent();
          }

          public string ResponseText
          {
               get { return ResponseTextBox.Text; }
               set { ResponseTextBox.Text = value; }
          }

          private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
          {
               DialogResult = true;
          }
     }
}