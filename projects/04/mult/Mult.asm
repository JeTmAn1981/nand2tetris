// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)

// Put your code here.
//SET R2 = 0
@R2
M=0

(Start)
//IF R1 <= 1 JMP END
@R1
D=M

@END
D;JLE

//SET R2 = R2 + R0
@R0
D=M

@R2
M=M+D

//SET R1 = R1 - 1
@R1
M=M-1

@Start     
0;JMP

(End)
@End
0;JMP