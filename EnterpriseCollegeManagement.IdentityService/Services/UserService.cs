using AutoMapper;
using EnterpriseCollegeManagement.IdentityService.DTOs.User.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.User.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;
using EnterpriseCollegeManagement.IdentityService.Exceptions;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseCollegeManagement.IdentityService.Services
{
    public class UserService : IUserService
    {
        private  readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;


        public UserService(UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager,
                IAuditService auditService, 
                IMapper mapper,
                ILogger<UserService> logger)
        {
            _auditService = auditService;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<UserResponseDto> AssignRoleAsync(string actorUserId, AssignRoleRequestDto request)
        {
            _logger.LogInformation(
                "Role assignment started. ActorUserId: {ActorUserId}, TargetUserId: {TargetUserId}, RequestedRole: {Role}",
                actorUserId,
                request.UserId,
                request.Role);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if(user == null)
            {
                _logger.LogWarning(
                  "Role assignment failed. Target user not found. TargetUserId: {TargetUserId}",
                  request.UserId);
                throw new NotFoundException("User not found.");
            }

            var allowedRole = new[] { "Student", "Teacher" };

            if(!allowedRole.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Invalid role assignment requested. Role: {Role}",
                    request.Role);

                throw new BadRequestException("Only Student or Teacher roles can be assigned.");
            }


            var roleExists = await _roleManager.RoleExistsAsync(request.Role);

            if (!roleExists)
            {
                _logger.LogWarning(
                    "Requested role does not exist. Role: {Role}",
                    request.Role);

                throw new BadRequestException("Requested role does not exist.");
            }
           
            var currentRoles = await _userManager.GetRolesAsync(user);

            var previousRole = currentRoles.FirstOrDefault();

            if (string.Equals(previousRole, request.Role, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                   "User already has requested role. UserId: {UserId}, Role: {Role}",
                   user.Id,
                   request.Role);

                throw new BadRequestException(
                    "User already has this role.");

            }

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError(
                        "Failed to remove existing roles. UserId: {UserId}",
                        user.Id);

                    throw new BadRequestException( "Failed to remove existing role.");
                }

            }
            var addResult = await _userManager.AddToRoleAsync(user, request.Role);

            if (!addResult.Succeeded)
            {
                _logger.LogError(
                    "Failed to assign new role. UserId: {UserId}, Role: {Role}",
                    user.Id,
                    request.Role);

                throw new BadRequestException("Failed to assign new role.");
            }

            await _auditService.LogAsync(
              actorUserId,
              "RoleChanged",
              "ApplicationUser",
              user.Id,
              $"User role changed from {previousRole ?? "None"} to {request.Role}.",
              previousRole,
              request.Role);

            
            _logger.LogInformation(
                "User role updated successfully. ActorUserId: {ActorUserId}, TargetUserId: {TargetUserId}, PreviousRole: {PreviousRole}, NewRole: {NewRole}",
                actorUserId,
                user.Id,
                previousRole,
                request.Role);



            return new UserResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName!,
                PreviousRole = previousRole,
                NewRole = request.Role,
                Message = "User role updated successfully."


            };
           
        }
    }
}
