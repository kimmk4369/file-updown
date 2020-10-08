using System;
using System.IO;

namespace MkCore
{
    /// <summary>
    /// Client에서 구현
    /// </summary>
    public class MkFile
    {
        readonly static string ID = "ID";
        readonly static string PW = "PW";

        // ? 싱글턴 구현
        private static readonly Lazy<FileService.FileServer> mfs = new Lazy<FileService.FileServer>(() => new FileService.FileServer());

        public static FileService.FileServer Mfs
        {
            get { return Mfs.Value; }
        }

        public static void FileUpload(string dirname, string filename, string filepath)
        {
            int offSet = 0;
            int chunkSize = 1048576;//1메가;
            byte[] buffer = new byte[chunkSize];

            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);

            try
            {
                lock (fs)
                {
                    long fileSize = new FileInfo(filepath).Length;

                    fs.Position = offSet;
                    int byteRead = 0;

                    while (offSet != fileSize)
                    {
                        byteRead = fs.Read(buffer, 0, chunkSize);

                        bool chunkAppend = false;

                        if (byteRead != buffer.Length)
                        {
                            chunkSize = byteRead;
                            byte[] trimmedBuffer = new byte[byteRead];
                            Array.Copy(buffer, trimmedBuffer, byteRead);
                            buffer = trimmedBuffer;

                            chunkAppend = Mfs.FileUpload(ID, PW, dirname, filename, buffer, offSet, true);
                        }
                        else
                        {
                            chunkAppend = Mfs.FileUpload(ID, PW, dirname, filename, buffer, offSet, false);
                        }

                        if (!chunkAppend) break;

                        offSet += byteRead;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }

        public static void FileDownload(string dirname, string filename, string localfilepath)
        {
            try
            {
                int offSet = 0;
                bool isEnd = false;

                if (offSet == 0) File.Create(localfilepath).Close();

                string vFileTemp = localfilepath + "_tmp";
                if (!File.Exists(vFileTemp)) File.Copy(localfilepath, vFileTemp, true);

                FileStream fs = new FileStream(vFileTemp, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                while (!isEnd)
                {
                    lock (fs)
                    {
                        fs.Seek(offSet, SeekOrigin.Begin);

                        byte[] content = Mfs.FileDownload(ID, PW, dirname, filename, ref isEnd, ref offSet);

                        fs.Write(content, 0, content.Length);
                    }
                }

                fs.Close();

                if (isEnd)
                {
                    File.Copy(vFileTemp, localfilepath, true);
                    if (File.Exists(vFileTemp)) File.Delete(vFileTemp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void FileDelete(string dirname, string filename)
        {
            try
            {
                Mfs.FileDelete(ID, PW, dirname, filename);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void MakeDir(string dirname)
        {
            try
            {
                Mfs.MakeDir(ID, PW, dirname);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void RemoveDir(string dirname)
        {
            try
            {
                Mfs.RemoveDir(ID, PW, dirname);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        }
    }
}
