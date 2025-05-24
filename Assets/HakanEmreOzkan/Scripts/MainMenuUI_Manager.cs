using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VInspector;
using UnityEngine.Rendering.Universal;

public class MainMenuUI_Manager : MonoBehaviour
{
    #region Variables

    [Foldout("UI")]
    [SerializeField] private List<GameObject> panels;
    [EndFoldout]

    [Foldout("UI/Button/Main Menu Buttons")]
    [SerializeField] private List<Button> mmButtons;
    [EndFoldout]

    [Foldout("UI/Button/Select Chapter Buttons")]
    [SerializeField] private List<Button> scButtons;
    [EndFoldout]

    [Foldout("UI/Button/Chapter 1 Levels Buttons")]
    [SerializeField] private List<Button> c1LButtons;
    [EndFoldout]

    [Foldout("UI/Button/Chapter 2 Levels Buttons")]
    [SerializeField] private List<Button> c2LButtons;
    [EndFoldout]

    [Foldout("UI/Button/Chapter 3 Levels Buttons")]
    [SerializeField] private List<Button> c3LButtons;
    [EndFoldout]


    [Foldout("UI/Button/Create Flag Buttons")]
    [SerializeField, ReadOnly] private string verySoon;
    [EndFoldout]

    [Foldout("UI/Settings UI Elements")]
    [SerializeField] private Slider sfxSld;
    [SerializeField] private Slider musicSld;
    [SerializeField] private TMP_Dropdown resolutionDrp;
    [SerializeField] private TMP_Dropdown antiAliasingDrp;
    [SerializeField] private TMP_Dropdown graphicQualityDrp;
    [SerializeField] private TMP_Dropdown shadowQualityDrp;
    [SerializeField] private TMP_Dropdown textureQualityDrp;
    [SerializeField] private Toggle postProcessingTgl;
    [SerializeField] private Toggle vSycnTgl;
    [EndFoldout]

    [Foldout("UI/Scene Names/Chapter 1")]
    [SerializeField] private string chapter1level1;
    [SerializeField] private string chapter1level2;
    [SerializeField] private string chapter1level3;
    [SerializeField] private string chapter1level4;
    [SerializeField] private string chapter1level5;
    [EndFoldout]

    [Foldout("UI/Scene Names/Chapter 2")]
    [SerializeField] private string chapter2level1;
    [SerializeField] private string chapter2level2;
    [SerializeField] private string chapter2level3;
    [SerializeField] private string chapter2level4;
    [SerializeField] private string chapter2level5;
    [EndFoldout]

    [Foldout("UI/Scene Names/Chapter 3")]
    [SerializeField] private string chapter3level1;
    [SerializeField] private string chapter3level2;
    [SerializeField] private string chapter3level3;
    [SerializeField] private string chapter3level4;
    [SerializeField] private string chapter3level5;
    [EndFoldout]
    #endregion

    private void Start() { SecondSetup(); }

    #region Setups

    private void SecondSetup()
    {
        Time.timeScale = 1;

        LoadSettings();
        InitializeChapterAndLevelPrefs();
        JustOpenTheMainMenu();

        #region Buttons

        #region Main Menu Buttons Setup

        mmButtons[0].onClick.AddListener(PlayGame);
        mmButtons[1].onClick.AddListener(OpenChapterPanel);
        mmButtons[2].onClick.AddListener(OpenCreateFlagPanel);
        mmButtons[3].onClick.AddListener(OpenSettingsPanel);
        mmButtons[4].onClick.AddListener(QuitGame);

        #endregion

        #region Select Chapter Buttons Setup

        scButtons[0].onClick.AddListener(OpenChapter1LevelsPanel);
        scButtons[1].onClick.AddListener(OpenChapter2LevelsPanel);
        scButtons[2].onClick.AddListener(OpenChapter3LevelsPanel);

        #endregion

        #region Chapter 1 Levels Buttons Setup

        c1LButtons[0].onClick.AddListener(OpenC1Level1);
        c1LButtons[1].onClick.AddListener(OpenC1Level2);
        c1LButtons[2].onClick.AddListener(OpenC1Level3);
        c1LButtons[3].onClick.AddListener(OpenC1Level4);
        c1LButtons[4].onClick.AddListener(OpenC1Level5);

        #endregion

        #region Chapter 2 Levels Buttons Setup

        c2LButtons[0].onClick.AddListener(OpenC2Level1);
        c2LButtons[1].onClick.AddListener(OpenC2Level2);
        c2LButtons[2].onClick.AddListener(OpenC2Level3);
        c2LButtons[3].onClick.AddListener(OpenC2Level4);
        c2LButtons[4].onClick.AddListener(OpenC2Level5);

        #endregion

        #region Chapter 3 Levels Buttons Setup

        c3LButtons[0].onClick.AddListener(OpenC3Level1);
        c3LButtons[1].onClick.AddListener(OpenC3Level2);
        c3LButtons[2].onClick.AddListener(OpenC3Level3);
        c3LButtons[3].onClick.AddListener(OpenC3Level4);
        c3LButtons[4].onClick.AddListener(OpenC3Level5);

        #endregion

        #endregion
    }

