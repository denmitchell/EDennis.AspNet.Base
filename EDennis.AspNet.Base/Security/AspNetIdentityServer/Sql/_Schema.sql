if not exists (select 0 from sys.schemas where name = N'_' )
    exec('create schema [_] authorization [dbo]');
go
