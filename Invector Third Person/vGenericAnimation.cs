using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Invector.vCharacterController.vActions
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
    public class Conditional
    {
        public ParameterType type;
        public string parameterName;

        [Header("Bool Parameter")]
        [SerializeField]
        public TrueFalseValue boolValue;

        [Header("Numeric Parameters")]
        [SerializeField]
        public EquivalenceTest conditional;
        public float value;

        [HideInInspector]
        public float originalValue;
        [HideInInspector]
        public bool originalBool;
    }


    [vClassHeader("Generic Animation", "Use this script to trigger a simple animation.")]
    public class vGenericAnimation : vMonoBehaviour
    {
        #region Variables

        [Tooltip("Input to trigger the custom animation")]
        public GenericInput actionInput = new GenericInput("L", "A", "A");
        [Tooltip("Name of the animation clip")]
        public string animationClip;
        [Tooltip("Which part of the animation will trigger the event OnEndAnimation. This is a normalized value (1-100%)")]
        public float animationEnd = 0.8f;

        public UnityEvent OnPlayAnimation;
        public UnityEvent OnEndAnimation;
        [Tooltip("These are the conditions that must be met if the Animation is going to be played")]
        public Conditional[] conditionals;

        [Tooltip("These are the Animation parameters that will be set when the animation is played")]
        public Conditional[] setParameters;
        public TrueFalseValue resetParameters = TrueFalseValue.True;

        protected bool isPlaying;
        protected bool triggerOnce;
        protected vThirdPersonInput tpInput;
        
        #endregion

        protected virtual void Awake()
        {
            tpInput = GetComponent<vThirdPersonInput>();
        }

        protected virtual void LateUpdate()
        {
            TriggerAnimation();
            AnimationBehaviour();            
        }

        protected virtual void TriggerAnimation()
        {
            // condition to trigger the animation
            if (actionInput.GetButtonDown())
            {
                bool playConditions = !isPlaying && !tpInput.cc.customAction && !string.IsNullOrEmpty(animationClip);
                playConditions &= (conditionals.Length > 0) ? ConditionalsPass() : playConditions;
                if (playConditions)
                {
                    SetParameters();
                    PlayAnimation();
                }
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
            isPlaying = tpInput.cc.baseLayerInfo.IsName(animationClip);

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
            bool pass = true;
            for (int i=0; i < conditionals.Length && pass; ++i)
            {
                if (cond.type == ParameterType.Float)
                {
                    float value = (float)cond.value;
                    float animValue = tpInput.cc.animator.GetFloat(cond.parameterName);
                    pass = CompareNumeric<float>(cond, value, animValue);
                }
                else if (cond.type == ParameterType.Int)
                {
                    int value = (int)cond.value;
                    int animValue = tpInput.cc.animator.GetInteger(cond.parameterName);
                    pass = CompareNumeric<int>(cond, value, animValue);
                }
                else
                {
                    bool val = tpInput.cc.animator.GetBool(cond.parameterName);
                    pass = (val == Convert.ToBoolean(cond.boolValue));
                }
            }
            return pass;
        }

        protected bool CompareNumeric<T>(Conditional cond, T value, T animValue)
        {
            int comparison = 0;

            switch (cond.conditional)
            {
                case EquivalenceTest.Equal:
                    if (!value.Equals(animValue)) return false;
                    break;
                case EquivalenceTest.NotEqual:
                    if (value.Equals(animValue)) return false;
                    break;
                case EquivalenceTest.LessThan:
                    comparison = Comparer<T>.Default.Compare(value, animValue);
                    if (comparison > 0 || comparison == 0)  return false;
                    break;
                case EquivalenceTest.GreaterThan:
                    comparison = Comparer<T>.Default.Compare(value, animValue);
                    if (comparison < 0 || comparison == 0) return false;
                    break;
            }
            return true;
        }

        protected void SetParameters()
        {
            foreach (Conditional cond in setParameters)
            {
                if (cond.type == ParameterType.Float)
                {
                    cond.originalValue = tpInput.cc.animator.GetFloat(cond.parameterName);
                    tpInput.cc.animator.SetFloat(cond.parameterName, cond.value);
                }
                else if (cond.type == ParameterType.Int)
                {
                    cond.originalValue = tpInput.cc.animator.GetInteger(cond.parameterName);
                    tpInput.cc.animator.SetFloat(cond.parameterName, cond.value);
                }
                else
                {
                    cond.originalBool = tpInput.cc.animator.GetBool(cond.parameterName);
                    tpInput.cc.animator.SetBool(cond.parameterName, Convert.ToBoolean(cond.boolValue));
                }
            }
        }

        protected void ResetParameters()
        {
            foreach (Conditional cond in setParameters)
            {
                if (cond.type == ParameterType.Float)
                {
                    tpInput.cc.animator.SetFloat(cond.parameterName, cond.originalValue);
                }
                else if (cond.type == ParameterType.Int)
                {
                    tpInput.cc.animator.SetInteger(cond.parameterName, (int)cond.originalValue);
                }
                else
                {
                    tpInput.cc.animator.SetBool(cond.parameterName, cond.originalBool);
                }
            }
        }
    }
}
