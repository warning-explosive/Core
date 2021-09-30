create user "SpaceEngineer";
alter user "SpaceEngineer" with encrypted password '1234';
grant connect on database postgres to "SpaceEngineer";
ALTER USER "SpaceEngineer" CREATEDB;