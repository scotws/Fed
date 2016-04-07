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
new line. Save it to a file with ```ww-file <FILENAME>``` while remembering that
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
rendering commands that might not be present with a hobbyist system. In contrast,
line editors don't care about the screen at all, they just print lines to the
whatever output are there. In fact, ed was created in an age when output was to
paper teletypes. 

Fed is meant to be small. It the fully commented form, it currently is about
13.3 Kb in size. To make it easier to use in small, BYO systems, there is a
"stripped" version included ```fed-stripped.fs``` without any commentary at all.
This comes to 3.2 Kb in size.


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
should use the first two entries on the stack instead of the first one only.

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

| Command | Effect |
| --- | :--- |
| _n_ **aa** | Append one or more lines after _n._ Terminate input with "." (period) on a new line. |
| _n_ **c** | Replace ("change") line _n_ by one or more lines. Terminate input with "." (period) on a new line. |
| _n m_ **cc** | Replace ("change") lines _n_ to _m_ by one or more lines.  Terminate input with "." (period) on a new line. |
| _n_ **d** | Delete line _n._ |
| _n m_ **dd** | Delete lines _n_ to _m._ |
| _n_ **ii** | Insert one or more lines before _n._ Terminate input with "." (period) in a new line. |
| _n_ **n** | Print line _n_ with the line number. |
| _n m_ **nn** | Print lines _n_ to _m_ with the line numbers. |
| _n_ **p** | Print line _n_ without the line number. |
| _n m_ **pp** | Print lines _n_ to _m_ without the line numbers. |
| _addr u_ **rr** | Read a text consisting of one or multiple lines from memory, starting at location _addr_ and continuing for _u_ characters. The text is appended to existing text. |
| **rr-file** _filename_ | Read a text from file _filename,_ appending it to any existing text. Written for Gforth. |
| _addr u_ **rr** | Read a text consisting of one or multiple lines from memory, starting at location _addr_ and continuing for _u_ characters. The text is appended to existing text. |
| **rr-file** _filename_ | Read a text from file _filename,_ appending it to any existing text. Written for Gforth. |
| _addr_ **ww** | Write complete text to memory location _addr_ as pure text, returning _addr u_ on the stack in a format accepted for instance by the Forth TYPE word. |
| **ww-file** _filename_ | Write the complete text to file _filename,_ overwriting any existing file. Written for Gforth. |


## List of range words

Ed uses special characters such as ```%``` to select all of the text. Fed
defines two special words for ranges:

| Word | Effect |
| :--- | :--- |
| **all** | Expands to _1 ll,_ selecting the complete text |
| **ll** | Inserts number of the last line |

For example, ```2 ll pp``` prints lines 2 to whatever the last line is, and
```all nn``` prints the whole text with line numbers. Since some of these
commands are used so often, we define compound words.


## List of compound Fed commands

| Word | Effect |
| :--- | :--- |
| **0ii** | Insert at beginning of text. ```1ii``` would be the correct form, but the ```1``` at the beginning is too easily confused with the letter ```l``` with some fonts |
| **ann** | Print complete text with line numbers; expands to ```all nn``` |
| **app** | Print complete text without line numbers; expands to ```all pp``` |
| **laa** | Insert at end of text, expands to ```ll aa```. Used to start input after calling Fed |

## Examples 

Start editing by calling Fed with ```gforth fed.ed``` then type ```laa``` to add
text to the bottom.
```
This is line
It is very fine
And all mine
So don't whine
.
```
Note the final line with a dot to end input. The Forth interpreter will respond
with a ```ok``` after this (the feedback of the interpreter is why Fed does not
have a "prompt mode" like ed). To see the whole text with line numbers, type
```ann``` (short form for ```all nn``` which is a short form for ```1 ll nn```).
```
1        This is a line
2        It is very fine
3        And all mine
4        So don't whine
 ok
```
To remove the rather impolite last line, type ```4 d``` (note single "d"). Fed
will not display the changed line, but merely respond ```ok```. To insert a new
line after the second one, type ```2 aa``` followed by the text. The compound
command ```app``` now gives us: 
```
This is a line
It is very fine
Written in time
And all mine
 ok
```
To write this text to a file (with Gforth), use ```ww-file poetry.txt```
(remember this will destroy any file with that name).

To leave the editor, use the normal Forth word ```bye```.


## Missing ed features that (probably) will never be added

**Regular expressions.** Adding them would increase the size and complexity of
Fed dramatically. Also, writing software to support regular expressions is
slightly less fun than summoning Cthulhu. This is a hobby project,  not an
exercise in masochism.


## Further literature and links 

* Fed is discussed in a forum at [6502.org](http://forum.6502.org/viewtopic.php?f=9&t=3375)

## Legal stuff

This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
this program. If not, see
[http://www.gnu.org/licenses/](http://www.gnu.org/licenses/).


