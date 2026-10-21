using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JahNomad.Reutov;

namespace JahNomad.Reutov.Editor {
 [InitializeOnLoad]
 public sealed class ReutovSceneSetup : UnityEditor.Build.IPreprocessBuildWithReport {
  public static int BuildSceneCount => EditorBuildSettings.scenes.Length;
  public int callbackOrder => -9000;
  public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report) { if(!File.Exists(PathToScene)) CreateScene(); }

  private const string PathToScene="Assets/Scenes/00_Boot.unity";
  static ReutovSceneSetup(){EditorApplication.delayCall+=EnsureScene;}
  private static void EnsureScene(){
   if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
   if(!File.Exists(PathToScene))CreateScene();
  }
  [MenuItem("JAH NOMAD/Создать стартовую сцену")]
  public static void CreateScene(){
   Directory.CreateDirectory("Assets/Scenes");
   Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
   camera.tag="MainCamera"; camera.transform.position=new Vector3(0,0,-10);
   camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
   camera.GetComponent<Camera>().backgroundColor=new Color(.03f,.04f,.08f);
   new GameObject("Boot Flow",typeof(BootFlowController));
   EditorSceneManager.SaveScene(scene,PathToScene);
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(PathToScene,true)};
   AssetDatabase.SaveAssets();
  }
 }
}