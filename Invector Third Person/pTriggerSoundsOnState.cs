using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pTriggerSoundsOnState : StateMachineBehaviour
{
    public GameObject audioSource;
    public List<AudioClip> sounds;
    public float[] triggerTimes;
    private int nextTrigger;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        nextTrigger = 0;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nextTrigger >= triggerTimes.Length)
            return;
        if (stateInfo.normalizedTime % 1 >= triggerTimes[nextTrigger])
        {
            TriggerSound(animator, stateInfo, layerIndex);
            nextTrigger++;
        }
    }

    void TriggerSound(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject audioObject = null;
        if (audioSource != null)
            audioObject = Instantiate(audioSource.gameObject, animator.transform.position, Quaternion.identity) as GameObject;
        else
        {
            audioObject = new GameObject("audioObject");
            audioObject.transform.position = animator.transform.position;
        }
        var source = audioObject.gameObject.GetComponent<AudioSource>();
        var clip = Random.Range(0, sounds.Count);
        source.PlayOneShot(sounds[clip]);
    }
}
