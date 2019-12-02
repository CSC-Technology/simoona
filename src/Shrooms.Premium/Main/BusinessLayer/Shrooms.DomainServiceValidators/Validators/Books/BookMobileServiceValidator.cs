﻿using System.Collections.Generic;
using System.Linq;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DomainExceptions.Exceptions.Book;
using Shrooms.EntityModels.Models;
using Resources = Shrooms.Resources.Models.Books;

namespace Shrooms.DomainServiceValidators.Validators.Books
{
    public class BookMobileServiceValidator : IBookMobileServiceValidator
    {
        public void ThrowIfBookExist(bool bookExists)
        {
            if (bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.BookAlreadyExist);
            }
        }

        public void ThrowIfBookDoesNotExist(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoBook);
            }
        }

        public void ThrowIfThereIsNoBookToReturn(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoBooksToReturn);
            }
        }

        public void ChecksIfUserHasAlreadyBorrowedSameBook(IEnumerable<string> borrowedBookUserIds, string applicationUserId)
        {
            if (borrowedBookUserIds != null)
            {
                var bookAlreadyBorrowed = borrowedBookUserIds.Contains(applicationUserId);
                if (bookAlreadyBorrowed)
                {
                    throw new BookException(Resources.Models.Books.Books.UserAlreadyHasSameBook);
                }
            }
        }

        public void ThrowIfOfficeDoesNotExist(bool officeExists)
        {
            if (!officeExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoOffice);
            }
        }

        public void ThrowIfBookIsAlreadyBorrowed(MobileBookOfficeLogsDTO officeBookWithLogs)
        {
            if (officeBookWithLogs.LogsUserIDs != null)
            {
                var availableBooks = officeBookWithLogs.Quantity - officeBookWithLogs.LogsUserIDs.Count();

                if (availableBooks < 1)
                {
                    throw new BookException(Resources.Models.Books.Books.NoAvailableBooks);
                }
            }
        }

        public void ThrowIfUserDoesNotExist(ApplicationUser applicationUser)
        {
            if (applicationUser == null)
            {
                throw new BookException(Resources.Models.Books.Books.UserDoesNotExist);
            }
        }

        public void ThrowIfBookDoesNotExistGoogleAPI(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoBooksInGoogleApi);
            }
        }
    }
}
