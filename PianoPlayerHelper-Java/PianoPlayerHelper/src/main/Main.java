package main;

import java.awt.Robot;

public class Main
{

	public static void main(String[] args)
	{
		try
		{
			if(args.length == 2 && args[0].equals("key-press"))
			{
				int keyCode = Integer.parseInt(args[1]);
				Robot r = new Robot();
				r.keyPress(keyCode);
				r.keyRelease(keyCode);
			}
			else if(args.length == 2 && args[0].equals("key-down"))
			{
				int keyCode = Integer.parseInt(args[1]);
				Robot r = new Robot();
				r.keyPress(keyCode);
			}
			else if(args.length == 2 && args[0].equals("key-up"))
			{
				int keyCode = Integer.parseInt(args[1]);
				Robot r = new Robot();
				r.keyRelease(keyCode);
			
			}
		}
		catch (Exception e) { System.exit(1); }
	}

}
