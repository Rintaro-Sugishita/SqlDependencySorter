set client_encoding to "UTF-8";

create or replace view "view1" as 
select *, func1() as col1 from tbl1;
