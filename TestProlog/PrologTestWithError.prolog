incrementTries :-
    triedDoor,
    tries(TryCount),
    retract(tries(TryCount)),
    retract(triedDoor),
    assert(tries(TryCount + 1)).
    
tries(0).

triesGreaterThan(Num) :-
   tries(TryCount),
   TryCount > Num.