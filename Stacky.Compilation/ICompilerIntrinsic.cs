namespace Stacky.Compilation;

public interface ICompilerIntrinsic
{
    CompilerStack Compile(CompilerFunctionContext context, CompilerStack stack);
}