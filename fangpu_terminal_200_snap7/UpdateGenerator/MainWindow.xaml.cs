using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

namespace UpdateGenerator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();

        }

        void Init()
        {
            output_box.IsEnabled = false;
            output_box.Text = UserSettings.Default.outputpath;
            author_box.Text = UserSettings.Default.author;
            version_box.Text = UserSettings.Default.version;
            
        }

        void xmledit(string version,string auther,string appname,string savepath,string hash="")
        {
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(".\\version\\version.xml");
                foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "Version")
                        node.Attributes["version"].Value=version;
                    if (node.Name == "ApplicationName")
                        node.Attributes["name"].Value=appname;
                    if (node.Name == "HashValue")
                    {
                        node.Attributes["hash"].Value=hash;
                    }
                }
                xDoc.Save(UserSettings.Default.xmlPath);
                xDoc.Save(savepath+"version.xml");

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("操作XML出错");
            }
        }
        public string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        private void filepath_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult filepath=fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
                path_box.Text = fbd.SelectedPath;
        }


        private void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream)
        {
            Crc32 crc = new Crc32();
            string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file))                     //如果当前是文件夹，递归
                {                   
                    CreateZipFiles(file, zipStream);
                }
                else                                            //如果是文件，开始压缩
                {
                    FileStream fileStream = File.OpenRead(file);
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);
                    ZipEntry entry = new ZipEntry(tempFile);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    fileStream.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipStream.PutNextEntry(entry);
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
        public void CreateZip(string sourceFilePath, string destinationZipFilePath,string filename)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;
            if (!Directory.Exists(destinationZipFilePath))
                Directory.CreateDirectory(destinationZipFilePath);
            ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath+filename));
            zipStream.SetLevel(6);  // 压缩级别 0-9
            CreateZipFiles(sourceFilePath, zipStream);
            zipStream.Finish();
            zipStream.Close();
        }

        private void generate_button_Click(object sender, RoutedEventArgs e)
        {
            progressbar1.Value = 10;
            string hash = "";
            try
            {
                CreateZip(path_box.Text, output_box.Text,"download.zip");
                progressbar1.Value = 60;
                hash=GetMD5HashFromFile(output_box.Text + "download.zip");
                progressbar1.Value = 80;
            }
            catch (Exception ex)
            {
                progressbar1.Value = 0;
                System.Windows.MessageBox.Show("压缩出现错误");
                return;
            }
            xmledit(version_box.Text, author_box.Text, UserSettings.Default.appname, output_box.Text, hash);
            progressbar1.Value = 100;
            System.Windows.MessageBox.Show("生成成功");

        }

        private void outputpath_Button_Click(object sender, RoutedEventArgs e)
        {
            output_box.IsEnabled = true;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult filepath = fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                output_box.Text = fbd.SelectedPath;
                UserSettings.Default.outputpath = fbd.SelectedPath;
                UserSettings.Default.author = author_box.Text;
                UserSettings.Default.version = version_box.Text;
                UserSettings.Default.Save();
            }
                
          
        }


    }
}
