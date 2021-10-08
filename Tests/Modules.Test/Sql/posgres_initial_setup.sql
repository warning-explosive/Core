create user "SpaceEngineer";
alter user "SpaceEngineer" with encrypted password 'Voyager1';
create database "SpaceEngineerDatabase";
grant all privileges on database "SpaceEngineerDatabase" to "SpaceEngineer";