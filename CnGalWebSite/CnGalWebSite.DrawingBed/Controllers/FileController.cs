﻿using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DrawingBed.Services;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CnGalWebSite.DrawingBed.Controllers
{
    [ApiController]
    [Route("api/files/[action]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IFileService _fileService;

        public FileController(IHttpClientFactory clientFactory, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IFileService fileService)
        {
            _clientFactory = clientFactory;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _fileService = fileService;

        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="files"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> UploadAsync([FromForm] List<IFormFile> files, [FromQuery] double x, [FromQuery] double y, [FromQuery] UploadFileType type)
        {
            if (files.Count == 0)
            {
                files.AddRange(HttpContext.Request.Form.Files);
            }
            var model = new List<UploadResult>();
            foreach (var item in files)
            {
                try
                {
                    model.Add(await _fileService.UploadFormFile(item, x, y, type));
                }
                catch (Exception ex)
                {
                    model.Add(new UploadResult
                    {
                        Uploaded = false,
                        FileName=item.Name,
                        Error = ex.Message
                    });
                }

            }

            return model;
        }

        /// <summary>
        /// 转存图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<UploadResult>> linkToImgUrlAsync([FromQuery] string url, [FromQuery] double x, [FromQuery] double y, [FromQuery] UploadFileType type)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                // 获取请求参数
                var request = HttpContext.Request;
                request.EnableBuffering();
                var postJson = "";
                var stream = HttpContext.Request.Body;
                var length = HttpContext.Request.ContentLength;
                if (length is not null and > 0)
                {
                    // 使用这个方式读取，并且使用异步
                    var streamReader = new StreamReader(stream, Encoding.UTF8);
                    postJson = streamReader.ReadToEndAsync().Result;
                }

                HttpContext.Request.Body.Position = 0;
                var urls = postJson.GetLinks();
                url = urls.FirstOrDefault();
            }


            try
            {
               var result= await _fileService.TransferDepositFile(url, x, y, type);

                return result;
            }
            catch (Exception ex)
            {
                return new UploadResult
                {
                    Uploaded = false,
                    OriginalUrl = url,
                    Error = ex.Message
                };
            }

        }

    }
}
