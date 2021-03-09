
Prerequisites: 
Create a model of the real world object. 
It needs to match it in overall measure (length, height etc.), and where interaction with the user 
will most likely take place. 

Think: 
Where will the user touch the object? 
How will he or she try to interact with it?
Where/How can he hurt himself?

Example.: When modeling a wall and door, make sure the user can't 
get his hand stuck between hinge and door because he can't see the gap,  
or hit his head on an edge he cannot see in VR.




How to use BigObjects in your scene:

1.	Place a new "BigObject.prefab" instance in the scene. 

2.	Add the model of the real world big object as child of "BigObject_Asset".

3.	Make sure the prefab is at the wanted height in your scene (e.g. the object standing firmly on the ground)

4.	Move the "Anchor_Position" gameobject and the "Anchor_Rotation" gameobject to 
	where your senders are placed on your real world big object.
	
5.	Think of names for your "Anchor_Position" and "Anchor_Rotation" (e.g. Anchor_Position_Table1, ...). 
	Set the names in their "ObjectPositioner" scripts. 
	Add two new Tags to your configuration server under "Senders" with the respective names filed under type. 

6.	Change the "Preset Position Y:" of the "ObjectPositioner" component of the "Anchor_Position" gameobject to adjust the 
	BigObjects height in the scene. "Preset Position Y:" should be set to the global height the "Anchor_Position" gameobject has at 
	gamestart.
	

How "BigObjectPositioning" works:


Anchor_Position:
"Anchor_Position" will determine the position of the BigObject in the scene. 
The BigObject will always be placed relative to the "Anchor_Position.transform.position" and the 
"BigObject_Asset.transform.position" at the gamestart (Vector3).
To make the object moveable only, go to "BigObject_Asset", and in the  
"BigObjectPositioner" component, set "Anchor_Rotation" to "BigObject_Asset".


Anchor_Rotation:
"Anchor_Rotation" will determine how the "BigObject" will be rotated. The distance between 
the "Anchor_Rotation" gameobject and any of the other objects does not matter. 
Only it's direction to the other objects is relevant. 
To make the object rotateable only, go to "BigObject_Asset", and in the  
"BigObjectPositioner" component, set "Anchor_Position" to "BigObject_Asset". 


Buffer Size:
The position data of both anchors is being smoothed by collecting and averaging several positions per sender tag.
The size of the buffer determines how many position values are being used for smoothing. 
A greater buffer size makes movement overall smoother, but will cause movement to be delayed.


Deadzone Radius: 
The distance an anchor has to move in any direction before the "BigObject_Asset" will be moved. 
Movement smaller than the Deadzone Radius will be ignored. 
Raise or lower this value to prevent jittering, which might be caused by lower quality sensor data. 

 







