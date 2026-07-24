using UnityEngine;

public class ControladorSonido : MonoBehaviour
{
    public static ControladorSonido Instance { get; private set; }

    [Header("Clips de Audio")]
    [SerializeField] private AudioClip musicaInicio;
    [SerializeField] private AudioClip sonidoClick;

    [Header("Volumenes")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenMusica = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float volumenEfectos = 0.7f;

    private AudioSource musicaFondo;
    private AudioSource efectosSonido;

    private bool sonidoActivado = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        musicaFondo = gameObject.AddComponent<AudioSource>();
        musicaFondo.loop = true;
        musicaFondo.playOnAwake = false;
        musicaFondo.volume = volumenMusica;

        efectosSonido = gameObject.AddComponent<AudioSource>();
        efectosSonido.loop = false;
        efectosSonido.playOnAwake = false;
        efectosSonido.volume = volumenEfectos;

        if (!PlayerPrefs.HasKey("SonidoActivado"))
        {
            PlayerPrefs.SetInt("SonidoActivado", 1);
            PlayerPrefs.Save();
        }

        sonidoActivado = PlayerPrefs.GetInt("SonidoActivado", 1) == 1;
    }

    void Start()
    {
        if (musicaInicio != null && musicaFondo != null)
        {
            musicaFondo.clip = musicaInicio;
            musicaFondo.Play();
        }
    }

    public void SetSonidoActivado(bool activado)
    {
        sonidoActivado = activado;
        PlayerPrefs.SetInt("SonidoActivado", activado ? 1 : 0);
        PlayerPrefs.Save();

        if (musicaFondo != null)
        {
            musicaFondo.volume = activado ? volumenMusica : 0f;
            if (activado && !musicaFondo.isPlaying && musicaFondo.clip != null)
                musicaFondo.Play();
            else if (!activado)
                musicaFondo.Pause();
        }

        if (efectosSonido != null)
        {
            efectosSonido.volume = activado ? volumenEfectos : 0f;
        }
    }

    public bool SonidoActivado()
    {
        return sonidoActivado;
    }

    public void ReproducirClick()
    {
        if (!sonidoActivado || efectosSonido == null || sonidoClick == null) return;
        efectosSonido.PlayOneShot(sonidoClick, volumenEfectos);
    }

    public void ReproducirMusica(AudioClip nuevaMusica)
    {
        if (musicaFondo == null || nuevaMusica == null) return;
        musicaFondo.clip = nuevaMusica;
        if (sonidoActivado) musicaFondo.Play();
    }
}