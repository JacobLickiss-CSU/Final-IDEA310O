using UnityEngine;
using System.Collections.Generic;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource Source;

    public List<AudioClip> Clips = new List<AudioClip>();

    public float Volume = 1.0f;

    public SoundPlayer Next;

    public float NextDelay = 0.0f;

    private bool didPlay = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (didPlay && !Source.isPlaying)
        {
            NextDelay -= Time.deltaTime;
            if (NextDelay <= 0.0f)
            {
                FinishPlaying();
            }
        }
    }

    public void Play()
    {
        if(Clips.Count == 0)
        {
            Destroy(this.gameObject);
            return;
        }

        Source.clip = ChooseClip();
        Source.volume = Volume;
        Source.Play();
        didPlay = true;
    }

    private AudioClip ChooseClip()
    {
        return Clips[Random.Range(0, Clips.Count)];
    }

    private void FinishPlaying()
    {
        if (Next != null)
        {
            GameObject next = Instantiate(Next.gameObject);
            next.GetComponent<SoundPlayer>().Play();
        }

        Destroy(this.gameObject);
    }
}
