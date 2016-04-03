\ Forth Ed (fed) - An ed-inspired single-line text editor for Forth
\ Copyright 2016 Scot W. Stevenson <scot.stevenson@gmail.com>
\ Written with gforth 0.7
\ First version: 10. Jul 2015 
\ This version: 03. April 2016

\ This program is placed in the public domain

\ This program is distributed in the hope that it will be useful, but WITHOUT
\ ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
\ FOR A PARTICULAR PURPOSE. Use it at your own risk.

\ Inspired by https://www.gnu.org/software/ed/ ; single-linked list code
\ based on
\ https://rosettacode.org/wiki/Singly-linked_list/Element_definition#Forth

\ Single-linked list contains two cells in each header: one for the address of
\ the next line's header, where zero signals the last entry; one for the length
\ of the string including a final end of line character (EOL)

hex

100 constant MAXCHARS   \ does not include EOL character
0a constant EOL   \ ASCII Line Feed (LF) is end of line character
8 constant INDENT \ number of spaces between line number and line text
char . constant EOI  \ End of Input (EOI) character, usuall "." 

decimal

\ TEXT is the name for the text we're working on. If you have a list already in
\ memory that you want to work on, you can save the first address in this
\ variable ("<ADDR> TEXT !") after loading FED. A zero in the link cell 
\ signals the end of the list
variable text  0 text !

\ We use the same machinery to accept a line from the user and one from
\ somewhere in memory
defer getpayload

\ Get a single line of text from user, add an EOL and store it. This is made
\ harder than it should be because ACCEPT does not add a EOL itself, and does
\ not allocate memory even when we save the string to HERE. However, we want
\ to have the built-in line editing functions of ACCEPT, so we deal with it.
\ Word is almost always called with HERE
: acceptline ( addr -- u)  
   dup  MAXCHARS  ( addr addr MAXCHARS)
   accept ( addr u ) 
   nip  ( u ) 
   EOL c, 1+ \ add EOL
   dup allot ; \ retroactively reserve the space we just used

\ Given an address in memory, return it with the length of the string to the
\ next EOL character. Note this just finds the line. This is the equivalent of
\ ACCEPTLINETEXT from the keyboard
: memoryline ( addr-s -- u ) 
   dup  ( addr-s addr-s ) 
   begin
      dup c@ EOL <> while
      char+  ( addr-s addr-s+1 ) 
   repeat
   over - ( addr-s u ) 
   1+  ( addr-s u+1 ) \ add one for the EOL character
   here swap  ( addr-s addr u+1 ) 
   dup >r
   move 
   r> ; 

\ Create a new line, header and all. The address passed as TOS depends on what
\ GETPAYLOAD is: If it is ACCEPTLINE, it is where the string is temporarily
\ stored, if it is MEMORYLINE, it is where to look for the new line. NEWLINE is reserved by Gforth
: makenewline ( addr-s -- addr )
   here dup  ( addr-s addr addr ) 
   cell+  ( addr-s addr addr+1 ) \ point to where length of string will be
   0 , 0 ,  \ save space for header information
   rot getpayload  ( addr addr+1 u ) 
   ! ( addr ) \ store length of string

\ Given an address in memory that points to the beginning of a text string,
\ return the address of the EOL character. Note if there is no EOL, this will
\ run through the memory
: eol-address  ( addr -- addr+1 )
   begin
      dup c@ EOL <> while
      char+  ( addr+1 ) 
   repeat
   1+ ;  \ add one to length for EOL character itself

\ Return the number of lines in the text, or, put differently, return the number
\ of the last line. Use: "#LINES", to print the last line "#LINES P"
: #lines  ( -- u) 
   0  text   ( 0 addr)
   begin
      dup while  ( 0 addr)  \ zero entry signals end of text
      1  ( 0 addr 1)
      rot   ( addr 1 0)
      + swap ( 1 addr)
      @  ( 1 addr)
   repeat
   drop 1- ;   \ we always count one line too many

\ Synonym for #LINES to make typing more intuitive
: lastline ( -- u )  #lines ; 

