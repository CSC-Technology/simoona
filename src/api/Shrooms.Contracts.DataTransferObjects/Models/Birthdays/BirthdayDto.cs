﻿namespace Shrooms.Contracts.DataTransferObjects.Models.Birthdays
{
    public class BirthdayDto
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DayOfWeek { get; set; }

        public string DateString { get; set; }

        public string PictureId { get; set; }
    }
}
