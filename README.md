# Fed - A ed-inspired line editor in Forth 

Fed is a simple line editor in Forth inspired by the classical Unix editor ed.
It is designed to be small enough to be included in BYO Forth systems for
processors such as the 6502 or 65816. Stripped of its comments, it comes to
about FEHLT Kb of code.  

## Who's Ed? 

It's a simple line editor, the oldest editor still shipped with modern Unix
systems. The first version was written in 1971. Today it mostly used for jokes
("Vim? EMACS? Real programmers use ed!") and some shell scripts. See [the
Wikipedia entry](https://en.wikipedia.org/wiki/Ed_(text_editor)) on ed and [the
source code](http://www.gnu.org/software/ed/) at GNU.org. 

## So Fed is a Forth clone of ed? 

No, that wouldn't make sense in a Forth environment. Though Fed is inspired by
ed, things have to be different because of the stack-based nature of Forth --
see the Fed Manual for details. Both are line editors, and some commands are the
same, but there are also major differences.


## What's a line editor anyway? Sounds primitive.

It is. As the name says, you are not free to move around the text as with a
full-screen editor such as vim, but work one line at a time. Even worse, 
if you want to change a word in a line, you have to retype the whole thing.


## That's stupid. Why would you want to recreate something like that?

While writing my own Forth, ["Tali Forth"](https://github.com/scotws/TaliForth)
for the 6502, I realized that there was no standard, simple, small, portable
editor that was not screen-based in the traditional Forth way (see Samuel A.
Falvo II's [Vibe](http://kestrelcomputer.github.io/kestrel/2016/03/29/vibe-2.2)
as an example of a block editor). So I decided to write my own.

## Seriously, why don't you write a _real_ editor instead?

I'm planning to, someday -- that's the "Far Future Forth Editor" (FFFE) project.
I assure you it will be most magnificent, awesome, and wonderful. However, I
have zero experience in writing editors, so this is part of the learning
process. Also, I need a small, portable Forth editor right now. 

## Where can I find more information about Fed?

There is a [discussion thread](http://forum.6502.org/viewtopic.php?f=9&t=3375) 
about Fed and Forth editors in general at 6502.org.

## Well, good luck with Fed.

Thanks! -- Scot W. Stevenson <scot.stevenson@gmail.com>
