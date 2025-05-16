using Azure.Core;
using crs.core.DbModels;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using Flurl.Http.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace crs.core.Services
{
    public class Crs_GptService : Crs_BaseService
    {
        public class GuideParameter
        {
            /// <summary>
            /// 当前患者应该进⾏的康复活动名称或代码
            /// </summary>
            [JsonProperty("module_name")] public string ModuleName { get; set; }

            /// <summary>
            /// 患者的唯⼀标识符
            /// </summary>
            [JsonProperty("patient_name")] public string PatientName { get; set; }

            /// <summary>
            /// 患者当前遇到的问题
            /// </summary>
            [JsonProperty("question")] public string Question { get; set; }

            /// <summary>
            /// 数字⼈姓名
            /// </summary>
            [JsonProperty("digital_person_name")] public string DigitalPersonName { get; set; }

            /// <summary>
            /// 数字⼈与患者的关系
            /// </summary>
            [JsonProperty("relationship")] public string Relationship { get; set; }

            /// <summary>
            /// 患者的性别
            /// </summary>
            [JsonProperty("patient_sex")] public string PatientSex { get; set; }

            /// <summary>
            /// 当前界面的截图（Base64编码）
            /// </summary>
            [JsonProperty("screenshot")] public string ScreenShot { get; set; }
        }

        public readonly static Crs_GptService Instance = new Lazy<Crs_GptService>(() => new Crs_GptService()).Value;

        readonly int timeOut = 30;
        readonly string domain = "http://127.0.0.1:8001";
        readonly string guideApi = "/gpt/guide/invoke";

        private Crs_GptService()
        {
            var path = @".\args\gpt-args.ini";
            if (File.Exists(path))
            {
                var settings = File.ReadAllLines(path);
                this.domain = settings.FirstOrDefault(m => m.StartsWith("--domain="))?.Split("=")[1];
            }
        }

        public async Task<(bool status, string msg, JToken data)> Guide(GuideParameter parameter, CancellationToken token = default)
        {
            try
            {
                var request = new Uri(domain).AppendPathSegment(guideApi).WithTimeout(timeOut);
                
                // 发送所有必需的参数
                var requestBody = new
                {
                    module_name = parameter.ModuleName ?? "注意力分配",
                    question = parameter.Question,
                    patient_name = parameter.PatientName ?? "Alice Johnson",
                    patient_sex = parameter.PatientSex ?? "女",
                    language = "Chinese",
                    context = "",
                    screenshot = parameter.ScreenShot
                };
                
                Debug.WriteLine($"[GPT Service] Sending request to {request}:");
                Debug.WriteLine($"[GPT Service] Request body: {JsonConvert.SerializeObject(requestBody, Formatting.Indented)}");
                Debug.WriteLine($"[GPT Service] Screenshot length: {(parameter.ScreenShot?.Length ?? 0)} bytes");
                
                var response = await request.PostJsonAsync(requestBody, cancellationToken: token);
                
                if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    var errorContent = await response.GetStringAsync();
                    Debug.WriteLine($"[GPT Service] Error Response ({response.ResponseMessage.StatusCode}):");
                    Debug.WriteLine($"[GPT Service] {errorContent}");
                    return (false, $"HTTP Error: {response.ResponseMessage.StatusCode} - {errorContent}", null);
                }

                var result = await response.GetStringAsync();
                Debug.WriteLine($"[GPT Service] Successful Response:");
                Debug.WriteLine($"[GPT Service] {result}");
                
                var jsonResult = JToken.Parse(result);
                
                // 检查响应格式
                if (jsonResult["output"] == null)
                {
                    Debug.WriteLine("[GPT Service] Warning: Response missing 'output' field");
                    return (false, "Invalid response format: missing 'output' field", null);
                }
                
                return (true, null, jsonResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GPT Service] Exception:");
                Debug.WriteLine($"[GPT Service] {ex.Message}");
                Debug.WriteLine($"[GPT Service] Stack trace: {ex.StackTrace}");
                return (false, ex.Message, null);
            }
        }
    }
}
