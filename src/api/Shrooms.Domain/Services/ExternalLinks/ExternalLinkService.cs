﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;

namespace Shrooms.Domain.Services.ExternalLinks
{
    public class ExternalLinkService : IExternalLinkService
    {
        private readonly IDbSet<ExternalLink> _externalLinkDbSet;
        private readonly IUnitOfWork2 _uow;

        public ExternalLinkService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _externalLinkDbSet = uow.GetDbSet<ExternalLink>();
        }

        public IEnumerable<ExternalLinkDTO> GetAll(int organizationId)
        {
            var externalLinks = _externalLinkDbSet
                .Where(x => x.OrganizationId == organizationId)
                .Select(x => new ExternalLinkDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Url = x.Url
                })
                .ToList();

            return externalLinks;
        }

        public void UpdateLinks(AddEditDeleteExternalLinkDTO updateLinksDto)
        {
            var timestamp = DateTime.UtcNow;
            DuplicateValuesValidation(updateLinksDto);

            UpdateLinks(updateLinksDto, timestamp);
            DeleteLinks(updateLinksDto, timestamp);
            CreateNewLinks(updateLinksDto, timestamp);
        }

        private void DuplicateValuesValidation(AddEditDeleteExternalLinkDTO updateLinksDto)
        {
            var externalLinks = _externalLinkDbSet
                .Where(x => x.OrganizationId == updateLinksDto.OrganizationId)
                .Select(x => new ExternalLinkDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Url = x.Url
                })
                .ToList();

            if (updateLinksDto.LinksToCreate.Any(c => externalLinks.Any(l => l.Name == c.Name || l.Url == c.Url)) ||
                updateLinksDto.LinksToUpdate.Any(c => externalLinks.Any(l => l.Name == c.Name || l.Url == c.Url)))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Provided values must be unique");
            }
        }

        private void CreateNewLinks(AddEditDeleteExternalLinkDTO updateLinks, DateTime timestamp)
        {
            foreach (var link in updateLinks.LinksToCreate)
            {
                var newLink = new ExternalLink
                {
                    Name = link.Name,
                    Url = link.Url,
                    OrganizationId = updateLinks.OrganizationId,
                    Created = timestamp,
                    CreatedBy = updateLinks.UserId,
                    Modified = timestamp
                };

                _externalLinkDbSet.Add(newLink);
            }

            _uow.SaveChanges(false);
        }

        private void DeleteLinks(AddEditDeleteExternalLinkDTO updateLinks, DateTime timestamp)
        {
            var linksToDelete = _externalLinkDbSet
                .Where(l =>
                    updateLinks.LinksToDelete.Contains(l.Id) &&
                    l.OrganizationId == updateLinks.OrganizationId)
                .ToList();
            foreach (var link in linksToDelete)
            {
                link.UpdateMetadata(updateLinks.UserId, timestamp);
            }

            _uow.SaveChanges(false);
            foreach (var link in linksToDelete)
            {
                _externalLinkDbSet.Remove(link);
            }
        }

        private void UpdateLinks(AddEditDeleteExternalLinkDTO updateLinks, DateTime timestamp)
        {
            var updatedLinksIds = updateLinks.LinksToUpdate.Select(l => l.Id);
            var linksToUpdate = _externalLinkDbSet
                .Where(l => updatedLinksIds.Contains(l.Id) && l.OrganizationId == updateLinks.OrganizationId)
                .ToList();
            foreach (var updatedLink in updateLinks.LinksToUpdate)
            {
                var link = linksToUpdate.First(l => l.Id == updatedLink.Id);

                link.Name = updatedLink.Name;
                link.Url = updatedLink.Url;
                link.Modified = timestamp;
                link.ModifiedBy = updateLinks.UserId;
            }
        }
    }
}
