A list of resources to improve upon Emerald AI.

**EmeraldAiColliders.cs**
This solves the problem of Emerald AI using a single BoxCollider to register AI hits. The problem with using a BoxCollider is the normal is never correct when it is hit with an object/projectile so the reflection will not look correct.  Also, when the AI hits the ground, it will not "roll".  It will move along the ground like a box and not a capsule or even a mesh.

Place this script on your top level AI and do the following in _EmeraldAISystem.cs_ (NOTE: Since this was written EAI has implemented their own location based damage but does not solve the BoxCollider issue or provide the flexibility of this system)

* remove the line in **RequireComponent[typeof(BoxCollider)]**
* replace the definition _public BoxCollider AIBoxCollider;_ with _public EmeraldAiColliders AIBoxCollider;_  // I kept the same name for traceability
* in _EmeraldAiInitializer.cs_ replace 
  `EmeraldComponent.AIBoxCollider = GetComponent<BoxCollider>();` with
  `EmeraldComponent.AIBoxCollider = GetComponent<EmeraldAiColliders>();`
  
Create your colliders on your AI skeleton & tag them with "Enemy Body Part" (or whatever you'd like).  Then select this Tag name from the DropDown.  Any collider with that tag will be registered as a "hit point" on your AI.
The script will populate when you Run. (Not you, Unity)

![image](https://user-images.githubusercontent.com/58187872/139158433-3aa40af1-d289-4b53-a4ac-d82b171d3e9d.png)

and your AI hitboxes will be setup.

![EmeraldAIHitboxes](https://user-images.githubusercontent.com/58187872/139869460-936e7e66-2477-4e4e-a4b4-5de84559b889.png)


When integrating with **Invector** the official Emerald AI docs state the following changes should be made to 
**vMeleeManager.cs**, and **vProjectileControl.cs**

```
//Emerald AI Damage
if (hitInfo.targetCollider.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>())
{
    hitInfo.targetCollider.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>().Damage(
        hitInfo.attackObject.damage.damageValue, EmeraldAI.EmeraldAISystem.TargetType.Player, transform, 400);
}
```

when using **EmeraldAiColliders.cs**

Simply change 
`hitInfo.targetCollider.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>()` to 
`hitInfo.targetCollider.gameObject.GetComponentInParent<EmeraldAIColliders>()`

Your hitBoxes are now children of your EmeraldAISystem so you have to call **GetComponentInParent**
Whichever controller you integrate with, remember conceptually the BoxCollider is no longer on the same level as your EmeralAISystem.  Your colliders are children so just take that into account.
Sounds like a lot but it's quick and worth it.

This implementation will also allow for direct usage of RASCAL Skinned Mesh Renderer (https://assetstore.unity.com/packages/tools/physics/rascal-skinned-mesh-collider-134833).  Something which cannot be done with EmeraldAI currently.
