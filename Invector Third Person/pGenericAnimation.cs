using UnityEngine;
using Invector;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using NaughtyAttributes;

namespace PBG
{
    [Serializable]
    public enum TrueFalseValue
    {
        False = 0,
        True = 1
    }

    [Serializable]
    public enum ParameterType
    {
        None = 0,
        Bool = 1 << 0,
        Float = 1 << 1,
        Int = 1 << 2
    }

    [Serializable]
    public enum EquivalenceTest
    {
        Equal = 1 << 0,
        NotEqual = 1 << 1,
        GreaterThan = 1 << 2,
        LessThan = 1 << 3
    }

    public struct Value
    {
        public int   intValue;
        public float floatValue;
        public bool  boolValue;
    }

    [Serializable]
    public class Condition
    {
        public string parameterName;

        [SerializeField]
        [Tooltip("How should the condition be tested OR how to set the parameter")]
        public EquivalenceTest equivalenceTest;

        [SerializeField]
        public ParameterType type;

        [ShowIf("type", ParameterType.Int)]
        [AllowNesting]
        public int intValue;
        [ShowIf("type", ParameterType.Float)]
        [AllowNesting]
        public float floatValue;
        [ShowIf("type", ParameterType.Bool)]
        [AllowNesting]
        public bool boolValue;

        [HideInInspector]
        public Value originalValue;
    }

    [vClassHeader("PBG Generic Animation", "Use this script to trigger a simple animation with conditional & parameter constraints.")]
    public class pGenericAnimation : vGenericAnimation
    {
        #region Variables

        [Tooltip("If this Generic Animation represents a sequence of animations, this is the last animation clip in the sequence")]
        public string endAnimationClip = null;
        [Tooltip("Name of the animation layer or layer number on which the clip lives")]
        public string animationLayer;

        [Tooltip("These are the conditions that must be met if the Animation is going to be played")]
        public Condition[] conditions;

        [Tooltip("These are the Animation Parameters that will be set when the animation is played")]
        public Condition[] setParameters;
        public TrueFalseValue resetParameters = TrueFalseValue.True;

        // This is simply used as an API for other classes to disable the use of this animation. It is not set in this script
        [HideInInspector]
        public bool DisableAnimation = false;

        protected int animStateIdx;
        protected AnimatorStateInfo animStateInfo;
        #endregion

        protected override void Start()
        {
            tpInput = GetComponent<vThirdPersonInput>();
            if (tpInput == null)
            {
                Debug.LogError("No vThirdPersonInput found on this object.");
            }
            if (endAnimationClip == null)
            {
                endAnimationClip = animationClip;
            }
            // if we weren't given a number as an AnimationLayer, pull it as a string
            if (!int.TryParse(animationLayer, out animStateIdx))
            {
                animStateIdx = tpInput.cc.animator.GetLayerIndex(animationLayer);
            }

            if (animStateIdx < 0)
            {
                Debug.LogWarning("Unable to find animation layer " + animationLayer + ".  Only parameters will be set");
            }
        }

        protected override void LateUpdate()
        {
            if (DisableAnimation) return;
            base.LateUpdate();
        }

        protected override void TriggerAnimation()
        {
            if (actionInput.GetButtonDown())
            {
                // condition to trigger the animation
                bool playConditions = !isPlaying && !tpInput.cc.customAction && !string.IsNullOrEmpty(animationClip);
                playConditions &= (conditions.Length > 0) ? ConditionalsPass() : playConditions;
                if (playConditions)
                {
                    // Set parameters before animation is played
                    SetParameters();
                    // We won't play an animation if no layer has been defined.
                    if (animStateIdx >= 0)
                        PlayAnimation();
                        
                    if (resetParameters == TrueFalseValue.True)
                        ResetParameters();
                }
            }
        }

        protected override void AnimationBehaviour()
        {
            if (animStateIdx < 0)
                return;
                
            // know if the animation is playing or not
            animStateInfo = tpInput.cc.animator.GetCurrentAnimatorStateInfo(animStateIdx);
            isPlaying = animStateInfo.IsName(endAnimationClip);

            if (isPlaying)
            {
                // detected the end of the animation clip to trigger the OnEndAnimation Event
                if (tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= animationEnd)
                {
                    if(triggerOnce)
                    {
                        triggerOnce = false;        // set to false so End functions are not called multiple times
                        OnEndAnimation.Invoke();    // call the OnEndAnimation Event at the end of the animation
                    }
                }
            }
        }

        protected bool ConditionalsPass()
        {
            foreach (Condition cond in conditions)
            {
                switch (cond.type)
                {
                    case ParameterType.Float:
                        {
                            float animValue = tpInput.cc.animator.GetFloat(cond.parameterName);
                            if (!TestEquivalence<float>(cond.equivalenceTest, cond.floatValue, animValue))
                                return false;
                            break;
                        }
                    case ParameterType.Int:
                        {
                            int animValue = tpInput.cc.animator.GetInteger(cond.parameterName);
                            if (!TestEquivalence<int>(cond.equivalenceTest, cond.intValue, animValue))
                                return false;
                            break;
                        }
                    case ParameterType.Bool:
                        {
                            bool val = tpInput.cc.animator.GetBool(cond.parameterName);
                            if (val != cond.boolValue)
                                return false;
                            break;
                        }
                }
            }
            return true;
        }

        protected bool TestEquivalence<T>(EquivalenceTest test, T value, T animValue)
        {
            int comparison = 0;
            switch (test)
            {
                case EquivalenceTest.Equal:
                    if (!value.Equals(animValue)) return false;
                    break;
                case EquivalenceTest.NotEqual:
                    if (value.Equals(animValue)) return false;
                    break;
                case EquivalenceTest.LessThan:
                    comparison = Comparer<T>.Default.Compare(animValue, value);
                    if (comparison > 0 || comparison == 0)  return false;
                    break;
                case EquivalenceTest.GreaterThan:
                    comparison = Comparer<T>.Default.Compare(animValue, value);
                    if (comparison < 0 || comparison == 0) return false;
                    break;
            }
            return true;
        }

        protected void SetParameters()
        {
            foreach (Condition cond in setParameters)
            {
                switch (cond.type)
                {
                    case ParameterType.Float:
                        cond.originalValue.floatValue = tpInput.cc.animator.GetFloat(cond.parameterName);
                        tpInput.cc.animator.SetFloat(cond.parameterName, cond.floatValue);
                        break;
                    case ParameterType.Int:
                        cond.originalValue.intValue = tpInput.cc.animator.GetInteger(cond.parameterName);
                        tpInput.cc.animator.SetInteger(cond.parameterName, cond.intValue);
                        break;
                    case ParameterType.Bool:
                        cond.originalValue.boolValue = tpInput.cc.animator.GetBool(cond.parameterName);
                        tpInput.cc.animator.SetBool(cond.parameterName, cond.boolValue);
                        break;
                };
            }
        }

        protected void ResetParameters()
        {
            foreach (Condition cond in setParameters)
            {
                if (cond.type == ParameterType.Float)
                {
                    tpInput.cc.animator.SetFloat(cond.parameterName, cond.originalValue.floatValue);
                }
                else if (cond.type == ParameterType.Int)
                {
                    tpInput.cc.animator.SetInteger(cond.parameterName, cond.originalValue.intValue);
                }
                else if (cond.type == ParameterType.Bool)
                {
                    tpInput.cc.animator.SetBool(cond.parameterName, cond.originalValue.boolValue);
                }
            }
        }
    }
}
