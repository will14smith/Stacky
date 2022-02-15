using System.Runtime.InteropServices;
using LLVMSharp.Interop;
using L = LLVMSharp.Interop.LLVM;

namespace Stacky.Compilation.LLVM;

public partial class CompilerEmitter
{
    private readonly string _targetPlatform = "x86_64-pc-gnu";
    
    private void InitOutput()
    {
        L.InitializeX86TargetInfo();
        L.InitializeX86Target();
        L.InitializeX86TargetMC();
        L.InitializeX86AsmParser();
        L.InitializeX86AsmPrinter();
    }

    private void SetTarget()
    {
        _module.Target = _targetPlatform;
    }

    private void RunTargetMachine(Action<LLVMTargetMachineRef> action)
    {
        if (!LLVMTargetRef.TryGetTargetFromTriple(_targetPlatform, out var targetRef, out var error))
        {
            throw new Exception($"Failed to get platform triple: {error}");
        }

        var machineRef = targetRef.CreateTargetMachine(_targetPlatform, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocPIC, LLVMCodeModel.LLVMCodeModelDefault);
        action(machineRef);
    }
    
    public void OutputAssembly(string name)
    {
        InitOutput();
        SetTarget();
        
        RunTargetMachine(machineRef =>
        {
            machineRef.EmitToFile(_module, name, LLVMCodeGenFileType.LLVMAssemblyFile);
        });
    }
    
    public void OutputObject(string name)
    {
        InitOutput();
        SetTarget();
        
        RunTargetMachine(machineRef =>
        {
            machineRef.EmitToFile(_module, name, LLVMCodeGenFileType.LLVMObjectFile);
        });
    }
}