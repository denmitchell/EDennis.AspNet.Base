ALTER TABLE AspNetRoles
	ADD CONSTRAINT fk_RolesApplications
		FOREIGN KEY ([Application])
			REFERENCES AspNetApplications (Name)
				ON DELETE CASCADE