    #endregion

    #region Methods

    #region General Methods
    private void InitializeChapterAndLevelPrefs()
    {
        //For Chapter Setup
        for (int i = 1; i <= 3; i++)
        {
            if (!PlayerPrefs.HasKey($"Chapter{i}"))
            {
                PlayerPrefs.SetInt($"Chapter{i}", 0);

                if (i >= 3) PlayerPrefs.SetInt("Chapter1", 1); // default setup
            }
        }

        //For Chapter 1 Levels Setup
        for (int i = 1; i <= 5; i++)
        {
            if (!PlayerPrefs.HasKey($"C1Level{i}"))
            {
                PlayerPrefs.SetInt($"C1Level{i}", 0);

                if (i >= 5) PlayerPrefs.SetInt("C1Level1", 1); // default setup
            }

        }

        //For Chapter 2 Levels Setup
        for (int i = 1; i <= 5; i++)
        {
            if (!PlayerPrefs.HasKey($"C2Level{i}"))
            {
                PlayerPrefs.SetInt($"C2Level{i}", 0);

                if (i >= 5) PlayerPrefs.SetInt("C1Level1", 1); // default setup
            }
        }

        //For Chapter 3 Levels Setup
        for (int i = 1; i <= 5; i++)
        {
            if (!PlayerPrefs.HasKey($"C3Level{i}"))
            {
                PlayerPrefs.SetInt($"C3Level{i}", 0);

                if (i >= 5) PlayerPrefs.SetInt("C1Level1", 1); // default setup
            }
        }

    }
    private void JustOpenTheMainMenu()
    {
        OpenTheOnePanel("MainMenuPanel");
    }

    private void SearchActiveLevel(int whichChapter)
    {
        for (int j = 1; j <= 5; j++)
        {
            if (PlayerPrefs.GetInt($"C{whichChapter}Level{j}") == 1)
            {
                switch (j)
                {
                    case 1:
                        switch (whichChapter)
                        {
                            case 1:
                                JustOpenTheScene(chapter1level1);
                                break;
                            case 2:
                                JustOpenTheScene(chapter2level1);
                                break;
                            case 3:
                                JustOpenTheScene(chapter3level1);
                                break;
                        }
                        break;
                    case 2:
                        switch (whichChapter)
                        {
                            case 1:
                                JustOpenTheScene(chapter1level2);
                                break;
                            case 2:
                                JustOpenTheScene(chapter2level2);
                                break;
                            case 3:
                                JustOpenTheScene(chapter3level2);
                                break;
                        }
                        break;
                    case 3:
                        switch (whichChapter)
                        {
                            case 1:
                                JustOpenTheScene(chapter1level3);
                                break;
                            case 2:
                                JustOpenTheScene(chapter2level3);
                                break;
                            case 3:
                                JustOpenTheScene(chapter3level3);
                                break;
                        }
                        break;
                    case 4:
                        switch (whichChapter)
                        {
                            case 1:
                                JustOpenTheScene(chapter1level4);
                                break;
                            case 2:
                                JustOpenTheScene(chapter2level4);
                                break;
                            case 3:
                                JustOpenTheScene(chapter3level4);
                                break;
                        }
                        break;
                    case 5:
                        switch (whichChapter)
                        {
                            case 1:
                                JustOpenTheScene(chapter1level5);
                                break;
                            case 2:
                                JustOpenTheScene(chapter2level5);
                                break;
                            case 3:
                                JustOpenTheScene(chapter3level5);
                                break;
                        }
                        break;
                }
            }
        }
    }

    private void JustOpenTheScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void OpenTheOnePanel(string panelName)
    {
        foreach (var panel in panels) if (panel.gameObject.name == panelName) panel.gameObject.SetActive(true); else panel.gameObject.SetActive(false);
    }

    #endregion

    #region Buttons Methods

    #region General Buttons Methods

    public void GoBack()
    {
        if (!panels[2].gameObject.activeInHierarchy &&
            !panels[3].gameObject.activeInHierarchy &&
            !panels[4].gameObject.activeInHierarchy)
        {
            foreach (var panel in panels)
            {
                if (panel.gameObject.activeInHierarchy)
                {
                    if (panel.gameObject.name == "SettingsPanel") SaveSettings();
                    panel.gameObject.SetActive(false);
                }
            }
            //main menu active
            panels[0].gameObject.SetActive(true);
        }
        else
        {
            foreach (var panel in panels)
            {
                if (panel.gameObject.activeInHierarchy) panel.gameObject.SetActive(false);

            }
            //select chapter panel active
            panels[1].gameObject.SetActive(true);
        }
    }

