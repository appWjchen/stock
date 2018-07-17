using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stock
{
    public interface IFileHelper
    {
        bool Exists(string filename);
        void WriteText(string filename, string text);
        string ReadText(string filename);
        IEnumerable<string> GetFiles();
        void Delete(string filename);
        string GetDocsPath();
        bool DirectoryExists(String path);
        void CreateDirectory(String path);
    }

    class FileHelper : IFileHelper
    {
        // public String databasePath = @"C:\Users\陳維志\Documents\stock\database\";
        public static String databasePath = null;
        public bool Exists(string filename)
        {
            return File.Exists(databasePath + filename);
        }

        public void WriteText(string filename, string text)
        {
            File.WriteAllText(databasePath + filename, text);
        }

        public string ReadText(string filename)
        {
            return File.ReadAllText(databasePath + filename);
        }

        //多一個取得所有檔案的方法，等等的範例要用
        public IEnumerable<string> GetFiles()
        {
            IEnumerable<string> filepaths = new List<string>();
            List<string> filenames = new List<string>();
            foreach (string filepath in filepaths)
            {
                filenames.Add(Path.GetFileName(filepath));
            }
            return filenames;
        }
        public void Delete(string filename)
        {
            File.Delete(databasePath + filename);
        }
        public string GetDocsPath()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        public bool DirectoryExists(String path)
        {
            return Directory.Exists(path);
        }
        public void CreateDirectory(String path)
        {
            Directory.CreateDirectory(path);
        }
    }
}
