create sequence seqAspNetOrganizations
	as int start with 1 increment by 1
go
create table AspNetOrganizations (
	Id int not null primary key default next value for seqAspNetOrganizations,
	Name varchar(255)
)
