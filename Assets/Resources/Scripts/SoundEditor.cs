using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class SoundEditor : EditorWindow
{
    public int sizeLargeWidth = 250;
    public int sizeMiddleWidth = 150;
    public int sizeSmallWidth = 100;

    private int nowCode = 0;
    private Vector2 posScroll_1 = Vector2.zero;
    private Vector2 posScroll_2 = Vector2.zero;

    private AudioClip audioClip = null;

    private static SoundXMLData soundXmlData;

    [MenuItem("Editors/Sound Editor")]
    static void init()
    {
        soundXmlData = ScriptableObject.CreateInstance<SoundXMLData>();
        soundXmlData.LoadData();

        SoundEditor window = GetWindow<SoundEditor>(false, "Sound Editor");
        window.Show();
    }

    private void OnGUI()
    {
        if (soundXmlData == null)
        {
            return;
        }

        EditorGUILayout.BeginVertical();
        {
            UnityEngine.Object editObj = audioClip;
            SoundAttr soundAttr = soundXmlData.soundAttrs[nowCode];

            EditorLib.setTopLayer(soundXmlData, ref nowCode, ref editObj, this.sizeMiddleWidth);
            audioClip = (AudioClip)editObj;

            EditorGUILayout.BeginHorizontal();
            {
                EditorLib.setListLayer(ref posScroll_1, soundXmlData, ref nowCode, ref editObj, this.sizeMiddleWidth);
                audioClip = (AudioClip)editObj;

                EditorGUILayout.BeginVertical();
                {
                    posScroll_2 = EditorGUILayout.BeginScrollView(this.posScroll_2);
                    {
                        if (soundXmlData.getDataCnt() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                EditorGUILayout.LabelField("ÄÚµå", nowCode.ToString(), GUILayout.Width(sizeLargeWidth));
                                soundXmlData.idx[nowCode] = EditorGUILayout.TextField("ÀÎµ¦½º", soundXmlData.idx[nowCode], GUILayout.Width(sizeLargeWidth));
                                soundAttr.soundAttrType = (SoundAttrType)EditorGUILayout.EnumPopup("audioType", soundAttr.soundAttrType, GUILayout.Width(sizeLargeWidth));
                                soundAttr.volumeMax = EditorGUILayout.FloatField("Volume Max", soundAttr.volumeMax, GUILayout.Width(sizeLargeWidth));
                                soundAttr.flagLoop = EditorGUILayout.Toggle("audioLoop", soundAttr.flagLoop, GUILayout.Width(sizeLargeWidth));
                                EditorGUILayout.Separator();


                                string _tmpSoundObjPath = string.Empty;
                                string _tmpSoundObjName = string.Empty;

                                if (this.audioClip == null && soundAttr.soundAttrName != string.Empty)
                                {
                                    this.audioClip = (AudioClip)ResourceManager.Load(soundAttr.soundAttrPath + soundAttr.soundAttrName);
                                }
                                this.audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", this.audioClip, typeof(AudioClip), false, GUILayout.Width(sizeLargeWidth));
                                if (audioClip != null)
                                {
                                    soundAttr.soundAttrPath = EditorLib.getAssetPath(audioClip);
                                    soundAttr.soundAttrName = audioClip.name;
                                    soundAttr.soundPitch = EditorGUILayout.Slider("SoundPitch", soundAttr.soundPitch, -3.0f, 3.0f, GUILayout.Width(sizeLargeWidth));
                                    soundAttr.dopplerEffectLv = EditorGUILayout.Slider("DopplerLv", soundAttr.dopplerEffectLv, 0.0f, 5.0f, GUILayout.Width(sizeLargeWidth));
                                    soundAttr.audioRolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup("Rolloff Volume", soundAttr.audioRolloffMode, GUILayout.Width(sizeLargeWidth));
                                    soundAttr.distanceMin = EditorGUILayout.FloatField("Distance min", soundAttr.distanceMin, GUILayout.Width(sizeLargeWidth));
                                    soundAttr.distanceMax = EditorGUILayout.FloatField("Distance max", soundAttr.distanceMax, GUILayout.Width(sizeLargeWidth));
                                    soundAttr.blendSparial = EditorGUILayout.Slider("BlendSparial", soundAttr.blendSparial, 0.0f, 1.0f, GUILayout.Width(sizeLargeWidth));
                                }
                                else
                                {
                                    soundAttr.soundAttrName = string.Empty;
                                    soundAttr.soundAttrPath = string.Empty;
                                }
                                EditorGUILayout.Separator();
                                if (GUILayout.Button("Set Loop", GUILayout.Width(sizeMiddleWidth)))
                                {
                                    soundXmlData.soundAttrs[nowCode].setLoop();
                                }

                                for (int i = 0; i < soundXmlData.soundAttrs[nowCode].ckTimes.Length; i++)
                                {
                                    EditorGUILayout.BeginVertical("box");
                                    {
                                        GUILayout.Label("Stemp Loop" + i, EditorStyles.boldLabel);
                                        if (GUILayout.Button("Delete", GUILayout.Width(sizeMiddleWidth)))
                                        {
                                            soundXmlData.soundAttrs[nowCode].deleteLoop(nowCode);
                                            return;
                                        }
                                        soundAttr.ckTimes[i] = EditorGUILayout.FloatField("Check Times", soundAttr.ckTimes[i], GUILayout.Width(sizeMiddleWidth));
                                        soundAttr.setTimes[i] = EditorGUILayout.FloatField("Set Times", soundAttr.setTimes[i], GUILayout.Width(sizeMiddleWidth));
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Reload Settings"))
            {
                soundXmlData = CreateInstance<SoundXMLData>();
                soundXmlData.LoadData();
                nowCode = 0;
                this.audioClip = null;
            }

            if (GUILayout.Button("Save"))
            {
                soundXmlData.writeXMLData();
                CreateEnumSturcture();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public void CreateEnumSturcture()
    {
        string enumName = "SoundList";
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < soundXmlData.idx.Length; i++)
        {
            if (soundXmlData.idx[i] != string.Empty)
            {
                builder.AppendLine("    " + soundXmlData.idx[i] + " = " + i.ToString() + ",");
            }
        }
        EditorLib.makeEnumClass(enumName, builder);
    }
}
