﻿using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Books.v1;
using Google.Apis.Services;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Infrastructure.GoogleBookApiService
{
    public class GoogleBookService : IBookInfoService
    {
        public async Task<ExternalBookInfo> FindBookByIsbnAsync(string isbn)
        {
            var query = $"isbn={isbn}";
            return await RequestBookAsync(query);
        }

        private async Task<ExternalBookInfo> RequestBookAsync(string query)
        {
            var service = new BooksService(new BaseClientService.Initializer
            {
                ApiKey = ConfigurationManager.AppSettings["GoogleAccountApiKey"]
            });

            var result = await service.Volumes.List(query).ExecuteAsync();
            if (result.Items == null)
            {
                return null;
            }

            var volume = result.Items.First().VolumeInfo;
            return new ExternalBookInfo
            {
                Author = volume.Authors == null ? "Authors not set" : string.Join(", ", volume.Authors),
                Title = volume.Title,
                Url = volume.InfoLink,
                CoverImageUrl = volume.ImageLinks.Thumbnail.Replace("zoom=1", "zoom=3")
            };
        }
    }
}
