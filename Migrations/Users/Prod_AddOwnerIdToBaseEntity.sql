START TRANSACTION;

ALTER TABLE "Users" ADD "OwnerId" text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250502184412_AddOwnerIdToBaseEntity', '8.0.2');

COMMIT;

