﻿using X.PagedList;

namespace Shrooms.Contracts.ViewModels
{
    public class PagedViewModel<T>
        where T : class
    {
        public IPagedList<T> PagedList { get; set; }

        public int PageCount { get; set; }

        public int ItemCount { get; set; }

        public int PageSize { get; set; }
    }
}