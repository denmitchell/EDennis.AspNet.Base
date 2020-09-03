Setup for AspNetIdentityServer
1. If desired, change or remove sample data in DomainIdentityDbContext in EDennis.NetStandard.Base
   
2. If you make a change to DomainIdentityDbContext ...
     -- PM>Remove-Migration -Context DomainIdentityDbContext -Project EDennis.AspNetIdentityServer -StartupProject EDennis.AspNetIdentityServer
     -- PM>Update-Database -Context DomainIdentityDbContext -Project EDennis.AspNetIdentityServer -StartupProject EDennis.AspNetIdentityServer
     -- PM>Add-Migration -Context DomainIdentityDbContext -Project EDennis.AspNetIdentityServer -StartupProject EDennis.AspNetIdentityServer -o Data/Migrations/DomainIdentity
     -- PM>Update-Database -Context DomainIdentityDbContext -Project EDennis.AspNetIdentityServer -StartupProject EDennis.AspNetIdentityServer

     If you drop the database, rather than removing the migration, you will need to run
     -- PM>Update-Database for the ConfigurationDbContext and the PersistedGrantDbContext
           (This must be done before Updating the database for DomainIdentityDbContext)

3. The launchSettings.json file defines some sample launch profiles for seeding the 
   database with clients, resources, users, and claims.  The seeding code in SeedDataLoader
   is designed to seed the database from json files generated from the SeedDataGenerator
   (in EDennis.NetStandard.Base/Security/Utils), which can be setup to run from a 
   launch profile from each application that uses the AspNetIdentityServer