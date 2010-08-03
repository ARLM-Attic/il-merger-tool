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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ILMerging;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace ILMerger
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      SetupCheckBoxes(this.spContent);
      SetupControls();

      if (IsBatchMode)
      {
        ExecuteBatch();
      }

      if (!string.IsNullOrWhiteSpace(BatchConfigurationFilePath))
      {
        ImportOptions(BatchConfigurationFilePath);
      }
    }

    #region BATCH MODE
    protected override void OnInitialized(EventArgs e)
    {
      IsBatchMode = false;
      IsSilent = true;
      string[] args = Environment.GetCommandLineArgs();
      if (args.Length > 1)
      {
        if (args[1].StartsWith("/cfg", StringComparison.InvariantCultureIgnoreCase))
        {
          IsBatchMode = true;

          if (args[1].StartsWith("/cfg_menu", StringComparison.InvariantCultureIgnoreCase))
          {
            IsSilent = false;
          }

          if (args.Length > 2)
          {
            BatchConfigurationFilePath = args[2];
          }
          else
          {
            Console.WriteLine("ILMerger - Configuration path missing.");

            if (!IsSilent)
            {
              MessageBox.Show("Merge failed: Configuration path missing.",
                              "IL Merger Tool",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }

            Environment.Exit(-3);
          }
        }
        else
        {
          BatchConfigurationFilePath = args[1];
        }
      }

      if (IsBatchMode)
      {
        this.Hide();
      }

      base.OnInitialized(e);
    }

    void ExecuteBatch()
    {
      int exitCode = -3;
      if (ImportOptions(BatchConfigurationFilePath))
      {
        Console.WriteLine("ILMerger - Configuration load completed.");

        try
        {
          SaveCheckBoxes();
          SaveControls();
          Merger.Merge();
          Console.WriteLine("ILMerger - Merge completed.");
          exitCode = 0;
        }
        catch
        {
          Console.WriteLine("ILMerger - Merge error.");
          exitCode = -2;
        }
      }
      else
      {
        Console.WriteLine("ILMerger - Configuration load error.");
        exitCode = -1;
      }

      Console.WriteLine("ILMerger - Closing.");

      if (!IsSilent)
      {
        switch (exitCode)
        {
          case 0:
            MessageBox.Show("Merge completed successfully.",
                "IL Merger Tool",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            break;
          case -1:
            MessageBox.Show("Merge failed: Configuration file cannot be loaded.\nThe file is corrupted or made by a newer version of the tool.",
                "IL Merger Tool",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            break;
          case -2:
            MessageBox.Show("Merge failed: Unknown error during merge.\nFor detailes, please check the log file.",
                "IL Merger Tool",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            break;
          case -3:
            MessageBox.Show("Merge failed: Configuration path missing.\nPlease check the file path.",
                "IL Merger Tool",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            break;
        }
      }

      Environment.Exit(exitCode);
    }

    private bool IsSilent
    {
      get;
      set;
    }

    private bool IsBatchMode
    {
      get;
      set;
    }

    private string BatchConfigurationFilePath
    {
      get;
      set;
    }
    #endregion

    public ILMerge Merger
    {
      get { return (ILMerge)GetValue(MergerProperty); }
      set { SetValue(MergerProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Merger.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MergerProperty =
        DependencyProperty.Register("Merger", typeof(ILMerge), typeof(MainWindow), new UIPropertyMetadata(new ILMerge()));




    Dictionary<string, CheckBox> CheckBoxes = new Dictionary<string, CheckBox>();
    private void SetupCheckBoxes(Panel panel)
    {
      foreach (UIElement element in panel.Children)
      {
        if (element is Panel)
        {
          SetupCheckBoxes(element as Panel);
          continue;
        }

        if (element is Expander)
        {
          ContentControl cc = (element as ContentControl);
          if (cc.Content != null && cc.Content is Panel)
          {
            SetupCheckBoxes(cc.Content as Panel);
            continue;
          }

        }

        if (element is CheckBox)
        {
          CheckBox cb = (element as CheckBox);
          string name = (cb.Tag != null ? cb.Tag.ToString() : cb.Content.ToString()).Replace(" ", "");

          if (name != "###")
          {
            CheckBoxes.Add(name, cb);
            cb.IsChecked = (bool)Merger.GetType().GetProperty(name).GetValue(Merger, null);
          }
        }
      }
    }

    private void SetupControls()
    {
      TargetPlatformIds.Add("v1");
      TargetPlatformIds.Add("v1.1");
      TargetPlatformIds.Add("v2");
      TargetPlatformIds.Add("v4");

      TargetPlatforms.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework\v1.0.3705");
      TargetPlatforms.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework\v1.1.4322");
      TargetPlatforms.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework\v2.0.50727");
      TargetPlatforms.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework\v4.0.30319");
    }


    private void SaveCheckBoxes()
    {
      foreach (KeyValuePair<string, CheckBox> item in CheckBoxes)
      {
        Merger.GetType().GetProperty(item.Key).SetValue(Merger, item.Value.IsChecked.Value, null);
      }
    }

    List<string> TargetPlatformIds = new List<string>();
    List<string> TargetPlatforms = new List<string>();
    private void SaveControls()
    {
      Merger.SetTargetPlatform(TargetPlatformIds[this.cbTargetPlatform.SelectedIndex],
                               TargetPlatforms[this.cbTargetPlatform.SelectedIndex]);

      Merger.OutputFile = this.tbOutput.Text;

      if (Merger.Log)
      {
        Merger.LogFile = this.tbLog.Text;
      }

      if (this.cbSign.IsChecked.Value)
      {
        Merger.KeyFile = this.tbSign.Text;
      }

      List<string> assemblies = new List<string>();
      foreach (string item in this.lbAssemblies.Items)
      {
        assemblies.Add(item);
      }
      assemblies.Remove(this.lbAssemblies.SelectedValue.ToString());
      assemblies.Insert(0, this.lbAssemblies.SelectedValue.ToString());
      Merger.SetInputAssemblies(assemblies.ToArray());

      Merger.TargetKind = ILMerge.Kind.SameAsPrimaryAssembly;
    }


    private void btnExit_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      Environment.Exit(0);
    }

    private void btnMerge_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      SaveCheckBoxes();
      SaveControls();

      this.gdOverlay.Visibility = Visibility.Visible;
      this.InvalidateVisual();
      try
      {
        Merger.Merge();

        MessageBox.Show("Merging completed.\nFor detailes please check the log file.",
                        "IL Merger Tool",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
      }
      catch
      {
        MessageBox.Show("Merging failed.\nFor detailes please check the log file.",
                        "IL Merger Tool",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
      }
      this.gdOverlay.Visibility = Visibility.Collapsed;
    }

    private void btnClearAssemblies_Click(object sender, RoutedEventArgs e)
    {
      this.lbAssemblies.Items.Clear();
    }

    private void btnAddAssembly_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Title = "Add assemblies to merge...";
      ofd.Filter = "Assemblies (*.exe, *.dll)|*.exe;*.dll";
      ofd.Multiselect = true;

      if (ofd.ShowDialog().Value)
      {
        foreach (string file in ofd.FileNames)
        {
          this.lbAssemblies.Items.Add(file);
        }
      }
    }

    private void btnBrowseSign_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Title = "Choose key file to sign the output...";
      ofd.Filter = "Strong Name Keys (*.snk)|*.snk";

      if (ofd.ShowDialog().Value)
      {
        this.tbSign.Text = ofd.FileName;
      }
    }

    private void btnBrowseLog_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog ofd = new SaveFileDialog();
      ofd.Title = "Choose log file...";
      ofd.Filter = "Log files (*.log)|*.log";

      if (ofd.ShowDialog().Value)
      {
        this.tbLog.Text = ofd.FileName;
      }
    }

    private void btnBrowseOutput_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog ofd = new SaveFileDialog();
      ofd.Title = "Choose output file name...";
      ofd.Filter = "Assemblies (*.exe, *.dll)|*.exe;*.dll";

      if (ofd.ShowDialog().Value)
      {
        this.tbOutput.Text = ofd.FileName;
      }
    }

    bool useRelativePathes = false;

    private bool ExportOptions(string fileName)
    {
      try
      {
        int pathLength = (new FileInfo(fileName).DirectoryName + "\\").Length;

        MergerFile file = MergerFile.Create();
        file.IsRelativPathes = useRelativePathes;

        foreach (KeyValuePair<string, CheckBox> item in CheckBoxes)
        {
          file.Bits.Add(item.Key, item.Value.IsChecked.Value);
        }
        file.BitsEx.Add("IsSign", this.cbSign.IsChecked.Value);

        List<string> assemblies = new List<string>();
        foreach (string item in this.lbAssemblies.Items)
        {
          assemblies.Add((useRelativePathes ? item.Remove(0, pathLength) : item));
        }
        assemblies.Remove((useRelativePathes ? this.lbAssemblies.SelectedValue.ToString().Remove(0, pathLength) : this.lbAssemblies.SelectedValue.ToString()));
        assemblies.Insert(0, (useRelativePathes ? this.lbAssemblies.SelectedValue.ToString().Remove(0, pathLength) : this.lbAssemblies.SelectedValue.ToString()));
        file.Assemblies = assemblies;

        file.Files["Key"] = (useRelativePathes ? this.tbSign.Text.Remove(0, pathLength) : this.tbSign.Text);
        file.Files["Log"] = (useRelativePathes ? this.tbLog.Text.Remove(0, pathLength) : this.tbLog.Text);
        file.Files["Output"] = (useRelativePathes ? this.tbOutput.Text.Remove(0, pathLength) : this.tbOutput.Text);

        file.TargetPlatformIndex = this.cbTargetPlatform.SelectedIndex;

        return file.Save(fileName);
      }
      catch
      {
        return false;
      }
    }

    private bool ImportOptions(string fileName)
    {
      try
      {
        MergerFile file = MergerFile.Open(fileName);

        if (file.Version == new Version(1, 0, 0, 0))
        {
          useRelativePathes = file.IsRelativPathes;

          foreach (KeyValuePair<string, bool> item in file.Bits)
          {
            CheckBoxes[item.Key].IsChecked = item.Value;
          }
          this.cbSign.IsChecked = file.BitsEx["IsSign"];

          this.lbAssemblies.Items.Clear();
          foreach (string item in file.Assemblies)
          {
            this.lbAssemblies.Items.Add(useRelativePathes ? file.GethPathAsAbsolute(item) : item);
          }
          this.lbAssemblies.SelectedIndex = 0;

          this.tbSign.Text = (useRelativePathes ? file.GethPathAsAbsolute(file.Files["Key"]) : file.Files["Key"]);
          this.tbLog.Text = (useRelativePathes ? file.GethPathAsAbsolute(file.Files["Log"]) : file.Files["Log"]);
          this.tbOutput.Text = (useRelativePathes ? file.GethPathAsAbsolute(file.Files["Output"]) : file.Files["Output"]);

          this.cbTargetPlatform.SelectedIndex = file.TargetPlatformIndex;

          return true;
        }

        return false;
      }
      catch
      {
        return false;
      }
    }

    private void btnImport_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Title = "Import merge configuration from file...";
      ofd.Filter = "Merge Configuration files (*.merge)|*.merge";

      if (ofd.ShowDialog().Value)
      {
        this.gdOverlay.Visibility = Visibility.Visible;
        this.InvalidateVisual();
        if (!ImportOptions(ofd.FileName))
        {
          MessageBox.Show("Configuration import failed.\nThe merger file is corrupted or made by a newer version of the tool.",
                          "IL Merger Tool",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
        }
        else
        {
          MessageBox.Show("Merge Configuration import completed successfully.",
                          "IL Merger Tool",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }
        this.gdOverlay.Visibility = Visibility.Collapsed;
      }
    }

    private void btnExport_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      ExportOptionsWindow eow = new ExportOptionsWindow();
      eow.UseRelativePathes = useRelativePathes;

      if (eow.ShowDialog().Value)
      {
        this.gdOverlay.Visibility = Visibility.Visible;
        this.InvalidateVisual();
        useRelativePathes = eow.UseRelativePathes;
        if (!ExportOptions(eow.FileName))
        {
          MessageBox.Show("Configuration export failed.\nIf relative pathes are enabled, please check, if all pathes are in the directory (or in one of the subdirectories) of merge configuration file's path.",
                          "IL Merger Tool",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
        }
        else
        {
          MessageBox.Show("Merge Configuration export completed successfully.",
                          "IL Merger Tool",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }
        this.gdOverlay.Visibility = Visibility.Collapsed;
      }
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.AllowedEffects.HasFlag(DragDropEffects.Copy))
      {
        e.Effects = DragDropEffects.Copy;
      }
      else
      {
        e.Effects = DragDropEffects.None;
      }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.AllowedEffects.HasFlag(DragDropEffects.Copy))
      {
        string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);

        this.gdOverlay.Visibility = Visibility.Visible;
        this.InvalidateVisual();
        if (!ImportOptions(fileNames[0]))
        {
          MessageBox.Show("Configuration import failed.\nThe merger file is corrupted or made by a newer version of the tool.",
                          "IL Merger Tool",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
        }
        else
        {
          MessageBox.Show("Merge Configuration import completed successfully.",
                          "IL Merger Tool",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }
        this.gdOverlay.Visibility = Visibility.Collapsed;
      }
    }

    private void btnDonate_Click(object sender, MouseButtonEventArgs e)
    {
      Process.Start(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WZMRNKLQBCBL2");
    }
  }
}
