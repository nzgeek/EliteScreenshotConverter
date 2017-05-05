using NZgeek.ElitePlayerJournal;
using NZgeek.ElitePlayerJournal.Events;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace NZgeek.EliteScreenshotConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var journal = new Journal();
            journal.Load();

            var screenshots = Enumerable.Reverse(journal.FindEvents(EventType.Screenshot));
            foreach (Screenshot screenshot in screenshots)
            {
                Console.WriteLine("[{0:yyyy/MM/dd HH:mm:ss}] {1}  @  {2}{3}",
                    screenshot.Timestamp,
                    screenshot.FilePath,
                    screenshot.SystemName,
                    !string.IsNullOrEmpty(screenshot.Body) ? $" ({screenshot.Body})" : null);

                ConvertScreenshot(screenshot);
            }
        }

        static void ConvertScreenshot(Screenshot screenshot)
        {
            if (!File.Exists(screenshot.FilePath))
                return;

            var convertedFolder = Path.Combine(screenshot.Journal.ScreenShotFolder, "Converted");
            Directory.CreateDirectory(convertedFolder);

            var convertedFileName = BuildFileName(screenshot);
            var convertedFilePath = Path.Combine(convertedFolder, convertedFileName.ToString());

            using (var sourceImage = new Bitmap(screenshot.FilePath))
            {
                sourceImage.Save(convertedFilePath, ImageFormat.Jpeg);
            }

            File.Delete(screenshot.FilePath);
        }

        static string BuildFileName(Screenshot screenshot)
        {
            var fileName = new StringBuilder();

            if (string.IsNullOrEmpty(screenshot.SystemName))
            {
                fileName.Append("Unknown");
            }
            else if (!string.IsNullOrEmpty(screenshot.Body) && screenshot.SystemName.StartsWith(screenshot.Body, StringComparison.InvariantCulture))
            {
                fileName.Append(screenshot.Body);
            }
            else
            {
                fileName.Append(screenshot.SystemName);
                fileName.Append(" @ ");
                fileName.Append(screenshot.Body);
            }

            fileName.AppendFormat("{0:' ('yyyyMMdd-HHmmss')'}", screenshot.Timestamp);

            if (screenshot.FileName.StartsWith("HighRes", StringComparison.InvariantCultureIgnoreCase))
            {
                fileName.Append(" (HighRes)");
            }

            fileName.Append(".jpg");

            return fileName.ToString();
        }
    }
}
