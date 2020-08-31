ALTER TABLE AspNetUsers
	ADD CONSTRAINT fk_UsersOrganizations
		FOREIGN KEY (Organization)
			REFERENCES AspNetOrganizations (Name)
				ON DELETE SET NULL

ALTER TABLE AspNetOrganizationApplications
	ADD CONSTRAINT fk_OrganizationApplications_App
		FOREIGN KEY (Organization)
			REFERENCES AspNetOrganizations (Name)
				ON DELETE SET NULL

ALTER TABLE AspNetOrganizationApplications
	ADD CONSTRAINT fk_OrganizationApplications_Org
		FOREIGN KEY (Application)
			REFERENCES AspNetApplications (Name)
				ON DELETE SET NULL

ALTER TABLE AspNetApplicationClaims
	ADD CONSTRAINT fk_ApplicaticationClaims_App
		FOREIGN KEY (Application)
			REFERENCES AspNetApplications (Name)
				ON DELETE SET NULL
