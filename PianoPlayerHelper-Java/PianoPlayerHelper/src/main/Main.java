package main;

import java.awt.AWTException;
import java.awt.Robot;
import java.util.Scanner;
import java.util.concurrent.Executors;

public class Main
{
	public static Robot robot;
	public static long lastPingTime;
	
	public static Scanner in_s = new Scanner(System.in);
	
	public static void main(String[] args)
	{
		lastPingTime = System.currentTimeMillis();
		
		//Just in case the DebugPlayerHelper() from the C# class decides to
		//test if Java works properly, exit the Java program because it needs to close itself.
		//Aka, make sure this app only runs when the specific argument is given.
		if(!String.join(" ", args).equals("start-helper")) return;
		
		try { robot = new Robot(); }
		catch (AWTException e1)
		{
			System.err.println("PianoPlayerHelper has failed to create an "
					+ "instance of the Robot class.");
			System.exit(1);
		}
		
		//create a thread that will monitor pings
		Executors.newCachedThreadPool().execute(new Runnable()
		{
			@Override
			public void run()
			{
				while(true)
				{
					try { Thread.sleep(100); } catch(Exception e) {}
					
					//Thanks to timeout, the helper will automatically exit about 3 seconds
					//after the main player application exits.
					if(System.currentTimeMillis() - lastPingTime > 3000) System.exit(0);
				}
			}
		});
		
		//Get input commands
		while(true)
		{
			//Get the commands from the window title and execute them,
			//then reset the title and wait for new commands
			try
			{
				String cmd = in_s.nextLine();
				if(cmd.startsWith("/"))
				{
					cmd = cmd.substring(1);
					ExecuteCommand(cmd);
				}
			}
			catch(Exception e) { try { Thread.sleep(50); } catch(Exception e2) {} }
			
		}
	}
	
	public static void ExecuteCommand(String cmd)
	{
		if(cmd.startsWith("key-press "))
		{
			cmd = cmd.substring(10);
			try
			{
				int keyCode = Integer.parseInt(cmd);
				robot.keyPress(keyCode);
				robot.keyRelease(keyCode);
				return;
			}
			catch (Exception e) {}
		}
		else if(cmd.startsWith("key-down "))
		{
			cmd = cmd.substring(9);
			try
			{
				int keyCode = Integer.parseInt(cmd);
				robot.keyPress(keyCode);
				return;
			}
			catch (Exception e) {}
		}
		else if(cmd.startsWith("key-up "))
		{
			cmd = cmd.substring(7);
			try
			{
				int keyCode = Integer.parseInt(cmd);
				robot.keyRelease(keyCode);
				return;
			}
			catch (Exception e) {}
		}
		else if(cmd.startsWith("ping"))
		{
			lastPingTime = System.currentTimeMillis();
			return;
		}
	}
}
