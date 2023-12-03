create function func2() returns varchar
$$
declare 
    a: varchar;
begin
    return a;
end;
$$
language plpgsql;