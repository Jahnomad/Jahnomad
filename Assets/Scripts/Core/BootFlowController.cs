using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace JahNomad.Reutov
{
    public enum ScreenMode { Studio, Menu, Intro, Playing, Settings, ExitNotice }

    /// <summary>Persistent boot flow, Russian title screen and a real transition to chapter one.</summary>
    [DisallowMultipleComponent]
    public sealed class BootFlowController : MonoBehaviour
    {
        private ScreenMode screen;
        private Canvas canvas;
        private AudioSource music;
        private CourtyardGameplay courtyard;
        private bool soundOn;
        private float studioEnd;
        private bool musicUnlockRequested;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootController()
        {
            if (Object.FindFirstObjectByType<BootFlowController>() == null)
                new GameObject("JAH NOMAD | Boot Flow").AddComponent<BootFlowController>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
            Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
            if (Camera.main == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                Camera gameCamera = cameraObject.GetComponent<Camera>();
                gameCamera.clearFlags = CameraClearFlags.SolidColor;
                gameCamera.backgroundColor = new Color(.03f, .04f, .08f);
            }

            GameObject ui = new GameObject("Reutov UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = ui.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = ui.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            if (Object.FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            soundOn = PlayerPrefs.GetInt("reutov.sound", 1) != 0;
            music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.loop = true;
            music.spatialBlend = 0;
            music.volume = .20f;
            // Prefer the finished synthwave composition when the OGG is supplied.
            // Keep a fully offline procedural fallback, so WebGL never starts silent.
            AudioClip recordedTheme = Resources.Load<AudioClip>("MenuTheme");
            music.clip = recordedTheme != null ? recordedTheme : ReutovSynthwave.GetMenuLoop();
            AudioListener.volume = soundOn ? 1f : 0f;
            Show(ScreenMode.Studio);
        }

        private void Update()
        {
            if (screen == ScreenMode.Studio && Time.unscaledTime >= studioEnd)
                Show(ScreenMode.Menu);

            // WebGL's audio context can be suspended until the first deliberate user gesture.
            if (soundOn && !musicUnlockRequested && screen != ScreenMode.Studio &&
                (Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.anyKeyDown))
            {
                musicUnlockRequested = true;
                PlayMusic();
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (screen != ScreenMode.Menu && screen != ScreenMode.Studio)
                    Show(ScreenMode.Menu);
            }
        }

        private void PlayMusic()
        {
            if (soundOn && music != null && music.clip != null && !music.isPlaying)
                music.Play();
        }

        private void ClearScreen()
        {
            if (courtyard != null)
            {
                courtyard.enabled = false;
                Destroy(courtyard);
                courtyard = null;
            }
            for (int i = canvas.transform.childCount - 1; i >= 0; --i)
                DeactivateAndDestroy(canvas.transform.GetChild(i).gameObject);
        }

        private static void DeactivateAndDestroy(GameObject node)
        {
            node.SetActive(false);
            Destroy(node);
        }

        private Image Picture(string name, Vector2 pos, Vector2 size)
        {
            GameObject node = new GameObject(name, typeof(RectTransform), typeof(Image));
            node.transform.SetParent(canvas.transform, false);
            RectTransform rect = node.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Image image = node.GetComponent<Image>();
            Texture2D texture = Resources.Load<Texture2D>(name);
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(.5f, .5f), 100f);
            }
            else
                image.sprite = ReutovRuntimeArt.GetSprite(name);
            image.preserveAspect = name != "Backdrop" && name != "CourtyardBackdrop";
            image.raycastTarget = false;
            return image;
        }

        private void Button(string asset, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            Image image = Picture(asset, pos, size);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock palette = button.colors;
            palette.normalColor = Color.white;
            palette.highlightedColor = new Color(.84f, 1f, 1f, 1f);
            palette.pressedColor = new Color(.65f, .85f, .92f, 1f);
            button.colors = palette;
            button.onClick.AddListener(() => { musicUnlockRequested = true; PlayMusic(); action(); });
        }

        private void TransparentButton(string asset, Vector2 pos, Vector2 size,
                                       UnityEngine.Events.UnityAction action)
        {
            Image hitArea = Picture(asset, pos, size);
            hitArea.color = new Color(1f, 1f, 1f, 0f);
            hitArea.raycastTarget = true;
            Button button = hitArea.gameObject.AddComponent<Button>();
            button.targetGraphic = hitArea;
            button.onClick.AddListener(() => { musicUnlockRequested = true; PlayMusic(); action(); });
        }

        // MenuMaster is a 16:9 authored image. Its touch regions follow the fitted image
        // instead of relying on fixed screen pixels; this prevents offset taps on 19.5:9.
        private void MenuHotspot(Image master, string label, Vector2 pos, Vector2 size,
                                 UnityEngine.Events.UnityAction action, bool visible = false)
        {
            Image target = Picture(label, Vector2.zero, size);
            target.transform.SetParent(master.transform, false);
            RectTransform box = target.rectTransform;
            box.anchorMin = new Vector2(.5f + (pos.x - size.x * .5f) / 1280f,
                                        .5f + (pos.y - size.y * .5f) / 720f);
            box.anchorMax = new Vector2(.5f + (pos.x + size.x * .5f) / 1280f,
                                        .5f + (pos.y + size.y * .5f) / 720f);
            box.offsetMin = box.offsetMax = Vector2.zero;
            target.color = visible ? Color.white : new Color(1f, 1f, 1f, 0f);
            target.raycastTarget = true;
            Button button = target.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            button.onClick.AddListener(() => { musicUnlockRequested = true; PlayMusic(); action(); });
        }

        private void Show(ScreenMode next)
        {
            screen = next;
            ClearScreen();
            if (next == ScreenMode.Playing)
            {
                courtyard = gameObject.AddComponent<CourtyardGameplay>();
                courtyard.Begin(canvas, () => Show(ScreenMode.Menu));
                PlayMusic();
                return;
            }

            Image background = Picture("Backdrop", Vector2.zero, new Vector2(1280, 720));
            RectTransform rect = background.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            switch (next)
            {
                case ScreenMode.Studio:
                    Picture("StudioLogo", new Vector2(0, 22), new Vector2(540, 162));
                    Button("Skip", new Vector2(0, -263), new Vector2(340, 58), () => Show(ScreenMode.Menu));
                    studioEnd = Time.unscaledTime + 2.1f;
                    break;

                case ScreenMode.Menu:
                    Texture2D approvedMenu = Resources.Load<Texture2D>("MenuMaster");
                    if (approvedMenu != null)
                    {
                        // The approved 16:9 pixel-art illustration includes all lettering and
                        // button artwork. Invisible Unity buttons stay responsive above it.
                        Image master = Picture("MenuMaster", Vector2.zero, new Vector2(1280, 720));
                        RectTransform full = master.rectTransform;
                        full.anchorMin = Vector2.zero;
                        full.anchorMax = Vector2.one;
                        full.offsetMin = full.offsetMax = Vector2.zero;
                        AspectRatioFitter aspect = master.gameObject.AddComponent<AspectRatioFitter>();
                        aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                        aspect.aspectRatio = 1672f / 941f;
                        MenuHotspot(master, "NewGame", new Vector2(-4, 12), new Vector2(340, 68),
                            () => Show(ScreenMode.Intro));
                        MenuHotspot(master, "Settings", new Vector2(-4, -59), new Vector2(340, 65),
                            () => Show(ScreenMode.Settings));
                        MenuHotspot(master, soundOn ? "SoundOn" : "SoundOff",
                            new Vector2(-4, -126), new Vector2(340, 63), ToggleSound, !soundOn);
                        MenuHotspot(master, "Quit", new Vector2(-4, -184), new Vector2(340, 60), ExitGame);
                    }
                    else
                    {
                        Picture("GameTitle", new Vector2(0, 175), new Vector2(920, 265));
                        Button("NewGame", new Vector2(0, -62), new Vector2(370, 64), () => Show(ScreenMode.Intro));
                        Button("Settings", new Vector2(0, -138), new Vector2(370, 64), () => Show(ScreenMode.Settings));
                        Button(soundOn ? "SoundOn" : "SoundOff", new Vector2(0, -215), new Vector2(310, 56), ToggleSound);
                        Button("Quit", new Vector2(0, -292), new Vector2(370, 62), ExitGame);
                    }
                    PlayMusic();
                    break;

                case ScreenMode.Intro:
                    Picture("PrologueCard", new Vector2(0, 73), new Vector2(980, 320));
                    Button("Continue", new Vector2(0, -178), new Vector2(355, 65), () => Show(ScreenMode.Playing));
                    Button("Back", new Vector2(0, -269), new Vector2(310, 60), () => Show(ScreenMode.Menu));
                    PlayMusic();
                    break;

                case ScreenMode.Settings:
                    Picture("GameTitle", new Vector2(0, 166), new Vector2(780, 220));
                    Button(soundOn ? "SoundOn" : "SoundOff", new Vector2(0, -85), new Vector2(370, 66), ToggleSound);
                    Button("Back", new Vector2(0, -213), new Vector2(320, 66), () => Show(ScreenMode.Menu));
                    break;

                case ScreenMode.ExitNotice:
                    Picture("ExitNotice", new Vector2(0, 45), new Vector2(960, 330));
                    Button("Back", new Vector2(0, -257), new Vector2(320, 64), () => Show(ScreenMode.Menu));
                    break;
            }
        }

        private void ToggleSound()
        {
            soundOn = !soundOn;
            PlayerPrefs.SetInt("reutov.sound", soundOn ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = soundOn ? 1f : 0f;
            if (soundOn)
            {
                musicUnlockRequested = true;
                PlayMusic();
            }
            else music.Pause();
            Show(screen == ScreenMode.Settings ? ScreenMode.Settings : ScreenMode.Menu);
        }

        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#elif UNITY_WEBGL
            Show(ScreenMode.ExitNotice);
#else
            Application.Quit();
#endif
        }
    }
}
