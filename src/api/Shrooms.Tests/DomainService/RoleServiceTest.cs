﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class RoleServiceTest
    {
        private IRoleService _roleService;
        private IPermissionService _permissionService;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IDbSet<ApplicationRole> _roleDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _roleDbSet = Substitute.For<IDbSet<ApplicationRole>>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            uow.GetDbSet<ApplicationRole>().Returns(_roleDbSet);

            _permissionService = Substitute.For<IPermissionService>();

            _roleService = new RoleService(uow, _permissionService);
        }

        [Test]
        public void Should_Get_Correctly_Mapped_Roles_For_AutoComplete()
        {
            MockRolesForAutocomplete();

            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            var roles = _roleService.GetRolesForAutocomplete("Test1", userOrg).ToList();

            Assert.AreEqual(2, roles.Count());
            Assert.AreEqual("roleId1", roles.ToArray()[0].Id);
            Assert.AreEqual("roleId3", roles.ToArray()[1].Id);
        }

        [Test]
        public async Task Should_Get_Role_With_All_Permissions_And_Users()
        {
            MockRoles();

            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 1
            };

            var roles = await _roleService.GetRoleByIdAsync(userAndOrg, "roleId1");

            Assert.AreEqual("Test1", roles.Name);
            Assert.AreEqual(3, roles.Permissions.Count());
            Assert.AreEqual(PermissionScopes.Basic, roles.Permissions.ToArray()[0].ActiveScope);
            Assert.AreEqual(PermissionScopes.Administration, roles.Permissions.ToArray()[1].ActiveScope);
            Assert.AreEqual("", roles.Permissions.ToArray()[2].ActiveScope);
            Assert.AreEqual(2, roles.Users.Count());
            Assert.AreEqual("first1 last1", roles.Users.ToArray()[0].FullName);
            Assert.AreEqual("first2 last2", roles.Users.ToArray()[1].FullName);
        }

        private void MockRoles()
        {
            var user1 = Substitute.For<ApplicationUser>();
            user1.Id = "userId1";
            user1.FirstName = "first1";
            user1.LastName = "last1";
            user1.Roles.Returns(new List<IdentityUserRole>
            {
                new IdentityUserRole
                {
                    RoleId = "roleId1"
                }
            });

            var user2 = Substitute.For<ApplicationUser>();
            user2.Id = "userId2";
            user2.FirstName = "first2";
            user2.LastName = "last2";
            user2.Roles.Returns(new List<IdentityUserRole>
            {
                new IdentityUserRole
                {
                    RoleId = "roleId1"
                }
            });

            var user3 = Substitute.For<ApplicationUser>();
            user3.Id = "userId3";
            user3.FirstName = "first3";
            user3.LastName = "last3";
            user3.Roles.Returns(new List<IdentityUserRole>
            {
                new IdentityUserRole
                {
                    RoleId = "roleId2"
                }
            });

            var users = new List<ApplicationUser> { user1, user2, user3 }.AsQueryable();

            var roles = new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Id = "roleId1",
                    Name = "Test1"
                },
                new ApplicationRole
                {
                    Id = "roleId2",
                    Name = "Test2"
                }
            }.AsQueryable();

            _permissionService.GetGroupNamesAsync(1).Returns(
                new List<PermissionGroupDTO>
                {
                    new PermissionGroupDTO
                    {
                        Name = "permission1"
                    },
                    new PermissionGroupDTO
                    {
                        Name = "permission2"
                    },
                    new PermissionGroupDTO
                    {
                        Name = "permission3"
                    }
                });

            _permissionService.GetRolePermissionsAsync("roleId1", 1).Returns(
                new List<PermissionDTO>
                {
                    new PermissionDTO
                    {
                        Name = "PERMISSION1_BASIC",
                        Scope = PermissionScopes.Basic
                    },
                    new PermissionDTO
                    {
                        Name = "PERMISSION2_ADMIN",
                        Scope = PermissionScopes.Administration
                    }
                });

            _roleDbSet.SetDbSetData(roles);
            _usersDbSet.SetDbSetData(users);
        }

        private void MockRolesForAutocomplete()
        {
            var roles = new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Id = "roleId1",
                    Name = "Test1",
                    OrganizationId = 2
                },
                new ApplicationRole
                {
                    Id = "roleId2",
                    Name = "Test2",
                    OrganizationId = 2
                },
                new ApplicationRole
                {
                    Id = "roleId3",
                    Name = "Test12",
                    OrganizationId = 2
                }
            }.AsQueryable();

            _roleDbSet.SetDbSetData(roles);
        }
    }
}
