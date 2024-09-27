using FlaUI.WebDriver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Buffers.Text;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FlaUI.WebDriver.Controllers
{

    [Route("session/{sessionId}/appium/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {


        private static ActionResult MissingParameter(string parameterName)
        {
            return WebDriverResult.NotFound(new ErrorResponse()
            {
                ErrorCode = "Missing JSON Parameter",
                Message = $"Parameter {parameterName} must be provided in the request."
            });
        }

        private static ActionResult FileNotFound(string fileName)
        {
            return WebDriverResult.NotFound(new ErrorResponse()
            {
                ErrorCode = "File Not Found",
                Message = $"File {fileName} does not exist."
            });
        }

        [HttpPost("push_file")]
        public async Task<ActionResult> PushFile([FromBody] PushFileRequest request)
        {
            if (request.Data == null)
            {
                // data could be an empty string, so just a simple null check
                return MissingParameter("data");
            }
            if (string.IsNullOrEmpty(request.Path))
            {
                return MissingParameter("path");
            }
            var parent = Path.GetDirectoryName(request.Path)!;
            if (!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }
            var data = Convert.FromBase64String(request.Data);
            await System.IO.File.WriteAllBytesAsync(request.Path, data);
            return WebDriverResult.Success();
        }


        [HttpPost("pull_file")]
        public async Task<ActionResult> PullFile([FromBody] PullFileRequest request)
        {
            if (string.IsNullOrEmpty(request.Path))
            {
                return MissingParameter("path");
            }
            if (!System.IO.File.Exists(request.Path)) { 
                return FileNotFound(request.Path);
            }
            var data = await System.IO.File.ReadAllBytesAsync(request.Path);
            return WebDriverResult.Success(Convert.ToBase64String(data));
        }

        [HttpPost("pull_folder")]
        public ActionResult PullFolder([FromBody] PullFileRequest request)
        {
            if (string.IsNullOrEmpty(request.Path))
            {
                return MissingParameter("path");
            }
            if (!Directory.Exists(request.Path))
            {
                return FileNotFound(request.Path);
            }
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                ZipFile.CreateFromDirectory(request.Path, ms);
                ms.Seek(0, SeekOrigin.Begin);
                bytes = ms.ToArray();
            }

            return WebDriverResult.Success(Convert.ToBase64String(bytes));
        }
    }
}
