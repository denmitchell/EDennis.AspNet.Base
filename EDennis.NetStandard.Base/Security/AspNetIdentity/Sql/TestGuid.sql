--generates guid that orders before other records
create or alter function _.TestGuid(@dec bigint)
returns uniqueidentifier as
begin
	declare @rem bigint
	declare @hex varchar(8) = ''
	while (@dec > 0)
	begin
		set @rem = @dec % 256
		set @dec = (@dec - @rem ) / 256
		set @hex = @hex + format(@rem,'X') 
	end
	return convert(uniqueidentifier,left(@hex + '00000000',8) + '-0000-0000-0000-000000000000')
end