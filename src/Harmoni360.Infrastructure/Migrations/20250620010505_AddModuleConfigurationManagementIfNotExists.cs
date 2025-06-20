using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleConfigurationManagementIfNotExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if tables already exist and only create if they don't
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'ModuleConfigurationAuditLogs') THEN
                        CREATE TABLE ""ModuleConfigurationAuditLogs"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""ModuleType"" integer NOT NULL,
                            ""Action"" character varying(50) NOT NULL,
                            ""OldValue"" jsonb,
                            ""NewValue"" jsonb,
                            ""UserId"" integer NOT NULL,
                            ""Timestamp"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            ""IpAddress"" character varying(45),
                            ""UserAgent"" character varying(500),
                            ""Context"" character varying(1000),
                            CONSTRAINT ""FK_ModuleConfigurationAuditLogs_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE RESTRICT
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'ModuleConfigurations') THEN
                        CREATE TABLE ""ModuleConfigurations"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""ModuleType"" integer NOT NULL UNIQUE,
                            ""IsEnabled"" boolean NOT NULL DEFAULT true,
                            ""DisplayName"" character varying(100) NOT NULL,
                            ""Description"" character varying(500),
                            ""IconClass"" character varying(50),
                            ""DisplayOrder"" integer NOT NULL DEFAULT 0,
                            ""ParentModuleType"" integer,
                            ""Settings"" jsonb,
                            ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            ""CreatedBy"" character varying(255) NOT NULL,
                            ""LastModifiedAt"" timestamp with time zone,
                            ""LastModifiedBy"" character varying(255),
                            CONSTRAINT ""FK_ModuleConfigurations_ModuleConfigurations_ParentModuleType"" FOREIGN KEY (""ParentModuleType"") REFERENCES ""ModuleConfigurations"" (""ModuleType"") ON DELETE RESTRICT
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'ModuleDependencies') THEN
                        CREATE TABLE ""ModuleDependencies"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""ModuleType"" integer NOT NULL,
                            ""DependsOnModuleType"" integer NOT NULL,
                            ""IsRequired"" boolean NOT NULL DEFAULT true,
                            ""Description"" character varying(500),
                            ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            ""CreatedBy"" character varying(255) NOT NULL,
                            ""LastModifiedAt"" timestamp with time zone,
                            ""LastModifiedBy"" character varying(255),
                            CONSTRAINT ""CK_ModuleDependency_NoSelfDependency"" CHECK (""ModuleType"" != ""DependsOnModuleType""),
                            CONSTRAINT ""FK_ModuleDependencies_ModuleConfigurations_DependsOnModuleType"" FOREIGN KEY (""DependsOnModuleType"") REFERENCES ""ModuleConfigurations"" (""ModuleType"") ON DELETE RESTRICT,
                            CONSTRAINT ""FK_ModuleDependencies_ModuleConfigurations_ModuleType"" FOREIGN KEY (""ModuleType"") REFERENCES ""ModuleConfigurations"" (""ModuleType"") ON DELETE CASCADE,
                            UNIQUE (""ModuleType"", ""DependsOnModuleType"")
                        );
                    END IF;
                END $$;
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfigurationAuditLog_Action"" ON ""ModuleConfigurationAuditLogs"" (""Action"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfigurationAuditLog_Module_Timestamp"" ON ""ModuleConfigurationAuditLogs"" (""ModuleType"", ""Timestamp"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfigurationAuditLog_ModuleType"" ON ""ModuleConfigurationAuditLogs"" (""ModuleType"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfigurationAuditLog_Timestamp"" ON ""ModuleConfigurationAuditLogs"" (""Timestamp"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfigurationAuditLog_UserId"" ON ""ModuleConfigurationAuditLogs"" (""UserId"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfiguration_DisplayOrder"" ON ""ModuleConfigurations"" (""DisplayOrder"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfiguration_IsEnabled"" ON ""ModuleConfigurations"" (""IsEnabled"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ModuleConfiguration_ModuleType"" ON ""ModuleConfigurations"" (""ModuleType"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleConfiguration_ParentModuleType"" ON ""ModuleConfigurations"" (""ParentModuleType"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleDependency_DependsOnModuleType"" ON ""ModuleDependencies"" (""DependsOnModuleType"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleDependency_IsRequired"" ON ""ModuleDependencies"" (""IsRequired"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ModuleDependency_Module_DependsOn"" ON ""ModuleDependencies"" (""ModuleType"", ""DependsOnModuleType"");
                CREATE INDEX IF NOT EXISTS ""IX_ModuleDependency_ModuleType"" ON ""ModuleDependencies"" (""ModuleType"");
            ");

            // Keep the original code commented out for reference
            /*
            migrationBuilder.CreateTable(
                name: "ModuleConfigurationAuditLogs",
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleConfigurationAuditLogs");

            migrationBuilder.DropTable(
                name: "ModuleDependencies");

            migrationBuilder.DropTable(
                name: "ModuleConfigurations");
        }
    }
}
