using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Windows.Media;
using ImageGenerator;

namespace StorIconGeneratorWeb.Controllers
{
    public class HomeController : Controller
    {
        public class ImageVm
        {
            public ImageVm(string link, string title)
            {
                Link = link;
                Id = title.Replace(".png","");
                Title = title;
            }

            public string Id { get; set; }

            public string Link { get; set; }
            public string Title { get; set; }
        }

        public static string[] Exts = { ".png", ".jpg", ".jpeg", ".gif" };

        public async Task<ActionResult> Index()
        {
            ViewBag.Original = null;
            ViewBag.Images = new List<string>();

            try
            {
                var subdir = Session["subdir"] as string;

                if (string.IsNullOrEmpty(subdir))
                {
                    Session["subdir"] = "empty";
                    await UpdateImages("#00000000");
                }

                var links = GetDir("Output").GetFiles().Select(fileInfo => new ImageVm(
                    UrlHelper.GenerateContentUrl("~/Output/" + subdir + "/" + fileInfo.Name, HttpContext),
                    fileInfo.Name)).ToList();

                var inputFile = GetDir("Input").GetFiles().FirstOrDefault(x => Exts.Contains(x.Extension));

                ViewBag.Images = links;
                ViewBag.Original = subdir + "/" + inputFile.Name;
            }
            catch (Exception ex)
            {
            }
            return View();
        }

        public ActionResult Image(string id)
        {
            var subdir = Session["subdir"] as string;

            var dir = Server.MapPath("~/Output/" + subdir);
            var path = Path.Combine(dir, id + ".png");
            return base.File(path, "image/png");
        }

        public DirectoryInfo GetDir(string name)
        {
            var subdir = Session["subdir"] as string;

            var mapped = Server.MapPath("~/" + name);
            //var subdir = Session.SessionID;
            var dirInfo = new DirectoryInfo(Path.Combine(mapped, subdir));
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            return dirInfo;
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "HELLO WORLD!";

            return View();
        }

        public async Task<ActionResult> UpdateImages(string color)
        {
            try
            {
                var generator = new IconsGenerator
                {
                    Color = (Color) ColorConverter.ConvertFromString(color)
                };

                var inputFile = GetDir("Input").GetFiles().FirstOrDefault(x => Exts.Contains(x.Extension));

                var subdir = Session["subdir"] as string;

                if (inputFile != null || subdir == "empty")
                {
                    generator.UpdateIcons();
                    await generator.CreateIcons(GetDir("Output").FullName, inputFile?.FullName);
                }
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> FileUpload(HttpPostedFileBase file, string color)
        {
            if (file != null)
            {
                Session["subdir"] = file.FileName.GetHashCode().ToString("X");

                var dirInfo = GetDir("Input");

//                var inputFiles = dirInfo.GetFiles();
//                foreach (var fileInfo in inputFiles)
//                {
//                    fileInfo.Delete();
//                }

                if (file != null)
                {
                    string pic = System.IO.Path.GetFileName(file.FileName);
                    string path = System.IO.Path.Combine(dirInfo.FullName, pic);
                    // file is uploaded
                    file.SaveAs(path);

                    // save the image path path to the database or you can send image 
                    // directly to database
                    // in-case if you want to store byte[] ie. for DB
                    using (MemoryStream ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                    }

                }
            }
            return await UpdateImages(color);

            // after successfully uploading redirect the user
//            return RedirectToAction("Index", "Home");
        }


    }
}