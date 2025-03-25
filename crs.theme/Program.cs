using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace crs.theme
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.FirstOrDefault(m => m == "DEBUG") == null)
            {
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyPath = new DirectoryInfo(assembly.Location).Parent.Parent.Parent.Parent;

            var path = @$"{assemblyPath}\Resources\Images";
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);

                var target = @$"{assemblyPath}\Themes\BitmapImage.xaml";
                if (File.Exists(target))
                {
                    using StreamWriter streamWriter = new StreamWriter(target);

                    streamWriter.WriteLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:po=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options\">");
                    streamWriter.WriteLine();

                    foreach (var item in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(item);

                        var templateString = $"<BitmapImage x:Key=\"crs_{fileName}\" po:Freeze=\"True\" UriSource=\"/crs.theme;component/Resources/Images/{fileName}.png\" />";
                        streamWriter.WriteLine(templateString);
                        streamWriter.WriteLine();
                    }

                    streamWriter.WriteLine("</ResourceDictionary>");
                }
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"crs.theme.Resources.Colors.txt"))
            using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
            {
                var target = @$"{assemblyPath}\Themes\Color.xaml";
                if (File.Exists(target))
                {
                    using StreamWriter streamWriter = new StreamWriter(target);

                    streamWriter.WriteLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:po=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options\">");
                    streamWriter.WriteLine();

                    string content;
                    while (!string.IsNullOrWhiteSpace((content = reader.ReadLine()?.Trim())))
                    {
                        var key = new String(content.Skip(1).ToArray());

                        var templateString = $"<Color x:Key=\"crs_Color.{key}\" po:Freeze=\"True\">{content}</Color>";
                        streamWriter.WriteLine(templateString);
                        streamWriter.WriteLine();

                        templateString = $"<SolidColorBrush x:Key=\"crs_SolidColorBrush.{key}\" po:Freeze=\"True\" Color=\"{{StaticResource crs_Color.{key}}}\" />";
                        streamWriter.WriteLine(templateString);
                        streamWriter.WriteLine();
                    }

                    streamWriter.WriteLine("</ResourceDictionary>");
                }
            }
        }
    }
}
