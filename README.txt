Fed - A simple ed-inspired line editor in Forth 
Scot W. Stevenson <scot.stevenson@gmail.com>
First version: 10. July 2015
This version:  27. July 2015

This is a simple single-line text editor in Forth inspired by the Unix editor ed. Note that it is not a 1-to-1 port of ed, but retains its Forth nature.  


SO WHAT'S THIS ED THING ANYWAY?

Ed is a simple line editor, the oldest editor still shipped with modern Unix systems. The first version was written in 1971. Today it mostly used in jokes ("Vim? EMACS? Real programmers use ed!") and some shell scripts. 


WHAT'S A LINE EDITOR? SOUNDS PRIMITIV.

It is. As the name says, you are not free to move around the text as with a full-screen editor such as vim, but work one line at a time. Even worse, with ed, if you want to change a word in a line, you have to retype the whole line. And by default, the only error message you will get from ed is "?". For everything.


YUCK. WHY WOULD YOU WANT TO RECREATE SOMETHING LIKE THAT?

While writing my own version of Forth ("Tali Forth", FEHLT) for the 65c02 8-bit processor, I noticed that there was not a standard, simple editor that was not screen-based in the traditional Forth way. When your total address space is 64k, that's not much of a problem. But now I'm looking at building a 65816-based 8/16-bit computer, and with a 24-bit address bus -- up to 16 MByte of memory -- I'll be needing an editor. 


THEN WHY DON'T YOU WRITE A REAL ONE INSTEAD OF THIS THING?

I'm planning to, someday -- that's the "Far Future Forth Editor" (FFFE) project. I assure you it will be most magnificient and wonderful. However, I have zilch experience in writing editors, so this is part of the learning process. Also, line-based editors have the advantage that you don't have to care about rendering at all: The lines are just printed on the screen (ed was originally used with paper teletypes). I'd like to have one of those to include in my 16-bit Forth instead of a traditional screen-based editor.


IS THIS A 1-TO-1 REPRODUCTION OF ED?

No, that wouldn't make sense in a Forth environment. Though fed is inspired by ed, because of the stack-based nature of Forth, things have to be changed. Also, there are some improvements over ed. The most important is that you don't have to retype the whole line if you just want to change a word. See the Manual for a list of differences and how things work under the hood.


YOU'VE RELEASED FED INTO THE PUBLIC DOMAIN, BUT YOUR OTHER STUFF IS GPL.

Fed is intended to be a piece of Forth infrastructure. The inventor of the languge, Charles Moore, started the tradition of releasing it into the public domain, and so I'm following that tradition with this program. Note that you are using this software completely at your own risk, though.


WHAT IF I WANT TO KNOW MORE ABOUT FED?

There is a discussion thread about the editor at 6502.org: http://forum.6502.org/viewtopic.php?f=9&t=3375 That is also where new versions and features are announced. 
