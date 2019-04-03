.data 
; amount of bytes in one pixel
BytesInPixel dword 4
Minus dword -1 ; minus to invert numbers
.code
; ==============================================================
; Main procedure used for blurring one pixel image row, 
; right and left border is not included.
; ### PARAMETERS ###
;	int index,			RCX 					[starting byte index of row]
;	int arrayWidth,		RDX (moved to EBX)		[array row width in pixels]
;	byte* original,		R8						[byte array of original image]
;	byte* filtered,		R9						[byte array of filtered image]
;	double* mask,		STACK [rbp+48] 			[gauss mask table pointer]
;	int maskSize		STACK [rbp+56] 			[gauss mask table size]
; ### USED REGISTERS ###
;	R10D, border position differece	
;	R11D, Y loop param
;	R12D, X loop param
;	R13D, Row loop param
;	R14D, real row width
;	R15D, gauss mask table counter
; ==============================================================

gauss proc
; prepare for stack deallocation
push rbp
mov rbp, rsp
push rax
push rbx
push rcx
push rdx
push r12
push r13
push r14
push r15
; arrayWidth dword ptr[rbp+40]
; index dword ptr[rbp+48]
; mov arrayWidth
mov ebx, edx
; calculate positionDiff to R10D
mov R10D, dword ptr[rbp+56]
sub R10D, 1
shr R10D, 1
; calculate realWidth to R14D
mov eax, R10D
shl eax, 1
sub edx, eax
mov eax, edx
mul BytesInPixel
mov R14D,eax

; row param for loop
xor r13d, r13d
ROWLOOP:
; set mask counter
	xor r15d, r15d 
; set R,G,B sum registers
	xorpd XMM0, XMM0
	xorpd XMM1, XMM1
	xorpd XMM2, XMM2
	xorpd XMM3, XMM3

; y param for loop
	mov r11d, r10d
	neg r11d
	dec r11d

; x param for loop
RESETX:
	mov r12d, R10D
	neg r12d

YLOOP:
	cmp r11d, R10D
; end loop if zero
	jz SETPIXEL 
	inc r11d
XLOOP:
; X loop code start:
; calculate Index into eax
	mov eax, r11d
	imul ebx
	add eax, r12d
	imul BytesInPixel
	add eax, ecx
; add to R 
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM0, XMM4
; increment index
	inc eax
; add to G
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM1, XMM4
; increment index
	inc eax
; add to B
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM2, XMM4
; increment index
	inc eax
; add to A
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM3, XMM4
; increment maskCounter
	add r15d, 8

; X loop code end
	cmp r12d, R10D
	jz RESETX ;
	inc r12d
	jmp XLOOP

SETPIXEL:

; allocate byte in pixel
; set R
	mov rdx, r9
	add rdx, rcx
	cvtsd2si eax, XMM0
	mov byte ptr[rdx], al
; set G
	inc rdx
	cvtsd2si eax, XMM1
	mov byte ptr[rdx], al
; set B
	inc rdx
	cvtsd2si eax, XMM2
	mov byte ptr[rdx], al
; set A
	inc rdx
	cvtsd2si eax, XMM3
	mov byte ptr[rdx], al
; increment row loop param
	add R13D, BytesInPixel
; increment current index in bitmap
	add ecx, BytesInPixel
	cmp R13D,R14D
	jnz ROWLOOP

CLEARSTACK:
; clear stack
	pop r15
	pop r14
	pop r13
	pop r12
	pop rdx
	pop rcx
	pop rbx
	pop rax
	pop rbp
	ret
gauss endp 

; ==============================================================
; Procedures sets top and bottom border of image.
; ###PARAMETERS###
;	byte* original, ecx (moved to r10d)		[byte array of original image]
;	byte* filtered, edx (moved to r11d)		[byte array of fultered image]
;	int topBottomBorderSize, r8d 			[amount of bytes in top/bottom borders]
;	int bottomBoundStartIndex, r9d 			[starting index of bottom border]
; ### USED REGISTERS ###
;	ebx, loop counter
; ==============================================================

border proc
; preconditions
	xor rbx, rbx
XLOOP:
; set top border
	mov r11d, dword ptr [rcx+rbx]
	mov dword ptr [rdx+rbx], r11d
; set bottom boder
	mov r10, rcx
	add r10, r9
	mov r11d, dword ptr[r10 + rbx]
	mov r10, rdx
	add r10, r9
	mov dword ptr [r10+rbx], r11d
; increment loop
add ebx, BytesInPixel
cmp r8d, ebx
jnz XLOOP

FINISHED:
ret
border endp


; ==============================================================
; Procedure to pixel a row 
; ### PARAMETERS ###
;	int columnsToFilter,	RCX 					[how many times the operation should be done for a row]
;	int rowIndex,			RDX (moved to EBX)		[row index]
;	int* original,			R8						[int array of original image]
;	int* pixelated,			R9						[int array of pixelated image]
;	int rowCounter,			STACK [rbp+48] 			[row counter to find main pixel]
;	int sourceWitdh,		STACK [rbp+56] 			[widht of the original file]
;	int boumd,				STACK [rbp+64]			[widht of pixelation radius]
; 	int radius,             STACK [rbp+72]			[Mask radius]
; ### USED REGISTERS ###
;	R10D, main pixel position	
;	R11D, row loop index
;	R12D, radius loop param
;	R13, Pixel position
; ==============================================================
pixelRow proc
; prepare for stack deallocation
push rbp
mov rbp, rsp
push rax
push rbx
push rcx
push rdx
push r12
push r13

mov rbx, rdx
;calculate main pixel ptr
mov eax, dword ptr [rbp+56]
add rax, 1
mul dword ptr [rbp+64]
mov r10, rax
mov rax, rbx
sub eax, dword ptr [rbp+48]
mul dword ptr [rbp+56]
add rax, r10
mul BytesInPixel
mov r10, rax

;calculate pixel position
mov rax, rbx
mul dword ptr [rbp+56]
mul BytesInPixel
mov r13, rax

mov r11, rcx;columns to filter
ROWLOOP:
mov r12d, dword ptr [rbp+72];radius
mov eax, dword ptr [r8+r10];main pixel
RADIUSLOOP:
mov dword ptr [r9+r13], eax
add r13, 4
dec r12
jnz RADIUSLOOP
mov eax, dword ptr [rbp+72]
mul BytesInPixel
add r10d, eax;main pixel + radius*4
dec r11
jnz ROWLOOP

mov rax, rcx; columns to filter
mul dword ptr [rbp+72]
imul Minus
add eax, dword ptr [rbp+56]
mov r12, rax;r12 pixels left

mov eax, dword ptr [rbp+64]
mul BytesInPixel
mul Minus
add r10d, eax
mov eax, dword ptr [r8+r10]
LASTLOOP:
mov dword ptr [r9+r13], eax
add r13, 4
dec r12
jnz LASTLOOP

CLEARSTACK:
; clear stack
	pop r13
	pop r12
	pop rdx
	pop rcx
	pop rbx
	pop rax
	pop rbp
	ret
pixelRow endp


end 