# Stacky

A garbage-collected, concatenative, stack-based programming language for fun

## Syntax

A valid program consists of a list of functions, one of them being `main` which acts as the entry point

### Functions

```
(i32 i32) calculate (i32 str) {
    ...
}
```
This `calculate` function acts on a stack topped by 2 parameters, both integers, and results in a stack topped by 2 parameters, an int and a string
The stack ordering is right to left, so the second integer parameter is at the top of the stack, and the string is at the top of the stack after the function returns

### Types

Primitive types for the language are:
- `i8`
- `u8` / `byte` 
- `i16`
- `u16`
- `i32`
- `u32`
- `i64`
- `u64`
- `str`
- `ptr T` - where `T` is another type 

User defined structs can also be created

```
struct IntPair {
    i16 a
    i16 b
}
```

### Conditionals

```
if { }
if { } else { }
```

### Operations

All operations use a stack based call mechanism:

- adding 2 numbers `1 2 add` or `1 2 +` -> `3`
- concatenating strings `"a" "b" "c" concat concat` (double call since there are 3 strings) -> `"a" "bc" concat` -> `"abc"`