    #endregion

    #region Main Menu Methods

    private void PlayGame()
    {
        for (int i = 1; i <= 3; i++)
        {
            if (PlayerPrefs.GetInt($"Chapter{i}") == 1)
            {
                switch (i)
                {
                    case 1:
                        SearchActiveLevel(i);
                        break;
                    case 2:
                        SearchActiveLevel(i);
                        break;
                    case 3:
                        SearchActiveLevel(i);
                        break;
                }
            }
        }
    }

    private void OpenChapterPanel()
    {
        for (int i = 1; i <= 3; i++)
        {
            if (PlayerPrefs.GetInt($"Chapter{i}") == 1)
            {
                foreach (var button in scButtons) if (scButtons.IndexOf(button) == i - 1) button.interactable = true; else button.interactable = false;
            }
        }
        OpenTheOnePanel("SelectChapterPanel");
    }

    private void OpenCreateFlagPanel()
    {
        OpenTheOnePanel("CreateFlagPanel");
    }

    private void OpenSettingsPanel()
    {
        LoadSettings();
        OpenTheOnePanel("SettingsPanel");
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    #endregion

    #region Select Chapter Methods

    private void OpenChapter1LevelsPanel()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (PlayerPrefs.GetInt($"C1Level{i}") == 1)
            {
                foreach (var button in c1LButtons) if (c1LButtons.IndexOf(button) == i - 1) button.interactable = true; else button.interactable = false;
            }
        }
        OpenTheOnePanel("Chapter1LevelsPanel");
    }
    private void OpenChapter2LevelsPanel()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (PlayerPrefs.GetInt($"C2Level{i}") == 1)
            {
                foreach (var button in c2LButtons) if (c2LButtons.IndexOf(button) == i - 1) button.interactable = true; else button.interactable = false;
            }
        }
        OpenTheOnePanel("Chapter2LevelsPanel");
    }
    private void OpenChapter3LevelsPanel()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (PlayerPrefs.GetInt($"C3Level{i}") == 1)
            {
                foreach (var button in c3LButtons) if (c3LButtons.IndexOf(button) == i - 1) button.interactable = true; else button.interactable = false;
            }
        }
        OpenTheOnePanel("Chapter3LevelsPanel");
    }

    #endregion

    #region Chapter 1 Levels Methods

    private void OpenC1Level1()
    {
        JustOpenTheScene(chapter1level1);
    }
    private void OpenC1Level2()
    {
        JustOpenTheScene(chapter1level2);
    }
    private void OpenC1Level3()
    {
        JustOpenTheScene(chapter1level3);
    }
    private void OpenC1Level4()
    {
        JustOpenTheScene(chapter1level4);
    }
    private void OpenC1Level5()
    {
        JustOpenTheScene(chapter1level5);
    }

    #endregion

    #region Chapter 2 Levels Methods

    private void OpenC2Level1()
    {
        JustOpenTheScene(chapter2level1);
    }
    private void OpenC2Level2()
    {
        JustOpenTheScene(chapter2level2);
    }
    private void OpenC2Level3()
    {
        JustOpenTheScene(chapter2level3);
    }
    private void OpenC2Level4()
    {
        JustOpenTheScene(chapter2level4);
    }
    private void OpenC2Level5()
    {
        JustOpenTheScene(chapter2level5);
    }

    #endregion

    #region Chapter 3 Levels Methods

    private void OpenC3Level1()
    {
        JustOpenTheScene(chapter3level1);
    }
    private void OpenC3Level2()
    {
        JustOpenTheScene(chapter3level2);
    }
    private void OpenC3Level3()
    {
        JustOpenTheScene(chapter3level3);
    }
    private void OpenC3Level4()
    {
        JustOpenTheScene(chapter3level4);
    }
    private void OpenC3Level5()
    {
        JustOpenTheScene(chapter3level5);
    }

    #endregion

    #endregion

    #region Settings UI Elements Methods
    public void SetQualityLevelDropdown()
    {
        QualitySettings.SetQualityLevel(graphicQualityDrp.value, false);
    }
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("graphicQuality", graphicQualityDrp.value);

    }

    private void LoadSettings()
    {
        if (!PlayerPrefs.HasKey("graphicQuality"))
        {
            graphicQualityDrp.value = 2;
            QualitySettings.SetQualityLevel(graphicQualityDrp.value, false);
            PlayerPrefs.SetInt("graphicQuality", graphicQualityDrp.value);
        }
        else
        {
            graphicQualityDrp.value = graphicQualityDrp.value = PlayerPrefs.GetInt("graphicQuality");
            QualitySettings.SetQualityLevel(graphicQualityDrp.value, false);
        }
    }
    #endregion
    #endregion
}
