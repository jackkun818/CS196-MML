using Azure.Core;
using crs.core.DbModels;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using Flurl.Http.Newtonsoft;
using Flurl.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace crs.core.Services
{
    public class Crs_HumanService : Crs_BaseService
    {
        public class HumanParameter
        {
            /// <summary>
            /// Digital people type
            /// </summary>
            [JsonProperty("name")] public string MetaType { get; set; }
        }

        public readonly static Crs_HumanService Instance = new Lazy<Crs_HumanService>(() => new Crs_HumanService()).Value;

        readonly int timeOut = 30;
        readonly string domain = "https://u477648-8af7-1eb671ef.westb.seetacloud.com:8443";
        public string WsDomain = "wss://u477648-8af7-1eb671ef.westb.seetacloud.com:8443/metahuman";
        readonly string restartSocketApi = "/selectMeta/restartSocket";
        readonly string shutdownApi = "/selectMeta/shutdown";
        readonly string getServerStatusApi = "/selectMeta/getServerStatus";       

        public readonly int VoiceSampleRate = 16000;
        public readonly int VoiceChannels = 1;
        public readonly int VideoFpsDelay = 35;
        public readonly ConcurrentDictionary<string, string> MetaTypeDict = new ConcurrentDictionary<string, string>();

        private Crs_HumanService()
        {
            var path = @".\args\human-args.ini";
            if (File.Exists(path))
            {
                var settings = File.ReadAllLines(path);

                domain = $"{settings.FirstOrDefault(m => m.StartsWith("--domain="))?.Split("=")[1]}";
                WsDomain = $"{settings.FirstOrDefault(m => m.StartsWith("--ws_domain="))?.Split("=")[1]}";

                if (int.TryParse(settings.FirstOrDefault(m => m.StartsWith("--sample_rate="))?.Split("=")[1], out var sampleRate))
                {
                    VoiceSampleRate = sampleRate;
                }

                if (int.TryParse(settings.FirstOrDefault(m => m.StartsWith("--channels="))?.Split("=")[1], out var channels))
                {
                    VoiceChannels = channels;
                }

                if (int.TryParse(settings.FirstOrDefault(m => m.StartsWith("--video_fps_delay="))?.Split("=")[1], out var fpsDelay))
                {
                    VideoFpsDelay = fpsDelay;
                }

                MetaTypeDict = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(settings.FirstOrDefault(m => m.StartsWith("--meta_type="))?.Split("=")[1]);
            }
        }

        public async Task<(bool status, string msg, JToken data)> RestartSocket(HumanParameter parameter, CancellationToken token = default)
        {
            var content = parameter;
            var result = await LogProxyAsync<string>(async parameter =>
            {
                parameter.Content = content;

                var request = new Uri(domain).AppendPathSegment(restartSocketApi).WithTimeout(timeOut);
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

        public async Task<(bool status, string msg, JToken data)> Shutdown(CancellationToken token = default)
        {
            var content = new object();
            var result = await LogProxyAsync<string>(async parameter =>
            {
                parameter.Content = content;

                var request = new Uri(domain).AppendPathSegment(shutdownApi).WithTimeout(timeOut);
                parameter.Request = request;

                var response = await request.GetAsync(cancellationToken: token);
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

        public async Task<(bool status, string msg, JToken data)> GetServerStatus(CancellationToken token = default)
        {
            var content = new object();
            var result = await LogProxyAsync<string>(async parameter =>
            {
                parameter.Content = content;

                var request = new Uri(domain).AppendPathSegment(getServerStatusApi).WithTimeout(timeOut);
                parameter.Request = request;

                var response = await request.GetAsync(cancellationToken: token);
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
