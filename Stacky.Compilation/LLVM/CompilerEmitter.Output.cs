using System.Runtime.InteropServices;
using LLVMSharp;
using L = LLVMSharp.LLVM;

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
        L.SetTarget(_module, _targetPlatform);
    }

    private void RunTargetMachine(Action<LLVMTargetMachineRef> action)
    {
        if (L.GetTargetFromTriple(_targetPlatform, out var targetRef, out var error))
        {
            throw new Exception($"Failed to get platform triple: {error}");
        }
        
        var machineRef = L.CreateTargetMachine(targetRef, _targetPlatform, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocPIC, LLVMCodeModel.LLVMCodeModelDefault);
        action(machineRef);
        L.DisposeTargetMachine(machineRef);
    }
    
    public void OutputAssembly(string name)
    {
        InitOutput();
        SetTarget();
        
        RunTargetMachine(machineRef =>
        {
            if (L.TargetMachineEmitToFile(machineRef, _module, Marshal.StringToHGlobalAnsi(name), LLVMCodeGenFileType.LLVMAssemblyFile, out var error))
            {
                throw new Exception($"Failed to emit output: {error}");
            }
        });
    }
    
    public void OutputObject(string name)
    {
        InitOutput();
        SetTarget();
        
        RunTargetMachine(machineRef =>
        {
            if (L.TargetMachineEmitToFile(machineRef, _module, Marshal.StringToHGlobalAnsi(name), LLVMCodeGenFileType.LLVMObjectFile, out var error))
            {
                throw new Exception($"Failed to emit output: {error}");
            }
        });
    }
}