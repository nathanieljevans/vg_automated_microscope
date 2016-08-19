README for vg_automated_microscope - August 17, 2016

VERSIONS
HARDWARE
SETUP AND RUN
STAGE COMMANDS
ADDITIONAL INFORMATION
=================================================================
SOFTWARE VERSIONS:
• priorStagePattern.py was created in Python 3.5.2
• camera_vg_take2.exe was created in Visual Studio Community 2015 using OpenCV 3.1.0
=================================================================
HARDWARE:
• priorStagePattern.py programs the Prior Scientific ProScan III Universal Microscope 
Automation Controller for the H101ANN Prior Scientific motorized stage

• camera_vg_take2.exe programs an MU500 Amscope Camera

• the stage and camera are mounted on an Olympus BX41 Microscope
=================================================================
SETUP and RUN:
Download the files priorStagePattern.py and camera_vg_take2.exe, both to the same location
Download Python 3.5 and save it to that same location
From the command line, start up python 3.5 and run priorStagePattern.py. The images will be saved 
to a folder called "Images" in the same location that you saved the above files.

If you would like to see the camera's livestream, run camera_vg_take2_showVid.exe which will show 
a video from the camera in a pop-up window. Press any key to exit out of the viewing window. 
You can also use ToupView software to view a live video from the camera. However, it is best to close 
these viewing windows before you run priorStagePattern.py to capture images since the camera connection may 
get overloaded if too many programs are running.

You can change the set starting and ending points of the pattern in priorStagePattern.py.

If you need to edit the executable (camera_vg_take2.exe), download the Visual-Studio-2015 folder from 
github and run "camera_vg_take2_newsln.sln" in Visual Studio 
(located on GitHub in vg_automated_microscope/Visual-Studio-2015/camera_vg_take2_newsln/)
	Edit camera_vg_take2.exe to change the image file type, change the location 
	that the images are saved to, change camera settings, or change the resolution of the images. To change
	resolution, edit the numerical values for CAP_PROP_FRAME_WIDTH and CAP_PROP_FRAME_HEIGHT.
=================================================================
STAGE COMMANDS:
Run the "basic_stage_control" program (python) to send commands directly to the stage. 
Some discriptions are quoted from the Prior Scientific ProScan III Manual.
To separate parts of a command use a space, comma, tab, colon or semicolon. 
For example, G 20 20 could be written as:
		G 20 20
		G,20,20
		G;20;20
• G x y --> "go to (x, y)" (example: G 30 50 = go to (30,50))
• M --> moves stage to position (0,0)
• J --> enables joystick
• H --> disables joystick
• PX --> reports current x position
• PY --> reports current y position
• X --> Reports the current step sizes for x and y in user units (see "Step Size" below about use units)
• B y --> moves back y number of steps (y is a number)
• F y --> moves forward y steps 
• R x --> moves to the right x number of steps (x is a number)
• L x --> moves to the left x steps

• JXD d (where d is a number) -->
		"Sets the direction of X axis under joystick control.
		d = 1: Joystick right, moves stage mechanically right
		d = -1: Joystick right, moves stage mechanically left."

• JYD d (where d is a number) -->
		"Sets the direction of Y axis under joystick control
		d = 1: Joystick forward, moves stage mechanically forward. 
		d = -1: Joystick forward, moves stage mechanically back."

• I --> "Stops movement in a controlled manner to reduce the risk of 
		losing position. The command queue is also emptied." 

• K --> "Immediately stops movement in all axes. Mechanical inertia may result 
		in the system continuing to move for a short period after the command 
		is received. In this case, the controller position and mechanical 	
		position will no longer agree. The command queue is also emptied. 
		This command is normally treated as an 	emergency stop."
• Step Size: 
		"The default convention is that the controller will move each stage  device by 1µm per number entered, 
		in other words a requested move of 1000,0 will result in the stage moving 1mm in the X axis. If desired 
		this can be over-ridden by using the scale stage (SS) command. The stage scale is determined by the model 
		of stage in use. A stage with a 2mm ball screw and a 200 step motor has an SS value of 25. See Appendix B 
		for full explanation of microsteping calculations. Changing SS value to 100 and requesting a move of 
		1000,0 will now result in the stage moving 4mm in the x axis."


For a complete list of software commands for the stage controller, 
refer to the Prior Scientific ProScan III Manual on page 40:
http://www.prior-us.com/files/ProScan%20III_%20Manual.pdf
Refer to page 58 of the manual for error codes.
=================================================================
ADDITIONAL INFORMATION:
• Use the knob on the right side of the microscope under the stage to adjust 
the amount of light. If the camera images turn out completely white this may be 
because there is too might light. If images show up all black add more light.

• Note that when the priorStagePattern.py code runs, the joystick is disabled while 
the pattern is running to prevent the user from accidently interfering with image 
data collection. The joystick gets enabled again at the end of the program, but if you 
close out the pattern program before it finishes, the joystick will remain disabled. 
Use the basic_stage_code with command "J" to enable the joystick again or restart the controller.

• The buttons to the right and left of the joystick are not programmed to do anything specific, 
but by default they cause the stage to move a certain amount when pressed. 
THEY WORK EVEN WHEN THE JOYSTICK IS DISABLED so don't bump them accidentally during data collection.

* The Visual Studio Solution User Options (.suo) files are not included in the Visual-Studio-2015 
folder because the files are too big to upload to GitHub.
