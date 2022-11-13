using BailCustomChars.Data;
using FreeStyle.Unity.Common;
using FreeStyle.Unity.Menu;
using FreeStyle.Unity.Obakeidoro;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;
using BepInEx.IL2CPP.Utils.Collections;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using HarmonyLib;

namespace BailCustomChars
{

    public class CharManager
    {

        public static CustomCharacter character;
            public static void LoadCharactersFromDirectory(string characterDirectory)
        {
            foreach (string text in from x in Directory.GetFiles(characterDirectory)
                                    where x.ToLower().EndsWith(".assets")
                                    select x)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
                AssetBundle realAssetBundle = AssetBundle.LoadFromFile(text);
                BailCharPlugin.realAssetBundles.Add(realAssetBundle);
                string[] realAssetNames = realAssetBundle.AllAssetNames();
                string jsonFile = Path.Combine(characterDirectory, fileNameWithoutExtension + ".json");
                if (File.Exists(jsonFile))
                {
                    var jsonString = File.ReadAllText(jsonFile);
                    character = JsonConvert.DeserializeObject<CustomCharacter>(jsonString);
                    ChrAsset.Item newChar = new ChrAsset.Item();

                    newChar.id = character.id;
                    newChar.name = character.name;
                    if (character.name != "") {
                        var nameMessage = new PsdMsgAsset.Msg();
                        nameMessage.en_us = character.name;
                        nameMessage.jp_jp = character.name;
                        if (PsdMsgAsset.g_msgDic.ContainsKey((MSG_ID)character.textId))
                        {
                            BailCharPlugin.Instance.Log.LogError("MSG_ID " + character.textId.ToString() + " is already in the list!");

                        } else {
                            PsdMsgAsset.g_msgDic.Add((MSG_ID)character.textId, nameMessage);
                            PsdMsgAsset.g_idDic.Add(((MSG_ID)character.textId).ToString(), (MSG_ID)character.textId);
                        }
                        newChar.chrName = ((MSG_ID)character.textId).ToString();
                    } if (character.description != "") {
                        var descMessage = new PsdMsgAsset.Msg();
                        descMessage.en_us = character.description;
                        descMessage.jp_jp = character.description;
                        if (PsdMsgAsset.g_msgDic.ContainsKey((MSG_ID)character.textId+1))
                        {
                            BailCharPlugin.Instance.Log.LogError("MSG_ID " + character.textId+1.ToString() + " is already in the list!");

                        }
                        else
                        {
                            PsdMsgAsset.g_msgDic.Add((MSG_ID)character.textId+1, descMessage);
                            PsdMsgAsset.g_idDic.Add(((MSG_ID)character.textId+1).ToString(), (MSG_ID)character.textId+1);
                        }
                        newChar.text = ((MSG_ID)character.textId + 1).ToString();
                    } if (character.isHuman)
                    {
                        newChar.jobType = ChrDefines.JOB_TYPE.JOB_TYPE_HUMAN;
                        newChar.aiFile = "Human";
                        newChar.ghostExclusiveLoopSE = -1;
                    } else
                    {
                        newChar.jobType = ChrDefines.JOB_TYPE.JOB_TYPE_GHOST;
                        newChar.aiFile = "Ghost";
                        newChar.ghostExclusiveLoopSE = character.ghostExclusiveLoopSE;
                    }
                    System.Type customCharType = character.GetType();
                    System.Type realCharType = newChar.GetType();
                    foreach (var field in realCharType.GetProperties())
                    {
                        if (field.Name == "Il2CppType" || field.Name == "modelIdArray" || field.Name == "ObjectClass"
                            || field.Name == "chrName" || field.Name == "text" || field.Name == "Pointer" || field.Name == "WasCollected"
                            ||field.Name == "animId")
                        {
                            continue;

                        }
                        if (customCharType.GetProperty(field.Name) != null)
                        {
                                BailCharPlugin.Instance.Log.LogInfo("found a field we should replace! " + field.Name);
                            if (field.PropertyType == typeof(int[]))
                            {
                                int trueLength = ((int[])customCharType.GetProperty(field.Name).GetValue(character)).Length;
                                UnhollowerBaseLib.Il2CppStructArray<int> whyGodWhy = new UnhollowerBaseLib.Il2CppStructArray<int>(trueLength);
                                for (int i = 0; i < trueLength; i++)
                                {
                                    whyGodWhy[i] = ((int[])customCharType.GetProperty(field.Name).GetValue(character))[i];
                                }
                                realCharType.GetProperty(field.Name).SetValue(newChar, whyGodWhy);
                            }
                            else
                            {
                                realCharType.GetProperty(field.Name).SetValue(newChar, customCharType.GetProperty(field.Name).GetValue(character));
                            }
                        } else
                        {
                            BailCharPlugin.Instance.Log.LogInfo("what do you mean we dont have this field? " + field.Name);
                            if (character.cloneId > -1 && realCharType.GetProperty(field.Name).SetMethod != null)
                            {
                                var valueFromChar = realCharType.GetProperty(field.Name).GetValue(ChrAsset.instance.GetAsset(character.cloneId));
                                if (realCharType.GetProperty(field.Name).PropertyType.Name.StartsWith("Int"))
                                {   if (realCharType.GetProperty(field.Name).GetValue(ChrAsset.instance.GetAsset(character.cloneId)).ToString().ToInt() > -1) {
                                        BailCharPlugin.Instance.Log.LogInfo("found an int field we should replace with another char! " + field.Name);
                                        realCharType.GetProperty(field.Name).SetValue(newChar, valueFromChar);
                                    } else
                                    {
                                        continue;
                                    }
                                }
                                else if (valueFromChar != null)
                                {
                                    BailCharPlugin.Instance.Log.LogInfo("Property Type: "+realCharType.GetProperty(field.Name).PropertyType.Name);
                                    BailCharPlugin.Instance.Log.LogInfo("found a field we should replace with another char! " + field.Name);
                                    realCharType.GetProperty(field.Name).SetValue(newChar, valueFromChar);
                                }
                            }
                        }
                    }
                    BailCharPlugin.Instance.Log.LogInfo("Field Replacement is over!");
                    if (character.newModelsPaths.Length > -1)
                    {
                        ModelAsset.Item tempItem = new ModelAsset.Item();
                        if (character.cloneModelId > -1)
                        {
                            ModelAsset.Item cloneAsset = new ModelAsset.Item();
                            cloneAsset = ModelAsset.instance.GetAsset(character.cloneModelId.ToString().ToInt());

                            tempItem.baseModelId = character.cloneModelId;
                            System.Type realModelType = tempItem.GetType();
                            foreach (var field in realModelType.GetProperties())
                            {
                                if (field.Name == "Il2CppType" || field.Name == "modelIdArray" || field.Name == "ObjectClass"
                                    || field.Name == "Pointer" || field.Name == "WasCollected"
                                    )
                                {
                                    continue;

                                }
                                var propertyMarket = realModelType.GetProperty(field.Name);
                                if (propertyMarket.CanWrite && cloneAsset != null) {
                                    BailCharPlugin.Instance.Log.LogInfo("attack of the clones!!! " + field.Name);
                                    var cloneParam = propertyMarket.GetValue(cloneAsset);
                                    propertyMarket.SetValue(tempItem, cloneParam);
                                }
                            }

                        }
                        tempItem.name = character.name;
                        tempItem.modelId = character.modelId;


                        foreach (string modelPath in character.newModelsPaths)
                        {
                            string modelPathWithPrefab = modelPath + ".prefab";
                            string modelPathWithoutExtension = Path.GetFileNameWithoutExtension(modelPathWithPrefab);
                            GameObject modelItself;
                            SkinnedMeshRenderer modelRenderer;
                            if (ResourceManager.GetInstance().m_unityObjectMap.ContainsKey(modelPathWithoutExtension))
                            {
                                modelItself = ResourceManager.GetInstance().Load(modelPathWithoutExtension).Cast<GameObject>();
                                modelRenderer = modelItself.GetComponentInChildren<SkinnedMeshRenderer>();
                                BailCharPlugin.Instance.Log.LogInfo("found an already loaded model! " + modelItself.name);
                                continue;
                            } else
                            {
                                modelItself = realAssetBundle.LoadAsset<GameObject>(modelPathWithPrefab);
                                modelRenderer = modelItself.GetComponentInChildren<SkinnedMeshRenderer>();
                            }
                            if (modelPathWithoutExtension.StartsWith("body"))
                            {
                                tempItem.bodyModelPath = modelPathWithoutExtension;
                                tempItem.bodyTexturePath = modelRenderer.material.mainTexture.name;
                                //tempItem.rootBonePath = modelPathWithoutExtension;

                                if (character.replaceBones == true)
                                {
                                    modelRenderer.bones = ResourceManager.GetInstance().Load(ModelAsset.instance.GetAsset(character.cloneModelId.ToString().ToInt()).bodyModelPath).Cast<GameObject>().GetComponentInChildren<SkinnedMeshRenderer>().bones;

                                }

                            }
                            if (modelPathWithoutExtension.StartsWith("face"))
                            {
                                tempItem.faceModelPath = modelPathWithoutExtension;
                                tempItem.face1TexturePath = modelRenderer.materials[0].name;
                                if (modelRenderer.materials.Count > 1) {
                                    tempItem.face2TexturePath = modelRenderer.materials[1].name;
                                    if (modelRenderer.materials.Count > 2)
                                    {
                                        tempItem.face3TexturePath = modelRenderer.materials[2].name;
                                    }
                                }
                            }
                            if (modelPathWithoutExtension.StartsWith("hair"))
                            {
                                tempItem.hairModelPath = modelPathWithoutExtension;
                                tempItem.hairTexturePath = modelRenderer.material.mainTexture.name;

                            }
                            if (modelPathWithoutExtension.StartsWith("acce"))
                            {
                                tempItem.accessoryModelPath = modelPathWithoutExtension;
                                tempItem.accessoryTexturePath = modelRenderer.material.mainTexture.name;

                            }

                            if (character.shouldCopyMaterialFromClone == true && character.cloneModelId > -1)
                            {
                                var tempTexture = modelRenderer.material.mainTexture;
                                modelRenderer.material = ResourceManager.GetInstance().Load(ModelAsset.instance.GetAsset(character.cloneModelId.ToString().ToInt()).bodyModelPath).Cast<GameObject>().GetComponentInChildren<SkinnedMeshRenderer>().material.MemberwiseClone().Cast<UnityEngine.Material>();
                                modelRenderer.material.mainTexture = tempTexture;
                            }

                                BailCharPlugin.Instance.Log.LogInfo("attempting to add modelObject to a resourceInfo object");
                                ResourceManager.GetInstance().m_unityObjectMap.Add(modelPathWithoutExtension, new ResourceManager.ResourceInfo());
                                ResourceManager.GetInstance().m_unityObjectMap[modelPathWithoutExtension].objects = new Il2CppReferenceArray<UnityEngine.Object>(2);
                                ResourceManager.GetInstance().m_unityObjectMap[modelPathWithoutExtension].objects[0] = modelItself;
                                ResourceManager.GetInstance().m_unityObjectMap[modelPathWithoutExtension].objects[1] = modelRenderer.sharedMesh;
                                BailCharPlugin.Instance.Log.LogInfo("at least my weird circumvention worked");


                                if (ResourceManager.GetInstance().m_unityObjectMap[modelPathWithoutExtension].objects[0].Cast<GameObject>().transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture != null && ResourceManager.GetInstance().m_unityObjectMap.ContainsKey(ResourceManager.GetInstance().m_unityObjectMap[modelPathWithoutExtension].objects[0].Cast<GameObject>().transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture.name) == false) {
                                    string textureName = ResourceManager.GetInstance().m_unityObjectMap[modelPathWithoutExtension].objects[0].Cast<GameObject>().transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture.name;
                                    BailCharPlugin.Instance.Log.LogInfo("texturename tryna be " + textureName);
                                    ResourceManager.GetInstance().m_unityObjectMap.Add(textureName, new ResourceManager.ResourceInfo());
                                    Texture2D tempTex = ResourceManager.GetInstance().m_unityObjectMap[modelPathWithoutExtension].objects[0].Cast<GameObject>().transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture.Cast<Texture2D>();
                                    BailCharPlugin.Instance.Log.LogInfo("temptex is real!!!! " + tempTex.name);
                                    ResourceManager.GetInstance().m_unityObjectMap[textureName].objects = new Il2CppReferenceArray<UnityEngine.Object>(1);
                                ResourceManager.GetInstance().m_unityObjectMap[textureName].objects[0] = (tempTex);
                            }
                                ModelDataAsset.Item justForGoodMeasure = new ModelDataAsset.Item();
                                justForGoodMeasure.outlineNormals = new Il2CppStructArray<Color>(0);
                                justForGoodMeasure.meshName = modelPathWithoutExtension;
                                justForGoodMeasure.path = modelPath;
                                ModelDataAsset.instance.items.Add(justForGoodMeasure);
                                BailCharPlugin.Instance.Log.LogInfo("added "+modelPath+" to Resource Manager!");


                        }
                        if (tempItem != null) {
                            ModelAsset.instance.items.Add(tempItem);
                        } else
                        {
                            BailCharPlugin.Instance.Log.LogError("temp item is null hehehehgehehe GHJKLFDSAGHJKADSFRT GVJABSCFHKMVFMHBJN,DASFASDGJKHAS");
                        }
                        newChar.modelIdArray = new Il2CppStructArray<int>(4);
                        newChar.modelIdArray[0] = tempItem.modelId;
                        newChar.modelIdArray[1] = tempItem.modelId; //will add unique modelids later
                        newChar.modelIdArray[2] = tempItem.modelId; //jsons will need updates though
                        newChar.modelIdArray[3] = tempItem.modelId;
                        if (character.cloneAnimId > -1) {
                            newChar.animId = character.animId;
                        }
                    }
                    if (character.animReplacePaths != null && character.animReplaceNames.Length > 0)
                    {
                        var tempAnims = new AnimAsset.Item();
                        System.Type tempType = tempAnims.GetType();
                        tempAnims.animId = character.animId;

                        GameObject origModelObject;
                        origModelObject = ResourceManager.GetInstance().Load<GameObject>(ModelAsset.instance.GetAsset(character.modelId).bodyModelPath);

                        if (character.cloneAnimId > -1)
                        {
                            foreach (var field in tempType.GetProperties())
                            {
                                if (tempType.GetProperty(field.Name).SetMethod == null || field.Name == "animId")
                                {
                                    continue;

                                }
                                if (tempType.GetProperty(field.Name).GetValue(tempAnims) == null && character.cloneAnimId != -1)
                                {
                                    tempType.GetProperty(field.Name).SetValue(tempAnims, tempType.GetProperty(field.Name).GetValue(AnimAsset.instance.GetAsset(character.cloneAnimId)));
                                }
                            }
                        }

                            for (var chuck = 0; chuck < character.animReplaceNames.Length; chuck++)
                            {
                            string currentName = character.animReplaceNames[chuck];
                            string currentPath = character.animReplacePaths[chuck];
                            BailCharPlugin.Instance.Log.LogInfo("key we wanna replace: "+currentName);
                            if (tempType.GetProperty(currentName).SetMethod == null || tempType.GetProperty(currentName) == null || !currentName.EndsWith("FileName"))
                            {
                                continue;

                            }
                                string animPathWithAnim = currentPath + ".fbx";
                                string animPathWithoutExtension = Path.GetFileNameWithoutExtension(currentPath);
                                tempType.GetProperty(currentName).SetValue(tempAnims, animPathWithoutExtension);
                                if (!ResourceManager.GetInstance().m_unityObjectMap.ContainsKey(animPathWithoutExtension))
                                {
                                BailCharPlugin.Instance.Log.LogInfo("about to add the resources for "+currentPath);
                                BailCharPlugin.Instance.Log.LogInfo("sharliePort "+animPathWithoutExtension+" does not obey and doesnt work");
                                GameObject animGameObject = realAssetBundle.LoadAsset<GameObject>(animPathWithAnim);
                                AnimationClip animOcelot = realAssetBundle.LoadAsset<AnimationClip>(animPathWithAnim);
                                Avatar animAirbender = realAssetBundle.LoadAsset<Avatar>(animPathWithAnim);
                                ResourceManager.GetInstance().m_unityObjectMap.Add(animPathWithoutExtension, new ResourceManager.ResourceInfo());
                                ResourceManager.GetInstance().m_unityObjectMap[animPathWithoutExtension].objects = new Il2CppReferenceArray<UnityEngine.Object>(3);
                                if (origModelObject != null)
                                {
                                    ResourceManager.GetInstance().m_unityObjectMap[animPathWithoutExtension].objects[2] = origModelObject.GetComponent<Animator>().avatar;
                                }
                                else
                                {
                                    ResourceManager.GetInstance().m_unityObjectMap[animPathWithoutExtension].objects[2] = animAirbender;
                                }
                                ResourceManager.GetInstance().m_unityObjectMap[animPathWithoutExtension].objects[0] = animGameObject;
                                ResourceManager.GetInstance().m_unityObjectMap[animPathWithoutExtension].objects[1] = animOcelot;


                            }
                        }

                        AnimAsset.instance.items.Add(tempAnims);
                    } else if (character.cloneAnimId > -1)
                    {
                        newChar.animId = character.cloneAnimId;
                    }

                    ChrAsset.instance.items.Add(newChar);
                    AocAsset.instance.items[1].chrList.Add(newChar.id);
                    BailCharPlugin.Instance.Log.LogInfo("added New Character " + newChar.name);

                } else
                {
                    BailCharPlugin.Instance.Log.LogError("json didnt exist for asset file "+fileNameWithoutExtension);
                }
            }


        }
    }
}