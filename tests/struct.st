struct IntPair { a i64 b i64 }

main () -> () { @IntPair 1 ~a 2 ~b sum print drop }

sum IntPair -> IntPair i64 { dup dup #a swap #b + }
