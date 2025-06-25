using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DisplayAnim
{
    public class DisplayAnimation : MonoBehaviour
    {
        public AnimationClip animationClip; // Assign an AnimationClip in the Inspector

        private Animator animator;
        private PlayableGraph playableGraph;
        private AnimationClipPlayable clipPlayable;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        void Awake()
        {
            animator = GetComponent<Animator>();

            if (animationClip == null)
            {
                Debug.LogError("Animator or AnimationClip is not assigned!");
                return;
            }
            
            // Save the initial position and rotation
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            
            // Create the PlayableGraph
            playableGraph = PlayableGraph.Create("AnimationGraph");

            PlayOneShot();
        }

        private System.Collections.IEnumerator ResetPositionRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            playableGraph.Stop();
            PlayOneShot();
        }

        private void PlayOneShot()
        {
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationOutput", animator);
            
            // Create an AnimationClipPlayable
            clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
            clipPlayable.SetDuration(animationClip.length);
            
            // Connect the playable to the output
            playableOutput.SetSourcePlayable(clipPlayable);
            
            // Start a coroutine to reset position after animation ends
            StartCoroutine(ResetPositionRoutine((float)clipPlayable.GetDuration()));
            
            // Play the graph
            playableGraph.Play();
        }

        void OnDestroy()
        {
            // Destroy the PlayableGraph when the object is destroyed
            playableGraph.Destroy();
        }
    }
}