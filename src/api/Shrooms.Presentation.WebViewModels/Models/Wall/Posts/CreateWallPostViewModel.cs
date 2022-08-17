﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Posts
{
    public class CreateWallPostViewModel
    {
        [Required]
        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string MessageBody { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int WallId { get; set; }

        public IEnumerable<string> Images { get; set; }

        public IEnumerable<string> MentionedUserIds { get; set; }
    }
}
