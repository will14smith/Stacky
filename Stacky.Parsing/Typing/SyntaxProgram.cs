using Stacky.Parsing.Syntax;

namespace Stacky.Parsing.Typing;

public record TypedProgram(SyntaxProgram Syntax, IReadOnlyList<TypedFunction> Functions, IReadOnlyList<TypedStruct> Structs);