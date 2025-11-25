using System;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class TeacherDto
    {
        public string Id { get; set; }
        public string Name { get; set; }  // or Username/FullName
        public string Email { get; set; }
    }

    public class StudentProfileViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; } // display name
        public string AvatarPath { get; set; }
        public string GroupId { get; set; }
        public List<TeacherDto> Teachers { get; set; } = new List<TeacherDto>();

        // stats
        public int TotalQuizzes { get; set; }
        public int TotalTps { get; set; }
        public int CompletedQuizzes { get; set; }
        public int CompletedTps { get; set; }
    }

    public class ProfileUpdateModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
