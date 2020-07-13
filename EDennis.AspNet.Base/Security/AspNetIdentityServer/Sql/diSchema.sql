if not exists (select 0 from sys.schemas where name = N'di' )
    exec('create schema [di] authorization [dbo]');
go
