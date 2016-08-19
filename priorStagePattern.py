import time 
import serial 
import subprocess #This imports subprocess itself
from subprocess import *  #This imports PIPE and universal_newlines for subprocess
 
# configure the serial connections
ser = serial.Serial( 
    port='COM3', #open the port
    baudrate=9600, 
    parity=serial.PARITY_ODD, #serial will return a string -- lines 12, 13, & 14 were in original starter code, this is what I found on google
    stopbits=serial.STOPBITS_TWO, #convert num or str to int
    bytesize=serial.SEVENBITS #similar to STOPBITS_TWO
) 
 
 
print ("opening port") 
if ser.isOpen() is not True:           #close if the port is not open
    print ("...port is not open...") 
    exit 
 
 
print ('Follow the setup instructions.\nInsert "exit" to leave the application.\nCommands are not cap sensitive.\n')

cmd = str.encode('\r\n') #initialize the variable cmd

#send/write a command to the serial
ser.write(str.encode('JXD -1\r\n')) #sets correlation between joystick direction and stage's X axis direction of movement 
								   #joystick right, stage moves mechanically right, image on screen moves right
time.sleep(1)

ser.write(str.encode('JYD 1\r\n')) #changes correlation between joystick direction and stage's Y axis direction of movement 
									#joystick forward, stage moves mechanically back, image on screen moves forward
time.sleep(1)


def pattern(imageX, imageY, columns, rows, imgX, imgY): # this function is called after the setup (below)--it snakes the stage back and forth
	ser.write(str.encode('H\r\n')) #disable joystick so user won't interrupt pattern

	ser.reset_output_buffer() #make sure buffer is cleared
	ser.reset_input_buffer()

	imgNum = 1 #initialize the counting system for labeling image files

	for r in range (0, int(rows) + 1): #rows --- add +1 to ensure that we make it all the way across the slide
		for c in range (0, int(columns) + 1): #columns ||| 

			imgStr = str(imgNum) #convert the imgNum to string so it can be passed in the stdin PIPE

			#"exe" refers to the c++ executable "camera_vg_take2.exe"
			#subprocess.Popen runs the c++ exe as a separate thread, stdin=PIPE opens a pipe to send input to exe
			p = subprocess.Popen("camera_vg_take2.exe", stdin = PIPE, universal_newlines = True) #universal_newlines means stdin can be a string instead of bytes
			p.communicate(imgStr) #"communicate" sends the imgStr through stdin PIPE so exe can label image file 
			#if images are turning out blurry, add "time.sleep()" here

			imgNum += 1 #update the image number so that most recent img file doesn't get overwritten

			ser.write(str.encode('L ' + str(imgX) + '\r\n')) #"L" tells the stage to move left by imgX number of steps--need space between 'L' and imgX 

		imgStr = str(imgNum)

		p = subprocess.Popen("camera_vg_take2.exe", stdin=PIPE, universal_newlines=True) #same as above
		p.communicate(imgStr)
		#if images are turning out blurry, add "time.sleep()" here

		imgNum += 1

		ser.write(str.encode('B ' + str(imgY) + '\r\n')) #"B" tells stage to move back by imgY number of steps

		imgX = imgX * -1 #this changes the sign of imgX so that next time thru the loop the stage moves R (negative L)

ser.write(str.encode('J\r\n')) #"J" activates the joystick  

in1 = input("Type \"end\" to go to end position\nor just press enter to manually set the position: ") #"end" moves stage to automatic end position (good when testing repeatedly)
if in1 == 'exit': #type exit to exit out of program
    ser.close() 
    exit()
if in1 == 'end':
	ser.write(str.encode('G -7347 -13896\r\n')) #"G x y" means "go to " (x, y)--these x and y coordinates can be changed for different automatic "end" position
	time.sleep(1) #let the stage move

print("Use the joystick if you want to manually set the end point of the pattern.") #end should be lower right corner of physical slide, upper right of camera image of slide

in1 = input("Enter PX: ")	#operator should literally type "px" (NOT cap sensitive)
cmd = str.encode(in1 + '\r\n') #"PX" means "get the current x coordinate of stage"
ser.reset_input_buffer() #clear the buffer so controller can get command
ser.write(cmd)
out = str.encode('') #initialize out variable to store controller's return later
time.sleep(1)
while ser.inWaiting() > 0: #wait for serial to reply
	out += ser.read(1).strip() #read whatever the controller returns (px = x position) and strip
print ("End position x: " + str(out))
	
endX = int(out.strip()) #store the x position in int variable (for use in calculations)


in1 = input("Enter PY: ") #operator should type "py"
if in1 == 'exit': 
   	ser.close() 
   	exit()
cmd = str.encode(in1 + '\r\n') #"PY" means "get the current y coordinate of stage"
ser.write(cmd)
out = str.encode('')
time.sleep(1)
while ser.inWaiting() > 0:
	out += ser.read(1) 
print ("End position y: " + str(out))

endY = int(out.strip())

in1 = input("Type \"start\" to go to start position\nor just press enter to manually set the position: ") #"start" moves stage to a set position (good when testing repeatedly)
if in1 == 'exit': 
	ser.close() 
	exit()
if in1 == 'start':                               #This section is same as above, but now for starting position
	cmd = str.encode('G 10706 3818\r\n') 
	ser.write(cmd) 
	time.sleep(1) 

print("Use the joystick if you want to manually set the start position.") #should be upper left corner of physical slide, lower left corner of camera image

in1 = input("Enter PX: ")
cmd = str.encode(in1 + '\r\n') 
ser.reset_input_buffer()
ser.write(cmd) 
out = str.encode('')
time.sleep(1)
while ser.inWaiting() > 0: 
	out += ser.read(1)
print ("Starting position x: ")
print (str(out))

startX = int(out.strip())


in1 = input("Enter PY: ") #get starting y position:
if in1 == 'exit': 
	ser.close() 
	exit()
cmd = str.encode(in1 + '\r\n') 
ser.reset_input_buffer()
ser.write(cmd) 
out = str.encode('') 
time.sleep(1) 
while ser.inWaiting() > 0: 
	out += ser.read(1) 
print ("Starting y position: \n" + str(out))

startY = int(out.strip())

patternX = abs(endX - startX) #patternX is the size of the wholel slide/pattern X length
patternY = abs(endY - startY) #patternY is the size of the whole slide/pattern Y height

imageX = 627 #number of stage user steps for image's X length
imageXstr = str(1140) 
imageY = 470 #number of stage user steps for image's Y height
imageYstr = str(845)

columns = patternX/imageX #calculate the number of columns the slide is to be divided into
rows = patternY/imageY #calculate number of rows

print("Slide width in stage steps: " + str(patternX) + ", Slide height in stage steps: " + str(patternY))
print("Number of columns: " + str(columns) + ", Number of rows: " + str(rows)) #this is optional--lets user see the values

time.sleep(1)

pattern(imageX, imageY, columns, rows, imageX, imageY) #calls the "pattern" function (written above)

in1 = "J" #activates the joystick after the pattern finishes (since pattern deactivates it)
ser.write(str.encode('J\r\n')) 
time.sleep(1)