using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    // Inspector Options
    [Header("Vignette")]
    [SerializeField] public float DefaultVignetteIntensity = 0.25f;
    [SerializeField] public bool DefaultVignetteEnabled = true;

    [Header("Bloom")]
    [SerializeField] public float DefaultBloomIntensity = 30f;
    [SerializeField] public bool DefaultBloomEnabled = true;

    [Header("Volume")]
    [SerializeField] public float DefaultVolumeMusic = 1f;
    [SerializeField] public float DefaultVolumeSFX = 1f;

    [Header("VSync")]
    [SerializeField] public int DefaultVSyncValue = 1;
    [SerializeField] public bool DefaultVSyncEnabled = true;

    [Header("Camera")]
    [SerializeField] public float DefaultSensitivityPan = 3f;
    [SerializeField] public float DefaultSensitivityZoom = 10f;

    // Static
    [HideInInspector] public static float VolumeSFX = 1.0f;
    [HideInInspector] public static float SensitivityPan = 3f;
    [HideInInspector] public static float SensitivityZoom = 10f;
    [HideInInspector] public static bool OptionsLoading = true;

    // Private
    private static int dropdownResolutionIndex = -1;

    private GameObject goPostProcessing;
    private GameObject goMenuMusic;

    private PostProcessVolume ppVolume;
    private Bloom ppBloom;
    private Vignette ppVignette;

    private AudioSource audioMenuMusic;

    private Slider sliderVignette;
    private Slider sliderBloom;
    private Slider sliderVolumeMusic;
    private Slider sliderVolumeSFX;
    private Slider sliderSensitivityPan;
    private Slider sliderSensitivityZoom;

    private Toggle toggleFullscreen;
    private Toggle toggleVignette;
    private Toggle toggleBloom;
    private Toggle toggleVSync;

    private Dropdown dropdownResolutions;
    private Dropdown dropdownQuality;

    private Color menuColor;

    public void Start()
    {
        menuColor = new Color32(255, 105, 180, 255); // Default Color

        goPostProcessing = DontDestroy.Go;

        if (goPostProcessing != null)
        {
            ppVolume = goPostProcessing.GetComponent<PostProcessVolume>();

            PostProcessProfile ppProfile = ppVolume.profile;

            ppProfile.TryGetSettings(out ppBloom);
            ppProfile.TryGetSettings(out ppVignette);
        }
        else
        {
            Debug.Log("Post Processing has to be loaded from the 'Menu' scene first.");
        }

        goMenuMusic = GameObject.Find("Menu Music");
        if (goMenuMusic != null)
        {
            audioMenuMusic = goMenuMusic.GetComponent<AudioSource>();
        }
        else
        {
            Debug.Log("Menu Music has to be loaded from the 'Menu' scene first.");
        }

        // Bloom
        sliderBloom = GameObject.Find("SliderBloom").GetComponent<Slider>();
        toggleBloom = GameObject.Find("ToggleBloom").GetComponent<Toggle>();

        // Volume
        sliderVolumeMusic = GameObject.Find("SliderVolumeMusic").GetComponent<Slider>();
        sliderVolumeSFX = GameObject.Find("SliderVolumeSFX").GetComponent<Slider>();

        // Vignette
        toggleVignette = GameObject.Find("ToggleVignette").GetComponent<Toggle>();
        sliderVignette = GameObject.Find("SliderVignette").GetComponent<Slider>();

        // Resolutions
        dropdownResolutions = GameObject.Find("DropdownResolutions").GetComponent<Dropdown>();
        InitializeResolutionsDropDown();

        // Quality
        dropdownQuality = GameObject.Find("DropdownQuality").GetComponent<Dropdown>();
        InitializeQualityDropDown();

        // VSync
        toggleVSync = GameObject.Find("ToggleVSync").GetComponent<Toggle>();

        // Fullscreen
        toggleFullscreen = GameObject.Find("ToggleFullscreen").GetComponent<Toggle>();

        // Camera
        sliderSensitivityPan = GameObject.Find("SliderSensitivityPan").GetComponent<Slider>();
        sliderSensitivityZoom = GameObject.Find("SliderSensitivityZoom").GetComponent<Slider>();

        InitializeValues();

        OptionsLoading = false;
    }

    private bool NotSetup()
    {
        return goPostProcessing == null || goMenuMusic == null;
    }

    private Resolution[] resolutions;

    private void InitializeResolutionsDropDown()
    {
        resolutions = Screen.resolutions;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];
            int refreshRate = res.refreshRate;

            dropdownResolutions.options.Add(new Dropdown.OptionData(ResolutionToString(res)));
        }

        dropdownResolutions.value = dropdownResolutionIndex == -1 ? resolutions.Length : dropdownResolutionIndex;
    }

    private void InitializeQualityDropDown()
    {
        string[] names = QualitySettings.names;

        for (int i = 0; i < names.Length; i++)
        {
            dropdownQuality.options.Add(new Dropdown.OptionData(names[i]));
        }
    }

    private string ResolutionToString(Resolution res)
    {
        return res.width + " x " + res.height + " (" + res.refreshRate + " Hz)";
    }

    private void InitializeValues()
    {
        if (NotSetup())
            return;

        // Bloom
        OptionToggle optionToggleBloom = new OptionToggle(toggleBloom, menuColor);
        optionToggleBloom.Instance.isOn = ppBloom.enabled.value;

        OptionSlider optionSliderBloom = new OptionSlider(sliderBloom, menuColor);
        optionSliderBloom.Instance.value = ppBloom.intensity.value;
        optionSliderBloom.Instance.interactable = toggleBloom.isOn;

        // Volume
        OptionSlider optionSliderVolumeMusic = new OptionSlider(sliderVolumeMusic, menuColor);
        optionSliderVolumeMusic.Instance.value = audioMenuMusic.volume;

        OptionSlider optionSliderVolumeSFX = new OptionSlider(sliderVolumeSFX, menuColor);
        optionSliderVolumeSFX.Instance.value = VolumeSFX;

        // Vignette
        OptionToggle optionToggleVignette = new OptionToggle(toggleVignette, menuColor);
        optionToggleVignette.Instance.isOn = ppVignette.enabled.value;

        OptionSlider optionSliderVignette = new OptionSlider(sliderVignette, menuColor);
        optionSliderVignette.Instance.value = ppVignette.intensity.value;

        // VSync
        OptionToggle optionToggleVSync = new OptionToggle(toggleVSync, menuColor);
        optionToggleVSync.Instance.isOn = QualitySettings.vSyncCount == 0 ? false : true;

        // Fullscreen
        OptionToggle optionToggleFullscreen = new OptionToggle(toggleFullscreen, menuColor);
        //optionToggleFullscreen.Instance.isOn = Screen.fullScreen;

        // Camera
        OptionSlider optionSliderSensitivityPan = new OptionSlider(sliderSensitivityPan, menuColor);
        optionSliderSensitivityPan.Instance.value = SensitivityPan;

        OptionSlider optionSliderSenitivityZoom = new OptionSlider(sliderSensitivityZoom, menuColor);
        optionSliderSenitivityZoom.Instance.value = SensitivityZoom;

        // Resolutions
        OptionDropdown optionDropdownResolutions = new OptionDropdown(dropdownResolutions, menuColor);
        optionDropdownResolutions.Instance.value = dropdownResolutionIndex;

        // Quality
        OptionDropdown optionDropdownQuality = new OptionDropdown(dropdownQuality, menuColor);
        optionDropdownQuality.Instance.value = QualitySettings.GetQualityLevel();
    }

    public void ChangeQuality()
    {
        QualitySettings.SetQualityLevel(dropdownQuality.value);
    }

    public void ChangeResolution()
    {
        dropdownResolutionIndex = dropdownResolutions.value;
        Screen.SetResolution(resolutions[dropdownResolutions.value].width, resolutions[dropdownResolutions.value].height, Screen.fullScreen);
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void ToggleVSync()
    {
        QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
    }

    public void ToggleBloom()
    {
        if (NotSetup())
            return;

        ppBloom.enabled.value = !ppBloom.enabled.value;
        sliderBloom.interactable = !sliderBloom.interactable;
    }

    public void ToggleVignette()
    {
        if (NotSetup())
            return;

        ppVignette.enabled.value = !ppVignette.enabled.value;
        sliderVignette.interactable = !sliderVignette.interactable;
    }

    public void SliderVignetteUpdate()
    {
        if (NotSetup())
            return;

        ppVignette.intensity.value = sliderVignette.value;
    }

    public void SliderBloomUpdate()
    {
        if (NotSetup())
            return;

        ppBloom.intensity.value = sliderBloom.value;
    }

    public void SliderVolumeMusicUpdate()
    {
        if (NotSetup())
            return;

        audioMenuMusic.volume = sliderVolumeMusic.value;
    }

    public void SliderVolumeSFXUpdate()
    {
        if (NotSetup())
            return;

        VolumeSFX = sliderVolumeSFX.value;
    }

    public void SliderSensitivityPanUpdate()
    {
        if (NotSetup())
            return;

        SensitivityPan = sliderSensitivityPan.value;
    }

    public void SliderSensitivityZoomUpdate()
    {
        if (NotSetup())
            return;

        SensitivityZoom = sliderSensitivityZoom.value;
    }

    public void ResetToDefaults()
    {
        if (NotSetup())
            return;

        // Bloom
        ppBloom.enabled.value = DefaultBloomEnabled;
        ppBloom.intensity.value = DefaultBloomIntensity;
        toggleBloom.isOn = DefaultBloomEnabled;
        sliderBloom.value = DefaultBloomIntensity;

        // Vignette
        ppVignette.enabled.value = DefaultVignetteEnabled;
        ppVignette.intensity.value = DefaultVignetteIntensity;
        toggleVignette.isOn = DefaultVignetteEnabled;
        sliderVignette.value = DefaultVignetteIntensity;

        // Volume (Music)
        sliderVolumeMusic.value = DefaultVolumeMusic;

        // Volume (SFX)
        sliderVolumeSFX.value = DefaultVolumeSFX;

        // VSync
        toggleVSync.isOn = DefaultVSyncEnabled;
        QualitySettings.vSyncCount = DefaultVSyncValue;

        // Camera Pan
        sliderSensitivityPan.value = DefaultSensitivityPan;

        // Camera Zoom
        sliderSensitivityZoom.value = DefaultSensitivityZoom;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}