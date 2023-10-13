-- pGenericAnimation.cs is a script which is derived from Invector's vGenericAnimation.cs script.  It's purpose is to add the same control over animator parameters that you would have if you were using the Animator.  For example, if you wanted to add a Slide vGenericAnimation you wouldn't want to slide if you were standing still.  With pGenericAnimation.cs you could attach the conditions, IsGrounded & InputMagnitude.  This would make sure your Player was Grounded and Running before a slide could happen.
There is also the facility to set parameters before any animation is played. The following video provides a walk-though on how to use this asset.
https://www.youtube.com/watch?v=UU9fgB1NnEc

REQUIRES: NaughtyAttributes. (https://assetstore.unity.com/packages/tools/utilities/naughtyattributes-129996#description)

-- SurfaceHitEffects.cs is a ScriptableObject which is used to combine all the effects across a number of packages, allowing you to easily select & swap between the effect you are going for.  Instancing is performed specific to the type of effect to make sure a hit against the surface normals are correct.

-- pTriggerSoundsOnState.cs is a script that extends/replaces the vTriggerSoundOnState.cs script which is attached to a State in the StateMachine.  It's purpose is to allow multiple sounds to be played (just like the original vTriggerSoundOnState) but you can add multiple 'TriggerTimes'.  Meaning you can have more than one sound played in your animation at certain keyframes.  Since afterall, an animation can be made up of multiple attacks.

-- vDecalManager to extend the capability to create effects based on Material names.  If 'By Material Names' is selected, a search for the hit material will be performed.  Otherwise, it will fallback & try to match the tag.


NOTE: I believe SurfaceHitEffects and vDecalManager are implmented by the current Invector package.  These were written prior to Invector's implementation.
