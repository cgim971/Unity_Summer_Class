using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

public class SoundObjManager : SingleMonobehaviour<SoundObjManager>
{
    public const string masterGroup = "Master";
    public const string EffectGroup = "Effect";
    public const string BGMGroup = "BGM";
    public const string UIGroup = "UI";
    public const string Mixer = "AudioMixer";
    public const string Container = "SoundContainer";

    public const string AudioFadeA = "FadeFront";
    public const string AudioFadeB = "FadeEnd";
    public const string AudioUI = "UI";
    public const string VolumeEffect = "Volume_effect";
    public const string VolumeBGM = "Volume_BGM";
    public const string VolumeUI = "Volume_UI";

    public enum AudioPlayingType
    {
        None = 0,
        SourceA = 1,
        SourceB = 2,
        AtoB = 3,
        BtoA = 4,
    }

    public AudioMixer audioMixer = null;
    public Transform audioTransformRoot = null;
    public AudioSource audioFadeFront = null;
    public AudioSource audioFadeEnd = null;
    public AudioSource[] audioEffect = null;
    public AudioSource audioUI = null;

    public float[] audioEffectPlayStartTimes = null;
    private int audioEffectChannelCnt = 5;
    private AudioPlayingType audioPlayingType = AudioPlayingType.None;
    private bool flagTicking = false;
    private SoundAttr nowSoundAttr = null;
    private SoundAttr lastSoundAttr = null;
    private float volumeMin = -80.0f;
    private float volumeMax = 0.0f;

    private void Start()
    {
        if (this.audioMixer == null)
        {
            this.audioMixer = (AudioMixer)ResourceManager.Load(Mixer);
        }

        if (this.audioTransformRoot == null)
        {
            audioTransformRoot = new GameObject(Container).transform;
            audioTransformRoot.SetParent(transform);
            audioTransformRoot.localPosition = Vector3.zero;
        }

        if (audioFadeFront == null)
        {
            GameObject fadeA = new GameObject(AudioFadeA, typeof(AudioSource));
            fadeA.transform.SetParent(audioTransformRoot);
            this.audioFadeFront = fadeA.GetComponent<AudioSource>();
            this.audioFadeFront.playOnAwake = false;
        }

        if (audioFadeEnd == null)
        {
            GameObject fadeB = new GameObject(AudioFadeB, typeof(AudioSource));
            fadeB.transform.SetParent(audioTransformRoot);
            this.audioFadeFront = fadeB.GetComponent<AudioSource>();
            this.audioFadeFront.playOnAwake = false;
        }

        if (audioUI == null)
        {
            GameObject ui = new GameObject(AudioUI, typeof(AudioSource));
            ui.transform.SetParent(audioTransformRoot);
            this.audioFadeFront = ui.GetComponent<AudioSource>();
            this.audioFadeFront.playOnAwake = false;
        }

        if (this.audioEffect == null || this.audioEffect.Length == 0)
        {
            this.audioEffectPlayStartTimes = new float[audioEffectChannelCnt];
            this.audioEffect = new AudioSource[audioEffectChannelCnt];
            for (int i = 0; i < audioEffectChannelCnt; i++)
            {
                audioEffectPlayStartTimes[i] = 0.0f;
                GameObject effect = new GameObject("Effect" + i.ToString(), typeof(AudioSource));
                effect.transform.SetParent(audioTransformRoot);
                this.audioEffect[i] = effect.GetComponent<AudioSource>();
                this.audioEffect[i].playOnAwake = false;
            }
        }

        if (this.audioMixer != null)
        {
            this.audioFadeFront.outputAudioMixerGroup = audioMixer.FindMatchingGroups(BGMGroup)[0];
            this.audioFadeEnd.outputAudioMixerGroup = audioMixer.FindMatchingGroups(BGMGroup)[0];
            this.audioUI.outputAudioMixerGroup = audioMixer.FindMatchingGroups(BGMGroup)[0];
            for (int i = 0; i < this.audioEffect.Length; i++)
            {
                this.audioEffect[i].outputAudioMixerGroup = audioMixer.FindMatchingGroups(EffectGroup)[0];
            }
        }
        audioVolumenInit();
    }

