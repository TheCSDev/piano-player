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