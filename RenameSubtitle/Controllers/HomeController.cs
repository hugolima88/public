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
        private string ROOT_FOLDER = System.Web.HttpContext.Current.Server.MapPath("~/") + @"rename-subtitles-files\";
        private string TEMP_FOLDER = System.Web.HttpContext.Current.Server.MapPath("~/") + @"rename-subtitles-files\tmp\";
        private string FORMATTED_SUBTITLES_FOLDER = System.Web.HttpContext.Current.Server.MapPath("~/") + @"rename-subtitles-files\formatted-subtitles\";
        private string[] ZIP_FORMATS_LIST = { ".zip", ".rar" };
        private string ZIP_SUBTITLES_FILE = "renamed-subtitles.zip";
        private string SUBTITLE_EXTENSION = ".srt";
        #endregion

        #region Publics Methods
        public ActionResult Index()
        {
            return View("_Home");
        }

        [HttpPost]
        public ActionResult UploadFiles(string videoNameFormatSample)
        {
            try
            {
                if (Request.Files.Count > 0)
                {
                    string mask = _GetMask(videoNameFormatSample);

                    _CreateFolders();

                    _DeleteCreatedFiles();

                    _InitSevenZipExtractor();

                    for (int i = 0; i < Request.Files.Count; i++ )
                    {
                        HttpPostedFileBase file = Request.Files[i];

                        if (file != null)
                        {
                            if (file.FileName.EndsWith(SUBTITLE_EXTENSION))
                            {
                                file.SaveAs(FORMATTED_SUBTITLES_FOLDER + _GetNewFileName(file.FileName, mask));
                            }
                            else if (file.FileName.EndsWithAny(ZIP_FORMATS_LIST))
                            {
                                file.SaveAs(TEMP_FOLDER + file.FileName);

                                _HandleZipFile(TEMP_FOLDER + file.FileName, mask);
                            }
                        }
                    }

                    if (_CreateZipFile())
                    {
                        _DownloadZipFile();
                    }

                    _DeleteCreatedFiles();
                }
            }
            catch(Exception ex)
            {
                return View("_ServerError", (object) ex.Message);
            }

            return View("_Home");
        }

        #endregion

        #region Privates Methods
        private void _CreateFolders()
        {
            System.IO.Directory.CreateDirectory(ROOT_FOLDER);
            System.IO.Directory.CreateDirectory(TEMP_FOLDER);
            System.IO.Directory.CreateDirectory(FORMATTED_SUBTITLES_FOLDER);
        }

        private string _GetMaskGeneric(string videoNameFormatSample, char seasonChar, char eposideChar)
        {
            string mask = string.Empty;
            bool maskFound = false;

            char[] chars = videoNameFormatSample.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].Equals(seasonChar) && !maskFound)
                {
                    string tmpNumber = chars[i + 1].ToString() + chars[i + 2].ToString();
                    int tmpSeason = 0;

                    if (int.TryParse(tmpNumber, out tmpSeason))
                    {
                        if (chars[i + 3].Equals(eposideChar))
                        {
                            tmpNumber = chars[i + 4].ToString() + chars[i + 5].ToString();

                            int tmpEpisode = 0;

                            if (int.TryParse(tmpNumber, out tmpEpisode))
                            {
                                mask += seasonChar + "{0}" + eposideChar + "{1}";
                                i += 5;
                                maskFound = true;
                                continue;
                            }
                        }
                    }
                }

                mask += chars[i];
            }

            return maskFound ? mask : string.Empty;
        }

        private string _GetMaskUppercase(string videoNameFormatSample)
        {
            return _GetMaskGeneric(videoNameFormatSample, 'S', 'E');
        }

        private string _GetMaskLowercase(string videoNameFormatSample)
        {
            return _GetMaskGeneric(videoNameFormatSample, 's', 'e');
        }

        private string _GetMask(string videoNameFormatSample)
        {
            string mask = _GetMaskUppercase(videoNameFormatSample);

            if (string.IsNullOrEmpty(mask))
                mask = _GetMaskLowercase(videoNameFormatSample);

            return mask;
        }

        private string _GetNewFileName(string fileName, string mask)
        {
            string newFileName = _GetNewFileNameUpper(fileName, mask);

            if (string.IsNullOrEmpty(newFileName))
                newFileName = _GetNewFileNameLowercase(fileName, mask);

            return newFileName;
        }

        private string _GetNewFileNameUpper(string fileName, string mask)
        {
            return _GetNewFileNameGeneric(fileName, mask, 'S', 'E');
        }

        private string _GetNewFileNameLowercase(string fileName, string mask)
        {
            return _GetNewFileNameGeneric(fileName, mask, 's', 'e');
        }

        private string _GetNewFileNameGeneric(string fileName, string mask, char seasonChar, char episodeChar)
        {
            string newFileName = string.Empty;

            char[] chars = fileName.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].Equals(seasonChar))
                {
                    string tmpNumber = chars[i + 1].ToString() + chars[i + 2].ToString();
                    int season = 0;

                    if (int.TryParse(tmpNumber, out season))
                    {
                        if (chars[i + 3].Equals(episodeChar))
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

        private void _InitSevenZipExtractor()
        {
            string libPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Libs/7z.dll");
            //SevenZipExtractor.SetLibraryPath(@"D:\Projetos Pessoais\RenameSubtitle\RenameSubtitle\Libs/7z.dll");
            SevenZipExtractor.SetLibraryPath(libPath);
        }

        private bool _CreateZipFile()
        {
            if (System.IO.Directory.GetFiles(FORMATTED_SUBTITLES_FOLDER).Count() > 0)
            {
                SevenZipCompressor compressor = new SevenZipCompressor();
                compressor.ArchiveFormat = OutArchiveFormat.Zip;
                compressor.CompressionMode = CompressionMode.Create;
                compressor.TempFolderPath = System.IO.Path.GetTempPath();
                compressor.CompressDirectory(FORMATTED_SUBTITLES_FOLDER, ROOT_FOLDER + ZIP_SUBTITLES_FILE);

                return true;
            }
            else
                return false;
        }

        private void _HandleZipFile(string zipFilePath, string mask)
        {
            SevenZipExtractor szip = new SevenZipExtractor(zipFilePath);
            szip.ExtractArchive(FORMATTED_SUBTITLES_FOLDER);

            string[] fileEntries = Directory.GetFiles(FORMATTED_SUBTITLES_FOLDER);
            string filePathTmp = string.Empty;
            foreach (string filePath in fileEntries)
            {
                if (filePath.EndsWith(SUBTITLE_EXTENSION))
                {
                    filePathTmp = filePath.Replace(FORMATTED_SUBTITLES_FOLDER, "");

                    filePathTmp = _GetNewFileName(filePathTmp, mask);

                    System.IO.File.Move(filePath, FORMATTED_SUBTITLES_FOLDER + filePathTmp);
                    }
                else
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private void _DownloadZipFile()
        {
            FileInfo file = new FileInfo(ROOT_FOLDER + ZIP_SUBTITLES_FILE);

            if (file.Exists)
            {
                System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;

                response.ClearContent();
                response.Clear();
                response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", file.Name));

                response.AddHeader("Content-Length", file.Length.ToString());

                response.ContentType = "application / zip";

                response.TransmitFile(file.FullName);
                response.Flush();
                
                response.End();

            }
        }

        private void _DeleteCreatedFiles()
        {
            if(System.IO.File.Exists(ROOT_FOLDER + ZIP_SUBTITLES_FILE))
                System.IO.File.Delete(ROOT_FOLDER + ZIP_SUBTITLES_FILE);

            System.IO.DirectoryInfo dir = new DirectoryInfo(TEMP_FOLDER);
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            dir = new DirectoryInfo(FORMATTED_SUBTITLES_FOLDER);
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }
        }
        #endregion
    }
}