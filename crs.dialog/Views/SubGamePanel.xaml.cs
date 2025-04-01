using crs.core.Services;
using crs.extension;
using crs.extension.Models;
using Microsoft.VisualBasic.Devices;
using System;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using static crs.core.Services.Crs_GptService;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using crs.theme.Extensions;
using System.Windows.Interop;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using crs.game;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Web.WebView2.Wpf;
using static crs.dialog.Views.SubGamePanel;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;
using Microsoft.Web.WebView2.WinForms;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using SkiaSharp;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static crs.core.Services.Crs_HumanService;
using System.Net.NetworkInformation;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for SubGamePanel
    /// </summary>
    public partial class SubGamePanel : UserControl, IHumanInterface
    {
        [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int memcpy(byte* dst, byte* src, int count);

        public interface IHumanInterface
        {
            Task SendMessageAsync(string message);
            Task StopMessageAsync();
        }

        bool isReady = false;
        bool isPorcess = false;
        DispatcherTimer timer;
        CancellationTokenSource readyClts = null;
        string lastText = null;

        CancellationTokenSource guideClts = null;

        bool isSend = false;

        CancellationTokenSource humanCts = null;

        public SubGamePanel()
        {
            InitializeComponent();
            this.Loaded += SubGamePanel_Loaded;
            this.Unloaded += SubGamePanel_Unloaded;


            SetBinding(DigitalHumanItemProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath("DigitalHumanItem"),
                Source = this.DataContext
            });

            SetBinding(PatientItemProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath("PatientItem"),
                Source = this.DataContext
            });

            SetBinding(ModeTypeProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath("ModeType"),
                Source = this.DataContext
            });

            SetBinding(InstanceProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath("Instance"),
                Mode = System.Windows.Data.BindingMode.OneWayToSource,
                Source = this.DataContext
            });

            this.SetValue(InstanceProperty, this);
        }

        static void WriteLine(string content)
        {
#if DEBUG
            Debug.WriteLine(content);
#endif
        }

        private async void SubGamePanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.timer?.Stop();
            this.timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Crs_SpeechToTextService.Instance.AwakenTime) };
            this.timer.Tick += Timer_Tick;

            var digitalHumanItem = DigitalHumanItem;
            if (digitalHumanItem == null)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync("Digital people information is empty");
                return;
            }

            humanCts?.Cancel();
            humanCts = new CancellationTokenSource();
            var token = humanCts.Token;

            var (status, msg, data) = await Crs_HumanService.Instance.GetServerStatus(token);
            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                return;
            }

            if (!Crs_HumanService.Instance.MetaTypeDict.TryGetValue(digitalHumanItem.Name, out var metaType))
            {
                metaType = Crs_HumanService.Instance.MetaTypeDict.FirstOrDefault().Value;
            }

            if (data.Value<int>("code") != 0 || data.Value<string>("data") != metaType)
            {
                (status, msg, data) = await Crs_HumanService.Instance.Shutdown(token);
                if (!status)
                {
                    await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                    return;
                }

                var parameter = new HumanParameter { MetaType = metaType };
                (status, msg, data) = await Crs_HumanService.Instance.RestartSocket(parameter, token);
                if (!status)
                {
                    await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                    return;
                }

                try
                {
                    do
                    {
                        await Task.Delay(500, token);
                        (status, msg, data) = await Crs_HumanService.Instance.GetServerStatus(token);
                        if (!status)
                        {
                            await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                            return;
                        }

                        if (data.Value<int>("code") == 0)
                        {
                            break;
                        }
                    } while (true);
                }
                catch (Exception) { return; }
            }

            (status, msg) = await Crs_HumanToolkit.OpenSocketAsync();
            WriteLine($"Crs_HumanToolkit.OpenSocketAsync->{(status, msg)}");

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                return;
            }

            RunHumanToolkitMultiMethod();
            async void RunHumanToolkitMultiMethod()
            {
                var (_status, _msg) = await Crs_HumanToolkit.SendStopMessageAsync();
                WriteLine($"Crs_HumanToolkit.SendStopMessageAsync->{(_status, _msg)}");

                (_status, _msg) = await Crs_HumanToolkit.ProcessAsync(ImageCallbackAsync, message => { WriteLine($"Crs_HumanToolkit.{message}"); });
                WriteLine($"Crs_HumanToolkit.ProcessAsync->{(_status, _msg)}");
            }

            (status, msg) = await Crs_SpeechToTextService.Instance.InitAsync();
            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                return;
            }

            Crs_SpeechToTextService.Instance.OnMessageReceivedEvent -= OnMessageReceivedEvent;
            Crs_SpeechToTextService.Instance.OnMessageReceivedEvent += OnMessageReceivedEvent;
            await Crs_SpeechToTextService.Instance.StartAsync();
        }

        private async void SubGamePanel_Unloaded(object sender, RoutedEventArgs e)
        {
            humanCts?.Cancel();
            this.timer?.Stop();

            Crs_SpeechToTextService.Instance.OnMessageReceivedEvent -= OnMessageReceivedEvent;
            _ = await Crs_SpeechToTextService.Instance.StopAsync();

            var (status, msg) = await Crs_HumanToolkit.CloseSocketAsync();
            WriteLine($"Crs_HumanToolkit.CloseSocketAsync->{(status, msg)}");
        }

        private void Timer_Tick(object sender, EventArgs e) => ResetSetting();

        private async Task ImageCallbackAsync(byte[] buffer)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                using var stream = new MemoryStream(buffer);
                using var bitmap = new Bitmap(stream);

                var image = this.webViewGravatar.Content as Image;
                var source = image?.Source as WriteableBitmap;

                if (source == null || source.Width != bitmap.Width || source.Height != bitmap.Height)
                {
                    source = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Bgr24, null);
                    image = new Image { Source = source, Stretch = Stretch.UniformToFill };
                    this.webViewGravatar.Content = image;
                }

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
            });

            unsafe void PixelCopy(IntPtr srcPtr, IntPtr dstPtr, int count)
            {
                byte* src = (byte*)srcPtr;
                byte* dst = (byte*)dstPtr;
                memcpy(dst, src, count);
            }
        }

        private void ResetSetting()
        {
            this.timer?.Stop();

            this.microphoneNotify.Visibility = Visibility.Collapsed;
            this.microphonePopup.IsOpen = false;

            this.isReady = false;
            this.lastText = null;
        }

        private async void OnMessageReceivedEvent(string text, bool isEndpoint)
            => await App.Current.Dispatcher.InvokeAsync(async () => await OnMessageReceivedEventDispatcher(text, isEndpoint));

        private async Task OnMessageReceivedEventDispatcher(string text, bool isEndpoint)
        {
            if (this.isPorcess)
            {
                return;
            }

            if (this.isReady)
            {
                this.timer.Stop();
                this.timer.Start();
                this.readyClts?.Cancel();

                if (this.lastText == null)
                {
                    this.microphoneTextBlock.Text = text;
                }
                else
                {
                    this.microphoneTextBlock.Text = this.lastText + "," + text;
                }
                text = this.microphoneTextBlock.Text;
            }

            if (!isEndpoint)
            {
                return;
            }

            if (!this.isReady && text.Contains(Crs_SpeechToTextService.Instance.AwakenWord))
            {
                await Crs_SpeechToTextService.Instance.StopAsync();
                await Crs_SpeechToTextService.Instance.StartAsync();
                await StopMessageAsync();

                this.microphoneTextBlock.Text = "Please say";

                this.microphoneNotify.Visibility = Visibility.Visible;
                this.microphonePopup.IsOpen = true;

                this.isReady = true;
                this.timer.Start();
                return;
            }

            if (!this.isReady)
            {
                return;
            }

            this.lastText = text;

            try
            {
                this.readyClts?.Cancel();
                this.readyClts = new CancellationTokenSource();
                var token = this.readyClts.Token;

                await Task.Delay(Crs_SpeechToTextService.Instance.DelayTime, token);
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                this.isPorcess = true;
                this.timer.Stop();
                this.lastText = null;

                this.microphoneTextBlock.Text = "Querying...";

                await Crs_SpeechToTextService.Instance.StopAsync();

                var digitalHumanItem = DigitalHumanItem;
                var patientItem = PatientItem;
                var modeType = ModeType;

                var parameter = new GuideParameter
                {
                    ModuleName = modeType?.ToString(),
                    PatientName = patientItem?.Name,
                    Question = text,
                    DigitalPersonName = digitalHumanItem.Name,
                    PatientSex = patientItem.SexType.ToString(),
                    Relationship = "other",
                };

                this.guideClts?.Cancel();
                this.guideClts = new CancellationTokenSource();
                var token = this.guideClts.Token;

                TaskCompletionSource<(bool status, string msg, JToken data)> _cancelTcs = new();

                var cancelTask = _cancelTcs.Task;
                var guideTask = Crs_GptService.Instance.Guide(parameter, token);

                token.Register(() => _cancelTcs?.TrySetResult((false, null, null)));

                var anyTask = await Task.WhenAny(guideTask, cancelTask);

                _cancelTcs?.TrySetCanceled();
                _cancelTcs = null;

                var (status, msg, data) = await anyTask;
                if (!status)
                {
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                    }
                    return;
                }

                var output = data.Value<string>("output");
                output = output.Replace("\r", string.Empty).Replace("\n", string.Empty);

                this.microphoneTextBlock.Text = output;

                await SendMessageAsyncDispatcher(output);
            }
            catch (Exception ex)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(ex.Message);
                return;
            }
            finally
            {
                this.isPorcess = false;
                ResetSetting();
            }
        }

        public async Task SendMessageAsync(string message)
            => await this.Dispatcher.Invoke(async () => await SendMessageAsyncDispatcher(message));

        private async Task SendMessageAsyncDispatcher(string message)
        {
            try
            {
                await StopMessageAsync();
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }

                while (this.isSend && humanCts != null) await Task.Delay(10, humanCts.Token);

                this.isSend = true;
                this.microphoneNotify.Visibility = Visibility.Visible;

                var (status, msg) = await Crs_HumanToolkit.SendMessageAsync(message);
                WriteLine($"Crs_HumanToolkit.SendMessageAsync->{(status, msg)}");

                (status, msg) = await Crs_HumanToolkit.WaitMediaProcessAsync();
                WriteLine($"Crs_HumanToolkit.WaitMediaProcessAsync->{(status, msg)}");
            }
            catch { }
            finally
            {
                this.microphoneNotify.Visibility = Visibility.Collapsed;
                this.isSend = false;
            }
        }

        public async Task StopMessageAsync()
        {
            this.readyClts?.Cancel();
            this.guideClts?.Cancel();

            var (status, msg) = await Crs_HumanToolkit.SendStopMessageAsync();
            WriteLine($"Crs_HumanToolkit.SendStopMessageAsync->{(status, msg)}");
        }

        private async void MicrophoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.isReady && !this.isSend)
            {
                if (!Crs_SpeechToTextService.Instance.IsInit)
                {
                    var msg = "The service is starting, please wait...";
                    await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                    return;
                }

                string message = Crs_SpeechToTextService.Instance.AwakenWord;
                await OnMessageReceivedEventDispatcher(message, true);

                //await Task.Delay(1000);
                //await OnMessageReceivedEventDispatcher("What should I do", true);
                return;
            }

            await StopMessageAsync();
            ResetSetting();
        }


        public DigitalHumanItem DigitalHumanItem
        {
            get { return (DigitalHumanItem)GetValue(DigitalHumanItemProperty); }
            set { SetValue(DigitalHumanItemProperty, value); }
        }

        public static readonly DependencyProperty DigitalHumanItemProperty =
            DependencyProperty.Register("DigitalHumanItem", typeof(DigitalHumanItem), typeof(SubGamePanel), new PropertyMetadata(null));


        public PatientItem PatientItem
        {
            get { return (PatientItem)GetValue(PatientItemProperty); }
            set { SetValue(PatientItemProperty, value); }
        }

        public static readonly DependencyProperty PatientItemProperty =
            DependencyProperty.Register("PatientItem", typeof(PatientItem), typeof(SubGamePanel), new PropertyMetadata(null));


        public Enum ModeType
        {
            get { return (Enum)GetValue(ModeTypeProperty); }
            set { SetValue(ModeTypeProperty, value); }
        }

        public static readonly DependencyProperty ModeTypeProperty =
            DependencyProperty.Register("ModeType", typeof(Enum), typeof(SubGamePanel), new PropertyMetadata(null));


        public IHumanInterface Instance
        {
            get { return (IHumanInterface)GetValue(InstanceProperty); }
            set { SetValue(InstanceProperty, value); }
        }

        public static readonly DependencyProperty InstanceProperty =
            DependencyProperty.Register("Instance", typeof(IHumanInterface), typeof(SubGamePanel), new PropertyMetadata(null));

    }
}
