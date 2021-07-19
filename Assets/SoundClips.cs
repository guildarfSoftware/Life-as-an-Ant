using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new SoundClips")]
public class SoundClips : ScriptableObject
{
        public List<AudioClip> audioClips;
        public int Count {get => audioClips.Count;}
        public AudioClip Get(int id) { return audioClips[id];}
}
