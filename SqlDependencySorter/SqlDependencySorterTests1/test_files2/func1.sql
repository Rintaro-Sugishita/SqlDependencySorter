create function func1() returns numeric
$$
declare 
    a: numeric;
begin
    return a;
end;
$$
language plpgsql;