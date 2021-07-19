using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Sounds
{

    public class BackgroundMusic : MonoBehaviour
    {

        static AudioSource source;
        static SoundClips clips;

        // Start is called before the first frame update
        void Awake()
        {
            if (source == null) source = GetComponent<AudioSource>();
            if (clips == null) clips = UnityEngine.Resources.Load<SoundClips>("SoundClips");
            PlayNormalMusic();
        }

        public static void PlayNormalMusic()
        {
            source.clip = clips.Get((int)ClipId.AmbientMusic);
            source.Play();
        }

        public static void PlayFinalBattleMusic()
        {
            source.clip = clips.Get((int)ClipId.CombatMusic);
            source.Play();
        }
    }

}