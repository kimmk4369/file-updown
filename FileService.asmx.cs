using System;
using System.IO;
using System.Web.Services;

namespace FileService
{
    /// <summary>
    /// FileService의 요약 설명입니다.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // ASP.NET AJAX를 사용하여 스크립트에서 이 웹 서비스를 호출하려면 다음 줄의 주석 처리를 제거합니다. 
    // [System.Web.Script.Services.ScriptService]
    public class FileService : System.Web.Services.WebService
    {
        private string vRemoteDir = ""; 
        private string vID = "";
        private string vPWD = "";

        public FileService()
        {
            //
            // TODO: 여기에 생성자 논리를 추가합니다.
            //
        }

        [WebMethod(Description = "FileUpload")]
        public bool FileUpload(string id, string pwd, string dirname, string filename, byte[] content, long offSet, bool isEnd)
        {
            bool result = false;

            if (id != vID || pwd != vPWD) throw new Exception("아이디 또는 패스워드 불일치");

            try
            {
                string vDir = vRemoteDir + dirname;
                string vFileName = vDir + "\\" + filename;

                if (!System.IO.Directory.Exists(vDir)) System.IO.Directory.CreateDirectory(vDir);
                if (offSet == 0) File.Create(vFileName).Close();

                string vFileTemp = vFileName + "_tmp";
                if (!File.Exists(vFileTemp)) File.Copy(vFileName, vFileTemp, true);

                using(var fs = new FileStream(vFileTemp, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    lock (fs)
                    {
                        fs.Seek(offSet, SeekOrigin.Begin);
                        fs.Write(content, 0, content.Length);
                    }

                    fs.Close();
                }

                if (isEnd)
                {
                    File.Copy(vFileTemp, vFileName, true);
                    if (File.Exists(vFileTemp)) File.Delete(vFileTemp);
                }

                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
                //throw new Exception("파일업로드중 오류가 발생했습니다.");
            }

            return result;
        }

        [WebMethod(Description = "FileDownload")]
        public byte[] FileDownload(string id, string pwd, string dirname, string filename, ref bool isEnd, ref int offSet)
        {
            if (id != vID || pwd != vPWD) throw new Exception("아이디 또는 패스워드 불일치");

            int chunkSize = 1048576;
            byte[] buffer = new byte[chunkSize];

            string vDir = vRemoteDir + dirname;
            string vFileName = vDir + "\\" + filename;

            FileStream fs = new FileStream(vFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            try
            {
                lock (fs)
                {
                    long fileSize = new FileInfo(vFileName).Length;

                    fs.Position = offSet;
                    int byteRead = fs.Read(buffer, 0, chunkSize);

                    if (byteRead != buffer.Length)
                    {
                        chunkSize = byteRead;
                        byte[] trimmedBuffer = new byte[byteRead];
                        Array.Copy(buffer, trimmedBuffer, byteRead);
                        buffer = trimmedBuffer;

                        offSet += byteRead;
                        isEnd = true;

                        return buffer;
                    }
                    else
                    {
                        offSet += byteRead;
                        return buffer;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [WebMethod(Description = "FileDelete", EnableSession = false)]
        public void FileDelete(string id, string pwd, string dirname, string filename)
        {
            if (id != vID || pwd != vPWD)
            {
                throw new Exception("아이디 또는 패스워드 불일치");
            }

            try
            {
                string vDir = vRemoteDir + dirname;
                string vFileName = vDir + "\\" + filename;

                System.IO.FileInfo file = new System.IO.FileInfo(vFileName);
                if (file.Exists) file.Delete();
            }
            catch (Exception)
            {
                throw new Exception("파일삭제중 오류가 발생했습니다.");
            }
        }

        [WebMethod(Description = "MakeDir", EnableSession = false)]
        public void MakeDir(string id, string pwd, string dirname)
        {
            if (id != vID || pwd != vPWD)
            {
                throw new Exception("아이디 또는 패스워드 불일치");
            }

            try
            {
                string vDir = vRemoteDir + dirname;

                if (!Directory.Exists(vDir)) Directory.CreateDirectory(vDir);
            }
            catch (Exception)
            {
                throw new Exception("디렉토리 생성중 오류가 발생했습니다.");
            }
        }

        [WebMethod(Description = "RemoveDir", EnableSession = false)]
        public void RemoveDir(string id, string pwd, string dirname)
        {
            if (id != vID || pwd != vPWD)
            {
                throw new Exception("아이디 또는 패스워드 불일치");
            }

            try
            {
                string vDir = vRemoteDir + dirname;

                if (Directory.Exists(vDir)) Directory.Delete(vDir);
            }
            catch (Exception)
            {
                throw new Exception("디렉토리 삭제중 오류가 발생했습니다.");
            }
        }
    }
}
