# nand2tetris

This is code I wrote for the nand2tetris online course.  The course takes students through almost every step of building a full-featured computer (in modeling language), starting from the level of logic gates and sequentially building up to memory, a CPU, and I/O.  That's the first part of the class.  

In the second part, you write an assembler for a toy assembly language to generate machine code for your modeled hardware, then a virtual machine translator to translate from a stack-base virtual machine language to assembly, then a full compiler for a C-like toy language which compiles to the virtual machine instructions.  Then you write operating system components in that high-level language for handling functions such as memory allocation and drawing to the screen.  It's truly a holistic process, and I had a lot of fun writing each of these components and gaining a deeper understanding of how computers and software really work.

For the first part of the class, I wrote the computer componets in hardware description language (HDL).  For the second part of the class I wrote the assembler, virtual machine translator and compiler using C#.  The projects from the second part of the class have their own folders, while those from the first part can be found in the projects folder.
