@256
D=A
@SP
M=D
@ReturnAddress0
D=A
@SP
A=M
M=D
@SP
M=M+1
@LCL
D=M
@SP
A=M
M=D
@SP
M=M+1
@ARG
D=M
@SP
A=M
M=D
@SP
M=M+1
@THIS
D=M
@SP
A=M
M=D
@SP
M=M+1
@THAT
D=M
@SP
A=M
M=D
@SP
M=M+1
@SP
D=M
@5
D=D-A
@ARG
M=D
@SP
D=M
@LCL
M=D
@sys.init
0;JMP
(ReturnAddress0)
(simplefunction.test)
@0
D=A
@SP
A=M
M=D
@SP
M=M+1
@0
D=A
@SP
A=M
M=D
@SP
M=M+1
@LCL
D=M
@0
A=D+A
D=M
@SP
A=M
M=D
@SP
M=M+1
@LCL
D=M
@1
A=D+A
D=M
@SP
A=M
M=D
@SP
M=M+1
@SP
A=M-1
D=M
@SP
M=M-1
@SP
A=M-1
M=M+D
@SP
A=M-1
M=!M
@ARG
D=M
@0
A=D+A
D=M
@SP
A=M
M=D
@SP
M=M+1
@SP
A=M-1
D=M
@SP
M=M-1
@SP
A=M-1
M=M+D
@ARG
D=M
@1
A=D+A
D=M
@SP
A=M
M=D
@SP
M=M+1
@SP
A=M-1
D=M
@SP
M=M-1
@SP
A=M-1
M=M-D
@LCL
D=M
@5
A=D-A
D=M
@RETADDR
M=D
@ARG
D=M
@0
D=D+A
@R13
M=D
@SP
A=M-1
D=M
@R13
A=M
M=D
@SP
M=M-1
@ARG
D=M+1
@SP
M=D
@LCL
D=M
@1
A=D-A
D=M
@THAT
M=D
@LCL
D=M
@2
A=D-A
D=M
@THIS
M=D
@LCL
D=M
@3
A=D-A
D=M
@ARG
M=D
@4
D=A
@LCL
A=M-D
D=M
@LCL
M=D
@RETADDR
A=M
0;JMP
(sys.init)
@1
D=A
@SP
A=M
M=D
@SP
M=M+1
@3
D=A
@SP
A=M
M=D
@SP
M=M+1
@ReturnAddress1
D=A
@SP
A=M
M=D
@SP
M=M+1
@LCL
D=M
@SP
A=M
M=D
@SP
M=M+1
@ARG
D=M
@SP
A=M
M=D
@SP
M=M+1
@THIS
D=M
@SP
A=M
M=D
@SP
M=M+1
@THAT
D=M
@SP
A=M
M=D
@SP
M=M+1
@SP
D=M
@7
D=D-A
@ARG
M=D
@SP
D=M
@LCL
M=D
@simplefunction.test
0;JMP
(ReturnAddress1)
(Main$while)
@Main$while
0;JMP
