using UnityEngine;

public class ControladorSonido : MonoBehaviour
{
    public static ControladorSonido Instance { get; private set; }

    [Header("Clips de Audio")]
    [SerializeField] private AudioClip musicaInicio;
    [SerializeField] private AudioClip sonidoClick;
    [SerializeField] private AudioClip sonidoWin;
    [SerializeField] private AudioClip sonidoLoss;

    [Header("Volumenes")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenMusica = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float volumenEfectos = 0.7f;

    private AudioSource musicaFondo;
    private AudioSource efectosSonido;

    private bool sonidoActivado = true;
    private bool musicaActivada = true;

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

        sonidoActivado = PlayerPrefs.GetInt("SonidoActivado", 1) == 1;
        musicaActivada = PlayerPrefs.GetInt("MusicaActivada", 1) == 1;

    }

    void Start()
    {
        if (musicaInicio != null && musicaFondo != null)
        {
            musicaFondo.clip = musicaInicio;
            if (musicaActivada)
            {
                musicaFondo.Play();
            }
        }
    }

    public void SetSonidoActivado(bool activado)
    {
        sonidoActivado = activado;
        PlayerPrefs.SetInt("SonidoActivado", activado ? 1 : 0);
        PlayerPrefs.Save();

        if (efectosSonido != null)
        {
            efectosSonido.volume = activado ? volumenEfectos : 0f;
        }

    }

    public bool SonidoActivado()
    {
        return sonidoActivado;
    }

    public void SetMusicaActivada(bool activado)
    {
        musicaActivada = activado;
        PlayerPrefs.SetInt("MusicaActivada", activado ? 1 : 0);
        PlayerPrefs.Save();

        if (musicaFondo != null && musicaFondo.clip != null)
        {
            if (activado)
            {
                musicaFondo.volume = volumenMusica;
                if (!musicaFondo.isPlaying)
                {
                    musicaFondo.Play();
                }
                else
                {
                    musicaFondo.UnPause();
                }
            }
            else
            {
                musicaFondo.volume = 0f;
                musicaFondo.Pause();
            }
        }

    }

    public bool MusicaActivada()
    {
        return musicaActivada;
    }

    public void ReproducirClick()
    {
        if (!sonidoActivado || efectosSonido == null || sonidoClick == null)
        {
            return;
        }
        efectosSonido.PlayOneShot(sonidoClick, volumenEfectos);
    }

    public void ReproducirWin()
    {
        if (!sonidoActivado || efectosSonido == null || sonidoWin == null) return;
        efectosSonido.PlayOneShot(sonidoWin, volumenEfectos);
    }

    public void ReproducirLoss()
    {
        if (!sonidoActivado || efectosSonido == null || sonidoLoss == null) return;
        efectosSonido.PlayOneShot(sonidoLoss, volumenEfectos);
    }

    public void ReproducirMusica(AudioClip nuevaMusica)
    {
        if (musicaFondo == null || nuevaMusica == null) return;
        musicaFondo.clip = nuevaMusica;
        if (musicaActivada)
        {
            musicaFondo.Play();
        }
    }

    public void ReproducirMusicaInicio()
    {
        if (musicaInicio != null)
        {
            ReproducirMusica(musicaInicio);
        }
    }
}