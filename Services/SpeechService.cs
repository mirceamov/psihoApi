using NAudio.Wave;
using PsihoApi.Helpers;
using System.Diagnostics;

namespace PsihoApi.Services
{
    public class SpeechService
    {
        private readonly string _whisperPath = @"C:\zzz\zzz\ai\whisper.cpp\build\bin\Release\whisper-cli.exe";
        private readonly string _modelPath = @"C:\zzz\zzz\ai\whisper.cpp\models\ggml-medium-q8_0.bin";
        private readonly string _audioFile = "recorded.wav";

        public async Task<string> RecordAndTranscribe()
        {
            Debug.WriteLine("Start recording");
            await RecordAudioAsync(_audioFile, 5);
            Debug.WriteLine("Finish recording. Starting the text extraction...");

            Debug.WriteLine("Starting the text extraction...");
            string transcript = await RunWhisperAsync(_whisperPath, _modelPath, _audioFile, "ro");
            Debug.WriteLine("Finish extracting text from speech : ");
            Debug.WriteLine(transcript);

            return transcript;
        }

        public async Task<string> TranslateText()
        {
            return await RunWhisperAsync(_whisperPath, _modelPath, _audioFile, "en", true);
        }

        async Task RecordAudioAsync(string outputFile, int seconds)
        {
            using var waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };
            using var writer = new WaveFileWriter(outputFile, waveIn.WaveFormat);

            waveIn.DataAvailable += (s, e) => writer.Write(e.Buffer, 0, e.BytesRecorded);

            waveIn.StartRecording();
            await Task.Delay(seconds * 1000);
            waveIn.StopRecording();
        }

        async Task<string> RunWhisperAsync(string whisperExe, string modelPath, string audioFile, string language, bool translate = false)
        {
            string taskOption = translate ? " --translate" : "";
            var startInfo = new ProcessStartInfo
            {
                FileName = whisperExe,
                Arguments = $"-m \"{modelPath}\" -f \"{audioFile}\" --language {language} --threads 8 {taskOption}",
                RedirectStandardOutput = true,
                RedirectStandardError = true, // Capturăm și erorile
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Debug.WriteLine("❌ Eroare la pornirea Whisper.");
                return "Eroare la pornirea Whisper.";
            }

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            /*
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("⚠️ Whisper ERROR: " + error);
            }
            */


            return string.IsNullOrEmpty(output) ? "⚠️ Whisper nu a returnat nimic!" : CleanTranscript(output);
        }

        private string CleanTranscript(string transcript)
        {
            // Remove timestamps
            transcript = System.Text.RegularExpressions.Regex.Replace(transcript, @"\[\d+:\d+:\d+\.\d+ --> \d+:\d+:\d+\.\d+\]", "").Trim();

            // Apply diacritics correction
            return transcript.ToRomanianDiacritics();
        }
    }
}
