using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundAttrType
{
    None = -1,
    Bgm,
    Effect,
    UI
}

public class SoundAttr
{

    public SoundAttrType soundAttrType = SoundAttrType.None;
    public string soundAttrName = string.Empty;
    public string soundAttrPath = string.Empty;
    public float volumeMax = 1.0f;

    public bool flagLoop = false;
    public float[] ckTimes = new float[0];
    public float[] setTimes = new float[0];

    public int code = 0;

    private AudioClip audioClip = null;
    public int nowLoop = 0;
    public float soundPitch = 1.0f;
    public float dopplerEffectLv = 1.0f;
    public AudioRolloffMode audioRolloffMode = AudioRolloffMode.Logarithmic;
    public float distanceMin = 5000.0f;
    public float distanceMax = 50000.0f;
    public float blendSparial = 1.0f;

    public float frontFadeTime = 0.0f;
    public float endFadeTime = 0.0f;

    //public Interpolate.Function interpolate_Func;

    public bool flagFadeIn = false;
    public bool flagFadeOut = false;

    public SoundAttr() { }

    public SoundAttr(string attrPath, string attrName)
    {
        this.soundAttrPath = attrPath;
        this.soundAttrName = attrName;
    }

    public void AudioPreLoad()
    {
        if (this.audioClip == null)
        {
            string audioClipFullPath = this.soundAttrPath + this.soundAttrName;
            this.audioClip = (AudioClip)ResourceManager.Load(audioClipFullPath);
        }
    }

    public void setLoop()
    {
        ArrayList _tmpList = new ArrayList();
        foreach (float _val in this.ckTimes)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(0.0f);
        this.ckTimes = (float[])_tmpList.ToArray(typeof(float));

        foreach (float _val in this.setTimes)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(0.0f);
        this.setTimes = (float[])_tmpList.ToArray(typeof(float));
    }

    public void deleteLoop(int index)
    {
        ArrayList _tmpList = new ArrayList();
        foreach (float _val in this.ckTimes)
        {
            _tmpList.Add(_val);
        }
        _tmpList.RemoveAt(index);
        this.ckTimes = (float[])_tmpList.ToArray(typeof(float));

        foreach (float _val in this.setTimes)
        {
            _tmpList.Add(_val);
        }
        _tmpList.RemoveAt(index);
        this.setTimes = (float[])_tmpList.ToArray(typeof(float));
    }

    public AudioClip getAudioClip()
    {
        if (this.audioClip == null)
        {
            AudioPreLoad();
        }

        if (this.audioClip == null && this.soundAttrName != string.Empty)
        {
            Debug.LogWarning(this.soundAttrName + "check Audio Clip");
            return null;
        }

        return this.audioClip;
    }

    public void resetAudioClip()
    {
        if (this.audioClip != null)
        {
            this.audioClip = null;
        }
    }

    public bool getFlagLoop()
    {
        return this.ckTimes.Length > 0;
    }

    public void nextAudioLoop()
    {
        this.nowLoop++;
        if (this.nowLoop >= this.ckTimes.Length)
        {
            this.nowLoop = 0;
        }
    }

    public void checkAudioLoop(AudioSource audioSource)
    {
        if (getFlagLoop() && audioSource.time >= this.ckTimes[this.nowLoop])
        {
            audioSource.time = this.setTimes[this.nowLoop];
            this.nextAudioLoop();
        }
    }

    public void audioFadeAttr(bool isFront, float time)
    {
        this.flagFadeIn = isFront;
        this.flagFadeOut = (!isFront);
        this.frontFadeTime = 0.0f;
        this.endFadeTime = time;
    }

    public void audioFade(float time, AudioSource audioSource)
    {
        this.frontFadeTime += time;

        if(this.frontFadeTime >= endFadeTime)
        {
            if (this.flagFadeIn)
            {
                this.flagFadeIn = false;
            }
            else if (this.flagFadeOut)
            {
                this.flagFadeOut = false;
                audioSource.Stop();
            }
        }
    }
}
