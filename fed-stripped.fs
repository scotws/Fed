hex
100 constant MAXCHARS
0a constant EOL
8 constant INDENT
char . constant EOI
10000 constant MAXFILESIZE
decimal
variable text  0 text !

: #lines
   0  text
   begin
      dup while 1 rot + swap @  
   repeat
   drop 1- ; 

: ll  #lines ; 

: num>address
   text swap
   begin 
      dup 0<> while swap @ swap 1-
   repeat
   drop ; 

: out-of-range? #lines > ; 
: empty? text @  0= ; 
: all empty? if 0 0 else 1 #lines then ; 
: address>type cell+ dup @ swap cell+ swap ;
: .line-by-address  address>type  type ; 

: .line-by-number
   dup out-of-range? if 
      ." ? (line out of range)" drop
   else
      num>address .line-by-address
   then ; 

: .indent INDENT 1- spaces ; 
: .linenumber  . .indent ; 
: p-raw  .line-by-number ; 
: p  cr p-raw ; 
: n-raw  dup .linenumber .line-by-number ; 
: n  cr n-raw ; 

defer printcommand

: .linerange cr  1+ swap  ?do  i printcommand  loop ; 
: pp  ['] p-raw is printcommand  .linerange ; 
: app all pp ; 
: nn  ['] n-raw is printcommand  .linerange ; 
: ann  all nn ; 
: eoi?  1 = swap c@  EOI = and ; 
: insert-line-after #lines min num>address dup @ rot tuck !  swap ! ;
: dummyheader, 0 , 0 , ; 

: aa
   begin
      here dup cell+ cell+ dup MAXCHARS cr accept 
   tuck eoi? invert while
      dummyheader, dup allot EOL c, 1+ over cell+ !  over insert-line-after 1+ 
   repeat
   drop 2drop ; 

: laa  ll aa ; 
: ii ll 1+ min 1- 0 max aa ; 
: 0ii  0 ii ; 
: deleteline dup num>address @ swap 1- num>address ! ; 

: linenumber-okay?
   empty? if 
      ." ? (text is empty)" drop false else
   dup 0= if
      ." ? (can't delete line 0)" drop false else
   dup out-of-range? if 
      ." ? (line " . ." is out of range)" false else
   drop true
      then then then ;

: different-lines?
   = if ." ? (start and end are the same)" false else true then ; 
   
: d  dup linenumber-okay? if deleteline else drop then ; 
: deletelines  ?do i d -1 +loop ; 
: dd  2dup different-lines? if deletelines else 2drop then ;
: c  dup linenumber-okay? if dup d ii else drop then ; 
: cc
   2dup different-lines? if 
      over swap deletelines ii 
   else 
      2drop then ; 

: #memlinechars
   dup
   begin
      dup c@ EOL <> while char+
   repeat
   1+ swap - ; 

: addmemline
   dup >r here dup >r cell+ cell+ swap move r> r> dummyheader, 
   dup allot over cell+ !  ll insert-line-after ; 

: rr
   begin
      over #memlinechars
   2dup >= while
      swap -rot 2dup addmemline dup >r + swap r> -
   repeat
   drop 2drop ; 

: rr-file
   parse-name r/o open-file
      0<> if ." Error opening file" then
   dup MAXFILESIZE allocate
      0<> if ." Error allocating memory" then
   dup >r
   MAXFILESIZE rot
   read-file
      0<> if ." Error reading file" then
   swap close-file
      0<> if ." Error closing file" then
   r> swap over -rot 2dup + 1+ EOL swap c!  1+ rr free
      0<> if ." Error freeing buffer after reading file" then ; 

: ww
   dup >r
   all 1+ swap ?do  
      dup i num>address address>type  dup >r -rot swap rot move r> +
   loop
   r>
   tuck
   - ; 

: ww-file 
   parse-name w/o create-file drop 
   here ww rot dup >r write-file if
      ." Error writing file" r> drop then 
   r> close-file drop ; 

