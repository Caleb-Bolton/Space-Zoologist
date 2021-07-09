using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextEffects : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float speed = default;

    [SerializeField] TMPro.TextMeshProUGUI Introduction = default;
    [SerializeField] List<string> IntroductionTexts = default;
    [SerializeField] List<Sprite> PrologueIllustrations = default;
    [SerializeField] Image BackgroundImage = default;
    [SerializeField] Image PreviousBackgroundImage = default;
    [SerializeField] SceneNavigator SceneNavigator = default;
    [SerializeField] MusicManager musicManager = default;
    private int Index = 0;


    private void Start()
    {
        var color = Introduction.color;
        color.a = 255;
        Introduction.color = color;
        SetupNextScene();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && Index < IntroductionTexts.Count)
        {
            SetupNextScene();
            if (Index == IntroductionTexts.Count)
            {
                Introduction.alignment = TMPro.TextAlignmentOptions.TopLeft;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            //musicManager.StartTransition();
            SceneNavigator.LoadMainMenu();
        }
        FadeNextSceneIn(Introduction);
    }

    private void FadeNextSceneIn(TMPro.TextMeshProUGUI line)
    {
        if (line.color.a <= 255)
        {
            var color = line.color;
            color.a += Time.deltaTime * speed;
            line.color = color;
        }
        if (BackgroundImage.color.a <= 255)
        {
            var color = BackgroundImage.color;
            color.a += Time.deltaTime * speed;
            BackgroundImage.color = color;
            color = PreviousBackgroundImage.color;
            color.a += Time.deltaTime * speed;
            PreviousBackgroundImage.color = color;
        }
    }

    private void SetupNextScene()
    {
        var color = Introduction.color;
        color.a = 0;
        Introduction.color = color;
        PreviousBackgroundImage.sprite = BackgroundImage.sprite;
        Introduction.text = IntroductionTexts[Index];
        BackgroundImage.sprite = PrologueIllustrations[Index];
        color = BackgroundImage.color;
        color.a = 0;
        BackgroundImage.color = color;
        Index++;
    }
}
