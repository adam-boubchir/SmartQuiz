using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public interface IChatContactService
    {
        Task<List<ContactDto>> GetContactsForUserAsync(string userId);
    }
}
