using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.IO;

public class SoundXMLData : DefaultData
{
    public SoundAttr[] soundAttrs = new SoundAttr[0];

    public string soundAttrPath = "Sound/";
    private string xmlPath = "";
    private string xmlName = "soundData.xml";
    private string dataPath = "Data/soundData";
    private const string SOUND = "sound";
    private const string DEFAULT = "default";

    public SoundXMLData() { }

    void setAudioLoopTime(bool flagCK, SoundAttr soundAttr, string strTime)
    {
        string[] arryTime = strTime.Split('/');
        for (int i = 0; i < arryTime.Length; i++)
        {
            if (arryTime[i] != string.Empty)
            {
                if (flagCK == true)
                {
                    soundAttr.ckTimes[i] = float.Parse(arryTime[i]);
                }
                else
                {
                    soundAttr.setTimes[i] = float.Parse(arryTime[i]);
                }
            }
        }
    }

    public void LoadData()
    {
        this.xmlPath = Application.dataPath + pathData;

        TextAsset dataAsset = (TextAsset)ResourceManager.Load(dataPath);
        if (dataAsset == null || dataAsset.text == null)
        {
            this.constructorData("New SoundData");
            return;
        }

        using (XmlTextReader xmlInfo = new XmlTextReader(new StringReader(dataAsset.text)))
        {
            int nowCode = 0;
            while (xmlInfo.Read())
            {
                if (xmlInfo.IsStartElement())
                {
                    switch (xmlInfo.Name)
                    {
                        case "length":
                            int length = int.Parse(xmlInfo.ReadString());
                            this.idx = new string[length];
                            this.soundAttrs = new SoundAttr[length];
                            break;
                        case "code":
                            nowCode = int.Parse(xmlInfo.ReadString());
                            this.soundAttrs[nowCode] = new SoundAttr();
                            this.soundAttrs[nowCode].code = nowCode;
                            break;
                        case "idx":
                            this.idx[nowCode] = xmlInfo.ReadString();
                            break;
                        case "audioLoops":
                            int cnt = int.Parse(xmlInfo.ReadString());
                            soundAttrs[nowCode].ckTimes = new float[cnt];
                            soundAttrs[nowCode].setTimes = new float[cnt];
                            break;
                        case "volMax":
                            soundAttrs[nowCode].volumeMax = float.Parse(xmlInfo.ReadString());
                            break;
                        case "soundPitch":
                            soundAttrs[nowCode].soundPitch = float.Parse(xmlInfo.ReadString());
                            break;
                        case "doppleerlevel":
                            soundAttrs[nowCode].dopplerEffectLv = float.Parse(xmlInfo.ReadString());
                            break;
                        case "rolloffMode":
                            soundAttrs[nowCode].audioRolloffMode = (AudioRolloffMode)Enum.Parse(typeof(AudioRolloffMode), xmlInfo.ReadString());
                            break;
                        case "distanceMin":
                            soundAttrs[nowCode].distanceMin = float.Parse(xmlInfo.ReadString());
                            break;
                        case "distanceMax":
                            soundAttrs[nowCode].distanceMax = float.Parse(xmlInfo.ReadString());
                            break;
                        case "blendSparial":
                            soundAttrs[nowCode].blendSparial = float.Parse(xmlInfo.ReadString());
                            break;
                        case "audioLoop":
                            soundAttrs[nowCode].flagLoop = true;
                            break;
                        case "soundAttrPath":
                            soundAttrs[nowCode].soundAttrPath = xmlInfo.ReadString();
                            break;
                        case "soundAttrName":
                            soundAttrs[nowCode].soundAttrName = xmlInfo.ReadString();
                            break;
                        case "ckTimes":
                            setAudioLoopTime(true, soundAttrs[nowCode], xmlInfo.ReadString());
                            break;
                        case "setTimes":
                            setAudioLoopTime(true, soundAttrs[nowCode], xmlInfo.ReadString());
                            break;
                        case "audioType":
                            soundAttrs[nowCode].soundAttrType = (SoundAttrType)Enum.Parse(typeof(SoundAttrType), xmlInfo.ReadString());
                            break;
                    }
                }
            }
        }

        foreach (SoundAttr soundAttr in soundAttrs)
        {
            soundAttr.AudioPreLoad();
        }
    }

