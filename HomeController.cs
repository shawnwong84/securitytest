using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TorroMarkono.Models;

namespace TorroMarkono.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("")]
        public IActionResult Index()
        {
            //directory
            var uploadDirectory = Path.Combine("wwwroot", "Upload");
            //filename
            var uploadedFiles = Directory.GetFiles(uploadDirectory).Select(Path.GetFileName);
            //model
            var viewModel = new FileUploadViewModel
            {
                UploadedFiles = uploadedFiles.Select(fileName => new FileUploadViewModel.UploadedFile
                {
                    FileName = fileName,
                    ContentType = GetContentType(fileName),
                    Length = new FileInfo(Path.Combine(uploadDirectory, fileName)).Length,
                    CreationTime = new FileInfo(Path.Combine(uploadDirectory, fileName)).CreationTime

                }).ToList()
            };

            //return to page with model
            return View("Index", viewModel);


        }
        



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Route("FileUpload")]
        public async Task<IActionResult> Index(List<IFormFile> files)
        {
            try
            {
                //file empty
                if (files == null || files.Count == 0)
                {
                    ViewBag.ErrorMessage = "Please select at least one file for upload.";
                    return View();
                }

                var uploadedFiles = new List<FileUploadViewModel.UploadedFile>();

                foreach (var file in files)
                {
                    //got file selected
                    if (file.Length > 0)
                    {
                        // Check if the file is executable or a zip file
                        var allowedExtensions = new[] { ".exe", ".zip" };
                        //file extension to lower char
                        var fileExtension = Path.GetExtension(file.FileName).ToLower();
                        //if the file is executable or a zip file, notallow upload
                        if (allowedExtensions.Contains(fileExtension))
                        {
                            ViewBag.ErrorMessage = "Executable and zip files are not allowed.";
                            return View();
                        }

                        // Generate a unique filename to avoid overwriting
                        var uniqueFileName = GetUniqueFileName(file.FileName);

                        // Get the file path to save
                        var filePath = Path.Combine("wwwroot/Upload", uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Store file information in the view model
                        var uploadedFile = new FileUploadViewModel.UploadedFile
                        {
                            FileName = uniqueFileName,
                            ContentType = fileExtension,
                            Length = file.Length,
                            CreationTime = DateTime.Now
                        };
                        uploadedFiles.Add(uploadedFile);
                    }
                }
                //success upload
                ViewBag.Message = "Files uploaded successfully!";
                var viewModel = new FileUploadViewModel
                {
                    UploadedFiles = uploadedFiles
                };

               
                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                //fail upload
                _logger.LogError(ex, "Error while uploading files.");
                ViewBag.ErrorMessage = "An error occurred while uploading files.";
                return View();
            }
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            string uniqueFileName = fileName;

            // Get the file path to the Upload folder
            var uploadFolderPath = Path.Combine("wwwroot", "Upload");

            // Check if the file already exists in the Upload folder
            if (System.IO.File.Exists(Path.Combine(uploadFolderPath, uniqueFileName)))
            {
                // If the file exists, generate a unique name by appending a version number
                int version = 1;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string fileExtension = Path.GetExtension(fileName);

                while (System.IO.File.Exists(Path.Combine(uploadFolderPath, uniqueFileName)))
                {
                    uniqueFileName = $"{fileNameWithoutExtension}_{version}{fileExtension}";
                    version++;
                }
            }

            return uniqueFileName;
        }

        private string GetContentType(string fileName)
        {
            // You can add more content types based on the file's extension
            if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ".pdf";
            }
            else if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                     fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return ".jpeg";
            }
            else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return ".png";
            }
            else if (fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                return ".docx";
            }

            return "others"; // Default content type
        }
    }
}
