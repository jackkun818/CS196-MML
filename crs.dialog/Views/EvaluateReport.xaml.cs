using crs.extension;
using System.Windows.Controls;
using crs.theme.Extensions;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Documents;
using Spire.Pdf;
using crs.dialog.ViewModels;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using Microsoft.Identity.Client.NativeInterop;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for EvaluateReport
    /// </summary>
    public partial class EvaluateReport : UserControl
    {
        public EvaluateReport()
        {
            InitializeComponent();
        }

        private async void SimplePanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("非交互区域，请尝试点击返回按钮");
        }

        private void DataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;

                // 激发一个鼠标滚轮事件
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;

                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }


        async void ExportPDFButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToTop();

            await Task.Delay(30);

            string filePath_XPS = "ExportPDF\\XPS";
            string filePath_PDF = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\力之报告";
            if (!Directory.Exists(filePath_XPS))
            {
                Directory.CreateDirectory(filePath_XPS);
            }
            if (!Directory.Exists(filePath_PDF))
            {
                Directory.CreateDirectory(filePath_PDF);
            }

            string xpsFilePath = filePath_XPS + $"\\{(DataContext as EvaluateReportViewModel).DateTime.Substring(0, 10)}_{(DataContext as EvaluateReportViewModel).ModuleItem.Name}.xps";
            string pdfFilePath = filePath_PDF + $"\\{(DataContext as EvaluateReportViewModel).DateTime.Substring(0, 10)}_{(DataContext as EvaluateReportViewModel).ModuleItem.Name}.pdf";
            xpsFilePath = CheckFileNameExist(xpsFilePath);
            pdfFilePath = CheckFileNameExist(pdfFilePath);

            XpsDocument xpsDoc = new XpsDocument(xpsFilePath, FileAccess.ReadWrite);
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);


            UIElement element = page as UIElement;
            // 将控件渲染为FixedPage 
            //renderBitmap的宽度手动设置为1550
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                1550, (int)element.DesiredSize.Height + 100, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(element);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));


            //获取标题前缀
            string configPath = @".\configs\reportTitlePrefix.json";
            string evaluateReportTitlePrefix = "";
            string trainReportTitlePrefix = "";
            if (File.Exists(configPath))
            {
                using var fileReader = File.OpenText(configPath);
                using var reader = new JsonTextReader(fileReader);

                var token = JToken.ReadFrom(reader);

                evaluateReportTitlePrefix = token.Value<string>("evaluateReportTitlePrefix");
                trainReportTitlePrefix = token.Value<string>("trainReportTitlePrefix");
            }

            using (Stream stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0;

                // 创建一个FixedPage，并将其添加到XPS文档中
                FixedPage fixedPage = new FixedPage();
                fixedPage.Width = renderBitmap.Width;
                fixedPage.Height = renderBitmap.Height;

                // 创建一个StackPanel作为布局容器
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical, // 设置为垂直排列
                    Width = renderBitmap.Width,
                    Height = renderBitmap.Height + 100
                };

                if (!string.IsNullOrWhiteSpace(evaluateReportTitlePrefix))
                {
                    // 添加第一个TextBlock
                    TextBlock textBlock1 = new TextBlock
                    {
                        Text = evaluateReportTitlePrefix,
                        FontSize = 54,
                        Foreground = System.Windows.Media.Brushes.Black,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0),
                    };
                    stackPanel.Children.Add(textBlock1);
                }


                // 创建一个Border容器
                Border border = new Border
                {
                    Width = 1470,
                    BorderThickness = new Thickness(0, 0, 0, 2), // 设置边框厚度
                    BorderBrush = System.Windows.Media.Brushes.Black, // 设置边框颜色
                    Padding = new Thickness(10), // 设置内部填充
                    Background = System.Windows.Media.Brushes.White, // 设置背景颜色
                };

                // 添加第二个TextBlock到Border中
                TextBlock textBlock2 = new TextBlock
                {
                    Text = "评估报告",
                    FontSize = 36,
                    Foreground = System.Windows.Media.Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                border.Child = textBlock2; // 将TextBlock添加到Border中

                // 将Border添加到StackPanel
                stackPanel.Children.Add(border);

                // 将PNG图像作为Image添加到StackPanel中
                System.Windows.Controls.Image image = new System.Windows.Controls.Image
                {
                    Source = renderBitmap,
                    Stretch = Stretch.None
                };
                stackPanel.Children.Add(image);


                // 将StackPanel添加到FixedPage
                fixedPage.Children.Add(stackPanel);

                // 将FixedPage写入XPS文档
                writer.Write(fixedPage);
            }

            xpsDoc.Close();
            PdfDocument pdf = new PdfDocument();
            pdf.LoadFromFile(xpsFilePath, FileFormat.XPS);
            pdf.ConvertOptions.SetXpsToPdfOptions(true);
            pdf.SaveToFile(pdfFilePath, FileFormat.PDF);

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("报告已导出，请在“此电脑-文档-力之报告”中查看。");

        }

        string CheckFileNameExist(string filePath)
        {
            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                // 文件存在，给路径添加(1)后缀
                string directoryPath = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);
                string newFileName = $"{fileNameWithoutExtension}(1){fileExtension}";
                string newFilePath = Path.Combine(directoryPath, newFileName);

                // 检查新文件名是否已存在，如果存在则继续添加递增的数字后缀
                int i = 1;
                while (File.Exists(newFilePath))
                {
                    i++;
                    newFileName = $"{fileNameWithoutExtension}({i}){fileExtension}";
                    newFilePath = Path.Combine(directoryPath, newFileName);
                }
                return newFilePath;
            }
            return filePath;
        }
    }
}
