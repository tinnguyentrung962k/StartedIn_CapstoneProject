using AutoMapper;
using Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace StartedIn.API.Hubs
{
    [Authorize]
    public class ProjectHub : Hub
    {
        private static readonly ConcurrentDictionary<string, List<string>> projectList = new ConcurrentDictionary<string, List<string>>();
        private static readonly ConcurrentDictionary<string, HashSet<string>> userConnections =
            new ConcurrentDictionary<string, HashSet<string>>();
        private readonly IMapper _mapper;
        private IServiceProvider _serviceProvider;
        private readonly IHubContext<ProjectHub> _hubContext;

        public ProjectHub(IMapper mapper, IServiceProvider serviceProvider, IHubContext<ProjectHub> hubContext)
        {
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            IEnumerable<UserProject> userProjects;

            // Instead of directly overwriting, maintain a collection of connection IDs for each user
            userConnections.AddOrUpdate(
                userId,
                // If the key doesn't exist, create a new HashSet with the current connection ID
                new HashSet<string> { Context.ConnectionId },
                // If the key exists, add the new connection ID to the existing set
                (key, existingConnections) =>
                {
                    existingConnections.Add(Context.ConnectionId);
                    return existingConnections;
                }
            );

            using (var scope = _serviceProvider.CreateScope())
            {
                var userProjectService = scope.ServiceProvider.GetRequiredService<IUserService>();
                userProjects = await userProjectService.GetProjectsByUserId(userId);
            }

            foreach (var project in userProjects)
            {
                if (!projectList.ContainsKey(project.ProjectId))
                {
                    projectList.AddOrUpdate(project.ProjectId, new List<string> { userId }, (key, value) => new List<string> { userId });
                }
                else
                {
                    // Check if user is already in the list, if so then don't add it again
                    if (!projectList[project.ProjectId].Contains(userId))
                    {
                        projectList[project.ProjectId].Add(userId);
                    }
                }

                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userConnections.ContainsKey(userId))
            {
                userConnections.Remove(userId, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        //Send message to all users connect to hub
        public async Task SendMessage(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task SendTaskDataToUsersInProject(string projectId, PayloadDTO<TaskResponseDTO> payload)
        {
            if (projectList.ContainsKey(projectId))
            {
                foreach (var userId in projectList[projectId])
                {
                    if (userConnections.ContainsKey(userId))
                    {
                        // Get the connection IDs for the userId
                        var connectionIds = userConnections[userId];
                        foreach (var connectionId in connectionIds)
                        {
                            await _hubContext.Clients.Client(connectionId).SendAsync("Project", payload);
                        }
                    }
                }
            }
        }

        public async Task SendMilestoneDataToUsersInProject(string projectId, PayloadDTO<MilestoneDetailsResponseDTO> payload)
        {
            if (projectList.ContainsKey(projectId))
            {
                foreach (var userId in projectList[projectId])
                {
                    if (userConnections.ContainsKey(userId))
                    {
                        var connectionIds = userConnections[userId];
                        foreach (var connectionId in connectionIds)
                        {
                            await _hubContext.Clients.Client(connectionId).SendAsync("Project", payload);
                        }
                    }
                }
            }
        }
    }
}