\ Convert a line number of a text to address. Note we start counting at 1, not
\ at 0, because this is a program for normal humans. Assumes we've checked that
\ text is not empty and that we have enought lines in text. 
\ Use: "2 NUM>ADDRESS"
: num>address ( u -- addr)
   text swap ( addr u)
   begin 
      dup 0<> while 
      swap @  ( u addr+1)
      swap 1- ( addr+1 u-1)
   repeat
   drop ; 

\ Return length of the line given its address. Includes the EOL character
: linelength ( addr - u )  cell+ @ ; 

\ See if line number given is out of range. We avoid CATCH and THROW because
\ simple Forths might not include them
: out-of-range? ( n -- f )  #lines > ; 

\ Test to see if we have an empty list, given the address of the first entry
\ Use: "EMPTY?"
: empty?  ( -- f)  text @  0= ; 

\ Get number range for whole text. Returns "0 0" if no text.
: all  ( -- n m )  empty? if 0 0 else 1 #lines then ; 

\ Convert line address to TYPE format. Includes the EOL character
: address>type ( addr -- addr u )
   cell+ dup @ ( addr+1 u) \ get length of string
   swap cell+ swap ; ( addr+2 u) \ get addr of first char

\ Given the address of a line, print it's payload (without the header),
\ including the EOL character. .LINE is reserved by Gforth. 
: .line-by-address  ( addr -- )  address>type  type ; 

\ Given the number of a line, print it
: .line-by-number ( u -- ) 
   dup out-of-range? if 
      ." ? (line out of range)" drop
   else
      num>address .line-by-address
   then ; 

\ Print spaces between line number and line text with commands such as N or NN
\ The line number adds a space so we delete one
: .indent ( -- ) INDENT 1- spaces ; 

\ Given the number of a line, print the number and indent. Note this will even
\ print a number if the line is out of range. This is intentional. 
: .linenumber ( u -- ) . .indent ; 

\ Print text of line line without a line number. Use: "1 P"
: p-raw ( u -- )  .line-by-number ; 
: p ( u -- ) cr p-raw ; 

\ Print text of line with a line number. Use: "1 N"
: n-raw ( u -- )  dup .linenumber .line-by-number ; 
: n ( u --) cr n-raw ; 

\ To print a range of lines in one single loop routine, we use either the p or
\ n command for the printing itself
defer printcommand

\ Print a range of lines, either with or without line numbers. Note that if the
\ last number is out of range, we print as many lines as we can and then stop
\ with an error message. This is used as a base for both PP and NN, don't call
\ this directly. 
\ TODO display output in pages so it doesn't run through
: .linerange ( n m -- )  cr  1+ swap  ?do  i printcommand  loop ; 

