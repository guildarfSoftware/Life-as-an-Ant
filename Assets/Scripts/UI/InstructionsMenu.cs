using System.Collections;
using System.Collections.Generic;
using RPG.Sounds;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsMenu : MonoBehaviour
{
    [SerializeField] GameObject[] slides;
    int currentSlideIndex;
    GameObject currentSlide;
    SoundClips clips;
    private void Start()
    {
        clips = UnityEngine.Resources.Load<SoundClips>("SoundClips");
        ShowSlide(0);
    }
    public void Next()
    {
        currentSlideIndex++;
        AudioSource.PlayClipAtPoint(clips.Get((int)ClipId.ButtonClick), Vector3.zero);

        if (currentSlideIndex < slides.Length)
        {
            ShowSlide(currentSlideIndex);
        }
        else
        {
            AudioSource audio = FindObjectOfType<AudioSource>();

            if (audio != null)
            {
                DontDestroyOnLoad(audio.gameObject);
            }
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void Previous()
    {
        currentSlideIndex--;
        AudioSource.PlayClipAtPoint(clips.Get((int)ClipId.ButtonClick), Vector3.zero);
        
        if (currentSlideIndex > 0)
        {
            ShowSlide(currentSlideIndex);
        }
        else
        {
            AudioSource audio = FindObjectOfType<AudioSource>();

            if(audio != null)
            {
                DontDestroyOnLoad(audio.gameObject);
            }

            SceneManager.LoadScene("MainMenu");
        }
    }

    void ShowSlide(int index)
    {
        Destroy(currentSlide);
        currentSlide = Instantiate(slides[index], transform);
        currentSlide.transform.SetSiblingIndex(1);

    }
}