    public void setVolumeByType(string audioType, float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        float volume = Mathf.Lerp(volumeMin, volumeMax, ratio);
        this.audioMixer.SetFloat(audioType, volume);
        PlayerPrefs.SetFloat(audioType, volume);
    }

    public float getVolumeByType(string audioType)
    {
        if (PlayerPrefs.HasKey(audioType))
        {
            return Mathf.Lerp(volumeMin, volumeMax, PlayerPrefs.GetFloat(audioType));
        }
        else
        {
            return volumeMax;
        }
    }

    private void audioVolumenInit()
    {
        if (this.audioMixer != null)
        {
            this.audioMixer.SetFloat(VolumeBGM, getVolumeByType(VolumeBGM));
            this.audioMixer.SetFloat(VolumeEffect, getVolumeByType(VolumeEffect));
            this.audioMixer.SetFloat(VolumeUI, getVolumeByType(VolumeUI));
        }
    }

    void audioPlayAttr(AudioSource audioSource, SoundAttr soundAttr, float volume)
    {
        if (audioSource == null || soundAttr == null)
        {
            return;
        }
        audioSource.Stop();
        audioSource.clip = soundAttr.getAudioClip();
        audioSource.volume = volume;
        audioSource.loop = soundAttr.flagLoop;
        audioSource.pitch = soundAttr.soundPitch;
        audioSource.dopplerLevel = soundAttr.dopplerEffectLv;
        audioSource.rolloffMode = soundAttr.audioRolloffMode;
        audioSource.minDistance = soundAttr.distanceMin;
        audioSource.maxDistance = soundAttr.distanceMax;
        audioSource.spatialBlend = soundAttr.blendSparial;
        audioSource.Play();
    }

    void PlayClipAtPoint(SoundAttr soundAttr, Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(soundAttr.getAudioClip(), position, volume);
    }

    public bool getFlagPlaying()
    {
        return (int)this.audioPlayingType > 0;
    }

