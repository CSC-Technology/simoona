﻿using System;
using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.Models.BlacklistUsers;
using Shrooms.Presentation.WebViewModels.Models.Certificate;
using Shrooms.Presentation.WebViewModels.Models.Exam;
using Shrooms.Presentation.WebViewModels.Models.Projects;
using Shrooms.Presentation.WebViewModels.Models.Skill;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserDetailsViewModel
    {
        public string Username { get; set; }

        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? BirthDay { get; set; }

        public string Bio { get; set; }

        public string PictureId { get; set; }

        public string JobTitle { get; set; }

        public ManagerMiniViewModel Manager { get; set; }

        public IEnumerable<ProjectsBasicInfoViewModel> Projects { get; set; }

        public IEnumerable<SkillMiniViewModel> Skills { get; set; }

        public QualificationLevelMiniViewModel QualificationLevel { get; set; }

        public IEnumerable<CertificateMiniViewModel> Certificates { get; set; }

        public IEnumerable<ExamMiniViewModel> Exams { get; set; }

        public IEnumerable<ApplicationRoleMiniViewModel> Roles { get; set; }

        public BlacklistUserViewModel BlacklistEntry { get; set; }

        public bool UserBlacklistedMoreThanOnce { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public WorkingHoursViewModel WorkingHours { get; set; }

        public int? RoomId { get; set; }

        public RoomMiniViewModel Room { get; set; }

        public TimeSpan? DailyMailingHour { get; set; }
    }
}