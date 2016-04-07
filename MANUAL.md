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

To get around this, we use *single letters for single lines* and *double letters
for ranges.* This way, ed's ```1,2p``` becomes ```1 2 pp``` in Fed.  Other
changes are the use of the Forth ```( addr u )``` format for strings, and the
ability to read and write to memory regions for small computers that don't
support file systems. 

Put together, this gives us the following Fed words.


## List of Fed Commands



*_n_ aa* - Add one or more lines to 



LIST OF FED COMMANDS

Most of these are taken from ed or inspired by them. "n" means a single-line parameter, "n m" a multi-line version. Note that where ed uses the same command for single- and multi-line operations (eg "p" to print one or more lines), we can't do that in Forth because we have no way of knowing how many parameters are on the stack. We solve this problem by doubling the original ed command: 

n p                - "Print" line n to screen (without line numbers)
n m pp                - "Print" lines n to m screen (without line numbers)

This currently gives us these further commands:


n n                - Print line n (with line numbers)
n m nn                - Print lines n to m (with line numbers)


n c                - Editor line n ("change")


n d                - "Delete" line n
n m dd                - "Delete" lines n to m 


j                - "Join" the current line with the one below it [WON'T WORK, FORTH J]
n m jj                - "Join" the lines n to m. N is then the new current line.


n a                - "Append" new line below line n and start editing
n i                 - "Insert" new line above line n and start editing [WON'T WORK FORTH I]



MISSING ED FEATURES THAT (PROBABLY) WON'T BE ADDED

Regular expressions. Adding them would increase the size and complexity of fed
dramatically. Also, writing software to support regular expressions is slightly
less fun than summoning Cthulhu, and this is currently a hobby project, not an
exercise in masochism.


LITERATURE AND LINKS 

See the 6502.org Forum at http://forum.6502.org/viewtopic.php?f=9&t=3375 for the
discussion thread on this editor.
