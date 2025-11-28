using System.Collections.Generic;

namespace SmartQuiz.Models;

public class EditStudentViewModel
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }

    public string? SelectedGroupId { get; set; }
   public List<GroupViewModel> Groups { get; set; } = new();

}
