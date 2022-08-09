using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class EffectEditor : EditorWindow
{
    public int sizeLargeWidth = 250;
    public int sizeMiddleWidth = 150;
    private int nowCode = 0;
    private Vector2 posScroll_1 = Vector2.zero;
    private Vector2 posScroll_2 = Vector2.zero;

    private GameObject effectObject = null;

    private static EffectXMLData effectXmlData;

    [MenuItem("Editors/Effect Editor")]
    static void init()
    {
        effectXmlData = ScriptableObject.CreateInstance<EffectXMLData>();
        effectXmlData.LoadData();

        EffectEditor window = GetWindow<EffectEditor>(false, "Effect Editor");
        window.Show();
    }

    private void OnGUI()
    {
        if (effectXmlData == null)
        {
            return;
        }

        EditorGUILayout.BeginVertical();
        {
            UnityEngine.Object editObj = effectObject;
            EditorLib.setTopLayer(effectXmlData, ref nowCode, ref editObj, this.sizeMiddleWidth);
            effectObject = (GameObject)editObj;

            EditorGUILayout.BeginHorizontal();
            {
                EditorLib.setListLayer(ref posScroll_1, effectXmlData, ref nowCode, ref editObj, this.sizeLargeWidth);
                effectObject = (GameObject)editObj;

                EditorGUILayout.BeginVertical();
                {
                    posScroll_2 = EditorGUILayout.BeginScrollView(this.posScroll_2);
                    {
                        if (effectXmlData.getDataCnt() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {

                                EditorGUILayout.Separator();
                                EditorGUILayout.LabelField("ƒ⁄µÂ", nowCode.ToString(), GUILayout.Width(sizeLargeWidth));
                                effectXmlData.idx[nowCode] = EditorGUILayout.TextField("¿Œµ¶Ω∫", effectXmlData.idx[nowCode], GUILayout.Width(sizeLargeWidth * 1.5f));

                                effectXmlData.effectAttrs[nowCode].effectAttrType = (EffectAttrType)EditorGUILayout.EnumPopup("¿Ã∆Â∆Æ ≈∏¿‘", effectXmlData.effectAttrs[nowCode].effectAttrType, GUILayout.Width(sizeLargeWidth));

                                EditorGUILayout.Separator();
                                if (effectObject == null && effectXmlData.effectAttrs[nowCode].effectObjName != string.Empty)
                                {
                                    effectXmlData.effectAttrs[nowCode].effectPreLoad();
                                    effectObject = (GameObject)ResourceManager.Load(effectXmlData.effectAttrs[nowCode].effectObjPath + effectXmlData.effectAttrs[nowCode].effectObjName);
                                }
                                effectObject = (GameObject)EditorGUILayout.ObjectField("¿Ã∆Â∆Æ", this.effectObject, typeof(GameObject), false, GUILayout.Width(sizeLargeWidth * 1.5f));

                                string _tmpEffectObjPath = string.Empty;
                                string _tmpEffectObjName = string.Empty;
                                if (effectObject != null)
                                {
                                    _tmpEffectObjPath = EditorLib.getAssetPath(this.effectObject);
                                    _tmpEffectObjName = effectObject.name;
                                }
                                else
                                {
                                    effectXmlData.effectAttrs[nowCode].effectObjPath = _tmpEffectObjPath;
                                    effectXmlData.effectAttrs[nowCode].effectObjName = _tmpEffectObjName;
                                    effectObject = null;
                                }
                                EditorGUILayout.Separator();
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
                effectXmlData = CreateInstance<EffectXMLData>();
                effectXmlData.LoadData();
                nowCode = 0;
                this.effectObject = null;
            }

            if (GUILayout.Button("Save"))
            {
                EffectEditor.effectXmlData.writeXmlData();

                CreateEnumSturcture();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public void CreateEnumSturcture()
    {
        string enumName = "EffectList";
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();
        for (int i = 0; i < effectXmlData.idx.Length; i++)
        {
            if (effectXmlData.idx[i] != string.Empty)
            {
                builder.AppendLine("    " + effectXmlData.idx[i] + " = " + i + ",");
            }
        }

        EditorLib.makeEnumClass(enumName, builder);
    }
}

