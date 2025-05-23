﻿using Azure.Core;
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
    public class Crs_TtsService : Crs_BaseService
    {
        public class TtsParameter
        {
            /// <summary>
            /// 输入文本
            /// </summary>
            [JsonProperty("text")] public string Text { get; set; }

            /// <summary>
            /// 选择语音：zh/en
            /// </summary>
            [JsonProperty("text_lang")] public string TextLanguage { get; set; } = "zh";

            /// <summary>
            /// 分割符号
            /// </summary>
            [JsonProperty("cut_punc")] public string CutPunc { get; set; }
        }

        public readonly static Crs_TtsService Instance = new Lazy<Crs_TtsService>(() => new Crs_TtsService()).Value;

        readonly int timeOut = 30;
        readonly string domain = "http://127.0.0.1:8001";
        readonly string ttsApi = "/vit/tts";

        public readonly int VoiceSampleRate = 32000;
        public readonly int VoiceChannels = 1;

        private Crs_TtsService()
        {
            var path = @".\args\tts-args.ini";
            if (File.Exists(path))
            {
                var settings = File.ReadAllLines(path);

                this.domain = settings.FirstOrDefault(m => m.StartsWith("--domain="))?.Split("=")[1];

                if (int.TryParse(settings.FirstOrDefault(m => m.StartsWith("--sample_rate="))?.Split("=")[1], out var sampleRate))
                {
                    this.VoiceSampleRate = sampleRate;
                }

                if (int.TryParse(settings.FirstOrDefault(m => m.StartsWith("--channels="))?.Split("=")[1], out var channels))
                {
                    this.VoiceChannels = channels;
                }
            }
        }

        public async Task<(bool status, string msg, byte[] data)> Tts(TtsParameter parameter, CancellationToken token = default)
        {
            var content = parameter;
            var result = await LogProxyAsync<byte[]>(async parameter =>
            {
                parameter.Content = content;

                var request = new Uri(domain).AppendPathSegment(ttsApi).WithTimeout(timeOut);
                parameter.Request = request;

                var response = await request.PostJsonAsync(content, cancellationToken: token);
                parameter.Response = response;

                var result = await response.GetStreamAsync();
                using var stream = result as MemoryStream;

                return (true, null, stream.ToArray());
            });

            return (result.status, result.msg, result.data);
        }
    }
}
