A list of resources to improve upon Emerald AI.

**EmeraldAiColliders.cs**
This solves the problem that Emerald AI has of using a single BoxCollider to register AI hits.  Place this script on your top level AI and do the following in _EmeraldAISystem.cs_ 

* remove the line in **RequireComponent[typeof(BoxCollider)]**
* replace the definition _public BoxCollider AIBoxCollider;_ with _public EmeraldAiColliders AIBoxCollider;_  // I kept the same name for traceability
* in _EmeraldAiInitializer.cs_ replace 
  `EmeraldComponent.AIBoxCollider = GetComponent<BoxCollider>();` with
  `EmeraldComponent.AIBoxCollider = GetComponent<EmeraldAiColliders>();`
  
Create your colliders on your AI skeleton & tag them colliders with "AI Body Part" (or whatever you'd like).

The script will populate...
![image](https://user-images.githubusercontent.com/58187872/139158433-3aa40af1-d289-4b53-a4ac-d82b171d3e9d.png)

and your AI hitboxes will be setup.
![EmeraldAIHitboxes](https://user-images.githubusercontent.com/58187872/139869460-936e7e66-2477-4e4e-a4b4-5de84559b889.png)


When integrating with Invector the official Emerald AI docs state
```

//Emerald AI Damage
if (hitInfo.targetCollider.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>())
{
    hitInfo.targetCollider.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>().Damage(
        hitInfo.attackObject.damage.damageValue, EmeraldAI.EmeraldAISystem.TargetType.Player, transform, 400);
}
```
Simply change 
`hitInfo.targetCollider.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>()` to 
`hitInfo.targetCollider.gameObject.GetComponentInParent<EmeraldAI.EmeraldAISystem>()`

Sounds like a lot but it's quick and worth it.
