﻿using System;
using System.Linq.Expressions;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Presentation.ModelMappings
{
    internal static class Helpers
    {
        public static IMappingExpression<TSrc, TDest> Ignore<TSrc, TDest>(this IMappingExpression<TSrc, TDest> expr, Expression<Func<TDest, object>> selection)
        {
            return expr.ForMember(selection, opt => opt.Ignore());
        }

        public static IMappingExpression<TSrc, TDest> IgnoreUserOrgDto<TSrc, TDest>(this IMappingExpression<TSrc, TDest> expr) where TDest : UserAndOrganizationDto
        {
            return expr
                .ForMember(x => x.OrganizationId, opt => opt.Ignore())
                .ForMember(x => x.UserId, opt => opt.Ignore());
        }
    }
}
