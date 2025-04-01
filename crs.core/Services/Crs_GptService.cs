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
            /// Specify the explanation of the content⾔，⽀Held Chinese, English, other such as Cantonese, Chaoshan dialect⾳Responsible for interaction.
            /// </summary>
            [JsonProperty("language")] public string Language { get; set; } = "Chinese";

            /// <summary>
            /// The current patient should be⾏Rehabilitation activity name or code
            /// </summary>
            [JsonProperty("module_name")] public string ModuleName { get; set; }

            /// <summary>
            /// Patient's only⼀Identifier
            /// </summary>
            [JsonProperty("patient_name")] public string PatientName { get; set; }

            /// <summary>
            /// The patient's current problems
            /// </summary>
            [JsonProperty("question")] public string Question { get; set; }

            /// <summary>
            /// number⼈Name
            /// </summary>
            [JsonProperty("digital_person_name")] public string DigitalPersonName { get; set; }

            /// <summary>
            /// number⼈Relationship with patients
            /// </summary>
            [JsonProperty("relationship")] public string Relationship { get; set; }

            /// <summary>
            /// The gender of the patient
            /// </summary>
            [JsonProperty("patient_sex")] public string PatientSex { get; set; }
        }

        public readonly static Crs_GptService Instance = new Lazy<Crs_GptService>(() => new Crs_GptService()).Value;

        readonly int timeOut = 30;
        readonly string domain = "https://u477648-bd7b-a6cf61d5.westb.seetacloud.com:8443";
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
