using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopBGMForced : MonoBehaviour
{
    [SerializeField] private string bgmName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopClipByName(bgmName);
        }
    }

    private void StopClipByName(string clipName)
    {
        // 현재 로드된 모든 씬의 AudioSource를 긁어모읍니다.
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource source in allSources)
        {
            if (source.clip == null) return;
            if (source.clip.name == clipName) source.Stop(); break;
        }
    }
}
