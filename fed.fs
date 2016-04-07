\ Forth Ed (fed) - An ed-inspired single-line text editor for Forth
\ Copyright 2016 Scot W. Stevenson <scot.stevenson@gmail.com>
\ Written with gforth 0.7
\ First version: 10. Jul 2015 
\ This version: 07. April 2016

\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
10000 constant MAXFILESIZE \ 64Kb maximal filesize for reading

decimal

\ TEXT is the name for the text we're working on. If you have a list already in
\ memory that you want to work on, you can save the first address in this
\ variable ("<ADDR> TEXT !") after loading FED. A zero in the link cell 
\ signals the end of the list
variable text  0 text !

\ Return the number of lines in the text, or, put differently, return the number
\ of the last line
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

\ Synonym for #LINES to make typing more intuitive. Use: "LASTLINE P"
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

\ See if line number given is out of range. We avoid CATCH and THROW because
\ simple Forths might not include them
: out-of-range? ( n -- f )  #lines > ; 

\ Test to see if we have an empty list, given the address of the first entry
\ Use: "EMPTY?"
: empty?  ( -- f)  text @  0= ; 

\ Get number range for whole text. Returns "0 0" if no text.
: all  ( -- n m )  empty? if 0 0 else 1 #lines then ; 

\ Convert line's payload to TYPE format, using its address. Includes the EOL 
\ character
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
: eoi?  ( addr u -- f )
   1 =   ( addr f)
   swap c@  EOI = ( f f ) 
   and ; 

\ Given the address of a new line, insert it after the line with the number u.
\ If u is outside the range, silently append the new line to end of text
: insert-line-after  ( addr u -- ) 
   #lines min  ( addr-n u ) \ silently adjust range of target line)
   num>address  ( addr-n addr-a ) \ get address of append-target-line
   dup @  ( addr-n addr-a addr-b ) \ get addrss of line after append-target
   rot  ( addr-a addr-b addr-n )
   tuck  ( addr-a addr-n addr-b addr-n )
   !  ( addr-a addr-n ) \ link new line to line after append-target
   swap ! ; \ link append-target to new line

\ Save two zeros as placeholder for the headers
: dummyheader, ( -- )  0 , 0 , ; 

\ Given a line number, start appending text after it, inserting line by line. If
\ number is out of range, we append to the end of the text. This is the main
\ insertion word. It works by saving the string that is typed in at the position
\ where it would be if the line is created, without committing at first.
: aa  ( u -- ) 
   begin
      here dup  ( u addr addr ) 
      cell+ cell+ dup MAXCHARS  ( u addr addr+2 addr+2 u )
      cr accept ( u addr addr+2 u ) 
   tuck eoi? invert while
      dummyheader,   ( u addr u )
      dup allot   \ retroactively reserve space we used for string
      EOL c, 1+  ( u addr u+1 ) \ add EOL character and allot byte for it
      over cell+ !  ( u addr ) \ save length of string
      over insert-line-after 
      1+ 
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

\ Ease-of-use synonym to add lines to top of the text. This is another thing you
\ can start off with when the text is empty
: 0ii  ( -- ) 0 ii ; 

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

\ Given the address of a line of text in memory, return the length of that line
\ including the final EOL character
: #memlinechars ( addr -- u )
   dup  ( addr addr )
   begin
      dup c@ EOL <> while
      char+  ( addr addr+1 )
   repeat
   1+  \ we need length, not count
   swap - ; 

\ Add a string from memory to the text. Assumes that string includes EOL 
\ character
: addmemline ( addr n -- )
   dup >r  ( addr n ) ( R: n )
   here  ( addr n addr-h ) ( R: n )
   dup >r ( addr n addr-h ) ( R: n addr-h )
   cell+ cell+ ( addr n addr-h+2 ) ( R: n addr-h ) \ start of string area
   swap move  ( ) ( R: n addr-h )
   r> r>  ( addr-h n )
   dummyheader, 
   dup allot ( addr-h n ) \ MOVE doesn't allot memory
   over cell+ ! ( addr-h ) \ save length of string
   lastline  ( addr-h n )
   insert-line-after ; 

\ Read a text with EOL-terminated strings from a memory range into the editor,
\ appending it at the end of the text
: rr ( addr n -- )
   begin  ( addr n0 )
      over #memlinechars  ( addr n0 n )
   2dup >= while
      swap  ( addr n n0 )
      -rot  ( n0 addr n )
      2dup addmemline ( n0 addr n )
      dup >r  ( n0 addr n ) ( R: n ) 
      +  ( n0 addr-n ) ( R: n )
      swap r>  ( addr-n n0 n ) 
      -  ( addr-n n )
   repeat
   drop 2drop ; 

\ Read text with EOL-Terminated strings from a file into the editor under Gforth. 
\ Use: "R-FILE <FILENAME>". Appends to the end of the text. Currently, this
\ routine ignores all error messages
: rr-file  ( "name" -- )
   parse-name r/o open-file ( fileid f )
      0<> if ." Error opening file" then
   dup  ( fileid fileid ) 
   MAXFILESIZE allocate ( fileid fileid addr f ) 
      0<> if ." Error allocating memory" then
   dup >r  ( fileid fileid addr ) ( R: addr )
   MAXFILESIZE rot  ( fileid addr n fileid ) ( R: addr )
   read-file ( fileid n f ) ( R: addr ) 
      0<> if ." Error reading file" then
   swap close-file ( n ) ( R: addr ) 
      0<> if ." Error closing file" then
   r> swap ( addr n ) 

   \ RR is unhappy if there is no final EOL, so we add one
   over -rot ( addr addr n ) 
   2dup + 1+  ( addr addr n addr+n+1 ) 
   EOL swap c! ( addr addr n ) 
   1+  ( addr addr n+1 ) 
   rr  ( addr )
   free ( f) \ give back the buffer that isn't needed anymore
      0<> if ." Error freeing buffer after reading file" then ; 

\ Write text to a given memory location as pure text, returning an address and
\ number fitting for the TYPE or other such command. This does not check if the
\ source and destination overlap, so use caution! Use: "HERE WW"
: ww  ( addr-t -- addr u )
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
: ww-file  ( "name" -- )
   parse-name w/o create-file drop  ( fileid ) 
   here ww  ( fileid addr u ) 
   rot dup >r  ( addr u fileid ) ( R: fileid)
   write-file if
      ." Error writing file" r> drop then 
   r> close-file drop ; 

\ END 
