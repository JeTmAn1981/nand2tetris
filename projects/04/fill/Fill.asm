    @index 
    M=0 
    
(Loop)	
	@index
	D=M
	
	@ZEROCOUNTER
	D;JLT
	
	@8191
	D=D-A
	
	@MAXCOUNTER
	D;JGE
	
(Main)
	@24576
	D=M
	
	@Black
	D;JGT

	@White
	D;JEQ
	
	(Black)
	//Get current index
	@index
	D=M
	
	//Set value of screen address + index from D to 1 to black out pixel
	@SCREEN
	A=D+A
	M=-1
	
	//Increment current index
	@index
	M=D+1
		
	@Loop
	0;JMP
		
	(White)
	//Get current index
	@index
	D=M
	
	//Set value of screen address + index from D to 1 to white out pixel
	@SCREEN
	A=D+A
	M=0
	
	//Decrement current index
	@index
	M=D-1
	
	@Loop
	0;JMP
	
	(ZEROCOUNTER)
	@index
	M=0
	
	@Main
	0;JMP
	
	(MAXCOUNTER)
	@8191
	D=A
	
	@index
	M=D
	
	@Main
	0;JMP
	