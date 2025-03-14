using System.Diagnostics;
using System.Text;
using PsihoApi.Helpers;

namespace PsihoApi.Services
{
    public class AIService : IPsihoServiceAI, IDisposable
    {
        private readonly string _llamaPath = @"C:\zzz\zzz\ai\llama.cpp\build\bin\Release\llama-cli.exe";
        private readonly string _modelPath = @"C:\zzz\zzz\ai\llama.cpp\models\mistral-7b-deepseek.gguf";
        private Process _llamaProcess;
        private StreamWriter _processWriter;
        private StreamReader _llamaInitReader;
        private StreamReader _responseReader;

        public AIService()
        {
            StartLlamaProcess();
        }

        private void StartLlamaProcess()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _llamaPath,
                Arguments = $"-m \"{_modelPath}\"  --interactive-first",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true, // 🔹 Capture errors (LLaMA might print system messages here)
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _llamaProcess = Process.Start(startInfo);
            if (_llamaProcess == null)
            {
                throw new Exception("Eroare la pornirea AI-ului.");
            }

            _processWriter = _llamaProcess.StandardInput;
            _llamaInitReader = _llamaProcess.StandardError; // 🔹 LLaMA prints everything here
            _responseReader = _llamaProcess.StandardOutput; // 🔹 LLaMA prints responses

            Debug.WriteLine($"PROCESS pornit... Așteptăm ca LLaMA să fie gata...");

            // 🔹 Wait for LLaMA to print the first prompt
            WaitForLlamaReady();
            Debug.WriteLine($"Llama Ready to INIT");

            string systemPrompt = "Ești un psiholog virtual. Răspunde DOAR în română fara sa repeti ce iti trimite utilizatorul, doar ofera raspunsuri simple si asertive. Scrie 'OK' dacă ai înțeles rolul tău, fara alte explicatii.";
            _processWriter.WriteLine(systemPrompt);
            _processWriter.Flush();

            // 🔹 Wait for "OK" confirmation from LLaMA
            WaitForLlamaAck();
            Debug.WriteLine("✅ LLaMA INIT OK");
            
            Task.Run(() =>
            {
                _llamaProcess.WaitForExit();
                Debug.WriteLine($"❌ LLaMA S-A ÎNCHIS CU COD: {_llamaProcess.ExitCode}");
            });
        }

        private void WaitForLlamaReady()
        {
            try
            {
                string line;
                while ((line = _llamaInitReader.ReadLine()) != null)
                {
                    Debug.WriteLine($"[LLaMA WaitForLlamaReady]: {line}");
                    if (string.IsNullOrWhiteSpace(line)) 
                        break; // 🔹 LLaMA signals readiness with an empty line
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERROR Waiting for LLaMA to Start: {ex.Message}");
            }
        }

        private void WaitForLlamaAck()
        {
            try
            {
                string line;
                bool isOk = false;
                while ((line = _responseReader.ReadLine()) != null)
                {
                    Debug.WriteLine($"[LLaMA Ack]: {line}");
                    
                    if (line.ToLowerInvariant().Contains("ok")) isOk = true;

                    if (isOk && string.IsNullOrWhiteSpace(line)) // 🔹 LLaMA said he acknowledges he is a psiholog and signals readiness with an empty line
                        break; 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERROR Waiting for LLaMA to Start: {ex.Message}");
            }
        }

        public async Task<string> AnalyzeText(string userText, bool newConversation = false)
        {
            if (_llamaProcess == null || _llamaProcess.HasExited)
            {
                throw new Exception("LLaMA nu este pornit.");
            }

            if (newConversation)
            {
                RestartLlamaProcess();
            }

            Debug.WriteLine($"USER INPUT : {userText}");

            // 🔹 Send user message to LLaMA
            _processWriter.WriteLine(userText);
            _processWriter.Flush();

            StringBuilder response = new StringBuilder();
            string line;

            while ((line = await _responseReader.ReadLineAsync()) != string.Empty)
            {
                Debug.WriteLine($"[LLaMA ResponseToUserQ]: {line}");
                response.AppendLine(line?.ToRomanianDiacritics());
            }
            return response.ToString().Trim();
        }

        private void RestartLlamaProcess()
        {
            Dispose();
            StartLlamaProcess();
        }

        public void Dispose()
        {
            if (_llamaProcess != null && !_llamaProcess.HasExited)
            {
                _processWriter.Close();
                _responseReader.Close();
                _llamaInitReader.Close();
                _llamaProcess.Kill();
                _llamaProcess.Dispose();
            }
        }
    }
}
