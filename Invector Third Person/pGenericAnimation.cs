using UnityEngine;
using Invector;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Invector.vCharacterController;

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

    [Serializable]
    public struct Value
    {
        public int   intValue;
        public float floatValue;
        public bool  boolValue;
    }

    [Serializable]
    public class Condition
    {
        [SerializeField]
        public ParameterType type;
        public string parameterName;

        [SerializeField]
        public EquivalenceTest equivalenceTest;

        [SerializeField]
        public Value toValue;

        [HideInInspector]
        public Value originalValue;
    }

    [vClassHeader("PBG Generic Animation", "Use this script to trigger a simple animation with conditional & parameter constraints.")]
    public class pGenericAnimation : vMonoBehaviour
    {
        #region Variables

        [Tooltip("Input to trigger the custom animation")]
        public GenericInput actionInput = new GenericInput("L", "A", "A");
        [Tooltip("Name of the animation clip")]
        public string animationClip;
        [Tooltip("If this Generic Animation represents a sequence of animations, this is the last animation clip in the sequence")]
        public string endAnimationClip = null;
        [Tooltip("Name of the animation layer on which the clip lives")]
        public string animationLayer;
        [Tooltip("Where in the end of the animation will trigger the event OnEndAnimation")]
        public float animationEnd = 0.8f;

        public UnityEvent OnPlayAnimation;
        public UnityEvent OnEndAnimation;
        [Tooltip("These are the conditions that must be met if the Animation is going to be played")]
        public Condition[] conditionals;

        [Tooltip("These are the Animation Parameters that will be set when the animation is played")]
        public Condition[] setParameters;
        public TrueFalseValue resetParameters = TrueFalseValue.True;

        protected bool isPlaying;
        protected bool triggerOnce;
        protected vThirdPersonInput tpInput;

        protected int animStateIdx;
        protected AnimatorStateInfo animStateInfo;
        #endregion

        protected virtual void Start()
        {
            tpInput = GetComponent<vThirdPersonInput>();
            if (endAnimationClip == null)
            {
                endAnimationClip = animationClip;
            }
        }

        protected virtual void LateUpdate()
        {
            TriggerAnimation();
            AnimationBehaviour();            
        }

        protected virtual void TriggerAnimation()
        {
            // condition to trigger the animation
            bool playConditions = !isPlaying && !tpInput.cc.customAction && !string.IsNullOrEmpty(animationClip);
            playConditions &= (conditionals.Length > 0) ? ConditionalsPass() : playConditions;

            if (actionInput.GetButtonDown() && playConditions)
            {
                if (animStateIdx < 0)
                    animStateIdx = tpInput.cc.animator.GetLayerIndex(animationLayer);

                SetParameters();
                PlayAnimation();
            }
        }

        public virtual void PlayAnimation()
        {
            // we use a bool to trigger the event just once at the end of the animation
            triggerOnce = true;
            // trigger the OnPlay Event
            OnPlayAnimation.Invoke();
            // trigger the animationClip
            tpInput.cc.animator.CrossFadeInFixedTime(animationClip, 0.1f);
        }

        protected virtual void AnimationBehaviour()
        {
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
                        triggerOnce = false;        // reset the bool so we can call the event again
                        OnEndAnimation.Invoke();    // call the OnEnd Event at the end of the animation
                        if (resetParameters == TrueFalseValue.True)
                            ResetParameters();
                    }
                }
            }
        }

        protected bool ConditionalsPass()
        {
            foreach (Condition cond in conditionals)
            {
                switch (cond.type)
                {
                    case ParameterType.Float:
                        {
                            float animValue = tpInput.cc.animator.GetFloat(cond.parameterName);
                            if (!TestEquivalence<float>(cond.equivalenceTest, cond.toValue.floatValue, animValue))
                                return false;
                            break;
                        }
                    case ParameterType.Int:
                        {
                            int animValue = tpInput.cc.animator.GetInteger(cond.parameterName);
                            if (!TestEquivalence<int>(cond.equivalenceTest, cond.toValue.intValue, animValue))
                                return false;
                            break;
                        }
                    case ParameterType.Bool:
                        {
                            bool val = tpInput.cc.animator.GetBool(cond.parameterName);
                            if (val != Convert.ToBoolean(cond.toValue.boolValue))
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
                        tpInput.cc.animator.SetFloat(cond.parameterName, cond.toValue.floatValue);
                        break;
                    case ParameterType.Int:
                        cond.originalValue.intValue = tpInput.cc.animator.GetInteger(cond.parameterName);
                        tpInput.cc.animator.SetInteger(cond.parameterName, cond.toValue.intValue);
                        break;
                    case ParameterType.Bool:
                        cond.toValue.boolValue = tpInput.cc.animator.GetBool(cond.parameterName);
                        tpInput.cc.animator.SetBool(cond.parameterName, Convert.ToBoolean(cond.toValue.boolValue));
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