\ Print text from line n to line m without line numbers. Use: "1 2 PP". 
: pp ( n m -- )  ['] p-raw is printcommand  .linerange ; 

\ Ease-of-use synonym to print whole text without line numbers
: app ( -- ) all pp ; 

\ Print text from line n to line m with line numbers. Use: "1 2 NN". 
: nn ( n m -- )  ['] n-raw is printcommand  .linerange ; 

\ Ease-of-use synonym to print whole text without line numbers
: ann ( -- ) all nn ; 

\ See if we have received the "end of input" (EOI) character, traditionally
\ a "." as the first character in a new line. The line is two characters long
\ because we also have the EOL character
: eoi?  ( addr l -- f )
   2 =   ( addr f)
   swap c@ EOI = ( f f ) 
   and ; 

\ Given the address of a new line, insert it after the line with the number u.
\ If u is outside the range, silently append the new line to end of text
\ Use: "GETNEWLINE 1 INSERT-LINE-AFTER", but used internally
: insert-line-after  ( addr u -- ) 
   #lines min  ( addr-n u ) \ silently adjust range of target line)
   num>address  ( addr-n addr-a ) \ get address of append-target-line
   dup @  ( addr-n addr-a addr-b ) \ get addrss of line after append-target
   rot  ( addr-a addr-b addr-n )
   tuck  ( addr-a addr-n addr-b addr-n )
   !  ( addr-a addr-n ) \ link new line to line after append-target
   swap ! ; \ link append-target to new line

\ Given a line number, start appending text after it, inserting line by line. If
\ number is out of range, we append to the end of the text. Use: "2 aa"
: aa  ( u -- ) 
   acceptline is getpayload
   begin
      cr here acceptline ( u addr-l u )
   2dup eoi? invert while
      makenewline ( u addr ) 
      over ( u addr u)
      insert-line-after ( u )  
      1+  ( u+1 ) 
   repeat
   drop 2drop ; 


\ Ease-of-use synonym to add lines to the end of the text. This is what you
\ start off with when the text is empty. 
: laa  ( -- ) lastline aa ; 

\ Given a line number, insert lines before it. Any number smaller or equal to
\ zero will add text before first line, any number larger than the last line
\ will append it to the end of the text. Use: "2 II"
: ii  ( u -- ) 
   lastline 1+ min   \ if > than lastline, we're lastline 1+
   1-  \ move up one line before the one requested ...
   0 max  \ ... but not so far that we're negative
   aa ; 

\ Delete a line of text. This is used internally and not to be used by the
\ user
: deleteline ( u -- ) 
   dup num>address  ( u addr )
   @  ( u addr+1 ) \ get address that the deleted line was pointing to
   swap 1- ( addr+1 u-1) \ get line that was pointing to this one
   num>address ( addr+1 addr-1)
   ! ; 

\ Check to see if it is legal to delete the line, printing error string if
\ not. We want to avoid using ABORT because of the ugly printout and THROW/CATCH
\ because simple Forths might not support it
: linenumber-okay?  ( u -- f ) 
   empty? if 
      ." ? (text is empty)" drop false else
   dup 0= if
      ." ? (can't delete line 0)" drop false else
   dup out-of-range? if 
      ." ? (line " . ." is out of range)" false else
   drop true
      then then then ;

\ Check to see if range of numbers are not the same
: different-lines? ( n m -- f ) 
   = if ." ? (start and end are the same)" false else true then ; 
   
\ Delete a line of text, user version with safeguards. We abort than rather
\ guess what to delete because there is currently no undo function.
\ Use: "1 D"
: d  ( n -- )  dup linenumber-okay? if deleteline else drop then ; 

\ Delete a range of text. This is used internally and not to be used the user
: deletelines ( n m -- ) ?do i d -1 +loop ; 

\ Delete lines of text. We go from back to front because the line we are
\ deleting changes the links of those around it. Note that if start and end line
\ are the same, this will fail. Use: "1 3 DD" 
: dd  ( n m -- )  2dup different-lines? if deletelines else 2drop then ;

\ Given a line number, change it and add any further text the user might type
\ after the first line. In theory, we might be able to fine a way to let the
\ user manipulate the input buffer directly, but that would probably bring up
\ issues with the portability. Use: "1 C"
: c  ( u -- ) dup linenumber-okay? if dup d ii else drop then ; 

\ Given two line numbers, replace that range by whatever the user types in
\ Note that if we give a range that is only partially in the text, CC will
\ delete the part that can and then start with insert instead of aborting
\ Use: "1 2 CC"
: cc  ( n m -- ) 
   2dup different-lines? if 
      over swap ( n n m ) 
      deletelines ( n )
      ii 
   else 
      2drop then ; 

\ Write text to a given memory location as pure text, returning an address and
\ number fitting for the TYPE or other such command. This does not check if the
\ source and destination overlap, so use caution! Use: "HERE W"
: w  ( addr-t -- addr u )
   dup >r
   all 1+ swap ?do  ( addr-t ) 
      dup ( addr-t addr-t ) 
      i num>address address>type  ( addr-t addr-t addr u ) 
      dup >r   ( addr-t addr-t addr u ) ( R: u)
      -rot swap rot  ( addr-t addr addr-t u ) ( R: u) 
      move  ( addr-t ) 
      r> +  ( addr-t+u ) 
   loop  ( addr-t+u )
   r>  ( addr-t+u addr-t ) 
   tuck ( addr-t addrt+u addr-t)
   - ; ( addr-t u ) 

\ Write the text to a file under Gforth. Use: "W-FILE <FILENAME>". Will
\ overwrite any files you might have with the same name
: w-file  ( "name" -- )
   parse-name w/o create-file drop  ( fileid ) 
   here w  ( fileid addr u ) 
   rot dup >r  ( addr u fileid ) ( R: fileid)
   write-file if
      ." Error writing file" r> drop then 
   r> close-file drop ; 

