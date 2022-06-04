create extension dblink;

create user "SpaceEngineer";
alter user "SpaceEngineer" with encrypted password 'Voyager1';

create database "SpaceEngineersTestDatabase";
grant all privileges on database "SpaceEngineersTestDatabase" to "SpaceEngineer";