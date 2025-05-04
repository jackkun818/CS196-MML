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
            /// 指定讲解内容的语⾔，⽀持中文，英文，其他如粤语，潮汕话由语⾳交互负责。
            /// </summary>
            [JsonProperty("language")] public string Language { get; set; } = "Chinese";

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
            var content = new { input = parameter };
            var result = await LogProxyAsync<string>(async parameter =>
            {
                parameter.Content = content;

                var request = new Uri(domain).AppendPathSegment(guideApi).WithTimeout(timeOut);
                parameter.Request = request;

                var response = await request.PostJsonAsync(content, cancellationToken: token);
                parameter.Response = response;

                var result = await response.GetStringAsync();
                return (true, null, result);
            });

            return (result.status, result.msg, result.data switch
            {
                not null => JToken.Parse(result.data),
                _ => null
            });
        }
    }
}
