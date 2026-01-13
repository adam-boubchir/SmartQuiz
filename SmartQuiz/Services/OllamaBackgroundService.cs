using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class OllamaBackgroundService : IHostedService
    {
        private Process _ollamaProcess;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Vérifier si Ollama est déjà lancé
            if (Process.GetProcessesByName("ollama").Length > 0)
                return Task.CompletedTask;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ollama",
                    Arguments = "serve",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                };

                _ollamaProcess = Process.Start(psi);
            }
            catch
            {
                // On ignore l'erreur si ollama n'est pas trouvé (évite crash)
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_ollamaProcess != null && !_ollamaProcess.HasExited)
                    _ollamaProcess.Kill();
            }
            catch { }

            return Task.CompletedTask;
        }
    }
}
