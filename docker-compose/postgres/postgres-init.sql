create extension if not exists dblink;

create extension if not exists pg_stat_statements;

create extension if not exists pg_store_plans;

create extension file_fdw;

create server fileserver foreign data wrapper file_fdw;

create foreign table loadavg
    (one text,
     five text,
     fifteen text,
     scheduled text,
     pid text)
    server fileserver
    options (filename '/proc/loadavg', format 'text', delimiter ' ');

create foreign table meminfo
    (stat text, value text)
    server fileserver
    options (filename '/proc/meminfo', format 'csv', delimiter ':');

create foreign table diskstats
    (major_number numeric,
     minor_number numeric,
     device_name text,
     reads_completed_successfully numeric,
     reads_merged numeric,
     sectors_read numeric,
     time_spent_reading_ms numeric,
     writes_completed numeric,
     writes_merged numeric,
     sectors_written numeric,
     time_spent_writing_ms numeric,
     currently_in_progress_io numeric,
     time_spent_doing_io_ms numeric,
     weighted_time_spent_doing_io_ms numeric,
     discards_completed_successfully numeric,
     discards_merged numeric,
     sectors_discarded numeric,
     time_spent_discarding numeric,
     flush_requests_completed_successfully numeric,
     time_spent_flushing numeric)
    server fileserver
    options (program 'cat /proc/diskstats | sed -re ''s/^[[:blank:]]+|[[:blank:]]+$//g'' -e ''s/[[:blank:]]+/ /g''', format 'csv', delimiter ' ');