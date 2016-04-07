# Fed, a simple ed-inspired line editor for Forth

Scot W. Stevenson <scot.stevenson@gmail.com>  
First version: 25. July 2015  
This version: 07. April 2015  

## For Impatient People

Fed is a simple, small line editor in Forth. The code is written in
[Gforth](https://www.gnu.org/software/gforth/). To test it, start Gforth with
```
gforth fed.fs
```
To start adding text, type ```laa```. End your input with a "." (period) on a
new line. Save it to a file with ```w-file <FILENAME>``` while remembering that
Fed will overwrite any other file with that name. To leave the editor, remember
that you're in Forth and type ```bye```.


## Background 

Nowadays, large Forth systems use files and normal editors such as vim or EMACS.
Forth editors traditionally work with a "screen"-based format of 16 lines of 64
characters for a total of 1024 bytes per screen (a "block").
[Comp.lang.forth](https://groups.google.com/forum/#!topic/comp.lang.forth/f1S_EotSc7g)
has a discussion about block editors; see
[VIBE](http://kestrelcomputer.github.io/kestrel/2016/03/29/vibe-2.2) as a modern
example of them, and the Gforth Manual [page on
blocks](https://www.complang.tuwien.ac.at/forth/gforth/Docs-html/Blocks.html)
for more background.

Block-based editors, however, are wasteful and require at least some basic
rendering commands that might not be present with a hobbist system. In contrast,
line editors don't care about the screen at all, they just print lines to the
whatever output are there. In fact, ed was created in an age when output was to
paper teletypes. 


## Basic Command Structure

Like ed, fed commands are composed of a "range" parameter that tells us which
line or lines we are dealing with, and the actually "command". The ed commands
Fed currently adapts are:

| Command | Does |
| --- | :--- |
| a | append after line |
| c | change line |
| d | delete line |
| i | insert before line |
| n | print line with line number |
| p | print line without line number |
| r | read text into editor |
| w | write text from editor |

(See the file TODO.txt for commands that are to be supported by future versions
of Fed) Ed puts the range parameters directly before the command, without
spaces, separated by a comma - ```1p``` for instance prints the first 
line without a line number, and ```1,2n``` prints the first two lines with line
numbers. 

This is unnatural for Forth, which works with the stack. We therefore split the
range parameter and the command, which gives us ```1 p``` for the first example.
The second example gets us into trouble: Forth would not know that ```1 2 p```
should use the firt two entries on the stack instead of the first one only.

To get around this, we use **single letters for single lines** and **double
letters for ranges.** This way, ed's ```1,2p``` becomes ```1 2 pp``` in Fed.
Other changes are the use of the Forth ```( addr u )``` format for strings, and
the ability to read and write to memory regions for small computers that don't
support file systems. 

Put together, this gives us the following Fed words.


## List of simple Fed commands

There are no single-line instructions such as "a" or "i" for input, because
input is potentially always multi-line, and terminated by a "." (period) as the
first character of a new line. We keep ```d``` and ```c``` as single-line
instructions for safety reasons, because they ensure that only one line is
deleted. Note there is no way to undo any of these actions.

_n_ **aa** - Append one or more lines after _n._ Terminate input with "." (period)
on a new line.

_n_ **c** - Replace ("change") line _n_ by one or more lines. Terminate input with
"." (period) on a new line. 

_n m_ **cc** - Replace ("change") lines _n_ to _m_ by one or more lines. Terminate
input with "." (period) on a new line.

_n_ **d** - Delete line _n._ 

_n m_ **dd** - Delete lines _n_ to _m._

_n_ **ii** - Insert one or more lines before _n._ Terminate input with "."
(period) in a new line.

_n_ **n** - Print line _n_ with the line number.

_n m_ **nn** - Print lines _n_ to _m_ with the line numbers.

_n_ **p** - Print line _n_ without the line number.

_n m_ **pp** - Print lines _n_ to _m_ without the line numbers.

_addr u_ **rr** - Read a text consisting of one or multiple lines from memory,
starting at location _addr_ and continuing for _u_ characters. The text is
currently appended to any existing text.

*rr-file* _<filename>_ - Read a text from file _<filename>,_ appending
it to any existing text. Written for Gforth.

_addr u_ **rr** - Read a text consisting of one or multiple lines from memory,
starting at location _addr_ and continuing for _u_ characters. The text is
currently appended to any existing text.

**rr-file** _<filename>_ - Read a text from file _<filename>,_ appending
it to any existing text. Written for Gforth.

_addr_ **ww** - Write complete text to memory location _addr_ as pure text,
returning _addr u_ on the stack in a format accepted for instance by the Forth
TYPE word.

**ww-file** _<filename>_ - Write the complete text to file _<filename>,_
overwriting any existing file. Written for Gforth.



## Special range words





MISSING ED FEATURES THAT (PROBABLY) WON'T BE ADDED

Regular expressions. Adding them would increase the size and complexity of fed
dramatically. Also, writing software to support regular expressions is slightly
less fun than summoning Cthulhu, and this is currently a hobby project, not an
exercise in masochism.


LITERATURE AND LINKS 

See the 6502.org Forum at http://forum.6502.org/viewtopic.php?f=9&t=3375 for the
discussion thread on this editor.