    public void writeXMLData()
    {
        using (XmlTextWriter xml = new XmlTextWriter(xmlPath + xmlName, System.Text.Encoding.Unicode))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement(SOUND);
            xml.WriteElementString("length", getDataCnt().ToString());
            for (int i = 0; i < this.idx.Length; i++)
            {
                SoundAttr soundAttr = this.soundAttrs[i];
                xml.WriteStartElement(DEFAULT);
                xml.WriteElementString("code", i.ToString());
                xml.WriteElementString("idx", this.idx[i]);
                xml.WriteElementString("audioLoops", soundAttr.ckTimes.Length.ToString());
                xml.WriteElementString("volMax", soundAttr.volumeMax.ToString());
                xml.WriteElementString("soundPitch", soundAttr.soundPitch.ToString());
                xml.WriteElementString("dopplerLv", soundAttr.dopplerEffectLv.ToString());
                xml.WriteElementString("rolloffMode", soundAttr.audioRolloffMode.ToString());
                xml.WriteElementString("distanceMin", soundAttr.distanceMin.ToString());
                xml.WriteElementString("distanceMax", soundAttr.distanceMax.ToString());
                xml.WriteElementString("blendSparial", soundAttr.blendSparial.ToString());
                if (soundAttr.flagLoop == true)
                {
                    xml.WriteElementString("audioLoop", "true");
                }
                xml.WriteElementString("soundAttrPath", soundAttr.soundAttrPath.ToString());
                xml.WriteElementString("soundAttrName", soundAttr.soundAttrName.ToString());
                xml.WriteElementString("ckTimesCnt", soundAttr.ckTimes.Length.ToString());

                string tmpStr = "";
                foreach (float temp in soundAttr.ckTimes)
                {
                    tmpStr += temp.ToString() + "/";
                }
                xml.WriteElementString("ckTimes", tmpStr);

                tmpStr = "";
                xml.WriteElementString("setTimeCnt", soundAttr.setTimes.Length.ToString());
                foreach (float temp in soundAttr.setTimes)
                {
                    tmpStr += temp.ToString() + "/";
                }
                xml.WriteElementString("setTimes", tmpStr);

                xml.WriteElementString("audioType", soundAttr.soundAttrType.ToString());

                xml.WriteEndElement();
            }

            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
    }

    public override int constructorData(string newIdx)
    {
        if (this.idx == null)
        {
            this.idx = new String[] { newIdx };
            this.soundAttrs = new SoundAttr[]
            {
                new SoundAttr()
            };
        }
        else
        {
            ArrayList _tmpList = new ArrayList();
            foreach (string _val in this.idx)
            {
                _tmpList.Add(_val);
            }
            _tmpList.Add(newIdx);
            this.idx = (string[])_tmpList.ToArray(typeof(string));

            _tmpList = new ArrayList();
            foreach (SoundAttr _val in this.soundAttrs)
            {
                _tmpList.Add(_val);
            }
            _tmpList.Add(new SoundAttr());
            this.soundAttrs = (SoundAttr[])_tmpList.ToArray(typeof(SoundAttr));
        }
        return getDataCnt();
    }

    public override void deleteData(int index)
    {
        ArrayList _tmpList = new ArrayList();
        foreach (string _val in this.idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.RemoveAt(index);
        this.idx = (string[])_tmpList.ToArray(typeof(string));

        if (this.idx.Length == 0)
        {
            this.idx = null;
        }

        _tmpList = new ArrayList();
        foreach (SoundAttr _val in this.soundAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.RemoveAt(index);
        this.soundAttrs = (SoundAttr[])_tmpList.ToArray(typeof(SoundAttr));
    }

    public SoundAttr GetCopy(int index)
    {
        if (index < 0 || index >= this.soundAttrs.Length)
        {
            return null;
        }

        SoundAttr beforeAttr = this.soundAttrs[index];
        SoundAttr attr = new SoundAttr();

        attr.code = index;
        attr.soundAttrPath = beforeAttr.soundAttrPath;
        attr.soundAttrName = beforeAttr.soundAttrName;
        attr.volumeMax = beforeAttr.volumeMax;
        attr.soundPitch = beforeAttr.soundPitch;
        attr.dopplerEffectLv = beforeAttr.dopplerEffectLv;
        attr.audioRolloffMode = beforeAttr.audioRolloffMode;
        attr.distanceMin = beforeAttr.distanceMin;
        attr.distanceMax = beforeAttr.distanceMax;
        attr.blendSparial = beforeAttr.blendSparial;
        attr.flagLoop = beforeAttr.flagLoop;
        attr.ckTimes = new float[beforeAttr.ckTimes.Length];
        attr.setTimes = new float[beforeAttr.setTimes.Length];
        attr.soundAttrType = beforeAttr.soundAttrType;

        for (int i = 0; i < attr.ckTimes.Length; i++)
        {
            attr.ckTimes[i] = beforeAttr.ckTimes[i];
            attr.setTimes[i] = beforeAttr.setTimes[i];
        }

        attr.AudioPreLoad();

        return attr;
    }

    public override void defulicateData(int index)
    {
        ArrayList _tmpList = new ArrayList();
        foreach (string _val in this.idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(this.idx[index]);
        this.idx = (string[])_tmpList.ToArray(typeof(string));

        _tmpList = new ArrayList();
        foreach (SoundAttr _val in this.soundAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(GetCopy(index));
        this.soundAttrs = (SoundAttr[])_tmpList.ToArray(typeof(SoundAttr));
    }
}
