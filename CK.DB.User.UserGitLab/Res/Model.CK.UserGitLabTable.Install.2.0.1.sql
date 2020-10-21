--[beginscript]

create table CK.tUserGitLab
(
	UserId int not null,
	-- The GitLab account identifier is the key to identify a GitLab user.
	GitLabAccountId varchar(36) collate Latin1_General_100_BIN2 not null,
	LastLoginTime datetime2(2) not null,
	constraint PK_CK_UserGitLab primary key (UserId),
	constraint FK_CK_UserGitLab_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserGitLab_GitLabAccountId unique( GitLabAccountId )
);

insert into CK.tUserGitLab( UserId, GitLabAccountId, LastLoginTime ) 
	values( 0, '', sysutcdatetime() );

--[endscript]
