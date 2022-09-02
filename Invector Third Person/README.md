-- pGenericAnimation.cs is a script which is derived from Invector's vGenericAnimation.cs script.  It's purpose is to add the same control over animator parameters that you would have if you were using the Animator.  For example, if you wanted to add a Slide vGenericAnimation you wouldn't want to slide if you were standing still.  With pGenericAnimation.cs you could attach the conditions, IsGrounded & InputMagnitude.  This would make sure your Player was Grounded and Running before a slide could happen.
There is also the facility to set parameters before any animation is played.

REQUIRES: NaughtyAttributes. (https://assetstore.unity.com/packages/tools/utilities/naughtyattributes-129996#description)

-- SurfaceHitEffects.cs is a ScriptableObject which is used to combine all the effects across a number of packages, allowing you to easily select & swap between the effect you are going for.  Instancing is performed specific to the type of effect to make sure a hit against the surface normals are correct.

-- pTriggerSoundsOnState.cs is a script that extends/replaces the vTriggerSoundOnState.cs script which is attached to a State in the StateMachine.  It's purpose is to allow multiple sounds to be played (just like the original vTriggerSoundOnState) but you can add multiple 'TriggerTimes'.  Meaning you can have more than one sound played in your animation at certain keyframes.  Since afterall, an animation can be made up of multiple attacks.
