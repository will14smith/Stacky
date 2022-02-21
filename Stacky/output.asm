	.text
	.file	"program"
	.globl	main
	.p2align	4, 0x90
	.type	main,@function
main:
	.cfi_startproc
	pushq	%r15
	.cfi_def_cfa_offset 16
	pushq	%r14
	.cfi_def_cfa_offset 24
	pushq	%rbx
	.cfi_def_cfa_offset 32
	.cfi_offset %rbx, -32
	.cfi_offset %r14, -24
	.cfi_offset %r15, -16
	leaq	.Lstr(%rip), %rbx
	movq	%rbx, %rdi
	callq	gc_root_add@PLT
	movq	stackPointer@GOTPCREL(%rip), %r15
	movq	(%r15), %rax
	movq	%rbx, (%rax)
	leaq	8(%rax), %rcx
	movq	%rcx, (%r15)
	movq	(%rax), %rbx
	movq	%rax, (%r15)
	leaq	.Lstr.1(%rip), %rsi
	movq	%rbx, %rdi
	callq	fopen@PLT
	movq	%rax, %r14
	movq	%rbx, %rdi
	callq	gc_root_remove@PLT
	movq	(%r15), %rax
	movq	%r14, (%rax)
	leaq	8(%rax), %rcx
	movq	%rcx, (%r15)
	movq	(%rax), %rcx
	movq	%rcx, 8(%rax)
	addq	$16, %rax
	movq	%rax, (%r15)
	leaq	.Lstr.2(%rip), %rbx
	movq	%rbx, %rdi
	callq	gc_root_add@PLT
	movq	(%r15), %rax
	movq	%rbx, (%rax)
	leaq	8(%rax), %rcx
	movq	%rcx, (%r15)
	movq	(%rax), %rbx
	movq	%rax, (%r15)
	movq	-8(%rax), %rdi
	addq	$-8, %rax
	movq	%rax, (%r15)
	leaq	.Lstr.3(%rip), %rsi
	movq	%rbx, %rdx
	xorl	%eax, %eax
	callq	fprintf@PLT
	movq	%rbx, %rdi
	callq	gc_root_remove@PLT
	movq	(%r15), %rax
	movq	-8(%rax), %rcx
	movq	%rcx, (%rax)
	addq	$8, %rax
	movq	%rax, (%r15)
	leaq	.Lstr.4(%rip), %rbx
	movq	%rbx, %rdi
	callq	gc_root_add@PLT
	movq	(%r15), %rax
	movq	%rbx, (%rax)
	leaq	8(%rax), %rcx
	movq	%rcx, (%r15)
	movq	(%rax), %rbx
	movq	%rax, (%r15)
	movq	-8(%rax), %rdi
	addq	$-8, %rax
	movq	%rax, (%r15)
	leaq	.Lstr.5(%rip), %rsi
	movq	%rbx, %rdx
	xorl	%eax, %eax
	callq	fprintf@PLT
	movq	%rbx, %rdi
	callq	gc_root_remove@PLT
	movq	(%r15), %rax
	movq	-8(%rax), %rcx
	movq	%rcx, (%rax)
	addq	$8, %rax
	movq	%rax, (%r15)
	leaq	.Lstr.6(%rip), %rbx
	movq	%rbx, %rdi
	callq	gc_root_add@PLT
	movq	(%r15), %rax
	movq	%rbx, (%rax)
	leaq	8(%rax), %rcx
	movq	%rcx, (%r15)
	movq	(%rax), %rbx
	movq	%rax, (%r15)
	movq	-8(%rax), %rdi
	addq	$-8, %rax
	movq	%rax, (%r15)
	leaq	.Lstr.7(%rip), %rsi
	movq	%rbx, %rdx
	xorl	%eax, %eax
	callq	fprintf@PLT
	movq	%rbx, %rdi
	callq	gc_root_remove@PLT
	movq	(%r15), %rax
	movq	-8(%rax), %rdi
	addq	$-8, %rax
	movq	%rax, (%r15)
	callq	fclose@PLT
	popq	%rbx
	.cfi_def_cfa_offset 24
	popq	%r14
	.cfi_def_cfa_offset 16
	popq	%r15
	.cfi_def_cfa_offset 8
	retq
.Lfunc_end0:
	.size	main, .Lfunc_end0-main
	.cfi_endproc

	.type	stackRoot,@object
	.bss
	.globl	stackRoot
	.p2align	4
stackRoot:
	.zero	16384
	.size	stackRoot, 16384

	.type	stackPointer,@object
	.data
	.globl	stackPointer
	.p2align	3
stackPointer:
	.quad	stackRoot
	.size	stackPointer, 8

	.type	.Lstr,@object
	.section	.rodata.str1.1,"aMS",@progbits,1
.Lstr:
	.asciz	"./temp.test"
	.size	.Lstr, 12

	.type	.Lstr.1,@object
.Lstr.1:
	.asciz	"w"
	.size	.Lstr.1, 2

	.type	.Lstr.2,@object
.Lstr.2:
	.asciz	"test"
	.size	.Lstr.2, 5

	.type	.Lstr.3,@object
.Lstr.3:
	.asciz	"%s"
	.size	.Lstr.3, 3

	.type	.Lstr.4,@object
.Lstr.4:
	.asciz	"123"
	.size	.Lstr.4, 4

	.type	.Lstr.5,@object
.Lstr.5:
	.asciz	"%s\n"
	.size	.Lstr.5, 4

	.type	.Lstr.6,@object
.Lstr.6:
	.asciz	"test"
	.size	.Lstr.6, 5

	.type	.Lstr.7,@object
.Lstr.7:
	.asciz	"%s\n"
	.size	.Lstr.7, 4

	.section	".note.GNU-stack","",@progbits
