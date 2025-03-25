using CommandLine;
using CommandLine.Text;
using PortAudioSharp;
using SherpaOnnx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace crs.core.Services
{
    public class Crs_SpeechToTextService
    {
        class Options
        {
            [Option(Required = true, HelpText = "Path to tokens.txt")]
            public string Tokens { get; set; }

            [Option(Required = false, Default = "cpu", HelpText = "Provider, e.g., cpu, coreml")]
            public string Provider { get; set; }

            [Option(Required = false, HelpText = "Path to transducer encoder.onnx")]
            public string Encoder { get; set; }

            [Option(Required = false, HelpText = "Path to transducer decoder.onnx")]
            public string Decoder { get; set; }

            [Option(Required = false, HelpText = "Path to transducer joiner.onnx")]
            public string Joiner { get; set; }

            [Option("paraformer-encoder", Required = false, HelpText = "Path to paraformer encoder.onnx")]
            public string ParaformerEncoder { get; set; }

            [Option("paraformer-decoder", Required = false, HelpText = "Path to paraformer decoder.onnx")]
            public string ParaformerDecoder { get; set; }

            [Option("num-threads", Required = false, Default = 1, HelpText = "Number of threads for computation")]
            public int NumThreads { get; set; }

            [Option("decoding-method", Required = false, Default = "greedy_search",
                    HelpText = "Valid decoding methods are: greedy_search, modified_beam_search")]
            public string DecodingMethod { get; set; }

            [Option(Required = false, Default = false, HelpText = "True to show model info during loading")]
            public bool Debug { get; set; }

            [Option("sample-rate", Required = false, Default = 16000, HelpText = "Sample rate of the data used to train the model")]
            public int SampleRate { get; set; }

            [Option("max-active-paths", Required = false, Default = 4,
                HelpText = @"Used only when --decoding--method is modified_beam_search.
It specifies number of active paths to keep during the search")]
            public int MaxActivePaths { get; set; }

            [Option("enable-endpoint", Required = false, Default = true,
                HelpText = "True to enable endpoint detection.")]
            public bool EnableEndpoint { get; set; }

            [Option("rule1-min-trailing-silence", Required = false, Default = 2.4F,
                HelpText = @"An endpoint is detected if trailing silence in seconds is
larger than this value even if nothing has been decoded. Used only when --enable-endpoint is true.")]
            public float Rule1MinTrailingSilence { get; set; }

            [Option("rule2-min-trailing-silence", Required = false, Default = 0.8F,
                HelpText = @"An endpoint is detected if trailing silence in seconds is
larger than this value after something that is not blank has been decoded. Used
only when --enable-endpoint is true.")]
            public float Rule2MinTrailingSilence { get; set; }

            [Option("rule3-min-utterance-length", Required = false, Default = 20.0F,
                HelpText = @"An endpoint is detected if the utterance in seconds is
larger than this value. Used only when --enable-endpoint is true.")]
            public float Rule3MinUtteranceLength { get; set; }
        }


        public readonly static Crs_SpeechToTextService Instance = new Lazy<Crs_SpeechToTextService>(() => new Crs_SpeechToTextService()).Value;

        readonly string[] args = Array.Empty<string>();

        bool isInit = false;
        public bool IsInit => isInit;

        string errorMessage = null;

        TaskCompletionSource stateTcls = null;
        ManualResetEventSlim stateMres = new ManualResetEventSlim(false);
        CancellationTokenSource stateClts = null;

        public readonly string AwakenWord = null;
        public readonly int AwakenTime = 6000;
        public readonly int DelayTime = 3000;

        public event Action<string, bool> OnMessageReceivedEvent;

        private Crs_SpeechToTextService()
        {
            var path = @".\args\sherpa-onnx-args.ini";
            if (File.Exists(path))
            {
                var settings = File.ReadAllLines(path).Where(m => m.StartsWith("--"));

                this.args = settings.Where(m => File.Exists(m.Split("=")[1])).ToArray();
                this.AwakenWord = settings.FirstOrDefault(m => m.StartsWith("--awaken_word="))?.Split("=")[1];
                this.AwakenTime = int.Parse(settings.FirstOrDefault(m => m.StartsWith("--awaken_time="))?.Split("=")[1]);
                this.DelayTime = int.Parse(settings.FirstOrDefault(m => m.StartsWith("--delay_time="))?.Split("=")[1]);
            }
        }

        public async Task<(bool status, string msg)> InitAsync()
        {
            try
            {
                if (isInit)
                {
                    return (true, null);
                }

                var initTcls = new TaskCompletionSource<(bool, string)>();

                stateClts?.Cancel();
                stateClts = new CancellationTokenSource();

                var token = stateClts.Token;

                _ = Task.Run(() =>
                {
                    var parser = new CommandLine.Parser(with => with.HelpWriter = null);
                    var parserResult = parser.ParseArguments<Options>(args);

                    parserResult.WithParsed(options => Run(options, initTcls, token)).WithNotParsed(errs => DisplayHelp(parserResult, errs, initTcls));
                });

                (isInit, errorMessage) = await initTcls.Task;
                return (isInit, errorMessage);
            }
            catch (Exception ex)
            {
                isInit = false;
                errorMessage = ex.Message;
                return (false, ex.Message);
            }
        }

        public async Task<(bool status, string msg)> StartAsync()
        {
            if (!isInit)
            {
                return (isInit, errorMessage);
            }

            if (!stateMres.IsSet)
            {
                stateTcls = new TaskCompletionSource();
                stateMres.Set();
                await stateTcls.Task;
                stateTcls = null;
            }

            return (true, null);
        }

        public async Task<(bool status, string msg)> StopAsync()
        {
            if (!isInit)
            {
                return (isInit, errorMessage);
            }

            if (stateMres.IsSet)
            {
                stateTcls = new TaskCompletionSource();
                stateMres.Reset();
                await stateTcls.Task;
                stateTcls = null;
            }

            return (true, null);
        }

        public void Dispose()
        {
            if (!isInit)
            {
                return;
            }

            stateClts?.Cancel();
            stateMres.Set();
        }

        private void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs, TaskCompletionSource<(bool, string)> tcls)
        {
            string usage = @"
(1) Streaming transducer models

dotnet run -c Release \
  --tokens ./sherpa-onnx/icefall-asr-zipformer-streaming-wenetspeech-20230615/data/lang_char/tokens.txt \
  --encoder ./sherpa-onnx/icefall-asr-zipformer-streaming-wenetspeech-20230615/exp/encoder-epoch-12-avg-4-chunk-16-left-128.onnx \
  --decoder ./sherpa-onnx/icefall-asr-zipformer-streaming-wenetspeech-20230615/exp/decoder-epoch-12-avg-4-chunk-16-left-128.onnx \
  --joiner ./sherpa-onnx/icefall-asr-zipformer-streaming-wenetspeech-20230615/exp/joiner-epoch-12-avg-4-chunk-16-left-128.onnx

(2) Streaming Paraformer models

dotnet run \
  --tokens=./sherpa-onnx/sherpa-onnx-streaming-paraformer-bilingual-zh-en/tokens.txt \
  --paraformer-encoder=./sherpa-onnx/sherpa-onnx-streaming-paraformer-bilingual-zh-en/encoder.int8.onnx \
  --paraformer-decoder=./sherpa-onnx/sherpa-onnx-streaming-paraformer-bilingual-zh-en/decoder.int8.onnx

Please refer to
https://k2-fsa.github.io/sherpa/onnx/pretrained_models/online-transducer/index.html
https://k2-fsa.github.io/sherpa/onnx/pretrained_models/online-paraformer/index.html
to download pre-trained streaming models.
";

            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = usage;
                h.Copyright = "Copyright (c) 2023 Xiaomi Corporation";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);

            Crs_LogHelper.Info(helpText);

            tcls.TrySetResult((false, helpText));
        }

        private void Run(Options options, TaskCompletionSource<(bool, string)> tcls, CancellationToken token)
        {
            var infoBuilder = new StringBuilder();

            try
            {
                infoBuilder.AppendLine(PortAudio.VersionInfo.versionText);
                PortAudio.Initialize();

                infoBuilder.AppendLine($"Number of devices: {PortAudio.DeviceCount}");
                for (int i = 0; i != PortAudio.DeviceCount; ++i)
                {
                    infoBuilder.AppendLine($" Device {i}");
                    DeviceInfo deviceInfo = PortAudio.GetDeviceInfo(i);

                    infoBuilder.AppendLine($"   Name: {deviceInfo.name}");
                    infoBuilder.AppendLine($"   Max input channels: {deviceInfo.maxInputChannels}");
                    infoBuilder.AppendLine($"   Default sample rate: {deviceInfo.defaultSampleRate}");
                }

                int deviceIndex = PortAudio.DefaultInputDevice;
                if (deviceIndex == PortAudio.NoDevice)
                {
                    infoBuilder.AppendLine("No default input device found");
                    tcls.TrySetResult((false, "未找到默认麦克风设备"));
                    return;
                }

                OnlineRecognizerConfig config = new OnlineRecognizerConfig();
                config.FeatConfig.SampleRate = options.SampleRate;

                // All models from icefall using feature dim 80.
                // You can change it if your model has a different feature dim.
                config.FeatConfig.FeatureDim = 80;

                config.ModelConfig.Transducer.Encoder = options.Encoder;
                config.ModelConfig.Transducer.Decoder = options.Decoder;
                config.ModelConfig.Transducer.Joiner = options.Joiner;

                config.ModelConfig.Paraformer.Encoder = options.ParaformerEncoder;
                config.ModelConfig.Paraformer.Decoder = options.ParaformerDecoder;

                config.ModelConfig.Tokens = options.Tokens;
                config.ModelConfig.Provider = options.Provider;
                config.ModelConfig.NumThreads = options.NumThreads;
                config.ModelConfig.Debug = options.Debug ? 1 : 0;

                config.DecodingMethod = options.DecodingMethod;
                config.MaxActivePaths = options.MaxActivePaths;
                config.EnableEndpoint = options.EnableEndpoint ? 1 : 0;

                config.Rule1MinTrailingSilence = options.Rule1MinTrailingSilence;
                config.Rule2MinTrailingSilence = options.Rule2MinTrailingSilence;
                config.Rule3MinUtteranceLength = options.Rule3MinUtteranceLength;

                OnlineRecognizer recognizer = new OnlineRecognizer(config);
                OnlineStream onlineStream = recognizer.CreateStream();
                DeviceInfo info = PortAudio.GetDeviceInfo(deviceIndex);

                infoBuilder.AppendLine(string.Empty);
                infoBuilder.AppendLine($"Use default device {deviceIndex} ({info.name})");

                StreamParameters param = new StreamParameters();
                param.device = deviceIndex;
                param.channelCount = 1;
                param.sampleFormat = SampleFormat.Float32;
                param.suggestedLatency = info.defaultLowInputLatency;
                param.hostApiSpecificStreamInfo = IntPtr.Zero;

                PortAudioSharp.Stream.Callback callback = (IntPtr input, IntPtr output,
                    UInt32 frameCount,
                    ref StreamCallbackTimeInfo timeInfo,
                    StreamCallbackFlags statusFlags,
                    IntPtr userData
                    ) =>
                {
                    float[] samples = new float[frameCount];
                    Marshal.Copy(input, samples, 0, (Int32)frameCount);

                    onlineStream.AcceptWaveform(options.SampleRate, samples);
                    return StreamCallbackResult.Continue;
                };

                PortAudioSharp.Stream stream = new PortAudioSharp.Stream(inParams: param, outParams: null, sampleRate: options.SampleRate,
                    framesPerBuffer: 0,
                    streamFlags: StreamFlags.ClipOff,
                    callback: callback,
                    userData: IntPtr.Zero
                    );

                infoBuilder.AppendLine(param.ToString());
                infoBuilder.AppendLine("Started! Please speak");

                stream.Start();
                tcls.TrySetResult((true, null));

                Crs_LogHelper.Info(infoBuilder.ToString());

                var lastIsSet = false;
                var lastText = "";

                while (!token.IsCancellationRequested)
                {
                    if (!stateMres.IsSet)
                    {
                        if (stream.IsActive)
                        {
                            stream.Stop();
                        }

                        lastIsSet = false;
                        recognizer.Reset(onlineStream);

                        var stateTcls = this.stateTcls;
                        stateTcls?.TrySetResult();
                    }

                    stateMres.Wait();

                    if (!lastIsSet && stateMres.IsSet)
                    {
                        if (stream.IsStopped)
                        {
                            stream.Start();
                        }

                        lastIsSet = true;
                        recognizer.Reset(onlineStream);

                        var stateTcls = this.stateTcls;
                        stateTcls?.TrySetResult();
                    }

                    while (recognizer.IsReady(onlineStream))
                    {
                        recognizer.Decode(onlineStream);
                    }

                    var text = recognizer.GetResult(onlineStream).Text;
                    var isEndpoint = recognizer.IsEndpoint(onlineStream);

                    if (!string.IsNullOrWhiteSpace(text) && lastText != text)
                    {
                        lastText = text;
                        OnMessageReceivedEvent?.Invoke(lastText, isEndpoint);
                        lastText = null;
                    }

                    if (isEndpoint)
                    {
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            Debug.WriteLine(text);
                            Console.WriteLine(text);
                        }
                        recognizer.Reset(onlineStream);
                    }

                    Thread.Sleep(10);
                }

                PortAudio.Terminate();
                Crs_LogHelper.Info("PortAudio.Terminate");
            }
            catch (Exception ex)
            {
                Crs_LogHelper.Error(ex.ToString(), ex);
                tcls.TrySetResult((false, ex.ToString()));
            }
        }
    }
}
