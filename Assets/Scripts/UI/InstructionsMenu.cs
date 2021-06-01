using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsMenu : MonoBehaviour
{
    [SerializeField] GameObject[] slides;
    int currentSlideIndex;
    GameObject currentSlide;

private void Start()
{
    ShowSlide(0);
}
    public void Next()
    {
        currentSlideIndex++;
        if (currentSlideIndex < slides.Length)
        {
            ShowSlide(currentSlideIndex);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void Previous()
    {
        currentSlideIndex--;
        if (currentSlideIndex > 0)
        {
            ShowSlide(currentSlideIndex);
        }
        else
        {
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
