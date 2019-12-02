﻿namespace Shrooms.DomainServiceValidators.Validators.Books
{
    public interface IBookServiceValidator
    {
        void CheckIfBookAlreadyExists(bool alreadyExists);
        void CheckIfBookAllQuantitiesAreNotZero(bool allQuantitiesNotZero);
        void CheckIfBookOfficesFoundWhileDeleting(bool booksWereFound);
        void CheckIfRequestedOfficesExist(bool officeIsValid);
        void CheckIfBookWasFoundByIsbnFromExternalProvider(object book);
        void ThrowIfBookCannotBeReturned(bool bookExists);
    }
}
