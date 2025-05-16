using crs.core.Services;
using crs.extension;
using crs.extension.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using static crs.core.Services.Crs_GptService;
using crs.theme.Extensions;
using System.Windows.Interop;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using crs.game;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using HandyControl.Controls;
using crs.theme;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for SubGamePanel
    /// </summary>
    public partial class SubGamePanel : UserControl
    {
        static void WriteLine(string content)
        {
#if DEBUG
            Debug.WriteLine(content);
#endif
        }

        bool isReady = false;
        bool isPorcess = false;
        DispatcherTimer timer;
        CancellationTokenSource readyClts = null;
        string lastText = null;
        CancellationTokenSource guideClts = null;

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
        }

        private async void SubGamePanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.timer?.Stop();
            this.timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Crs_SpeechToTextService.Instance.AwakenTime) };
            this.timer.Tick += Timer_Tick;

            var digitalHumanItem = DigitalHumanItem;
            if (digitalHumanItem == null)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync("数字人信息为空");
                return;
            }

            var (status, msg) = await Crs_SpeechToTextService.Instance.InitAsync();
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
            this.timer?.Stop();

            Crs_SpeechToTextService.Instance.OnMessageReceivedEvent -= OnMessageReceivedEvent;
            _ = await Crs_SpeechToTextService.Instance.StopAsync();
        }

        private void Timer_Tick(object sender, EventArgs e) => ResetSetting();

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

                this.microphoneTextBlock.Text = "您请说";

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

                this.microphoneTextBlock.Text = "正在查询...";

                await Crs_SpeechToTextService.Instance.StopAsync();

                // 捕获当前界面截图
                var screenshot = CaptureScreen();
                if (screenshot == null)
                {
                    Debug.WriteLine("[Screenshot] Failed to capture screen");
                }
                else
                {
                    Debug.WriteLine($"[Screenshot] Captured {screenshot.Length} bytes");
                }
                string base64Image = Convert.ToBase64String(screenshot);
                Debug.WriteLine($"[Screenshot] Base64 length: {base64Image.Length}");
                Debug.WriteLine($"[Screenshot] First 100 chars of base64: {base64Image.Substring(0, Math.Min(100, base64Image.Length))}");

                var digitalHumanItem = DigitalHumanItem;
                var patientItem = PatientItem;
                var modeType = ModeType;

                var parameter = new GuideParameter
                {
                    Question = text,
                    ScreenShot = base64Image  // 添加截图数据
                };

                Debug.WriteLine($"[Request] Sending request with screenshot length: {base64Image.Length}");

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

                var output = data?["output"]?.ToString();
                if (string.IsNullOrEmpty(output))
                {
                    Debug.WriteLine("Warning: Empty response from GPT service");
                    output = "抱歉，没有收到有效的回答。";
                }
                else
                {
                    Debug.WriteLine($"Received GPT response: {output}");
                }

                output = output.Replace("\r", string.Empty).Replace("\n", string.Empty);
                this.microphoneTextBlock.Text = output;
                
                // 确保弹出窗口可见
                this.microphonePopup.IsOpen = true;
                this.microphoneNotify.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing GPT response: {ex.Message}");
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(ex.Message);
                return;
            }
            finally
            {
                this.isPorcess = false;
                // ResetSetting(); // 不再自动关闭弹窗，改为用户手动关闭
            }
        }

        private byte[] CaptureScreen()
        {
            try
            {
                // 获取当前窗口
                var window = System.Windows.Window.GetWindow(this);
                if (window == null)
                {
                    Debug.WriteLine("[Screenshot] Failed to get window");
                    return null;
                }

                Debug.WriteLine($"[Screenshot] Window size: {window.ActualWidth}x{window.ActualHeight}");

                // 创建截图
                var renderTargetBitmap = new RenderTargetBitmap(
                    (int)window.ActualWidth,
                    (int)window.ActualHeight,
                    96, 96, PixelFormats.Pbgra32);

                // 渲染整个窗口
                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    var brush = new VisualBrush(window);
                    drawingContext.DrawRectangle(brush, null, new Rect(0, 0, window.ActualWidth, window.ActualHeight));
                }
                renderTargetBitmap.Render(visual);

                // 转换为PNG格式
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (var stream = new MemoryStream())
                {
                    pngEncoder.Save(stream);
                    var bytes = stream.ToArray();
                    Debug.WriteLine($"[Screenshot] Successfully captured {bytes.Length} bytes");
                    return bytes;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Screenshot] Error capturing screen: {ex.Message}");
                Debug.WriteLine($"[Screenshot] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private async void MicrophoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.isReady)
            {
                if (!Crs_SpeechToTextService.Instance.IsInit)
                {
                    var msg = "服务正在启动中，请稍后...";
                    await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                    return;
                }

                string message = Crs_SpeechToTextService.Instance.AwakenWord;
                await OnMessageReceivedEventDispatcher(message, true);
                return;
            }

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
    }
}
