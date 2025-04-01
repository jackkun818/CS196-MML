using crs.extension;
using System.Windows.Controls;
using crs.theme.Extensions;
using System.Windows.Input;
using System.Windows;
using crs.dialog.ViewModels;
using Spire.Pdf;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for MmseReport
    /// </summary>
    public partial class MmseReport : UserControl
    {
        public MmseReport()
        {
            InitializeComponent();
        }

        private async void SimplePanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Non-interactive area, please try to click the return button");
        }


        async void ExportPDFButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToTop();
            await Task.Delay(30);

            string filePath_XPS = "ExportPDF\\XPS";
            string filePath_PDF = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Power Report";
            if (!Directory.Exists(filePath_XPS))
            {
                Directory.CreateDirectory(filePath_XPS);
            }
            if (!Directory.Exists(filePath_PDF))
            {
                Directory.CreateDirectory(filePath_PDF);
            }

            string xpsFilePath = filePath_XPS + $"\\{(DataContext as MmseReportViewModel).DateTime.Substring(0, 10)}_{(DataContext as MmseReportViewModel).ModuleItem.Name}.xps";
            string pdfFilePath = filePath_PDF + $"\\{(DataContext as MmseReportViewModel).DateTime.Substring(0, 10)}_{(DataContext as MmseReportViewModel).ModuleItem.Name}.pdf";
            xpsFilePath = CheckFileNameExist(xpsFilePath);
            pdfFilePath = CheckFileNameExist(pdfFilePath);

            XpsDocument xpsDoc = new XpsDocument(xpsFilePath, FileAccess.ReadWrite);
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);

            UIElement element = page as UIElement;
            // Render the control asFixedPage 
            //renderBitmapThe width is manually set to 1550
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                1550, (int)element.DesiredSize.Height + 100, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(element);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));


            //Get the title prefix
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

                // Create aFixedPageand add it toXPSIn the document
                FixedPage fixedPage = new FixedPage();
                fixedPage.Width = renderBitmap.Width;
                fixedPage.Height = renderBitmap.Height;

                // Create aStackPanelAs a layout container
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical, // Set to vertical arrangement
                    Width = renderBitmap.Width,
                    Height = renderBitmap.Height + 100
                };

                if (!string.IsNullOrWhiteSpace(evaluateReportTitlePrefix))
                {
                    // Add the first oneTextBlock
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


                // Create aBordercontainer
                Border border = new Border
                {
                    Width = 1470,
                    BorderThickness = new Thickness(0, 0, 0, 2), // Set the border thickness
                    BorderBrush = System.Windows.Media.Brushes.Black, // Set border color
                    Padding = new Thickness(10), // Set internal fill
                    Background = System.Windows.Media.Brushes.White, // Set background color
                };

                // Add a secondTextBlockarriveBordermiddle
                TextBlock textBlock2 = new TextBlock
                {
                    Text = "MMSEScale Report",
                    FontSize = 36,
                    Foreground = System.Windows.Media.Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                border.Child = textBlock2; // WillTextBlockAdd toBordermiddle

                // WillBorderAdd toStackPanel
                stackPanel.Children.Add(border);

                // WillPNGImage asImageAdd toStackPanelmiddle
                System.Windows.Controls.Image image = new System.Windows.Controls.Image
                {
                    Source = renderBitmap,
                    Stretch = Stretch.None
                };
                stackPanel.Children.Add(image);


                // WillStackPanelAdd toFixedPage
                fixedPage.Children.Add(stackPanel);

                // WillFixedPageWriteXPSdocument
                writer.Write(fixedPage);
            }

            xpsDoc.Close();
            PdfDocument pdf = new PdfDocument();
            pdf.LoadFromFile(xpsFilePath, FileFormat.XPS);
            pdf.ConvertOptions.SetXpsToPdfOptions(true);
            pdf.SaveToFile(pdfFilePath, FileFormat.PDF);

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("The report has been exported, please“This computer-document-Power Report”View in.");
        }

        string CheckFileNameExist(string filePath)
        {
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // The file exists, add the path(1)suffix
                string directoryPath = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);
                string newFileName = $"{fileNameWithoutExtension}(1){fileExtension}";
                string newFilePath = Path.Combine(directoryPath, newFileName);

                // Check if the new file name already exists, and if so, continue to add incremental numeric suffix
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