    public bool flagDifferentSound(SoundAttr soundAttr)
    {
        if (soundAttr == null)
        {
            return false;
        }

        if (nowSoundAttr != null && nowSoundAttr.code == soundAttr.code && getFlagPlaying() && !nowSoundAttr.flagFadeOut)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private IEnumerator ckProcess()
    {
        while (this.flagTicking && getFlagPlaying())
        {
            yield return new WaitForSeconds(0.05f);
            if (this.nowSoundAttr.getFlagLoop())
            {
                switch (audioPlayingType)
                {
                    case AudioPlayingType.SourceA:
                        nowSoundAttr.checkAudioLoop(audioFadeFront);
                        break;
                    case AudioPlayingType.SourceB:
                        nowSoundAttr.checkAudioLoop(audioFadeEnd);
                        break;
                    case AudioPlayingType.AtoB:
                        this.lastSoundAttr.checkAudioLoop(this.audioFadeFront);
                        this.nowSoundAttr.checkAudioLoop(this.audioFadeEnd);
                        break;
                    case AudioPlayingType.BtoA:
                        this.lastSoundAttr.checkAudioLoop(this.audioFadeEnd);
                        this.nowSoundAttr.checkAudioLoop(this.audioFadeFront);
                        break;
                }
            }
        }
    }

    public void checkingProcess()
    {
        StartCoroutine(ckProcess());
    }

    public void setFadeTo(SoundAttr soundAttr, float time)
    {
        if (audioPlayingType == AudioPlayingType.None)
        {
            setFadeIn(soundAttr, time);
        }
        else if (this.flagDifferentSound(soundAttr))
        {
            if (this.audioPlayingType == AudioPlayingType.AtoB)
            {
                this.audioFadeFront.Stop();
                this.audioPlayingType = AudioPlayingType.SourceB;
            }
            else if (this.audioPlayingType == AudioPlayingType.BtoA)
            {
                this.audioFadeEnd.Stop();
                this.audioPlayingType = AudioPlayingType.SourceA;
            }

            lastSoundAttr = nowSoundAttr;
            nowSoundAttr = soundAttr;

            if (audioPlayingType == AudioPlayingType.SourceA)
            {
                audioPlayAttr(audioFadeEnd, nowSoundAttr, 0.0f);
                audioPlayingType = AudioPlayingType.AtoB;
            }
            else if (audioPlayingType == AudioPlayingType.SourceB)
            {
                audioPlayAttr(audioFadeFront, nowSoundAttr, 0.0f);
                audioPlayingType = AudioPlayingType.BtoA;
            }

            if (nowSoundAttr.getFlagLoop())
            {
                this.flagTicking = true;
                checkingProcess();
            }
        }
    }

    public void setFadeTo(int index, float time)
    {
        this.setFadeIn(DataXMLManager.SoundData().GetCopy(index), time);
    }

    private void setFadeIn(SoundAttr soundAttr, float time)
    {
        if (this.flagDifferentSound(soundAttr))
        {
            this.audioFadeFront.Stop();
            this.audioFadeEnd.Stop();
            this.lastSoundAttr = this.nowSoundAttr;
            this.nowSoundAttr = soundAttr;

            audioPlayAttr(audioFadeFront, nowSoundAttr, 0.0f);

            this.nowSoundAttr.audioFadeAttr(true, time);
            this.audioPlayingType = AudioPlayingType.SourceA;
            if (this.nowSoundAttr.getFlagLoop())
            {
                this.flagTicking = true;
                checkingProcess();
            }
        }
    }

    public void setFadeIn(int index, float time)
    {
        this.setFadeIn(DataXMLManager.SoundData().GetCopy(index), time);
    }

    public void setFadeOut(float time)
    {
        if (this.nowSoundAttr != null)
        {
            this.nowSoundAttr.audioFadeAttr(false, time);
        }
    }

    public void audioPlayByBGM(SoundAttr soundAttr)
    {
        if (this.flagDifferentSound(soundAttr))
        {
            this.audioFadeEnd.Stop();
            this.lastSoundAttr = this.nowSoundAttr;
            this.nowSoundAttr = soundAttr;
            audioPlayAttr(audioFadeFront, soundAttr, soundAttr.volumeMax);
            if (nowSoundAttr.getFlagLoop())
            {
                this.flagTicking = true;
                checkingProcess();
            }
        }
    }

    public void audioPlayByBGM(int index)
    {
        SoundAttr soundAttr = DataXMLManager.SoundData().GetCopy(index);
        audioPlayByBGM(soundAttr);
    }

    public void audioPlayByUI(SoundAttr soundAttr)
    {
        audioPlayAttr(audioUI, soundAttr, soundAttr.volumeMax);
    }

    public void audioPlayByEffect(SoundAttr soundAttr)
    {
        bool flagPlaySuccess = false;
        for (int i = 0; i < this.audioEffectChannelCnt; i++)
        {
            if (!this.audioEffect[i].isPlaying)
            {
                audioPlayAttr(this.audioEffect[i], soundAttr, soundAttr.volumeMax);
                this.audioEffectPlayStartTimes[i] = Time.realtimeSinceStartup;
                flagPlaySuccess = true;
                break;
            }
            else if (this.audioEffect[i].clip == soundAttr.getAudioClip())
            {
                this.audioEffect[i].Stop();
                audioPlayAttr(audioEffect[i], soundAttr, soundAttr.volumeMax);
                this.audioEffectPlayStartTimes[i] = Time.realtimeSinceStartup;
                flagPlaySuccess = true;
                break;
            }
        }

        if (!flagPlaySuccess)
        {
            float maxTime = 0.0f;
            int selectIndex = 0;
            for (int i = 0; i < audioEffectChannelCnt; i++)
            {
                if (this.audioEffectPlayStartTimes[i] > maxTime)
                {
                    maxTime = this.audioEffectPlayStartTimes[i];
                    selectIndex = i;
                }
            }
            audioPlayAttr(this.audioEffect[selectIndex], soundAttr, soundAttr.volumeMax);
        }
    }

    public void audioPlayByEffect(SoundAttr soundAttr, Vector3 position, float volume)
    {
        bool flagPlaySuccess = false;
        for (int i = 0; i < this.audioEffectChannelCnt; i++)
        {
            if (!this.audioEffect[i].isPlaying)
            {
                PlayClipAtPoint(soundAttr, position, volume);
                this.audioEffectPlayStartTimes[i] = Time.realtimeSinceStartup;
                flagPlaySuccess = true;
                break;
            }
            else if (this.audioEffect[i].clip == soundAttr.getAudioClip())
            {
                this.audioEffect[i].Stop();
                PlayClipAtPoint(soundAttr, position, volume);
                this.audioEffectPlayStartTimes[i] = Time.realtimeSinceStartup;
                flagPlaySuccess = true;
                break;
            }
        }

        if (!flagPlaySuccess)
        {
            PlayClipAtPoint(soundAttr, position, volume);
        }
    }

    public void audioPlayByEffectOnce(int index, Vector3 position, float volume)
    {
        SoundAttr soundAttr = DataXMLManager.SoundData().GetCopy(index);
        if (soundAttr == null)
        {
            return;
        }
        audioPlayByEffect(soundAttr, position, volume);
    }

    public void audioPlayShotOnce(SoundAttr soundAttr)
    {
        if (soundAttr == null)
        {
            return;
        }

        switch (soundAttr.soundAttrType)
        {
            case SoundAttrType.Effect:
                audioPlayByEffect(soundAttr);
                break;
            case SoundAttrType.Bgm:
                audioPlayByBGM(soundAttr);
                break;
            case SoundAttrType.UI:
                audioPlayByUI(soundAttr);
                break;
        }
    }

    public void audioPlayStop(bool flagAllStop = false)
    {
        if (flagAllStop)
        {
            this.audioFadeFront.Stop();
            this.audioFadeEnd.Stop();
        }

        this.setFadeOut(0.5f);
        this.audioPlayingType = AudioPlayingType.None;
        StopAllCoroutines();
    }

    private void Update()
    {
        if (nowSoundAttr == null)
        {
            return;
        }

        switch (audioPlayingType)
        {
            case AudioPlayingType.SourceA:
                this.nowSoundAttr.audioFade(Time.deltaTime, audioFadeFront);
                break;
            case AudioPlayingType.SourceB:
                this.nowSoundAttr.audioFade(Time.deltaTime, audioFadeEnd);
                break;
            case AudioPlayingType.AtoB:
                this.lastSoundAttr.audioFade(Time.deltaTime, audioFadeFront);
                this.nowSoundAttr.audioFade(Time.deltaTime, audioFadeEnd);
                break;
            case AudioPlayingType.BtoA:
                this.lastSoundAttr.audioFade(Time.deltaTime, audioFadeEnd);
                this.nowSoundAttr.audioFade(Time.deltaTime, audioFadeFront);
                break;
        }

        if (this.audioFadeFront.isPlaying && !this.audioFadeEnd.isPlaying)
        {
            this.audioPlayingType = AudioPlayingType.SourceA;
        }
        else if (this.audioFadeEnd.isPlaying && !this.audioFadeFront.isPlaying)
        {
            this.audioPlayingType = AudioPlayingType.SourceB;
        }
        else if (!this.audioFadeFront.isPlaying && !this.audioFadeEnd.isPlaying)
        {
            this.audioPlayingType = AudioPlayingType.None;
        }
    }
}
