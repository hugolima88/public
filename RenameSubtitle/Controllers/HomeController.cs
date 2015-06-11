using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SevenZip;


namespace RenameSubtitle.Controllers
{
    public class HomeController : Controller
    {
        #region Constants
        private const string ROOT_PATH = @"D:\Temp\";
        private const string UNZIP_PATH = ROOT_PATH + @"Unzip\";
        private const string UNZIP_FORMATTED_PATH = ROOT_PATH + @"Unzip\"; 
        #endregion

        #region Publics Methods
        public ActionResult Index()
        {
            ViewBag.Title = "Rename All Your Subtitles :)";
            return View("_UploadFiles");
        }

        [HttpPost]
        public ActionResult UploadFiles(string videoNameFormatSample)
        {
            string mask = _GetMask(videoNameFormatSample);

            if (Request.Files.Count > 0)
            {
                foreach (string fileKey in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileKey];

                    if (file != null)
                    {
                        if (file.FileName.EndsWith("srt"))
                        {
                            file.SaveAs(ROOT_PATH + _GetNewFileName(file.FileName, mask));
                        }
                        else if (file.FileName.EndsWith("rar"))
                        {
                            string zipFilePath = file.FileName.Replace(".rar", ".zip");
                            file.SaveAs(ROOT_PATH + zipFilePath);

                            _HandleZipFile(ROOT_PATH + zipFilePath, mask);
                        }
                        else if (file.FileName.EndsWith("zip"))
                        {
                            file.SaveAs(ROOT_PATH + file.FileName);

                            _HandleZipFile(ROOT_PATH + file.FileName, mask);
                        }
                    }
                }
            }

            return View();
        } 
        #endregion

        #region Privates Methods
        private void _HandleZipFile(string zipFilePath, string mask)
        {
            SevenZipExtractor.SetLibraryPath(@"D:\Projetos Pessoais\RenameSubtitle\libs\7z.dll");
            SevenZipExtractor szip = new SevenZipExtractor(zipFilePath);
            szip.ExtractArchive(UNZIP_PATH);

            string[] fileEntries = Directory.GetFiles(UNZIP_PATH);
            string filePathTmp = string.Empty;
            foreach(string filePath in fileEntries)
            {
                if(filePath.EndsWith(".srt"))
                {
                    filePathTmp = filePath.Replace(UNZIP_PATH, "");

                    filePathTmp = _GetNewFileName(filePathTmp, mask);

                    System.IO.File.Move(filePath, UNZIP_PATH + filePathTmp);
                }
                else
                {
                    System.IO.File.Delete(filePath);
                }
                
            }

        }

        private string _GetMask(string videoNameFormatSample)
        {
            string mask = string.Empty;
            bool maskFound = false;

            char[] chars = videoNameFormatSample.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].Equals('S') && !maskFound)
                {
                    string tmpNumber = chars[i + 1].ToString() + chars[i + 2].ToString();
                    int tmpSeason = 0;

                    if (int.TryParse(tmpNumber, out tmpSeason))
                    {
                        if (chars[i + 3].Equals('E'))
                        {
                            tmpNumber = chars[i + 4].ToString() + chars[i + 5].ToString();

                            int tmpEpisode = 0;

                            if (int.TryParse(tmpNumber, out tmpEpisode))
                            {
                                mask += "S{0}E{1}";
                                i += 5;
                                maskFound = true;
                                continue;
                            }
                        }
                    }
                }

                mask += chars[i];
            }

            return mask;
        }

        private string _GetNewFileName(string fileName, string mask)
        {
            string newFileName = string.Empty;

            char[] chars = fileName.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].Equals('S'))
                {
                    string tmpNumber = chars[i + 1].ToString() + chars[i + 2].ToString();
                    int season = 0;

                    if (int.TryParse(tmpNumber, out season))
                    {
                        if (chars[i + 3].Equals('E'))
                        {
                            tmpNumber = chars[i + 4].ToString() + chars[i + 5].ToString();

                            int episode = 0;

                            if (int.TryParse(tmpNumber, out episode))
                            {
                                newFileName = String.Format(mask, season.ToString("00"), episode.ToString("00")) + ".srt";
                                break;
                            }
                        }
                    }
                }
            }

            return newFileName;
        } 
        #endregion
    }
}