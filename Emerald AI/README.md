A list of resources to improve upon Emerald AI.

**EmeraldAiColliders.cs**
This solves the problem that Emerald AI has of using a single BoxCollider to register AI hits.  Place this script on your top level AI and do the following in _EmeraldAISystem.cs_ 

* remove the line in **RequireComponent[typeof(BoxCollider)]**
* replace the definition _public BoxCollider AIBoxCollider;_ with _public EmeraldAiColliders AIBoxCollider;_  // I kept the same name for traceability
* in _EmeraldAiInitializer.cs_ replace 
  `EmeraldComponent.AIBoxCollider = GetComponent<BoxCollider>();` with
  `EmeraldComponent.AIBoxCollider = GetComponent<EmeraldAiColliders>();`
  
Now simply add a new tag "Enemy Body Part" (or whatever you'd like), create your colliders on your AI skeleton & tag the Components as the new tag you just defined.

![image](https://user-images.githubusercontent.com/58187872/139158433-3aa40af1-d289-4b53-a4ac-d82b171d3e9d.png)


