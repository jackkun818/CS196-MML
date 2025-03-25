using crs.core.Services;
using crs.extension.Controls;
using Microsoft.VisualBasic;
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
    public static class Crs_WaveInToolkit
    {
        static WaveInEvent waveIn;
        static TaskCompletionSource<byte[]> voiceTcs;

        static Crs_WaveInToolkit()
        {
            var sampleRate = Crs_TtsService.Instance.VoiceSampleRate;
            var channels = Crs_TtsService.Instance.VoiceChannels;

            var waveFormat = new WaveFormat(sampleRate: sampleRate, channels: channels);
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = waveFormat;
        }

        public static async Task<(bool status, string msg, byte[] voiceBuffer)> RecordAudioStartAsync()
        {
            var deviceCount = WaveInEvent.DeviceCount;
            if (deviceCount == 0)
            {
                return (false, "microphone device not found", null);
            }

            voiceTcs?.TrySetCanceled();
            _ = RecordAudioCompleteAsync();
            if (waveIn == null)
            {
                return (false, "waveIn is null", null);
            }

            var stream = new MemoryStream();
            var waveWriter = new WaveFileWriter(stream, waveIn.WaveFormat);

            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;

            async void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
            {
                await waveWriter.WriteAsync(e.Buffer, 0, e.BytesRecorded);
            }

            async void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
            {
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.RecordingStopped -= WaveIn_RecordingStopped;

                if (voiceTcs != null)
                {
                    var voiceBuffer = stream.ToArray();
                    voiceTcs?.TrySetResult(voiceBuffer);
                }

                await waveWriter.DisposeAsync();
                await stream.DisposeAsync();
            }

            waveIn.StartRecording();

            voiceTcs = new TaskCompletionSource<byte[]>();
            try
            {
                var voiceBuffer = await voiceTcs.Task;
                return (true, null, voiceBuffer);
            }
            catch (Exception) { voiceTcs = null; }
            return (false, "voiceBuffer is null", null);
        }

        public static async Task<(bool status, string msg, byte[] voiceBuffer)> RecordAudioCompleteAsync(bool cancel = false)
        {
            var deviceCount = WaveInEvent.DeviceCount;
            if (deviceCount == 0)
            {
                return (false, "microphone device not found", null);
            }

            if (waveIn == null)
            {
                return (false, "waveIn is null", null);
            }

            if (cancel)
            {
                voiceTcs?.TrySetCanceled();
            }

            waveIn.StopRecording();

            if (voiceTcs != null)
            {
                try
                {
                    var voiceBuffer = await voiceTcs.Task;
                    return (true, null, voiceBuffer);
                }
                catch (Exception) { voiceTcs = null; }
            }
            return (false, "voiceBuffer is null", null);
        }
    }
}
