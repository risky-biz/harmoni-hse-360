using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarmoniHSE360.Infrastructure.Services;

public interface IDataSeeder
{
    Task SeedAsync();
}

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAndPermissionsAsync();
            await _context.SaveChangesAsync(); // Save roles first

            await SeedUsersAsync();
            await _context.SaveChangesAsync(); // Save users

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    private async Task SeedRolesAndPermissionsAsync()
    {
        var roleCount = await _context.Roles.CountAsync();
        _logger.LogInformation("Current role count: {RoleCount}", roleCount);

        if (roleCount > 0)
        {
            _logger.LogInformation("Roles already exist, skipping seeding");
            return;
        }

        _logger.LogInformation("Starting role and permission seeding...");

        var adminRole = Role.Create("Admin", "System Administrator");
        var hseManagerRole = Role.Create("HSEManager", "HSE Manager");
        var employeeRole = Role.Create("Employee", "Regular Employee");

        await _context.Roles.AddRangeAsync(adminRole, hseManagerRole, employeeRole);
        await _context.SaveChangesAsync(); // Save roles first to get IDs

        // Create unique permissions first
        var allPermissions = new List<Permission>
        {
            Permission.Create("incidents.view", "View Incidents", "Can view incident reports", "Incidents"),
            Permission.Create("incidents.create", "Create Incidents", "Can create new incident reports", "Incidents"),
            Permission.Create("incidents.update", "Update Incidents", "Can modify incident reports", "Incidents"),
            Permission.Create("incidents.delete", "Delete Incidents", "Can delete incident reports", "Incidents"),
            Permission.Create("incidents.investigate", "Investigate Incidents", "Can investigate and assign incidents", "Incidents"),
            Permission.Create("users.manage", "Manage Users", "Can manage user accounts and roles", "Users"),
            Permission.Create("reports.view", "View Reports", "Can view all reports and analytics", "Reports"),
            Permission.Create("reports.create", "Create Reports", "Can generate reports", "Reports")
        };

        await _context.Permissions.AddRangeAsync(allPermissions);
        await _context.SaveChangesAsync(); // Save permissions first

        // Get permission references for role assignment
        var incidentsView = allPermissions.First(p => p.Name == "incidents.view");
        var incidentsCreate = allPermissions.First(p => p.Name == "incidents.create");
        var incidentsUpdate = allPermissions.First(p => p.Name == "incidents.update");
        var incidentsDelete = allPermissions.First(p => p.Name == "incidents.delete");
        var incidentsInvestigate = allPermissions.First(p => p.Name == "incidents.investigate");
        var usersManage = allPermissions.First(p => p.Name == "users.manage");
        var reportsView = allPermissions.First(p => p.Name == "reports.view");
        var reportsCreate = allPermissions.First(p => p.Name == "reports.create");

        // Assign permissions to roles
        // Admin gets all permissions
        adminRole.AddPermission(incidentsView);
        adminRole.AddPermission(incidentsCreate);
        adminRole.AddPermission(incidentsUpdate);
        adminRole.AddPermission(incidentsDelete);
        adminRole.AddPermission(incidentsInvestigate);
        adminRole.AddPermission(usersManage);
        adminRole.AddPermission(reportsView);
        adminRole.AddPermission(reportsCreate);

        // HSE Manager gets incident and report permissions
        hseManagerRole.AddPermission(incidentsView);
        hseManagerRole.AddPermission(incidentsCreate);
        hseManagerRole.AddPermission(incidentsUpdate);
        hseManagerRole.AddPermission(incidentsInvestigate);
        hseManagerRole.AddPermission(reportsView);
        hseManagerRole.AddPermission(reportsCreate);

        // Employee gets basic incident permissions
        employeeRole.AddPermission(incidentsView);
        employeeRole.AddPermission(incidentsCreate);
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
            return;

        var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
        var hseManagerRole = await _context.Roles.FirstAsync(r => r.Name == "HSEManager");
        var employeeRole = await _context.Roles.FirstAsync(r => r.Name == "Employee");

        // Hash demo passwords for production use
        var passwordHashService = new PasswordHashService();

        var users = new List<User>
        {
            User.Create("admin@bsj.sch.id", passwordHashService.HashPassword("Admin123!"), "System Administrator", "ADM001", "IT", "System Administrator"),
            User.Create("hse.manager@bsj.sch.id", passwordHashService.HashPassword("HSE123!"), "HSE Manager", "HSE001", "Health & Safety", "HSE Manager"),
            User.Create("john.doe@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "John Doe", "EMP001", "Facilities", "Maintenance Supervisor"),
            User.Create("jane.smith@bsj.sch.id", passwordHashService.HashPassword("Employee123!"), "Jane Smith", "EMP002", "Academic", "Teacher")
        };

        // Assign roles
        users[0].AssignRole(adminRole);
        users[1].AssignRole(hseManagerRole);
        users[2].AssignRole(employeeRole);
        users[3].AssignRole(employeeRole);

        await _context.Users.AddRangeAsync(users);
    }
}