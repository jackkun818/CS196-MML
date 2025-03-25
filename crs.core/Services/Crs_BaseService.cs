using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Azure.Core;
using crs.core.DbModels;
using Flurl.Http.Newtonsoft;
using System.Diagnostics;

namespace crs.core.Services
{
    public class Crs_BaseService
    {
        static Crs_BaseService()
        {
            FlurlHttp.Clients.UseNewtonsoft();
        }

        protected class HttpParameter
        {
            public object Content;
            public IFlurlRequest Request;
            public IFlurlResponse Response;
        }

        protected async Task<(bool status, string msg, T data)> LogProxyAsync<T>(Func<HttpParameter, Task<(bool status, string msg, T data)>> action) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            var logBuilder = new StringBuilder(512);
            var parameter = new HttpParameter();

            try
            {
                var result = await action.Invoke(parameter);

                var content = parameter.Content;
                var request = parameter.Request;

                logBuilder.Append($"url：{request.Url}\r\n");
                logBuilder.Append($"method：{request.Verb.Method}\r\n");
                logBuilder.Append($"header：{JsonConvert.SerializeObject(request.Headers.ToDictionary(m => m.Name, m => m.Value))}\r\n");
                logBuilder.Append($"content：{JsonConvert.SerializeObject(content)}\r\n");

                logBuilder.Append($"{result.data switch
                {
                    byte[] => $"result：StreamLength={(result.data as byte[])?.Length}",
                    string => $"result：{result.data}",
                    _ => result.data?.ToString()
                }}\r\n");

                return result;
            }
            catch (Exception ex)
            {
                var content = parameter.Content;
                var request = parameter.Request;

                logBuilder.Clear();
                logBuilder.Append($"url：{request.Url}\r\n");
                logBuilder.Append($"method：{request.Verb.Method}\r\n");
                logBuilder.Append($"header：{JsonConvert.SerializeObject(request.Headers.ToDictionary(m => m.Name, m => m.Value))}\r\n");
                logBuilder.Append($"content：{JsonConvert.SerializeObject(content)}\r\n");
                logBuilder.Append($"exception：{ex.Message}\r\n");

                return (false, ex.Message, null);
            }
            finally
            {
                stopwatch.Stop();
                var timeSpan = stopwatch.Elapsed;
                var time = $"{timeSpan.Hours.ToString("00")}:{timeSpan.Minutes.ToString("00")}:{timeSpan.Seconds.ToString("00")}.{timeSpan.Milliseconds.ToString("000")}";
                logBuilder.Append($"time：{time}");

                Crs_LogHelper.Info(logBuilder.ToString());
            }
        }
    }
}
