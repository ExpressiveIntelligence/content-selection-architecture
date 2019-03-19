person(socratese).
person(ulyses).
god(zeus).
mortal(X) :- person(X).
test(X) :- mortal(Y), god(Z), X=foo(Y, Z).