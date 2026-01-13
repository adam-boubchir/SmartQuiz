using SmartQuiz.Services;
using System.Diagnostics;

public class OllamaService : IOllamaService
{
    public async Task<string> GenerateTextAsync(string prompt)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ollama",
            Arguments = "run llama3",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);

        await process.StandardInput.WriteAsync(prompt);
        process.StandardInput.Close();

        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return output;
    }
}
