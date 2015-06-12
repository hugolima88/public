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
        private const string ROOT_FOLDER = @"D:\Temp\";
        private const string FORMATTED_SUBTITLES_FOLDER = ROOT_FOLDER + @"formatted-subtitles\";
        private readonly string[] ZIP_FORMATS_LIST = { ".zip", ".rar" };
        private const string ZIP_SUBTITLES_FILE = "renamed-subtitles.zip";
        private const string SUBTITLE_EXTENSION = ".srt";
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
            try
            {
                if (Request.Files.Count > 0)
                {
                    foreach (string fileKey in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[fileKey];

                        if (file != null)
                        {
                            string mask = _GetMask(videoNameFormatSample);

                            _CreateFolders();

                            _InitSevenZipExtractor();

                            if (file.FileName.EndsWith(SUBTITLE_EXTENSION))
                            {
                                file.SaveAs(FORMATTED_SUBTITLES_FOLDER + _GetNewFileName(file.FileName, mask));
                            }
                            else if (file.FileName.EndsWithAny(ZIP_FORMATS_LIST))
                            {
                                file.SaveAs(ROOT_FOLDER + file.FileName);

                                _HandleZipFile(ROOT_FOLDER + file.FileName, mask);
                            }

                            _CreateZipFile();

                            _DownloadZipFile();

                            _DeleteCreatedFiles(file.FileName);
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return View("_UploadFiles");
        }

        #endregion

        #region Privates Methods
        private void _CreateFolders()
        {
            System.IO.Directory.CreateDirectory(ROOT_FOLDER);
            System.IO.Directory.CreateDirectory(FORMATTED_SUBTITLES_FOLDER);
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

        private void _InitSevenZipExtractor()
        {
            string libPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Libs/7z.dll");
            //SevenZipExtractor.SetLibraryPath(@"D:\Projetos Pessoais\RenameSubtitle\RenameSubtitle\Libs/7z.dll");
            SevenZipExtractor.SetLibraryPath(libPath);
        }

        private void _CreateZipFile()
        {
            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.Zip;
            compressor.CompressionMode = CompressionMode.Create;
            compressor.TempFolderPath = System.IO.Path.GetTempPath();
            compressor.CompressDirectory(FORMATTED_SUBTITLES_FOLDER, ROOT_FOLDER + ZIP_SUBTITLES_FILE);
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

        private void _DeleteCreatedFiles(string receivedZipFile)
        {
            if(System.IO.File.Exists(ROOT_FOLDER + ZIP_SUBTITLES_FILE))
                System.IO.File.Delete(ROOT_FOLDER + ZIP_SUBTITLES_FILE);

            if (System.IO.File.Exists(ROOT_FOLDER + receivedZipFile))
                System.IO.File.Delete(ROOT_FOLDER + receivedZipFile);


            System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(FORMATTED_SUBTITLES_FOLDER);
            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }
        }
        #endregion
    }
}