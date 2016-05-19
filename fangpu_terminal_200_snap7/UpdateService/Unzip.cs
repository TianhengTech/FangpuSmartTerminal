using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Forms;

namespace UpdateService
{
    class ZipManager
    {

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public static void CreateZip(string sourceFilePath, string destinationZipFilePath)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;
            ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));
            zipStream.SetLevel(6);  // 压缩级别 0-9
            CreateZipFiles(sourceFilePath, zipStream);
            zipStream.Finish();
            zipStream.Close();
        }
        /// <summary>
        /// 递归压缩文件
        /// </summary>
        /// <param name="sourceFilePath">待压缩的文件或文件夹路径</param>
        /// <param name="zipStream">打包结果的zip文件路径（类似 D:\WorkSpace\a.zip）,全路径包括文件名和.zip扩展名</param>
        /// <param name="staticFile"></param>
        private static void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream)
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
        /// <summary>
        /// 解压Zip
        /// </summary>
        /// <param name="zipFilePath"></param>
        public static void UnZipFile(string zipFilePath,string filepath)
        {
            if (!File.Exists(zipFilePath))
            {
                Console.WriteLine("Cannot find file '{0}'", zipFilePath);
                return;
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {                 
                    Console.WriteLine(theEntry.Name);
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    // create directory
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(filepath+"\\"+directoryName);
                    }

                    if (fileName != String.Empty)
                    {
                        Directory.CreateDirectory(filepath);
                        using (FileStream streamWriter = File.Create(filepath+"\\"+theEntry.Name))
                        {
                           // if (theEntry.Name == "datalocal.db") ;
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    
        //使用GZIP压缩文件的方法
        public static bool GZipFile(string sourcefilename, string zipfilename)
        {
            bool blResult;//表示压缩是否成功的返回结果
            //为源文件创建读取文件的流实例
            FileStream srcFile = File.OpenRead(sourcefilename);
            //为压缩文件创建写入文件的流实例，
            GZipOutputStream zipFile = new GZipOutputStream(File.Open(zipfilename, FileMode.Create));
            try
            {
                byte[] FileData = new byte[srcFile.Length];//创建缓冲数据
                srcFile.Read(FileData, 0, (int)srcFile.Length);//读取源文件
                zipFile.Write(FileData, 0, FileData.Length);//写入压缩文件
                blResult = true;
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                blResult = false;
            }
            srcFile.Close();//关闭源文件
            zipFile.Close();//关闭压缩文件
            return blResult;
        }
        //使用GZIP解压文件的方法
        public static bool UnGzipFile(string zipfilename, string unzipfilename)
        {
            bool blResult;//表示解压是否成功的返回结果
            //创建压缩文件的输入流实例
            GZipInputStream zipFile = new GZipInputStream(File.OpenRead(zipfilename));
            //创建目标文件的流
            FileStream destFile = File.Open(unzipfilename, FileMode.Create);
            try
            {
                int buffersize = 2048;//缓冲区的尺寸，一般是2048的倍数
                byte[] FileData = new byte[buffersize];//创建缓冲数据
                while (buffersize > 0)//一直读取到文件末尾
                {
                    buffersize = zipFile.Read(FileData, 0, buffersize);//读取压缩文件数据
                    destFile.Write(FileData, 0, buffersize);//写入目标文件
                }
                blResult = true;
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                blResult = false;
            }
            destFile.Close();//关闭目标文件
            zipFile.Close();//关闭压缩文件
            return blResult;
        }
    }
}
