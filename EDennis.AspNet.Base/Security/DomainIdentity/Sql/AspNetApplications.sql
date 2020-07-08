create sequence seqAspNetApplications
	as int start with 1 increment by 1
go
create table AspNetApplications (
	Id int not null primary key default next value for seqAspNetApplications,
	Name varchar(255)
)
