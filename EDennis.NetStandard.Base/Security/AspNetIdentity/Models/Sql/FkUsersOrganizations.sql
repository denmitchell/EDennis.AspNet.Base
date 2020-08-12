ALTER TABLE AspNetUsers
	ADD CONSTRAINT fk_UsersOrganizations
		FOREIGN KEY (Organization)
			REFERENCES AspNetOrganizations (Name)
				ON DELETE SET NULL