﻿using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.Birthdays
{
    public class UserBirthdayInfoDto
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDay { get; set; }

        public string PictureId { get; set; }
    }
}
