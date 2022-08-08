using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.IO;

public class EffectXMLData : DefaultData
{
    public EffectAttr[] effectAttrs = new EffectAttr[0];

    public string effectAttrPath = "Resources/Prefab/Effects/";
    private string xmlPath = "";
    private string xmlName = "effecXmltData.xml";
    private string dataPath = "Data/effectData";

    private const string EFFECT = "effect";
    private const string DEFAULT = "default";

    private EffectXMLData() { }

    public void LoadData()
    {
        this.xmlPath = Application.dataPath + pathData;

        TextAsset dataAsset = (TextAsset)ResourceManager.Load(dataPath);
        if (dataAsset == null || dataAsset.text == null)
        {
            this.constructorData("New EffectData");
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
                            this.effectAttrs = new EffectAttr[length];
                            break;
                        case "code":
                            nowCode = int.Parse(xmlInfo.ReadString());
                            this.effectAttrs[nowCode] = new EffectAttr();
                            this.effectAttrs[nowCode].code = nowCode;
                            break;
                        case "idx":
                            this.idx[nowCode] = xmlInfo.ReadString();
                            break;
                        case "effectAttrType":
                            this.effectAttrs[nowCode].effectAttrType = (EffectAttrType)Enum.Parse(typeof(EffectAttrType), xmlInfo.ReadString());
                            break;
                        case "effectObjName":
                            this.effectAttrs[nowCode].effectObjName = xmlInfo.ReadString();
                            break;
                        case "effectObjPath":
                            this.effectAttrs[nowCode].effectObjPath = xmlInfo.ReadString();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public void writeXmlData()
    {
        using (XmlTextWriter xmlInfo = new XmlTextWriter(xmlPath + xmlName, System.Text.Encoding.Unicode))
        {
            xmlInfo.WriteStartDocument();
            xmlInfo.WriteStartElement(EFFECT);
            xmlInfo.WriteElementString("length", getDataCnt().ToString());
            for (int i = 0; i < this.idx.Length; i++)
            {
                EffectAttr attr = this.effectAttrs[i];
                xmlInfo.WriteStartElement(DEFAULT);
                xmlInfo.WriteElementString("code", i.ToString());
                xmlInfo.WriteElementString("idx", this.idx[i]);
                xmlInfo.WriteElementString("effectAttrType", attr.effectAttrType.ToString());
                xmlInfo.WriteElementString("effectObjName", attr.effectObjName);
                xmlInfo.WriteElementString("effectObjPath", attr.effectObjPath);
                xmlInfo.WriteEndElement();
            }

            xmlInfo.WriteEndElement();
            xmlInfo.WriteEndDocument();
        }
    }

    public override int constructorData(string newName)
    {
        if (this.idx == null)
        {
            this.idx = new String[] { name };
            this.effectAttrs = new EffectAttr[]
            {
                new EffectAttr()
            };
        }
        else
        {
            ArrayList _tmpList = new ArrayList();
            foreach (string _val in this.idx)
            {
                _tmpList.Add(_val);
            }
            _tmpList.Add(name);
            this.idx = (string[])_tmpList.ToArray(typeof(string));

            _tmpList = new ArrayList();
            foreach (EffectAttr _val in this.effectAttrs)
            {
                _tmpList.Add(_val);
            }
            _tmpList.Add(new EffectAttr());
            this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr));
        }
        return getDataCnt();
    }

    private T[] addVal<T>(T[] attrs)
    {
        ArrayList _tmpList = new ArrayList;
        foreach(T _val in attrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add((T)Activator.CreateInstance(typeof(T)));
        return (T[])_tmpList.ToArray(typeof(T));
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
        foreach (EffectAttr _val in this.effectAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.RemoveAt(index);
        this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr));
    }

    public void ClearData()
    {
        foreach (EffectAttr attr in this.effectAttrs)
        {
            attr.delectEffect();
        }
        this.effectAttrs = null;
        this.idx = null;
    }

    public EffectAttr GetCopy(int index)
    {
        if (index < 0 || index >= this.effectAttrs.Length)
        {
            return null;
        }

        EffectAttr beforeAttr = this.effectAttrs[index];
        EffectAttr attr = new EffectAttr();
        attr.effectObjFullPath = beforeAttr.effectObjFullPath;
        attr.effectObjName = beforeAttr.effectObjName;
        attr.effectAttrType = beforeAttr.effectAttrType;
        attr.effectObjPath = beforeAttr.effectObjPath;
        attr.code = this.effectAttrs.Length;
        return attr;
    }

    public EffectAttr getAttr(int index)
    {
        if (index < 0 || index >= this.effectAttrs.Length)
        {
            return null;
        }

        effectAttrs[index].effectPreLoad();
        return effectAttrs[index];
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
        foreach (EffectAttr _val in this.effectAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(GetCopy(index));
        this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr));
    }
}
