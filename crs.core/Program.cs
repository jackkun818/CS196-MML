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
            //    ModuleName = "专注注意力",
            //    PatientName = "小明",
            //    Question = "我要做什么",
            //    DigitalPersonName = "智能机器人",
            //    PatientSex = "男",
            //    Relationship = "其他",
            //};

            //var task = await Crs_GptService.Instance.Guide(parameter);

            //var parameter = new TtsParameter
            //{
            //    Text = "先帝创业未半而中道崩殂，今天下三分，益州疲弊，此诚危急存亡之秋也。",
            //    CutPunc = "，。"
            //};

            //var task = await Crs_TtsService.Instance.Tts(parameter);
        }
    }
}
