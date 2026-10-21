using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace JahNomad.Reutov {
 public enum ScreenMode { Studio, Menu, Intro, Settings }
 public sealed class BootFlowController : MonoBehaviour {
  private ScreenMode screen;
  private Canvas canvas;
  private AudioSource audioPlayer;
  private float deadline;
  private bool soundOn=true;
  private void Awake(){
   Application.targetFrameRate=60;
   var go=new GameObject("UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
   canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
   scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
   if(FindFirstObjectByType<EventSystem>()==null) new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   audioPlayer=gameObject.AddComponent<AudioSource>();audioPlayer.playOnAwake=false;
   audioPlayer.clip=Resources.Load<AudioClip>("JahNomadSting");
   Show(ScreenMode.Studio);
  }
  private void Update(){
   if(screen==ScreenMode.Studio&&Time.unscaledTime>=deadline) Show(ScreenMode.Menu);
   if(Input.GetKeyDown(KeyCode.Escape))Show(ScreenMode.Menu);
  }
  private void Clear(){for(int i=canvas.transform.childCount-1;i>=0;i--) Destroy(canvas.transform.GetChild(i).gameObject);}
  private Image Picture(string name,Vector2 pos,Vector2 size){
   var g=new GameObject(name,typeof(RectTransform),typeof(Image));
   g.transform.SetParent(canvas.transform,false);
   var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;
   var img=g.GetComponent<Image>();img.sprite=Resources.Load<Sprite>(name);
   if(img.sprite==null){var t=Resources.Load<Texture2D>(name);if(t!=null)img.sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f));}
   img.preserveAspect=true;img.raycastTarget=false;return img;
  }
  private void Button(string asset,Vector2 pos,UnityEngine.Events.UnityAction action){
   var img=Picture(asset,pos,new Vector2(390,70));img.raycastTarget=true;
   var b=img.gameObject.AddComponent<Button>();b.targetGraphic=img;b.onClick.AddListener(action);
  }
  private void Show(ScreenMode next){
   screen=next;Clear();
   var bg=Picture("Backdrop",Vector2.zero,new Vector2(1280,720));
   var r=(RectTransform)bg.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
   if(next==ScreenMode.Studio){
    Picture("StudioLogo",new Vector2(0,20),new Vector2(850,270));
    Button("Skip",new Vector2(0,-250),()=>Show(ScreenMode.Menu));
    deadline=Time.unscaledTime+3f;
    if(soundOn&&audioPlayer.clip!=null)audioPlayer.Play();
   }else if(next==ScreenMode.Menu){
    audioPlayer.Stop();Picture("GameTitle",new Vector2(0,165),new Vector2(1000,250));
    Button("NewGame",new Vector2(0,-80),()=>Show(ScreenMode.Intro));
    Button("Settings",new Vector2(0,-173),()=>Show(ScreenMode.Settings));
    Button("Quit",new Vector2(0,-266),()=>Show(ScreenMode.Intro));
   }else if(next==ScreenMode.Intro){
    Picture("PrologueCard",new Vector2(0,30),new Vector2(1000,380));
    Button("Back",new Vector2(0,-260),()=>Show(ScreenMode.Menu));
   }else{
    Picture("GameTitle",new Vector2(0,175),new Vector2(800,210));
    Button(soundOn?"SoundOn":"SoundOff",new Vector2(0,-80),()=>{
      soundOn=!soundOn;AudioListener.volume=soundOn?1f:0f;Show(ScreenMode.Settings);
    });
    Button("Back",new Vector2(0,-230),()=>Show(ScreenMode.Menu));
   }
  }
 }
}