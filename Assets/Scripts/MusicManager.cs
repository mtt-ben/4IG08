using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip menuMusic;
    public AudioClip levelMusic;
    public static MusicManager Instance;

    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SetVolume(savedVolume);
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void Awake()
    {
         if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    void PlayMusicForScene(string sceneName)
    {
        AudioClip targetClip;
        if (sceneName == "MainMenu" || sceneName == "Settings" || sceneName == "Tutorial"){
            targetClip = menuMusic;
        }
        else {
            targetClip = levelMusic;
        }
        if (audioSource.clip == targetClip && audioSource.isPlaying) {
            return;
        }
        audioSource.clip = targetClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void SetVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
        PlayerPrefs.Save();
        audioSource.volume = sliderValue;
    }
}
