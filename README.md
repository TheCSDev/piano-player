# Piano Player
Piano Player is a macro-like application which automatically presses keys on your keyboard. The purpose of this application is to automatically play virtual instruments featured in websites, applications, and video games. An example of where this application can be used is https://virtualpiano.net/. The Virtual Piano website features a virtual piano which can be played using your keyboard. With this application, you could automate that. Virtual Piano already features a built-in auto-player which can be used to auto-play the virtual piano, but some other applications and video games do not, which is where you could use this application.

![Screenshot 2021-05-07 235029](https://user-images.githubusercontent.com/66475965/117512006-0d1b2900-af8f-11eb-81a7-87be5e19fac5.png)</br>
Piano Player v1.3.1

# How it works
Piano Player works in a way where you provide it with a piano "sheet", and then you press the "Play" button and the player will begin working. A sheet is a set of instructions telling the player which keys to press and when to press them.</br>
An example of a sheet is something like this:</br>

>a b c | d e f</br>

As seen above, the sheet contains letters, spaces, and a break (aka '|'). Letters represent the piano keys that the player will press, spaces are small breaks that tell the player to wait a little before pressing the next key, and breaks (aka '|') are bigger breaks that will tell the player to wait a little longer before pressing the next keys.

![Screenshot 2021-05-08 001451](https://user-images.githubusercontent.com/66475965/117513631-6e90c700-af92-11eb-80f4-7062713d2127.png)

When writing a sheet, you can also define starting time per note, space, and break (aka tpn, tps, and tpb), they are measured in milliseconds (ms) (1000ms = 1sec). Time per note is the amount of time the player will wait after automatically pressing a key on your keyboard, time per space is the amount of time the player will wait when there is a space between notes, and time per break is the amount of time the player will wait when there is a break (aka '|').

Note groups are groups of notes that will be played at the same time.</br>
An example of note groups is something like this:</br>

>a b c | \[def\] | g h i</br>

As seen above, the "d", "e", and "f" keys are placed in a note group. To define a group of notes, "\[" and "\]" are used.

# = Old description below, a new one is being written above =
# Some info about the application

Piano Player is a macro-like application mainly designed to work on the http://virtualpiano.net/ website, but can work on other
websites and applications as well. Piano Player is designed to play a keyboard piano by simulating physical keyboard input.

The project is written in both C# and Java (but mainly C#), and doesn't require Java to be installed in order to run.
The IDE used to write the C# code is Visual Studio while Eclipse is used for Java.

If you wish to create your own fork of this project, make sure you have Visual Studio installed as well as the ".NET Desktop development"
workload installed, and if you wish to modify the Java code then make sure you have the Eclpise IDE installed and make sure the Java
compiler compliance level is set to 1.8 so that the project is always exported with the Java 8 version because exporting the Java
project using a higher version will result in having to install the higher version of Java which most users don't do.

# Exporting instructions

1.  Open the Visual Studio project using the Visual Studio IDE
2.  Set the Solution Configuration to "Release" (not "Debug")
3.  Build the project

4.  Open the Elipse Java helper project using the Eclipse IDE
5.  In the package explorer, right click the project
6.  Select "Properties"
7.  Go to "Java Compiler"
8.  Make sure JDK Compliance settings are set to 1.8 (Java 8)
    because the jar needs to be exported with the Java 8 version of Java.
9.  Close the properties window, right click the project in the project explorer and select "Export"
10. Export the project as a runnable jar file (jar entry point is: Main.java)
    Make sure the name of the exported jar file is "PianoPlayerHelper" and that the file extension is "jar".

11. Create an empty folder
12. Move the contents of the exported Visual Studio project to the new empty folder
    Exported content is usually located at: "piano-player\Visual Studio Project\Piano Player\bin\Release"
13. Move the exported Java project jar file to the new folder

14. That's all, the app should now be ready to use
00. You could use a 3rd party tool such as https://jrsoftware.org/isinfo.php to create an installer for the app
