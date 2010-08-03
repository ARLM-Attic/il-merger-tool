using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

namespace ILMerger
{
  #region MergerFile
  [Serializable]
  public class MergerFile
  {
    #region ctor
    private MergerFile()
    {
      Bits = new Dictionary<string, bool>();
      BitsEx = new Dictionary<string, bool>();
      Assemblies = new List<string>();
      Files = new Dictionary<string, string>();
      TargetPlatformIndex = 0;
      IsRelativPathes = false;
    }
    #endregion

    #region Properties
    #region Name
    [NonSerialized]
    private string name;
    public string Name
    {
      get 
      { 
        return name; 
      }
      private set 
      { 
        name = value; 
      }
    } 
    #endregion
    #region Version
    private Version version = new Version(1, 0, 0, 0);
    public Version Version 
    {
      get
      { 
        return version;
      }      
    }
    #endregion
    #region IsRelativePathes
    public bool IsRelativPathes 
    { 
      get;
      set; 
    }
    #endregion

    #region Bits
    public Dictionary<string, bool> Bits 
    { 
      get;
      set;
    }
    #endregion
    #region BitsEx
    public Dictionary<string, bool> BitsEx 
    {
      get;
      set;
    }
    #endregion
    #region Assemblies
    public List<string> Assemblies 
    { 
      get;
      set;
    }
    #endregion
    #region Files
    public Dictionary<string, string> Files 
    { 
      get;
      set;
    }
    #endregion
    #region TargetPlatformIndex
    public int TargetPlatformIndex 
    { 
      get;
      set;
    }
    #endregion
    #endregion

    #region Methods
    #region GetPathAsAbsolute
    public string GethPathAsAbsolute(string path)
    {
      return Path.Combine((new FileInfo(Name)).DirectoryName, path);
    }
    #endregion  
    #region Save
    public bool Save(string fileName)
    {
      this.Name = fileName;
      return SerializerTool.SerializeToStream(this, File.Create(fileName));
    }
    #endregion
    #region Open
    public static MergerFile Open(string fileName)
    {
      MergerFile file = (MergerFile)SerializerTool.DeserializeFromStream(File.Open(fileName, FileMode.Open));
      file.Name = fileName;

      return file;
    }
    #endregion
    #region Create
    public static MergerFile Create()
    {
      return new MergerFile();
    }
    #endregion
    #endregion
  }
  #endregion

  #region Serialization
  public class SerializerTool
  {
    public static bool SerializeToStream(object obj, Stream stream)
    {
      BinaryFormatter bf = new BinaryFormatter();
      bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

      try
      {
        bf.Serialize(stream, obj);
        return true;
      }
      catch
      {
        return false;
      }
      finally
      {
        stream.Close();
      }
    }

    public static object DeserializeFromStream(Stream stream)
    {
      BinaryFormatter bf = new BinaryFormatter();
      bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

      try
      {
        return bf.Deserialize(stream);
      }
      catch
      {
        return null;
      }
      finally
      {
        stream.Close();
      }
    }
  }
  #endregion
}