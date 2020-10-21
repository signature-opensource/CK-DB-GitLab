--[beginscript]

alter table CK.tUserGitLab add
	ScopeSetId int not null constraint DF_TEMP default(0);
	 
alter table CK.tUserGitLab add
	constraint FK_CK_UserGitLab_ScopeSetId foreign key (ScopeSetId) references CK.tAuthScopeSet(ScopeSetId);

alter table CK.tUserGitLab drop constraint DF_TEMP;

-- ScopeSetId are let to 0 here:
-- The Anonymous holds the default scopes: it is created in by Settle and every 
-- already created UserGitLab is associated to an independent scope set.
-- Once done, a unique constraint is set on the ScopeSetId column to secure the 
-- data: no ScopeSet can be shared by two users.

--[endscript]
