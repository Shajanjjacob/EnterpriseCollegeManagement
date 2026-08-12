using AutoMapper;
using EnterpriseCollegeManagement.IdentityService.Data;
using EnterpriseCollegeManagement.IdentityService.DTOs.Audit.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Audit.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;
using EnterpriseCollegeManagement.IdentityService.Exceptions;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EnterpriseCollegeManagement.IdentityService.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<PagedResultDto<AuditLogResponseDto>> GetUserHistoryAsync(string userId, AuditHistoryFilterDto filter)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }


            _logger.LogInformation("Fetching audit history for UserId: {UserId}", userId);

            var query = _context.AuditLogs
                 .Where(x => x.UserId == userId);

            //filter

            if (filter.Date.HasValue)
            {
                var startDate = filter.Date.Value.Date;
                var endDate = filter.Date.Value.AddDays(1);

                query = query.Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate);


            }
                //total count

                var totalRecords = await query.CountAsync();

            //pagination

            var logs = await query
                     .OrderByDescending(x => x.CreatedDate)
                     .Skip((filter.PageNumber - 1) * filter.PageSize)
                     .Take(filter.PageSize)
                     .ToListAsync();

            //calculate totalpages 

            var totalPages = (int)Math.Ceiling(totalRecords / (double)filter.PageSize);

                var items = _mapper.Map<List<AuditLogResponseDto>>(logs);

                _logger.LogInformation(
                "Audit history retrieved. UserId: {UserId}, TotalRecords: {TotalRecords}",
                userId,
                totalRecords);

                return new PagedResultDto<AuditLogResponseDto>
                {
                    Items = items,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                };
            }
        

        public async Task LogAsync(string? userId, string action, string entityname, string? entityId = null, string? description = null, string? oldValues = null, string? newValues = null)
        {
            _logger.LogInformation("Creating audit log. UserId: {UserId}, Action: {Action}, Entity: {EntityName}", userId, action, entityname);

            var auditlog = new AuditLog
            {
                UserId = userId,
                Actiom = action,
                EntityName = entityname,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues,
                CreatedDate = DateTime.UtcNow,
            };

            _context.AuditLogs.Add(auditlog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created successfully. UserId: {UserId}, Action: {Action}", userId, action);

        }

        
    }
}
