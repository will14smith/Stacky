# Stacky

A garbage-collected, concatenative, stack-based programming language for fun

TODO List:
- [ ] While
- [ ] Type inference
- [ ] Structs
- [ ] IO - basic read/write for files/stdio
- [ ] Implement GC
- [ ] Stacky self interpreter
- [ ] Optimising compiler - removing stack operations when not needed 

## Syntax

A valid program consists of a list of functions, one of them being `main` which acts as the entry point

### Functions

```
calculate i32 i32 -> i32 str {
    ...
}
```
This `calculate` function acts on a stack topped by 2 parameters, both integers, and results in a stack topped by 2 parameters, an int and a string
The stack ordering is right to left, so the second integer parameter is at the top of the stack, and the string is at the top of the stack after the function returns

### Types

Primitive types for the language are:
- `bool`
- `i8`
- `u8`
- `i16`
- `u16`
- `i32`
- `u32`
- `i64`
- `u64`
- `str`
- `func` (see the Functions section)

User defined structs can also be created

```
struct IntPair {
    i16 a
    i16 b
}
```

### Operations

All operations use a stack based call mechanism:

- adding 2 numbers `1 2 add` or `1 2 +` -> `3`
- concatenating strings `"a" "b" "c" concat concat` (double call since there are 3 strings) -> `"a" "bc" concat` -> `"abc"`

### Functions

Function types signify values that are invokable. They can be expressed in the language as `input -> output`, where either parameter can be `()` to say it is empty

#### Anonymous Functions

Functions are first class values, so defining sub-functions (useful for flow control) is done by surrounding the code in braces 
`{ ... }`

e.g a function that tests if the top of the stack is positive with the type `i64 -> bool`
```{ 0 > }```

### Flow Control

#### If

```
if X bool (X -> X) -> X
if-else X bool (X -> X T) (X -> X T) -> X T
```
where `X` and `T` are any type or `()`

e.g. `1 false { 1 + } if` would return a stack with `1`
e.g. `1 true  { 1 + } if` would return a stack with `2`
e.g. `1 false { 2 + } { 1 + } if-else` would return a stack with `2`
e.g. `1 true  { 2 + } { 1 + } if-else` would return a stack with `3`

#### While

```
while X (X -> X bool) (X -> X) -> X
```
where `X` is any type or `()`

e.g. `5 { dup 0 > } { dup print 1 - } while` would output `54321`

### Structs

Custom structs can be defined as:

```
struct IntPair {
    i16 a
    i16 b
}
```

A new instance of them can be created using the `@` prefix, e.g. `@IntPair () -> IntPair`, when first created they will be zero-initialized.

Fields can be accessed on structs using the `#` prefix, e.g. `#a IntPair -> i16`, the can be updated using the `~` prefix e.g. `~a IntPair i16 -> IntPair`

An example combining all those features - initialising a new object, setting both properties, the printing the sum of them:

```
struct IntPair { i16 a i16 b }

main () -> () {
    @IntPair 1 ~a 2 ~b sum print drop
}

sum IntPair -> IntPair i16 {
    dup dup #a #b +
}
```