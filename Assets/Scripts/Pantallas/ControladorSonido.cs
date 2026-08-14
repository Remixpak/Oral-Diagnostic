using UnityEngine;

/// <summary>
/// Gestiona la reproducción de música de fondo y efectos de sonido en la aplicación, 
/// administrando el estado de silencio/activación y la persistencia de datos de audio. 
/// Implementa el patrón Singleton.
/// 
/// Clases dependientes que utiliza:
/// - AudioSource: Componente de Unity creado dinámicamente para la reproducción de pistas continuas y efectos puntuales.
/// - AudioClip: Recursos de audio serializados (música de inicio, clic, victoria, derrota) consumidos por los componentes AudioSource.
/// - PlayerPrefs: Utilizada para persistir las preferencias locales del usuario sobre el estado de la música y los efectos ("SonidoActivado", "MusicaActivada").
/// </summary>
public class ControladorSonido : MonoBehaviour
{
    /// <summary>
    /// Acceso global Singleton a la instancia activa del ControladorSonido.
    /// </summary>
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

    /// <summary>
    /// Garantiza la unicidad del Singleton, configura la persistencia entre escenas y crea los componentes AudioSource internos.
    /// </summary>
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

    /// <summary>
    /// Inicializa y reproduce la música de inicio predeterminada en caso de estar activada la música.
    /// </summary>
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

    /// <summary>
    /// Habilita o deshabilita la reproducción de efectos de sonido y guarda la preferencia en PlayerPrefs.
    /// </summary>
    /// <param name="activado">True para activar el sonido; False para silenciarlo.</param>
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

    /// <summary>
    /// Consulta si los efectos de sonido se encuentran actualmente habilitados.
    /// </summary>
    /// <returns>True si el sonido está activado; de lo contrario, False.</returns>
    public bool SonidoActivado()
    {
        return sonidoActivado;
    }

    /// <summary>
    /// Habilita o deshabilita la música de fondo, reanudándola o pausándola, y guarda la preferencia en PlayerPrefs.
    /// </summary>
    /// <param name="activado">True para activar la música; False para pausarla o silenciarla.</param>
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

    /// <summary>
    /// Consulta si la música de fondo se encuentra actualmente habilitada.
    /// </summary>
    /// <returns>True si la música está activada; de lo contrario, False.</returns>
    public bool MusicaActivada()
    {
        return musicaActivada;
    }

    /// <summary>
    /// Reproduce el efecto de sonido asignado a la interacción de clic en la interfaz.
    /// </summary>
    public void ReproducirClick()
    {
        if (!sonidoActivado || efectosSonido == null || sonidoClick == null)
        {
            return;
        }
        efectosSonido.PlayOneShot(sonidoClick, volumenEfectos);
    }

    /// <summary>
    /// Reproduce el efecto de sonido asignado al evento de victoria.
    /// </summary>
    public void ReproducirWin()
    {
        if (!sonidoActivado || efectosSonido == null || sonidoWin == null) return;
        efectosSonido.PlayOneShot(sonidoWin, volumenEfectos);
    }

    /// <summary>
    /// Reproduce el efecto de sonido asignado al evento de derrota o fallo.
    /// </summary>
    public void ReproducirLoss()
    {
        if (!sonidoActivado || efectosSonido == null || sonidoLoss == null) return;
        efectosSonido.PlayOneShot(sonidoLoss, volumenEfectos);
    }

    /// <summary>
    /// Asigna y reproduce un nuevo clip de música de fondo.
    /// </summary>
    /// <param name="nuevaMusica">Instancia de AudioClip a reproducir.</param>
    public void ReproducirMusica(AudioClip nuevaMusica)
    {
        if (musicaFondo == null || nuevaMusica == null) return;
        musicaFondo.clip = nuevaMusica;
        if (musicaActivada)
        {
            musicaFondo.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ReproducirMusicaInicio()
    {
        if (musicaInicio != null)
        {
            ReproducirMusica(musicaInicio);
        }
    }
}