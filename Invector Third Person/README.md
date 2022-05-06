- vGenericAnimation.cs is a first iteration of the work. It places the class in the Invector namespace which was not proper and the Properties were less legible. pGenericAnimation.cs is the second iteration which tries to clean up these issues.

Use pGenericAnimation.cs.

- SurfaceHitEffects.cs is a ScriptableObject which is used to combine all the effects across a number of packages, allowing you to easily select & swap between the effect you are going for.  Instancing is performed specific to the type of effect to make sure a hit against the surface normals are correct.
