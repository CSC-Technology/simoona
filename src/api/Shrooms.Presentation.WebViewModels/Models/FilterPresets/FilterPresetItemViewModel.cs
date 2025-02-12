﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Enums;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class FilterPresetItemViewModel
    {
        [Required]
        [EnumDataType(typeof(FilterType))]
        public FilterType FilterType { get; set; }

        [Required]
        public IEnumerable<string> Types { get; set; }
    }
}
