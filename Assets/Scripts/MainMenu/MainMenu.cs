using System.Collections;
using System.Collections.Generic;
using RPG.Sounds;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    SoundClips clips;

    private void Awake()
    {
        clips = UnityEngine.Resources.Load<SoundClips>("SoundClips");
    }

    public void OnPlayClick()
    {

        AudioSource.PlayClipAtPoint(clips.Get((int)ClipId.ButtonClick), Vector3.zero);
        AudioSource audio = FindObjectOfType<AudioSource>();

        if (audio != null)
        {
            DontDestroyOnLoad(audio.gameObject);
        }
        SceneManager.LoadScene("InGame"); 
    }

    public void OnInstructionsClick()
    {

        AudioSource.PlayClipAtPoint(clips.Get((int)ClipId.ButtonClick), Vector3.zero);
        AudioSource audio = FindObjectOfType<AudioSource>();

        if (audio != null)
        {
            DontDestroyOnLoad(audio.gameObject);
        }
        SceneManager.LoadScene("Instructions");
    }

    public void OnExitClick()
    {
        AudioSource.PlayClipAtPoint(clips.Get((int)ClipId.ButtonClick), Vector3.zero);
        Application.Quit();
    }

}
