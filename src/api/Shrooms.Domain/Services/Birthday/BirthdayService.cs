﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.Birthdays;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;

namespace Shrooms.Domain.Services.Birthday
{
    public class BirthdayService : IBirthdayService
    {
        private const int SevenDays = 7;
        private const int OneDay = 1;
        private readonly IDbSet<ApplicationUser> _userDbSet;
        private readonly IRoleService _roleService;

        public BirthdayService(IUnitOfWork2 uow, IRoleService roleService)
        {
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _roleService = roleService;
        }

        public async Task<IEnumerable<BirthdayDTO>> GetWeeklyBirthdaysAsync(DateTime date)
        {
            var firstDayOfTheWeek = date
                .AddDays(-(OneDay + (date.DayOfWeek != DayOfWeek.Saturday ? (int)date.DayOfWeek : -OneDay)))
                .Date;

            var lastDayOfTheWeek = firstDayOfTheWeek.AddDays(SevenDays).Date;

            var userBirthdays = await GetUsersBirthdayInfoAsync(firstDayOfTheWeek, lastDayOfTheWeek);

            return userBirthdays.Select(MapUserBirthdayInfoToBirthdayDto(firstDayOfTheWeek, lastDayOfTheWeek));
        }

        private async Task<IEnumerable<UserBirthdayInfoDTO>> GetUsersBirthdayInfoAsync(DateTime firstDayOfTheWeek, DateTime lastDayOfTheWeek)
        {
            return await _userDbSet
                    .Where(u => u.BirthDay.HasValue)
                    .Where(FilterWeeklyBirthdays(firstDayOfTheWeek, lastDayOfTheWeek))
                    .Where(_roleService.ExcludeUsersWithRole(Contracts.Constants.Roles.NewUser))
                    .OrderByDescending(x => x.BirthDay.Value.Month)
                    .ThenByDescending(x => x.BirthDay.Value.Day)
                    .Select(MapUserBirthdayInfo())
                    .ToListAsync();
        }

        private Expression<Func<ApplicationUser, bool>> FilterWeeklyBirthdays(DateTime firstDayOfTheWeek, DateTime lastDayOfTheWeek)
        {
            if (firstDayOfTheWeek.Month == lastDayOfTheWeek.Month)
            {
                // When all week is in one month e.g from January 1 till January 8
                return x => x.BirthDay.Value.Day > firstDayOfTheWeek.Day
                         && x.BirthDay.Value.Day <= lastDayOfTheWeek.Day
                         && x.BirthDay.Value.Month == lastDayOfTheWeek.Month;
            }

            // When week is separated into two months e.g from January 29 till February 5
            return x => (x.BirthDay.Value.Day > firstDayOfTheWeek.Day
                     && x.BirthDay.Value.Month == firstDayOfTheWeek.Month)
                     || (x.BirthDay.Value.Day <= lastDayOfTheWeek.Day
                     && x.BirthDay.Value.Month == lastDayOfTheWeek.Month);
        }

        private Expression<Func<ApplicationUser, UserBirthdayInfoDTO>> MapUserBirthdayInfo()
        {
            return user => new UserBirthdayInfoDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDay = user.BirthDay,
                PictureId = user.PictureId
            };
        }

        private Func<UserBirthdayInfoDTO, BirthdayDTO> MapUserBirthdayInfoToBirthdayDto(DateTime firstDayOfTheWeek, DateTime lastDayOfTheWeek)
        {
            return userInfo => new BirthdayDTO
            {
                Id = userInfo.Id,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                PictureId = userInfo.PictureId,
                DateString = GetBirthDayCurrentDate(userInfo.BirthDay.Value, GetYear(userInfo.BirthDay.Value, firstDayOfTheWeek, lastDayOfTheWeek)).ToString("yyyy-MM-dd"),
                DayOfWeek = GetBirthDayCurrentDate(userInfo.BirthDay.Value, GetYear(userInfo.BirthDay.Value, firstDayOfTheWeek, lastDayOfTheWeek)).DayOfWeek.ToString()
            };
        }

        /// <summary>
        /// Need to process current birthday for edge cases like leaping year.
        /// </summary>
        private static DateTime GetBirthDayCurrentDate(DateTime userBirthDay, int year)
        {
            var newDate = userBirthDay;
            var birthday = userBirthDay;

            try
            {
                newDate = new DateTime(year, birthday.Month, birthday.Day);
            }
            catch (ArgumentOutOfRangeException)
            {
                if (birthday.Month == 2 && birthday.Day == 29)
                {
                    newDate = new DateTime(year, 3, 1);
                }
            }

            return newDate;
        }

        /// <summary>
        /// When birthday occurs in a week of the end of year. We need to get correct year.
        /// </summary>
        private static int GetYear(DateTime birthDay, DateTime firstDayOfTheWeek, DateTime lastDayOfTheWeek)
        {
            if (firstDayOfTheWeek.Year != lastDayOfTheWeek.Year)
            {
                return birthDay.Month == firstDayOfTheWeek.Month ? firstDayOfTheWeek.Year : lastDayOfTheWeek.Year;
            }

            return firstDayOfTheWeek.Year;
        }
    }
}
