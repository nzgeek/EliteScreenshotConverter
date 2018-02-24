using NZgeek.ElitePlayerJournal;
using NZgeek.ElitePlayerJournal.Events;
using NZgeek.ElitePlayerJournal.Events.Exploration;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NZgeek.EliteScreenshotConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var journal = new Journal();
            journal.Load();

            var startDate = ParseStartDateArgument(args);
            var endDate = ParseEndDateArgument(args);

            var screenshotEvents = Enumerable.Reverse(journal.FindEvents(startDate, endDate, EventType.Screenshot));
            foreach (Screenshot screenshotEvent in screenshotEvents)
            {
                Console.WriteLine("[{0:yyyy/MM/dd HH:mm:ss}] {1}  @  {2}{3}",
                    screenshotEvent.Timestamp,
                    screenshotEvent.FilePath,
                    screenshotEvent.SystemName,
                    !string.IsNullOrEmpty(screenshotEvent.Body) ? $" ({screenshotEvent.Body})" : null);

                ConvertScreenshot(screenshotEvent);
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static DateTime ParseEndDateArgument(string[] args)
        {
            if (args.Length < 2) return DateTime.MaxValue;

            if (DateTime.TryParse(args[1], out DateTime date))
            {
                return date;
            }

            return DateTime.MaxValue;
        }

        private static DateTime ParseStartDateArgument(string[] args)
        {
            if (args.Length < 1) return DateTime.MinValue;

            if (DateTime.TryParse(args[0], out DateTime date))
            {
                return date;
            }

            return DateTime.MinValue;
        }

        static void ConvertScreenshot(Screenshot screenshotEvent)
        {
            if (!File.Exists(screenshotEvent.FilePath))
                return;

            var convertedFolder = Path.Combine(screenshotEvent.Journal.ScreenShotFolder, "Converted");
            Directory.CreateDirectory(convertedFolder);

            var convertedFileName = BuildFileName(screenshotEvent);
            var convertedFilePath = Path.Combine(convertedFolder, convertedFileName.ToString());

            using (var sourceImage = new Bitmap(screenshotEvent.FilePath))
            {
                sourceImage.Save(convertedFilePath, ImageFormat.Jpeg);
            }

            File.Delete(screenshotEvent.FilePath);

            if (screenshotEvent.Timestamp != DateTime.MinValue)
            {
                File.SetCreationTime(convertedFilePath, screenshotEvent.Timestamp);
            }
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
                var bodyName = screenshot.Body?.Replace(screenshot.SystemName, "");
                fileName.Append(bodyName);
            }

            fileName.AppendFormat("{0:' ('yyyyMMdd-HHmmss')'}", screenshot.Timestamp);

            if (screenshot.FileName.StartsWith("HighRes", StringComparison.InvariantCultureIgnoreCase))
            {
                fileName.Append(" (HighRes)");
            }

            fileName.Append(".jpg");

            return fileName.ToString().Replace("  ", " ");
        }
    }
}
