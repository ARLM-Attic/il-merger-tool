using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ILMerger
{
  /// <summary>
  /// Interaction logic for ExportOptionsWindow.xaml
  /// </summary>
  public partial class ExportOptionsWindow : Window
  {
    public ExportOptionsWindow()
    {
      InitializeComponent();
    }

    public bool UseRelativePathes
    {
      get { return (bool)GetValue(UseRelativePathesProperty); }
      set { SetValue(UseRelativePathesProperty, value); }
    }

    // Using a DependencyProperty as the backing store for UseRelativePathes.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty UseRelativePathesProperty =
        DependencyProperty.Register("UseRelativePathes", typeof(bool), typeof(ExportOptionsWindow), new UIPropertyMetadata(true));

    public string FileName 
    { 
      get;
      private set;
    }
        
    private void btnBrowseOutput_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog ofd = new SaveFileDialog();
      ofd.Title = "Choose merge configuration file name...";
      ofd.Filter = "Merge Configuration files (*.merge)|*.merge";

      if (ofd.ShowDialog().Value)
      {
        this.tbOutput.Text = ofd.FileName;
        this.FileName = ofd.FileName;
      }
    }

    bool canClose = false;

    private void btnExport_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      if (!string.IsNullOrWhiteSpace(this.tbOutput.Text))
      {
        canClose = true;
        this.DialogResult = true;
        this.Close();
      }
    }

    private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      canClose = true;
      this.DialogResult = false;
      this.Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      e.Cancel = !canClose;
    }
  }
}
