using NAudio.Wave;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Rectangle = System.Drawing.Rectangle;

namespace test.websocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int memcpy(byte* dst, byte* src, int count);

        ClientWebSocket clientWebSocket;

        Uri serverUri;

        BufferedWaveProvider waveProvider;
        WaveOut waveOut;
        int sampleRate = (int)(16000 * 2.0);

        Channel<(byte[] buffer, bool status)> videoChannel;
        Channel<byte[]> audioChannel;

        TaskCompletionSource mediaTcs;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // new WriteableBitmap(roiImage.Width, roiImage.Height, 96, 96, PixelFormats.Bgr24, null)

            videoChannel = Channel.CreateUnbounded<(byte[] buffer, bool status)>();
            audioChannel = Channel.CreateUnbounded<byte[]>();

            waveProvider = new BufferedWaveProvider(new WaveFormat(sampleRate: sampleRate, channels: 1))
            {
                BufferLength = sampleRate * 2 * 60,
                DiscardOnBufferOverflow = true
            };
            waveOut = new WaveOut();
            waveOut.Init(waveProvider);
            waveOut.Play();

            clientWebSocket = new ClientWebSocket();
            serverUri = new Uri("ws://192.168.50.182:8765");

            await clientWebSocket.ConnectAsync(serverUri, default);

            if (clientWebSocket.State == WebSocketState.Open)
            {
                ProcessWebSocket();
                ProcessMedia();

                stop_Click();
            }
        }

        void ProcessMedia()
        {
            ProcessVideo();
            ProcessAudio();

            async void ProcessVideo()
            {
                try
                {
                    await Task.Run(_ProcessVideo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                MessageBox.Show("ProcessVideo End");

                async Task _ProcessVideo()
                {
                    while (await videoChannel.Reader.WaitToReadAsync())
                    {
                        if (videoChannel.Reader.TryRead(out var item))
                        {
                            var buffer = item.buffer;
                            var status = item.status;

                            if (status) await Task.Delay(35);

                            await this.Dispatcher.InvokeAsync(() =>
                            {
                                using var stream = new MemoryStream(buffer);
                                using var bitmap = new Bitmap(stream);

                                var source = this.image.Source as WriteableBitmap;
                                if (source == null || source.Width != bitmap.Width || source.Height != bitmap.Height)
                                {
                                    source = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Bgr24, null);
                                    this.image.Source = source;
                                }
                                UpdateWriteableBitmapSource(source, bitmap);
                            });
                        }
                    }
                }
            }

            async void ProcessAudio()
            {
                try
                {
                    await Task.Run(_ProcessAudio);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                MessageBox.Show("ProcessAudio End");

                async Task _ProcessAudio()
                {
                    while (await audioChannel.Reader.WaitToReadAsync())
                    {
                        if (audioChannel.Reader.TryRead(out var buffer))
                        {
                            if (buffer.Length == 0)
                            {
                                while (waveProvider.BufferedBytes > 0)
                                {
                                    await Task.Delay(10);
                                    continue;
                                }

                                mediaTcs?.TrySetResult();
                                continue;
                            }
                            waveProvider.AddSamples(buffer, 0, buffer.Length);

                            //webSocketSendTcs?.TrySetCanceled();
                            //webSocketSendTcs ??= new TaskCompletionSource();
                        }
                    }
                }
            }
        }

        async void ProcessWebSocket()
        {
            var buffer = new byte[10 * 1024 * 1024];
            var mediaStatus = false;

            var videoFps = 30L;
            var _videoFps = videoFps;

            try
            {
                await Task.Run(_ProcessWebSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show("ProcessWebSocket End");

            async Task _ProcessWebSocket()
            {
                while (clientWebSocket.State == WebSocketState.Open)
                {
                    var result = await clientWebSocket.ReceiveAsync(buffer, default);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await this.Dispatcher.InvokeAsync(() => this.result.Text = text);
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

                        var _buffer = buffer.Skip(1).Take(result.Count - 1).ToArray();
                        await videoChannel.Writer.WriteAsync((_buffer, mediaStatus));
                        continue;
                    }

                    if (flag == 0x01)
                    {
                        if (!mediaStatus) mediaStatus = true;

                        var _buffer = buffer.Skip(1).Take(result.Count - 1).ToArray();
                        await audioChannel.Writer.WriteAsync(_buffer);
                        continue;
                    }

                    if (flag == 0x11)
                    {
                        if (!mediaStatus) continue;
                        mediaStatus = false;

                        var _buffer = buffer.Skip(1).Take(result.Count - 1).ToArray();
                        await audioChannel.Writer.WriteAsync(_buffer);
                        continue;
                    }
                }
            }
        }

        private async void send_Click(object sender, RoutedEventArgs e)
        {
            this.send.IsEnabled = false;
            this.result.Text = null;

            mediaTcs?.TrySetCanceled();
            mediaTcs = new TaskCompletionSource();

            var requestText = message.Text;
            object humanRequest = new
            {
                human = new
                {
                    @type = "echo",
                    text = requestText
                }
            };

            string requestJson = System.Text.Json.JsonSerializer.Serialize(humanRequest);
            byte[] bytes = Encoding.UTF8.GetBytes(requestJson);
            await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            try
            {
                await mediaTcs.Task;

                MessageBox.Show("play end");
            }
            catch (Exception)
            {
                MessageBox.Show("play cancel");
            }
            finally
            {
                this.send.IsEnabled = true;
            }
        }

        private async void stop_Click(object sender = null, RoutedEventArgs e = null)
        {
            this.result.Text = null;
            mediaTcs?.TrySetCanceled();

            object humanRequest = new
            {
                human = new
                {
                    @type = "stop"
                }
            };

            string requestJson = System.Text.Json.JsonSerializer.Serialize(humanRequest);
            byte[] bytes = Encoding.UTF8.GetBytes(requestJson);
            await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// renewWriteableBitmap
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static void UpdateWriteableBitmapSource(WriteableBitmap source, Bitmap bitmap)
        {
            try
            {
                source.Lock();
                BitmapData imageData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                if (imageData.Stride == source.BackBufferStride)
                {
                    PixelCopy(imageData.Scan0, source.BackBuffer, imageData.Stride * imageData.Height);
                }
                else
                {
                    IntPtr srcPtr = imageData.Scan0;
                    var srcStride = imageData.Stride;

                    IntPtr dstPtr = source.BackBuffer;
                    var dstStride = source.BackBufferStride;

                    for (int i = 0; i < imageData.Height; i++)
                    {
                        PixelCopy(srcPtr, dstPtr, dstStride);

                        srcPtr += srcStride;
                        dstPtr += dstStride;
                    }
                }

                bitmap.UnlockBits(imageData);
                source.AddDirtyRect(new Int32Rect(0, 0, bitmap.Width, bitmap.Height));
            }
            finally
            {
                source.Unlock();
            }

            unsafe void PixelCopy(IntPtr srcPtr, IntPtr dstPtr, int count)
            {
                byte* src = (byte*)srcPtr;
                byte* dst = (byte*)dstPtr;
                memcpy(dst, src, count);
            }
        }
    }
}