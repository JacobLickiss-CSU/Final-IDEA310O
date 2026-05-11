using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource player = null;

    public AudioClip Level1Music = null;

    public float Level1MusicStartTime = 0f;

    public AudioClip Level2Music = null;

    public float Level2MusicStartTime = 0f;

    public AudioClip Level3Music = null;

    public float Level3MusicStartTime = 0f;

    public float TransitionTime = 2;

    public float MusicVolume = 0.3f;

    private bool inTransition = false;

    private float transitionTimer = 0;

    private AudioClip nextUp = null;

    private float nextUpTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        player = GetComponent<AudioSource>();
        SwitchMusic(Level1Music, Level1MusicStartTime);
    }

    // Update is called once per frame
    void Update()
    {
        ProcessTransition();
    }

    public void StartLevelMusic(int level)
    {
        switch(level)
        {
            case 0: SwitchMusic(null, 0f); break;
            case 1: SwitchMusic(Level1Music, Level1MusicStartTime); break;
            case 2: SwitchMusic(Level2Music, Level2MusicStartTime); break;
            case 3: SwitchMusic(Level3Music, Level3MusicStartTime); break;
            default: SwitchMusic(null, 0f); break;
        }
    }

    void SwitchMusic(AudioClip clip, float startTime)
    {
        if (gameObject == null) return;
        if (player.clip == clip) return;

        if(player.isPlaying)
        {
            inTransition = true;
            transitionTimer = -TransitionTime;
            nextUp = clip;
            nextUpTime = startTime;
        }
        else
        {
            //inTransition = true;
            //transitionTimer = 0;
            //nextUp = clip;
            //nextUpTime = startTime;
            SwitchImmediately(clip, startTime);
        }
    }

    void ProcessTransition()
    {
        if(inTransition)
        {
            bool crossesSign = transitionTimer <= 0 && (transitionTimer + Time.deltaTime) > 0;
            transitionTimer += Time.deltaTime;

            player.volume = Mathf.Abs((transitionTimer / TransitionTime) * MusicVolume);

            if(crossesSign)
            {
                if(nextUp != null)
                {
                    player.clip = nextUp;
                    player.Play();
                    player.time = nextUpTime;
                }
                else
                {
                    player.Stop();
                    player.clip = null;

                    EndTransition();
                    return;
                }
            }

            if(transitionTimer >= TransitionTime)
            {
                EndTransition();
                return;
            }
        }
    }

    void EndTransition()
    {
        inTransition = false;
        transitionTimer = 0;
        nextUpTime = 0f;

        player.volume = MusicVolume;
    }

    void SwitchImmediately(AudioClip clip, float startTime)
    {
        EndTransition();
        player.clip = clip;
        player.Play();
        player.time = startTime;
    }
}
