using System.Diagnostics;

namespace API.Service
{
    public static class VideoFrameExtractor
    {
        public static List<string> ExtractFrames(string videoPath, double frameRate = 0.2)
        {
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            string outputPattern = Path.Combine(outputDir, "frame_%03d.jpg");
            string args = $"-i \"{videoPath}\" -vf fps={frameRate} \"{outputPattern}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();

            return Directory.GetFiles(outputDir, "frame_*.jpg").ToList();
        }
    }
}
