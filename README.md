AutoAssigner will add a button to the top of all script inspectors. 
The `AutoAssign` button will attempt to assign a valid value to all the object reference fields based on the name of the property.

![image](https://github.com/pnarimani/AutoAssigner/assets/30625116/d7d9b2a9-9cae-4635-9248-862a732b20aa)

AutoAssigner will take the name of the gameobject, name of the script and name of the property into account to find suitable candidates for the property. 

Prefab Handling:
* If the name of the property contains the word "prefab", AutoAssigner will only take prefabs into account as possible candidates.
* If the type of the property inherits from `UnityEngine.Component` and the property name does not contain the word "prefab", AutoAssigner will first try to find a target inside the scene. If no target is found, AutoAssigner will try to find a target in the prefabs.
