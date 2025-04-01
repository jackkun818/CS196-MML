using crs.core.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static crs.core.Services.Crs_GptService;
using static crs.core.Services.Crs_TtsService;
using static System.Net.Mime.MediaTypeNames;

namespace crs.core
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.FirstOrDefault(m => m == "DEBUG") == null)
            {
                return;
            }

            await Task.Yield();

            //var parameter = new GuideParameter
            //{
            //    ModuleName = "Focus on attention",
            //    PatientName = "Xiao Ming",
            //    Question = "What I want to do",
            //    DigitalPersonName = "Intelligent robot",
            //    PatientSex = "male",
            //    Relationship = "other",
            //};

            //var task = await Crs_GptService.Instance.Guide(parameter);

            //var parameter = new TtsParameter
            //{
            //    Text = "The late emperor's business was not halfway through, but the middle way collapsed. Today, the lower three points were the result of Yizhou's fatigue. This is indeed a critical survival and death.",
            //    CutPunc = "，。"
            //};

            //var task = await Crs_TtsService.Instance.Tts(parameter);
        }
    }
}
