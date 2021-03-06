using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class Options : MonoBehaviour
{
    // Default Values
    [Header("Vignette")]
    public float DefaultVignetteIntensity = 0.25f;
    public bool DefaultVignetteEnabled = true;

    [Header("Bloom")]
    public float DefaultBloomIntensity = 30f;
    public bool DefaultBloomEnabled = true;

    [Header("Volume")]
    public float DefaultVolumeMusic = 1f;
    public float DefaultVolumeSFX = 1f;

    [Header("VSync")]
    public int DefaultVSyncValue = 1;
    public bool DefaultVSyncEnabled = true;

    [Header("Camera")]
    public float DefaultSensitivityPan = 3f;
    public float DefaultSensitivityZoom = 10f;

    // Links
    [Header("Links Options General")]
    public GameObject ToggleVignette;
    public GameObject SliderVignette;
    public GameObject ToggleBloom;
    public GameObject SliderBloom;
    public GameObject ToggleFullscreen;
    public GameObject ToggleVSync;
    public GameObject DropdownResolution;
    public GameObject DropdownQuality;
    public GameObject SliderVolumeMusic;
    public GameObject SliderVolumeSFX;
    public GameObject SliderSensitivityZoom;
    public GameObject SliderSensitivityPan;
    public GameObject ButtonResetToDefaults;
    public GameObject ButtonBackToMenu;
    public GameObject ButtonControls;
    public GameObject OptionsGeneral;
    public GameObject OptionsControls;
    public GameObject ButtonBackToOptionsGeneral;

    [Header("Links Options Hotkeys")]
    public GameObject HotkeyEscape;

    public static float VolumeSFX = 1.0f;
    public static float VolumeMusic = 1.0f;
    public static float SensitivityPan = 3f;
    public static float SensitivityZoom = 10f;

    private GameObject goPostProcessing;
    private GameObject goMusic;

    private PostProcessVolume ppVolume;
    private Bloom ppBloom;
    private Vignette ppVignette;

    private AudioSource musicAudioSource;

    private Resolution[] resolutions;
    private static int dropdownResolutionIndex = -1;

    private UISlider uiSliderBloom;
    private UIToggle uiToggleBloom;
    private UISlider uiSliderVolumeMusic;
    private UISlider uiSliderVolumeSFX;
    private UIToggle uiToggleVignette;
    private UISlider uiSliderVignette;
    private UIDropdown uiDropdownResolution;
    private UIDropdown uiDropdownQuality;
    private UIToggle uiToggleVSync;
    private UIToggle uiToggleFullscreen;
    private UISlider uiSliderSensitivityPan;
    private UISlider uiSliderSensitivityZoom;

    private UIButton buttonResetToDefaults;
    private UIButton buttonBackToMenu;
    private UIButton buttonGoToControls;
    private UIButton buttonGoToOptionsGeneral;

    private UIHotkey hotkeyEscape;

    private bool inControls;

    public void Awake()
    {
        SceneManager.sceneUnloaded += ClearStaticUIs;
    }

    public void Start()
    {
        #region Setup

        goPostProcessing = DontDestroy.DontDestroyObjects[0]; // Referencing by index[0] seems too abstract

        if (goPostProcessing == null)
        {
            Debug.Log("Post Processing has to be loaded from the 'Menu' scene first.");
        }
        else
        {
            ppVolume = goPostProcessing.GetComponent<PostProcessVolume>();

            PostProcessProfile ppProfile = ppVolume.profile;

            ppProfile.TryGetSettings(out ppBloom);
            ppProfile.TryGetSettings(out ppVignette);
        }

        GameObject goMenuMusic = GameObject.Find("Menu Music");
        GameObject goGameMusic = GameObject.Find("Game Music");

        goMusic = goMenuMusic == null ? goGameMusic : goMenuMusic;
        musicAudioSource = goMusic.GetComponent<AudioSource>();

        if (goPostProcessing == null || goMusic == null)
        {
            Debug.Log(goPostProcessing == null ? "Post Processing is not loaded." : "Post Processing is loaded.");
            Debug.Log(goMusic == null ? "Music is not loaded." : "Music is loaded.");
            return;
        }

        // --Msc--
        // Vignette
        uiToggleVignette = new UIToggle(ToggleVignette);
        uiSliderVignette = new UISlider(SliderVignette);

        // Bloom
        uiToggleBloom = new UIToggle(ToggleBloom);
        uiSliderBloom = new UISlider(SliderBloom);

        // Fullscreen
        uiToggleFullscreen = new UIToggle(ToggleFullscreen);
        // VSync
        uiToggleVSync = new UIToggle(ToggleVSync);

        // Resolutions
        uiDropdownResolution = new UIDropdown(DropdownResolution);
        InitializeResolutionsDropDown();

        // Quality
        uiDropdownQuality = new UIDropdown(DropdownQuality);
        InitializeQualityDropDown();

        // --Volume--
        // Volume
        uiSliderVolumeMusic = new UISlider(SliderVolumeMusic);
        uiSliderVolumeSFX = new UISlider(SliderVolumeSFX);

        // --Game--
        // Camera
        uiSliderSensitivityZoom = new UISlider(SliderSensitivityZoom);
        uiSliderSensitivityPan = new UISlider(SliderSensitivityPan);

        buttonResetToDefaults = new UIButton(ButtonResetToDefaults);
        buttonResetToDefaults.AddListener(() =>
        {
            ResetToDefaults();
        });

        buttonBackToMenu = new UIButton(ButtonBackToMenu);
        buttonBackToMenu.AddListener(() =>
        {
            if (SceneManager.GetActiveScene().name == "Options")
            {
                SceneManager.LoadScene("Menu");
            }
            else
            {
                UIListener.InOptions = false;
                UIListener.MenuMain.SetActive(true);
                UIListener.MenuOptions.SetActive(false);
            }
        });

        buttonGoToControls = new UIButton(ButtonControls);
        buttonGoToControls.AddListener(() =>
        {
            inControls = !inControls;
            OptionsGeneral.SetActive(!inControls);
            OptionsControls.SetActive(inControls);
        });

        buttonGoToOptionsGeneral = new UIButton(ButtonBackToOptionsGeneral);
        buttonGoToOptionsGeneral.AddListener(() =>
        {
            inControls = !inControls;
            OptionsGeneral.SetActive(!inControls);
            OptionsControls.SetActive(inControls);
        });

        hotkeyEscape = new UIHotkey(HotkeyEscape);

        #endregion Setup

        InitializeUIValues();
        InitializePlayerPrefs();
        InitializeOptionValues();
    }

    private bool keyHeldDown;

    public void OnGUI()
    {
        if (Event.current.keyCode != KeyCode.None)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (!keyHeldDown)
                {
                    for (int i = 0; i < UIHotkey.UIHotkeys.Count; i++)
                    {
                        if (UIHotkey.UIHotkeys[i].Selected)
                        {
                            keyHeldDown = true;
                            UIHotkey.UIHotkeys[i].UpdateText(Event.current.keyCode.ToString());
                        }
                    }
                }
            }

            if (Event.current.type == EventType.KeyUp)
            {
                keyHeldDown = false;
            }
        }
    }

    private void InitializePlayerPrefs()
    {
        uiToggleBloom.Save("uis.bloom.enabled", DefaultBloomEnabled);
        uiSliderBloom.Save("uis.bloom.intensity", DefaultBloomIntensity);
        uiSliderVolumeMusic.Save("uis.volume.music", DefaultVolumeMusic);
        uiSliderVolumeSFX.Save("uis.volume.sfx", DefaultVolumeSFX);
        uiToggleVignette.Save("uis.vignette.enabled", DefaultVignetteEnabled);
        uiSliderVignette.Save("uis.vignette.intensity", DefaultVignetteIntensity);
        uiDropdownResolution.Save("uis.resolution", resolutions.Length);
        uiDropdownQuality.Save("uis.quality", QualitySettings.GetQualityLevel());
        uiToggleFullscreen.Save("uis.fullscreen", Screen.fullScreen);
        uiToggleVSync.Save("uis.vsync", QualitySettings.vSyncCount == 0 ? true : false);
        uiSliderSensitivityPan.Save("uis.sensitivity.pan", DefaultSensitivityPan);
        uiSliderSensitivityZoom.Save("uis.sensitivity.zoom", DefaultSensitivityZoom);
    }

    private void InitializeUIValues()
    {
        ppBloom.enabled.value = PlayerPrefsX.GetBool("uis.bloom.enabled");
        ppBloom.intensity.value = PlayerPrefs.GetFloat("uis.bloom.intensity");
        ppVignette.enabled.value = PlayerPrefsX.GetBool("uis.vignette.enabled");
        ppVignette.intensity.value = PlayerPrefs.GetFloat("uis.vignette.intensity");
        uiToggleFullscreen.Instance.isOn = PlayerPrefsX.GetBool("uis.fullscreen");
        uiToggleVSync.Instance.isOn = PlayerPrefsX.GetBool("uis.vsync");
    }

    private void InitializeOptionValues()
    {
        // Bloom
        uiToggleBloom.AddListener(() =>
        {
            ppBloom.enabled.value = !ppBloom.enabled.value;
            uiSliderBloom.Instance.interactable = !uiSliderBloom.Instance.interactable;
            PlayerPrefsX.SetBool("uis.bloom.enabled", ppBloom.enabled.value);
        });

        uiSliderBloom.Instance.interactable = uiToggleBloom.Instance.isOn;
        uiSliderBloom.AddListener(() =>
        {
            ppBloom.intensity.value = uiSliderBloom.Instance.value;
            PlayerPrefs.SetFloat("uis.bloom.intensity", ppBloom.intensity.value);
        });

        // Volume
        uiSliderVolumeMusic.AddListener(() =>
        {
            VolumeMusic = uiSliderVolumeMusic.Instance.value;
            musicAudioSource.volume = VolumeMusic;
            PlayerPrefs.SetFloat("uis.volume.music", VolumeMusic);
        });

        uiSliderVolumeSFX.AddListener(() =>
        {
            VolumeSFX = uiSliderVolumeSFX.Instance.value;
            PlayerPrefs.SetFloat("uis.volume.sfx", VolumeSFX);
        });

        // Vignette
        uiToggleVignette.AddListener(() =>
        {
            ppVignette.enabled.value = !ppVignette.enabled.value;
            uiSliderVignette.Instance.interactable = !uiSliderVignette.Instance.interactable;
            PlayerPrefsX.SetBool("uis.vignette.enabled", ppVignette.enabled.value);
        });

        uiSliderVignette.Instance.interactable = uiToggleVignette.Instance.isOn;
        uiSliderVignette.AddListener(() =>
        {
            ppVignette.intensity.value = uiSliderVignette.Instance.value;
            PlayerPrefs.SetFloat("uis.vignette.intensity", ppVignette.intensity.value);
        });

        // VSync
        uiToggleVSync.AddListener(() =>
        {
            QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
            PlayerPrefsX.SetBool("uis.vsync", QualitySettings.vSyncCount == 1 ? true : false);
        });

        // Fullscreen
        uiToggleFullscreen.AddListener(() =>
        {
            Screen.fullScreen = !Screen.fullScreen;
            PlayerPrefsX.SetBool("uis.fullscreen", Screen.fullScreen);
        });

        // Camera
        uiSliderSensitivityPan.AddListener(() =>
        {
            SensitivityPan = uiSliderSensitivityPan.Instance.value;
            PlayerPrefs.SetFloat("uis.sensitivity.pan", SensitivityPan);
        });

        uiSliderSensitivityZoom.AddListener(() =>
        {
            SensitivityZoom = uiSliderSensitivityZoom.Instance.value;
            PlayerPrefs.SetFloat("uis.sensitivity.zoom", SensitivityZoom);
        });

        // Resolutions
        uiDropdownResolution.AddListener(() =>
        {
            dropdownResolutionIndex = uiDropdownResolution.Instance.value;
            Screen.SetResolution(resolutions[uiDropdownResolution.Instance.value].width, resolutions[uiDropdownResolution.Instance.value].height, Screen.fullScreen);
            PlayerPrefs.SetInt("uis.resolution", dropdownResolutionIndex);
        });

        // Quality
        uiDropdownQuality.AddListener(() =>
        {
            QualitySettings.SetQualityLevel(uiDropdownQuality.Instance.value);
            PlayerPrefs.SetInt("uis.quality", uiDropdownQuality.Instance.value);
        });

        /*foreach (UIElement ui in UIElement.UIElements)
        {
            ui.SetActive(true);
        }*/
    }

    private void ResetToDefaults()
    {
        ppBloom.enabled.value = DefaultBloomEnabled; // Bloom
        ppBloom.intensity.value = DefaultBloomIntensity;
        uiToggleBloom.Instance.isOn = DefaultBloomEnabled;
        uiSliderBloom.Instance.value = DefaultBloomIntensity;

        ppVignette.enabled.value = DefaultVignetteEnabled; // Vignette
        ppVignette.intensity.value = DefaultVignetteIntensity;
        uiToggleVignette.Instance.isOn = DefaultVignetteEnabled;
        uiSliderVignette.Instance.value = DefaultVignetteIntensity;

        uiSliderVolumeMusic.Instance.value = DefaultVolumeMusic; // Volume (Music)
        uiSliderVolumeSFX.Instance.value = DefaultVolumeSFX; // Volume (SFX)

        uiToggleVSync.Instance.isOn = DefaultVSyncEnabled; // VSync
        QualitySettings.vSyncCount = DefaultVSyncValue;

        uiSliderSensitivityPan.Instance.value = DefaultSensitivityPan; // Camera Pan
        uiSliderSensitivityZoom.Instance.value = DefaultSensitivityZoom; // Camera Zoom
    }

    private void InitializeResolutionsDropDown()
    {
        resolutions = Screen.resolutions;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];
            int refreshRate = res.refreshRate;

            uiDropdownResolution.Instance.options.Add(new TMP_Dropdown.OptionData(ResolutionToString(res)));
        }

        uiDropdownResolution.Instance.value = PlayerPrefs.GetInt("uis.resolution");
    }

    private void InitializeQualityDropDown()
    {
        string[] names = QualitySettings.names;

        for (int i = 0; i < names.Length; i++)
        {
            uiDropdownQuality.Instance.options.Add(new TMP_Dropdown.OptionData(names[i]));
        }

        uiDropdownQuality.Instance.value = PlayerPrefs.GetInt("uis.quality");
    }

    private string ResolutionToString(Resolution res)
    {
        return res.width + " x " + res.height + " (" + res.refreshRate + " Hz)";
    }

    private void ClearStaticUIs<Scene>(Scene scene)
    {
        UIElement.UIElements.Clear();
    }
}