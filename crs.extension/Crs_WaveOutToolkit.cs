using crs.core.Services;
using crs.extension.Controls;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static crs.core.Services.Crs_TtsService;

namespace crs.extension
{
    public static class Crs_WaveOutToolkit
    {
        static BufferedWaveProvider waveProvider;
        static WaveOutEvent waveOut;

        static ConcurrentDictionary<string, byte[]> voiceDict = new ConcurrentDictionary<string, byte[]>();
        static CancellationTokenSource voiceCts;

        static Crs_WaveOutToolkit()
        {
            var sampleRate = Crs_TtsService.Instance.VoiceSampleRate;
            var channels = Crs_TtsService.Instance.VoiceChannels;

            var waveFormat = new WaveFormat(sampleRate: sampleRate, channels: channels);
            waveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = sampleRate * 2 * 60,
                DiscardOnBufferOverflow = true
            };

            waveOut = new WaveOutEvent();
            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        public static async Task<(bool status, string msg, byte[] voiceBuffer)> TextToSpeechAsync(string voiceText)
        {
            if (string.IsNullOrWhiteSpace(voiceText))
            {
                return (false, "voiceText is null", null);
            }

            if (!voiceDict.TryGetValue(voiceText, out var voiceBuffer))
            {
                voiceCts = new CancellationTokenSource();
                var token = voiceCts.Token;

                var parameter = new TtsParameter
                {
                    Text = voiceText,
                    CutPunc = "，。：、？"
                };

                var (status, msg, data) = await Crs_TtsService.Instance.Tts(parameter, token);
                if (!status)
                {
                    return (false, msg, null);
                }

                voiceBuffer = data;
                voiceDict.TryAdd(voiceText, voiceBuffer);
            }

            if (voiceBuffer != null)
            {
                return (true, null, voiceBuffer);
            }
            return (false, "voiceBuffer is null", null);
        }

        public static (bool status, string msg) AudioPlay(byte[] voiceBuffer)
        {
            AudioCancel();
            if (voiceBuffer == null)
            {
                return (false, "voiceBuffer is null");
            }

            if (waveProvider == null)
            {
                return (false, "waveProvider is null");
            }

            waveProvider.AddSamples(voiceBuffer, 0, voiceBuffer.Length);
            return (true, null);
        }

        public static (bool status, string msg) AudioCancel()
        {
            voiceCts?.Cancel();
            if (waveProvider == null)
            {
                return (false, "waveProvider is null");
            }

            waveProvider.ClearBuffer();
            return (true, null);
        }
    }
}
