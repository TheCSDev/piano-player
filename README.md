# Piano Player
Piano Player is a macro-like application which automatically presses keys on your keyboard. The purpose of this application is to automatically play virtual instruments featured in websites, applications, and video games. An example of where this application can be used is https://virtualpiano.net/. The Virtual Piano website features a virtual piano which can be played using your keyboard. With this application, you could automate that. Virtual Piano already features a built-in auto-player which can be used to auto-play the virtual piano, but some other applications and video games do not, which is where you could use this application.

![Screenshot 2021-05-07 235029](https://user-images.githubusercontent.com/66475965/117512006-0d1b2900-af8f-11eb-81a7-87be5e19fac5.png)</br>
Piano Player v1.3.1

# How it works
Piano Player works in a way where you provide it with a piano "sheet", and then you press the "Play" button and the player will begin working. A sheet is a set of instructions telling the player which keys to press and when to press them.</br>
An example of a sheet is something like this:</br>

>a b c | d e f</br>

As seen above, the sheet contains letters, spaces, and a break (aka '|'). Letters represent the piano keys that the player will press, spaces are small breaks that tell the player to wait a little before pressing the next key, and breaks (aka '|') are bigger breaks that will tell the player to wait a little longer before pressing the next keys.

![Screenshot 2021-05-09 154137](https://user-images.githubusercontent.com/66475965/117574383-16b2a700-b0dd-11eb-96b8-d93877727756.png)

When writing a sheet, you can also define starting time per note, space, and break (aka tpn, tps, and tpb), they are measured in milliseconds (ms) (1000ms = 1sec). Time per note is the amount of time the player will wait after automatically pressing a key on your keyboard, time per space is the amount of time the player will wait when there is a space between notes, and time per break is the amount of time the player will wait when there is a break (aka '|').

Note groups are groups of notes that will be played at the same time.</br>
An example of note groups is something like this:</br>

>a b c | \[def\] | g h i</br>

As seen above, the "d", "e", and "f" keys are placed in a note group. To define a group of notes, "\[" and "\]" are used.

There are more advanced note groups called "player commands" which can be used to make the player do something or to change the player settings mid-way through a sheet.

# Player Commands
Player commands are used to tell the player to do something at a certain point in time while playing a sheet. Player commands can be used to do things such as making the player wait a certain amount of time before playing the next notes and modifying the player settings (tpn/tps/tpb) mid-way through a sheet.</br>
Each command starts with a the command prefix. The currently default command prefix for Piano Player is \"\_\".</br>
Commands are defined inside of note groups ("\[" and "\]"). An example of a player command is something like this:

> \[\_w 1000\]</br>

The player command above makes the player wait 1000ms (aka 1sec) before playing the next notes.</br>
Below is the list of currently available commands for the latest Piano Player version.</br>

<b>w [time ms]</b></br>
Makes the player wait a certain amount of time before playing the next set of notes.</br>
Example: \[\_w 1000\]</br>
</br>
<b>tpn [time ms]</b></br>
Sets the time per note.</br>
Example: \[\_tpn 150\]</br>
</br>
<b>tps [time ms]</b></br>
Sets the time per space.</br>
Example: \[\_tps 100\]</br>
</br>
<b>tpb [time ms]</b></br>
Sets the time per break.</br>
Example: \[\_tpb 300\]</br>
</br>
<b>reverse</b></br>
Reverses all previous notes.</br>
Command example:</br>
\- Input: a b c d \[\_reverse\] e f g</br>
\- Output: d c b a e f g</br>
</br>
