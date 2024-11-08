﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Filters
{
    public class ProjectAccessAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserService _userService;

        public ProjectAccessAuthorizationFilter(IUserService userService)
        {
            _userService = userService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var projectId = context.RouteData.Values["projectId"]?.ToString();

            if (string.IsNullOrEmpty(projectId))
            {
                context.Result = new BadRequestResult();
                return;
            }

            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            }
            catch (Exception ex)
            {
                context.Result = new ForbidResult(ex.Message);
            }
        }
    }
}