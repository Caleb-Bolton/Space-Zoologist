using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] PopulationManager PopulationManager = default;
    [SerializeField] BehaviorPatternUpdater BehaviorPatternUpdater = default;
    [SerializeField] GridSystem GridSystem = default;
    [SerializeField] public GameObject PauseButton = default;
    [SerializeField] private Sprite PauseSprite = default;
    [SerializeField] private Sprite ResumeSprite = default;
    private Image PauseButtonSprite = default;
    private Button PauseButtonButton = default;

    public bool IsPaused { get; private set; }
    public bool WasPaused { get; private set; }

    private void Awake()
    {
        this.IsPaused = false;
        this.WasPaused = false;
        this.PauseButtonSprite = this.PauseButton.GetComponent<Image>();
        this.PauseButtonButton = this.PauseButton.GetComponent<Button>();
    }

    public void TogglePauseButton()
    {
        this.PauseButton.SetActive(!this.PauseButton.activeSelf);

    }

    public void TryToPause()
    {
        if (this.IsPaused)
        {
            this.WasPaused = true;
        }
        else
        {
            this.Pause();
        }
    }

    public void TryToUnpause()
    {
        if (this.WasPaused)
        {
            this.WasPaused = false;
        }
        else
        {
            this.Unpause();
        }
    }

    public void TogglePause()
    {
        if (this.IsPaused)
        {
            this.Unpause();
        }
        else
        {
            this.Pause();
        }
    }

    public void Pause()
    {
        Time.timeScale = 1;
        this.IsPaused = true;
        this.PauseButtonSprite.sprite = this.ResumeSprite;
        this.PauseButtonButton.onClick.RemoveAllListeners();
        this.PauseButtonButton.onClick.AddListener(this.Unpause);
        this.BehaviorPatternUpdater.IsPaused = true;
        this.PauseAllAnimalsMovementController();
        this.GridSystem.UpdateAnimalCellGrid();
        AudioManager.instance.PlayOneShot(SFXType.Pause);
    }

    public void Unpause()
    {
        this.IsPaused = false;
        this.PauseButtonSprite.sprite = this.PauseSprite;
        this.PauseButtonButton.onClick.RemoveAllListeners();
        this.PauseButtonButton.onClick.AddListener(this.Pause);
        this.BehaviorPatternUpdater.IsPaused = false;
        this.UnpauseAllAnimalsMovementController();
        AudioManager.instance.PlayOneShot(SFXType.Unpause);
    }

    public void TwoTimeSpeed() {
        Time.timeScale = 2;
    }

    public void FourTimeSpeed() {
        Time.timeScale = 4;
    }

    public void PauseAllAnimalsMovementController()
    {
       foreach (Population population in PopulationManager.Populations)
        {
            population.PauseAnimalsMovementController();
        }
    }

    public void UnpauseAllAnimalsMovementController()
    {
        foreach (Population population in PopulationManager.Populations)
        {
            population.UnpauseAnimalsMovementController();
        }
    }
}
