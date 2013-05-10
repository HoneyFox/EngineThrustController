using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using EngineThrustController;
public class AutoFlight : Part
{

	protected Rect windowPos;
	static AutoFlight control;
	List<Part> Accesses=new List<Part>(4);
	List<Part> p1=new List<Part>(4); 
	List<Part> p2=new List<Part>(4);
	bool RState=false;
	public void exc ()
	{
		p1=new List<Part>(4);
		p2=new List<Part>(4);
		Accesses = vessel.Parts;
		foreach (Part i in Accesses) {
			if(i.Modules.Contains("ModuleEngineThrustController"))
			{
				ModuleEngineThrustController controller = i.Modules["ModuleEngineThrustController"] as ModuleEngineThrustController;
				if(controller.gp == 1)
					p1.Add(i);
				if(controller.gp == 2)
					p2.Add(i);
				Debug.Log("Lodaded an engine");
			}

		}

	}
	float abs (float a)
	{
		if (a > 0)
			return a;
		return -a;
	}
	public void Run ()
	{
		float l1 = 0, l2 = 0, ratio;
		foreach (Part i in p1) {
			//Debug.Log (vessel.transform.InverseTransformPoint (i.transform.position).x.ToString()+","+vessel.transform.InverseTransformPoint (i.transform.position).y.ToString()+","+vessel.transform.InverseTransformPoint (i.transform.position).z.ToString());
			l1 += (vessel.transform.InverseTransformPoint (i.transform.position) - vessel.findLocalCenterOfMass ()).y*i.maxThrust;
		}
		foreach (Part i in p2) {
			l2 += (vessel.transform.InverseTransformPoint (i.transform.position) - vessel.findLocalCenterOfMass ()).y*i.maxThrust;
		}
		//Debug.Log (l1.ToString()+"  "+l2.ToString());
		l1 = abs (l1);
		l2 = abs (l2);
		if (l1 > l2) {
			ratio = l2 / l1;
			foreach (Part i in p1) {
				ModuleEngineThrustController controller = i.Modules["ModuleEngineThrustController"] as ModuleEngineThrustController;
				controller.SetPercentage (ratio);
			}
		}
		else 
		{
			ratio = l1 / l2;
			foreach (Part i in p2) {
				ModuleEngineThrustController controller = i.Modules["ModuleEngineThrustController"] as ModuleEngineThrustController;
				controller.SetPercentage (ratio);
			}
		}
	}
	public void UnRun ()
	{
		foreach (Part i in p1) {
			ModuleEngineThrustController controller = i.Modules ["ModuleEngineThrustController"] as ModuleEngineThrustController;
			controller.SetPercentage (1);
		}
		foreach (Part i in p2) {
			ModuleEngineThrustController controller = i.Modules ["ModuleEngineThrustController"] as ModuleEngineThrustController;
			controller.SetPercentage (1);
		}
	}
	private void WindowGUI(int windowID)
	{
		GUIStyle mySty = new GUIStyle(GUI.skin.button); 
		mySty.normal.textColor = mySty.focused.textColor = Color.white;
		mySty.hover.textColor = mySty.active.textColor = Color.yellow;
		mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
		mySty.padding = new RectOffset(8, 8, 8, 8);
			
		GUILayout.BeginVertical();
		if (GUILayout.Button("Update Group Settings",mySty,GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
		{	
			exc();
		}
		if(GUILayout.Button("Take Control",mySty,GUILayout.ExpandWidth(true)))
		{
			RState=true;
		}
		if(GUILayout.Button("Restore",mySty,GUILayout.ExpandWidth(true)))
		{
			RState=false;
			UnRun();
		}
		GUILayout.EndVertical();
			
		//DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
		//clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
		//dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
		//it may "cover up" your controls and make them stop responding to the mouse.
		GUI.DragWindow(new Rect(0, 0, 10000, 20));
			
	}
	private void drawGUI()
	{
			if (control != this) {
				//Debug.Log ("unControled");
				return;
			}
		GUI.skin = HighLogic.Skin;
		windowPos = GUILayout.Window(212, windowPos, WindowGUI, "VTOL Control", GUILayout.MinWidth(100));	 
	}
	static void callcontrol (AutoFlight c)
		{
			//Debug.Log ("Did reached here?");
			if (control != c) {
				control = c;
				//Debug.Log("Controled");
			}
		}
	protected override void onFlightStart()  //Called when vessel is placed on the launchpad
	{
		Debug.Log ("Did reached here?");
		base.onFlightStart ();
		RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
		callcontrol (this);
	}
	protected override void onPartStart()
	{
		if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
		{
			windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
		}
	}
	protected override void onPartDestroy() 
	{
		RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
	}
	protected override void onPartFixedUpdate ()
	{
		base.onPartFixedUpdate ();
		callcontrol (this);
		if (RState)
			Run ();
	}

}
