using System.Threading.Tasks;

namespace SmartQuiz.Services
{
	public interface IOllamaService
	{
		Task<string> GenerateTextAsync(string prompt);
	}
}
