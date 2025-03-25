using crs.core;
using crs.core.Services;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace crs.extension
{
    public static class Crs_HumanToolkit
    {
        class ChannelDataItem
        {
            public byte[] Buffer;
            public bool Status;
            public TaskCompletionSource MediaTcs;
        }

        static BufferedWaveProvider waveProvider;
        static WaveOutEvent waveOut;

        static Uri serverUri;
        static int sampleRate = (int)(16000 * 2.0);
        static int channels = 1;

        static ClientWebSocket clientWebSocket;

        static Channel<ChannelDataItem> videoChannel;
        static Channel<ChannelDataItem> audioChannel;

        static TaskCompletionSource mediaTcs;
        static TaskCompletionSource<string> statusTcs;
        static CancellationTokenSource processCts;
        static CancellationTokenSource messageCts;

        static Crs_HumanToolkit()
        {
            serverUri = new Uri(Crs_HumanService.Instance.WsDomain);
            sampleRate = Crs_HumanService.Instance.VoiceSampleRate;
            channels = Crs_HumanService.Instance.VoiceChannels;

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

        public async static Task<(bool status, string msg)> OpenSocketAsync()
        {
            if (serverUri == null)
            {
                return (false, "serverUri is 'Null'");
            }

            try
            {
                if (clientWebSocket != null)
                {
                    if (clientWebSocket.State == WebSocketState.Open)
                    {
                        return (true, $"socketState is '{clientWebSocket.State}'");
                    }

                    clientWebSocket.Dispose();
                    clientWebSocket = null;
                }

                clientWebSocket = new ClientWebSocket();
                await clientWebSocket.ConnectAsync(serverUri, default);

                return (clientWebSocket.State == WebSocketState.Open, $"socketState is '{clientWebSocket.State}'");
            }
            catch (Exception ex)
            {
                Crs_LogHelper.Error("Crs_HumanToolkit.OpenSocketAsync.Error", ex);

                clientWebSocket = null;
                return (false, ex.Message);
            }
        }

        public async static Task<(bool status, string msg)> CloseSocketAsync()
        {
            if (clientWebSocket == null)
            {
                return (false, "socketClient is 'Null'");
            }

            try
            {
                mediaTcs?.TrySetCanceled();
                statusTcs?.TrySetCanceled();
                processCts?.Cancel();
                messageCts?.Cancel();

                if (clientWebSocket.State == WebSocketState.Open)
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default);
                }

                clientWebSocket.Dispose();
                clientWebSocket = null;
                return (true, "socketClient is 'Dispose'");
            }
            catch (Exception ex)
            {
                Crs_LogHelper.Error("Crs_HumanToolkit.CloseSocketAsync.Error", ex);

                clientWebSocket = null;
                return (false, ex.Message);
            }
        }

        public async static Task<(bool status, string msg)> SendMessageAsync(string content)
        {
            if (clientWebSocket == null)
            {
                return (false, "socketClient is 'Null'");
            }

            if (clientWebSocket.State != WebSocketState.Open)
            {
                return (false, $"socketState is '{clientWebSocket.State}'");
            }

            try
            {
                mediaTcs?.TrySetCanceled();
                mediaTcs = new TaskCompletionSource();

                statusTcs?.TrySetCanceled();
                statusTcs = new TaskCompletionSource<string>();

                messageCts?.Cancel();
                messageCts = new CancellationTokenSource();
                var token = messageCts.Token;

                object humanRequest = new
                {
                    human = new
                    {
                        @type = "echo",
                        text = content
                    }
                };

                string requestJson = JsonConvert.SerializeObject(humanRequest);
                byte[] buffer = Encoding.UTF8.GetBytes(requestJson);
                await clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, token);

                //var json = await statusTcs.Task;
                //var result = JsonConvert.DeserializeObject<JToken>(json);
                //if (result.Value<int>("code") != 0)
                //{
                //    return (false, result.Value<string>("data"));
                //}
                return (true, requestJson);
            }
            catch (TaskCanceledException tcEx)
            {
                return (false, tcEx.Message);
            }
            catch (Exception ex)
            {
                Crs_LogHelper.Error("Crs_HumanToolkit.SendMessageAsync.Error", ex);
                return (false, ex.Message);
            }
        }

        public async static Task<(bool status, string msg)> SendStopMessageAsync()
        {
            if (clientWebSocket == null)
            {
                return (false, "socketClient is 'Null'");
            }

            if (clientWebSocket.State != WebSocketState.Open)
            {
                return (false, $"socketState is '{clientWebSocket.State}'");
            }

            try
            {
                mediaTcs?.TrySetCanceled();

                statusTcs?.TrySetCanceled();
                statusTcs = new TaskCompletionSource<string>();

                messageCts?.Cancel();
                messageCts = new CancellationTokenSource();
                var token = messageCts.Token;

                object humanRequest = new
                {
                    human = new
                    {
                        @type = "stop"
                    }
                };

                string requestJson = JsonConvert.SerializeObject(humanRequest);
                byte[] buffer = Encoding.UTF8.GetBytes(requestJson);
                await clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, token);

                //var json = await statusTcs.Task;
                //var result = JsonConvert.DeserializeObject<JToken>(json);
                //if (result.Value<int>("code") != 0)
                //{
                //    return (false, result.Value<string>("data"));
                //}
                return (true, requestJson);
            }
            catch (TaskCanceledException tcEx)
            {
                return (false, tcEx.Message);
            }
            catch (Exception ex)
            {
                Crs_LogHelper.Error("Crs_HumanToolkit.SendStopMessageAsync.Error", ex);
                return (false, ex.Message);
            }
        }

        public static async Task<(bool status, string msg)> WaitMediaProcessAsync()
        {
            try
            {
                var _mediaTcs = mediaTcs;
                if (_mediaTcs != null)
                {
                    await _mediaTcs.Task;
                    return (true, "task is 'Completed'");
                }
                return (true, "task is 'Null'");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool status, string msg)> ProcessAsync(Func<byte[], Task> bitmapAction, Action<string> messageAction)
        {
            try
            {
                processCts?.Cancel();
                processCts = new CancellationTokenSource();
                var token = processCts.Token;

                var mediaTask = RunMediaAsync(bitmapAction, messageAction, token);
                var socketTask = RunWebSocketAsync(messageAction, token);

                await Task.WhenAll(mediaTask, socketTask);
                return (true, "task is 'Completed'");
            }
            catch (TaskCanceledException tcEx)
            {
                return (false, tcEx.Message);
            }
            catch (Exception ex)
            {
                Crs_LogHelper.Error("Crs_HumanToolkit.ProcessAsync.Error", ex);
                return (false, ex.Message);
            }
        }

        static async Task RunWebSocketAsync(Action<string> messageAction, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[10 * 1024 * 1024];
            var mediaStatus = false;

            var videoFps = 100L;
            var _videoFps = videoFps;

            TaskCompletionSource _mediaTcs = null;

            await Task.Run(Process);
            async Task Process()
            {
                while (true)
                {
                    var _clientWebSocket = clientWebSocket;
                    if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
                    {
                        return;
                    }

                    var result = await _clientWebSocket.ReceiveAsync(buffer, cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        statusTcs?.TrySetResult(text);
                        messageAction?.Invoke($"RunWebSocketAsync->{(result.MessageType, text)}");
                        continue;
                    }

                    if (result.Count == 0)
                    {
                        continue;
                    }

                    var flag = buffer[0];
                    if (flag == 0x00)
                    {
                        if (!mediaStatus)
                        {
                            if (--_videoFps < 0) continue;
                        }
                        else _videoFps = videoFps;

                        var item = new ChannelDataItem
                        {
                            Buffer = buffer.Skip(1).Take(result.Count - 1).ToArray(),
                            Status = mediaStatus,
                            MediaTcs = _mediaTcs
                        };

                        await videoChannel.Writer.WriteAsync(item, cancellationToken);
                        messageAction?.Invoke($"RunWebSocketAsync->{(result.MessageType + $"(0x00)", $"bufferLength={result.Count}")}");
                        continue;
                    }

                    if (flag == 0x01)
                    {
                        if (!mediaStatus) mediaStatus = true;

                        if (_mediaTcs == null) _mediaTcs = mediaTcs;

                        var item = new ChannelDataItem
                        {
                            Buffer = buffer.Skip(1).Take(result.Count - 1).ToArray(),
                            MediaTcs = _mediaTcs
                        };

                        await audioChannel.Writer.WriteAsync(item, cancellationToken);
                        messageAction?.Invoke($"RunWebSocketAsync->{(result.MessageType + $"(0x01)", $"bufferLength={result.Count}")}");
                        continue;
                    }

                    if (flag == 0x11)
                    {
                        if (!mediaStatus) continue;
                        mediaStatus = false;

                        var item = new ChannelDataItem
                        {
                            Buffer = buffer.Skip(1).Take(result.Count - 1).ToArray(),
                            MediaTcs = _mediaTcs
                        };
                        _mediaTcs = null;

                        await audioChannel.Writer.WriteAsync(item, cancellationToken);
                        messageAction?.Invoke($"RunWebSocketAsync->{(result.MessageType + $"(0x11)", $"bufferLength={result.Count}")}");
                        continue;
                    }
                }
            }
        }

        static async Task RunMediaAsync(Func<byte[], Task> bitmapAction, Action<string> messageAction, CancellationToken cancellationToken = default)
        {
            videoChannel?.Writer?.Complete();
            videoChannel = Channel.CreateUnbounded<ChannelDataItem>();

            audioChannel?.Writer?.Complete();
            audioChannel = Channel.CreateUnbounded<ChannelDataItem>();

            var videoTask = ProcessVideo();
            var audioTask = ProcessAudio();

            await Task.WhenAll(videoTask, audioTask);

            async Task ProcessVideo()
            {
                await Task.Run(Process);
                async Task Process()
                {
                    while (await videoChannel.Reader.WaitToReadAsync(cancellationToken))
                    {
                        if (videoChannel.Reader.TryRead(out var dataItem))
                        {
                            var buffer = dataItem.Buffer;
                            var status = dataItem.Status;
                            var mediaTcs = dataItem.MediaTcs;

                            bool GetTaskCompleted() => mediaTcs switch
                            {
                                not null => mediaTcs.Task.IsCanceled || mediaTcs.Task.IsCompleted || mediaTcs.Task.IsCompletedSuccessfully || mediaTcs.Task.IsFaulted,
                                _ => true
                            };

                            var delay = Crs_HumanService.Instance.VideoFpsDelay;
                            if (status && !GetTaskCompleted()) await Task.Delay(delay, cancellationToken);

                            if (bitmapAction != null)
                            {
                                await bitmapAction.Invoke(buffer);
                                messageAction?.Invoke($"RunMediaAsync.ProcessVideo->(bufferLength={buffer.Length})");
                            }
                        }
                    }
                }
            }

            async Task ProcessAudio()
            {
                await Task.Run(Process);
                async Task Process()
                {
                    while (await audioChannel.Reader.WaitToReadAsync(cancellationToken))
                    {
                        if (audioChannel.Reader.TryRead(out var dataItem))
                        {
                            var buffer = dataItem.Buffer;
                            //var status = dataItem.Status;
                            var mediaTcs = dataItem.MediaTcs;

                            bool GetTaskCompleted() => mediaTcs switch
                            {
                                not null => mediaTcs.Task.IsCanceled || mediaTcs.Task.IsCompleted || mediaTcs.Task.IsCompletedSuccessfully || mediaTcs.Task.IsFaulted,
                                _ => true
                            };

                            if (buffer.Length == 0)
                            {
                                var taskCompleted = GetTaskCompleted();
                                while (waveProvider.BufferedBytes > 0 && !taskCompleted)
                                {
                                    await Task.Delay(10, cancellationToken);
                                    taskCompleted = GetTaskCompleted();
                                    continue;
                                }

                                waveProvider.ClearBuffer();
                                mediaTcs?.TrySetResult();
                                messageAction?.Invoke($"RunMediaAsync.ProcessAudio->(ClearBuffer->TrySetResult)");
                                continue;
                            }

                            if (GetTaskCompleted())
                            {
                                waveProvider.ClearBuffer();
                                messageAction?.Invoke($"RunMediaAsync.ProcessAudio->(ClearBuffer)");
                                continue;
                            }

                            waveProvider.AddSamples(buffer, 0, buffer.Length);
                            messageAction?.Invoke($"RunMediaAsync.ProcessAudio->(bufferLength={buffer.Length})");
                        }
                    }
                }
            }
        }
    }
}
