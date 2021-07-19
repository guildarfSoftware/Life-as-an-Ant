using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using RPG.Harvest;
using UnityEngine;
using RPG.Movement;

namespace RPG.Sounds
{

    public enum ClipId
    {
        OpenUpgrades = 0,
        SpawnWorker,
        SpawnFollower,
        BuyUpgrade,
        BuildUpgrade,
        AntDeath,
        SpiderDeath,
        FoodHit,
        StorageHit,
        AntAttack,
        SpiderAttack,
        Message,
        AntWalk,
        SpiderWalk,
        WaveAlert,
        foodSpawn,
        Victory,
        AmbientMusic,
        CombatMusic,
        ButtonClick,
        PheromonesOff,
        PheromonesOn,
        FollowerSend,
        FollowerReturned,
    }


    public class SoundEffects : MonoBehaviour
    {
        enum EntityType
        {
            undef,
            player,
            ant,
            spider,
            spiderQueen,
        }
        static SoundClips audioClips;
        static SoundEffects playerInstance;
        AudioSource audioSource;
        [SerializeField] EntityType entityType;

        private void Awake()
        {
            if(audioClips == null)
            { 
                audioClips = UnityEngine.Resources.Load<SoundClips>("SoundClips");
            }
            Fighter fighter = null;
            Harvester harvester = null;
            Health health = null;
            Mover mover = null;

            if (transform.parent != null)
            {
                fighter = transform.parent.GetComponent<Fighter>();
                harvester = transform.parent.GetComponent<Harvester>();
                health = transform.parent.GetComponent<Health>();
                mover = transform.parent.GetComponent<Mover>();
            }
            
            audioSource = GetComponent<AudioSource>();

            switch (entityType)
            {
                case EntityType.player:
                    {
                        playerInstance = this;

                        fighter.CombatHit += () => { _playSound(ClipId.AntAttack); };
                        harvester.fooodGrabbed += () => { _playSound(ClipId.FoodHit); };
                        harvester.foodDeposit += () => { _playSound(ClipId.StorageHit); };
                        health.OnDeath += (g) => { _playSound(ClipId.AntDeath); };
                        mover.startMovement += () => { _startLoop(ClipId.AntWalk); };
                        mover.endMovement += () => { _endLoop(); };
                        break;
                    }
                case EntityType.ant:
                    {
                        fighter.CombatHit += () => { _playSound(ClipId.AntAttack); };
                        harvester.fooodGrabbed += () => { _playSound(ClipId.FoodHit); };
                        harvester.foodDeposit += () => { _playSound(ClipId.StorageHit); };
                        health.OnDeath += (g) => { _playSound(ClipId.AntDeath); };
                        mover.startMovement += () => { _startLoop(ClipId.AntWalk); };
                        mover.endMovement += () => { _endLoop(); };
                        break;
                    }
                case EntityType.spider:
                case EntityType.spiderQueen:
                    {
                        fighter.CombatHit += () => { _playSound(ClipId.SpiderAttack); };
                        health.OnDeath += (g) => { _playSound(ClipId.SpiderDeath); };
                        mover.startMovement += () => {_startLoop(ClipId.SpiderWalk);};
                        mover.endMovement += () => {_endLoop();};
                        break;
                    }
            }
        }


        public void _startLoop(ClipId soundId)
        {
            audioSource.loop = true;
            _playSound(soundId);
        }
        public void _endLoop()
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
        public static void PlaySound(ClipId soundId)
        {
            if (playerInstance == null) return;
            AudioClip audioClip = GetClip(soundId);
            if (audioClip == null) return;
            playerInstance.audioSource.PlayOneShot(audioClip);
        }

        void _playSound(ClipId soundId)
        {
            if (audioSource == null) return;
            AudioClip audioClip = GetClip(soundId);
            if (audioClip == null) return;
            audioSource.clip = audioClip;
            audioSource.Play();
            audioSource.loop=false;
        }

        public static AudioClip GetClip(ClipId soundId)
        {
            int id = (int)soundId;
            if (id >= audioClips.Count || id < 0) return null;

            return audioClips.Get(id);

        }
    }
}