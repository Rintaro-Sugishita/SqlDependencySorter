
create or replace view "view2" as 
select *, func1() as col1 from "view1";
