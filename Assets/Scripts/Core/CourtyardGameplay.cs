using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace JahNomad.Reutov
{
    /// <summary>Mobile multitouch joystick; independent pointer ID permits simultaneous actions.</summary>
    public sealed class ReutovJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Vector2 Direction { get; private set; }
        public RectTransform Knob;
        private RectTransform area;
        private int activePointer = int.MinValue;

        private void Awake() { area = GetComponent<RectTransform>(); }

        public void OnPointerDown(PointerEventData data)
        {
            if (activePointer != int.MinValue) return;
            activePointer = data.pointerId;
            OnDrag(data);
        }

        public void OnDrag(PointerEventData data)
        {
            if (activePointer != data.pointerId || area == null) return;
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                area, data.position, data.pressEventCamera, out local)) return;
            Direction = Vector2.ClampMagnitude(local / Mathf.Max(1f, area.rect.width * .323f), 1f);
            if (Knob != null) Knob.anchoredPosition = Direction * (area.rect.width * .219f);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (data.pointerId != activePointer) return;
            ResetInput();
        }

        private void ResetInput()
        {
            activePointer = int.MinValue;
            Direction = Vector2.zero;
            if (Knob != null) Knob.anchoredPosition = Vector2.zero;
        }

        private void OnDisable() { ResetInput(); }
    }

    public sealed class ReutovPressHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool Held { get; private set; }
        public void OnPointerDown(PointerEventData data) { Held = true; }
        public void OnPointerUp(PointerEventData data) { Held = false; }
        private void OnDisable() { Held = false; }
    }

    /// <summary>First playable Reutov chapter. Static geometry and collision footprints share one coordinate system.</summary>
    public sealed class CourtyardGameplay : MonoBehaviour
    {
        private const float W = 1280f, H = 720f;
        private const float Radius = 14f;
        private readonly Rect[] obstacles = {
            new Rect(100, 314, 280, 145), // abandoned Moskvich
            new Rect(835, 307, 285, 104), // black Volga
            new Rect(917, 480, 175, 113), // playground swings
            new Rect(1050, 584, 127, 91), // sandbox
            new Rect(32, 486, 223, 100), // garage walls
            new Rect(930, 168, 180, 115), // shop
            new Rect(697, 570, 95, 46), // bench
            new Rect(430, 585, 70, 56) // concrete planter
        };
        private readonly Vector2 spawn = new Vector2(604, 578);
        private readonly Vector2 regina = new Vector2(736, 498);
        private Vector2 pos;
        private Vector2 facing = Vector2.up;
        private RectTransform sceneRoot;
        private Canvas canvas;
        private Image marker, hint, dialoguePanel;
        private Button actionButton;
        private string[][] conversations;
        private int dialoguePage;
        private int pendingStage;
        private bool complete;
        private readonly Vector2 entrance = new Vector2(601, 270);
        private readonly Vector2[] targets = {
            new Vector2(736, 498), new Vector2(387, 420), new Vector2(295, 603),
            new Vector2(736, 498), new Vector2(601, 270)
        };
        private Image character, flashlight, objective, battery, notice, agentA, agentB, coneA, coneB;
        private ReutovJoystick joystick;
        private ReutovPressHold sprintButton;
        private Action back;
        private int stage;
        private bool flashlightEnabled = true;
        private bool dialog;
        private float clock;
        private float detectionCooldown;
        private Vector2 coneOriginA = new Vector2(878, 428);
        private Vector2 coneOriginB = new Vector2(1147, 482);
        private Vector2 visionA, visionB;

        public void Begin(Canvas target, Action returnToMenu)
        {
            canvas = target;
            back = returnToMenu;
            pos = spawn;
            stage = 0;
            clock = 0f;
            // Fit the world and every hit area together; wide screens must not stretch collisions.
            GameObject frame = new GameObject("Courtyard 16:9", typeof(RectTransform), typeof(AspectRatioFitter));
            sceneRoot = frame.GetComponent<RectTransform>();
            sceneRoot.SetParent(canvas.transform, false);
            sceneRoot.anchorMin = Vector2.zero;
            sceneRoot.anchorMax = Vector2.one;
            sceneRoot.offsetMin = sceneRoot.offsetMax = Vector2.zero;
            AspectRatioFitter fit = frame.GetComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio = W / H;
            conversations = new [] {
                new [] { "РЕГИНА: МИША, ТВОЙ БРАТ НЕ ВЕРНУЛСЯ.\nЯ ВИДЕЛА ЕГО У ГАРАЖЕЙ.",
                         "РЕГИНА: ВОЗЬМИ БАТАРЕЙКИ ИЗ МОСКВИЧА.\nИ НЕ ПОПАДАЙСЯ ЛЮДЯМ У ЧЁРНОЙ ВОЛГИ." },
                new [] { "МИША: ДВЕ БАТАРЕЙКИ. ФОНАРЬ РАБОТАЕТ.\nТЕПЕРЬ МОЖНО ОСМОТРЕТЬ ГАРАЖИ." },
                new [] { "МИША: ЭТО ДНЕВНИК БРАТА...\nНА ОБЛОЖКЕ КАКАЯ-ТО ЧЁРНАЯ СЛИЗЬ.",
                         "ЗАПИСЬ: ЕСЛИ НЕ ВЕРНУСЬ, ИЩИТЕ У СТАНЦИИ.\nПОСЛЕ ДВЕНАДЦАТИ НЕ ВЕРЬТЕ ГОЛОСАМ." },
                new [] { "РЕГИНА: У СТАНЦИИ? ЗНАЧИТ, ОН УЗНАЛ...\nТИШЕ. КАЖЕТСЯ, НАС УСЛЫШАЛИ.",
                         "РЕГИНА: В ПОДЪЕЗД, ПОКА ОНИ НЕ ПОДОШЛИ!\nДНЕВНИК НЕЛЬЗЯ ОСТАВЛЯТЬ ЗДЕСЬ." },
                new [] { "ДВЕРЬ ЗАКРЫЛАСЬ. ШАГИ ОСТАЛИСЬ СНАРУЖИ.\nВ ДНЕВНИКЕ ЕСТЬ АДРЕС У СТАНЦИИ.",
                         "ПЕРВЫЙ ЭПИЗОД ПРОЙДЕН.\nМИША И РЕГИНА НАШЛИ ПЕРВУЮ ЗАЦЕПКУ." }
            };
            UI("CourtyardBackdrop", new Vector2(0, 0), new Vector2(W, H), false, true);

            // Patrol cones are behind the moving character and rotate around patrol guards.
            coneA = UI("PatrolCone", World(coneOriginA), new Vector2(204, 165), false);
            coneB = UI("PatrolCone", World(coneOriginB), new Vector2(204, 165), false);
            agentA = UI("Agent", World(coneOriginA), new Vector2(45, 79), false);
            agentB = UI("Agent", World(coneOriginB), new Vector2(45, 79), false);
            UI("Regina", World(regina), new Vector2(49, 89), false);
            UI("Entrance", World(entrance + new Vector2(0, -33)), new Vector2(66, 94), false);
            marker = UI("QuestMarker", World(regina), new Vector2(32, 43), false);

            flashlight = UI("FlashCone", World(pos), new Vector2(196, 150), false);
            character = UI("Misha", World(pos), new Vector2(56, 92), false);
            UI("ChapterTitle", new Vector2(-270, 319), new Vector2(700, 60), false);
            objective = UI("ObjectiveBattery", new Vector2(-300, 256), new Vector2(640, 65), false);
            battery = UI("Battery0", new Vector2(529, 319), new Vector2(200, 60), false);
            notice = UI("QuestNotice", new Vector2(0, 85), new Vector2(760, 145), false);
            notice.gameObject.SetActive(false);

            Image backButton = UI("Back", new Vector2(200, 319), new Vector2(145, 55), true);
            Click(backButton, () => back?.Invoke());

            Image pad = UI("JoyBase", new Vector2(-530, -267), new Vector2(155, 155), true);
            joystick = pad.gameObject.AddComponent<ReutovJoystick>();
            Image knob = CreateChild(pad.rectTransform, "JoyKnob", new Vector2(73, 73));
            knob.sprite = ReutovRuntimeArt.GetSprite("JoyKnob");
            joystick.Knob = knob.rectTransform;

            Image run = UI("Run", new Vector2(398, -283), new Vector2(116, 116), true);
            sprintButton = run.gameObject.AddComponent<ReutovPressHold>();
            Image action = UI("Action", new Vector2(531, -214), new Vector2(118, 118), true);
            Click(action, Interact);
            actionButton = action.GetComponent<Button>();
            Image toggleLight = UI("Light", new Vector2(540, -320), new Vector2(85, 70), true);
            Click(toggleLight, () => { flashlightEnabled = !flashlightEnabled; });
            hint = UI("QuestNotice", new Vector2(0, -322), new Vector2(620, 46), false);
            hint.sprite = ReutovRuntimeArt.StoryPanel("WASD / СТРЕЛКИ - ХОДЬБА    SHIFT - БЕГ", false);
            // Dialogue is above controls and captures taps so closing it cannot trigger movement.
            dialoguePanel = UI("QuestNotice", new Vector2(0, 34), new Vector2(1050, 232), true);
            Click(dialoguePanel, Interact);
            dialoguePanel.gameObject.SetActive(false);
            UpdateQuestHUD();
            ShowDialogue("МИША: ПОЛНОЧЬ. ДВОР СЛОВНО ВЫМЕР.\nРЕГИНА ЖДЁТ У СКАМЕЙКИ. НАДО ПОГОВОРИТЬ.");
            pendingStage = 0;
        }

        private static Vector2 World(Vector2 point) { return new Vector2(point.x - W / 2f, H / 2f - point.y); }

        private Image UI(string asset, Vector2 point, Vector2 size, bool canTouch, bool stretch = false)
        {
            GameObject go = new GameObject(asset, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(sceneRoot, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchorMin = new Vector2(.5f + (point.x - size.x / 2f) / W, .5f + (point.y - size.y / 2f) / H);
            rect.anchorMax = new Vector2(.5f + (point.x + size.x / 2f) / W, .5f + (point.y + size.y / 2f) / H);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            Image image = go.GetComponent<Image>();
            Texture2D texture = Resources.Load<Texture2D>(asset);
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(.5f, .5f), 100f);
            }
            else image.sprite = ReutovRuntimeArt.GetSprite(asset);
            image.preserveAspect = !stretch;
            image.raycastTarget = canTouch;
            return image;
        }

        private static Image CreateChild(Transform parent, string label, Vector2 size)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchorMin = new Vector2(.5f - size.x / 310f, .5f - size.y / 310f);
            rect.anchorMax = new Vector2(.5f + size.x / 310f, .5f + size.y / 310f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        private static void Click(Image image, UnityEngine.Events.UnityAction action)
        {
            Button b = image.gameObject.AddComponent<Button>();
            b.targetGraphic = image;
            b.onClick.AddListener(action);
        }

        private void Update()
        {
            if (canvas == null || character == null) return;
            float dt = Mathf.Min(.05f, Time.deltaTime);
            clock += dt;
            detectionCooldown = Mathf.Max(0f, detectionCooldown - dt);

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                Interact();
            if (Input.GetKeyDown(KeyCode.F)) flashlightEnabled = !flashlightEnabled;

            Vector2 stick = joystick != null ? joystick.Direction : Vector2.zero;
            Vector2 keyboard = new Vector2(
                (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) -
                (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0) -
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0));
            Vector2 move = keyboard.sqrMagnitude > .01f ? keyboard.normalized : stick;
            float length = move.magnitude;
            if (length > 1f) move.Normalize();
            if (!dialog && !complete && length > .1f)
            {
                facing = move.normalized;
                bool sprint = (sprintButton != null && sprintButton.Held) ||
                    Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                float distance = (sprint ? 235f : 148f) * dt;
                Vector2 nextX = new Vector2(pos.x + move.x * distance, pos.y);
                if (Free(nextX)) pos = nextX;
                Vector2 nextY = new Vector2(pos.x, pos.y + move.y * distance);
                if (Free(nextY)) pos = nextY;
            }

            Vector2 offset = World(pos);
            Place(character, offset +
                new Vector2(0, !dialog && !complete && length > .1f ? Mathf.Sin(clock * 16f) * 2f : 0f));
            flashlight.gameObject.SetActive(flashlightEnabled && stage >= 2);
            if (flashlightEnabled && stage >= 2)
            {
                Place(flashlight, offset + new Vector2(facing.x * 66f, -facing.y * 66f));
                flashlight.rectTransform.localEulerAngles = new Vector3(0, 0,
                    -Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f);
            }

            if (stage < targets.Length && !complete)
            {
                Place(marker, World(targets[stage]) + new Vector2(0, 65 + Mathf.Sin(clock * 3f) * 5f));
                bool nearby = Vector2.Distance(pos, targets[stage]) < 95f;
                actionButton.interactable = dialog || nearby;
                hint.sprite = ReutovRuntimeArt.StoryPanel(dialog ? "НАЖМИТЕ НА ДИАЛОГ ИЛИ E - ДАЛЕЕ" :
                    nearby ? "E / ДЕЙСТВИЕ - " + (stage == 0 || stage == 3 ? "ГОВОРИТЬ" : stage == 4 ? "ВОЙТИ" : "ОСМОТРЕТЬ") :
                    "ИДИТЕ К МЕРЦАЮЩЕЙ МЕТКЕ    F - ФОНАРЬ", false);
            }
            visionA = new Vector2(-.4f + Mathf.Sin(clock * .42f) * .3f, .9f).normalized;
            visionB = new Vector2(-.7f + Mathf.Sin(clock * .28f) * .2f, .7f).normalized;
            PositionVision(coneA, visionA, coneOriginA);
            PositionVision(coneB, visionB, coneOriginB);

            if (!dialog && !complete && detectionCooldown <= 0f &&
                (VisibleTo(coneOriginA, visionA) || VisibleTo(coneOriginB, visionB)))
            {
                pos = spawn;
                detectionCooldown = 3f;
                notice.sprite = ReutovRuntimeArt.GetSprite("Detected");
                notice.gameObject.SetActive(true);
                Invoke(nameof(HideNotice), 1.7f);
            }
        }

        private bool Free(Vector2 at)
        {
            if (at.x < 64 || at.x > 1215 || at.y < 240 || at.y > 661)
                return false;
            foreach (Rect obstacle in obstacles)
            {
                Vector2 closest = new Vector2(
                    Mathf.Clamp(at.x, obstacle.xMin, obstacle.xMax),
                    Mathf.Clamp(at.y, obstacle.yMin, obstacle.yMax));
                if ((closest - at).sqrMagnitude < Radius * Radius) return false;
            }
            return true;
        }

        private bool VisibleTo(Vector2 from, Vector2 direction)
        {
            Vector2 delta = pos - from;
            return delta.magnitude < 147f && delta.sqrMagnitude > 100f &&
                   Vector2.Dot(delta.normalized, direction) > .83f;
        }

        private void PositionVision(Image image, Vector2 direction, Vector2 from)
        {
            if (image == null) return;
            Place(image, World(from + direction * 70f));
            image.rectTransform.localEulerAngles = new Vector3(0, 0,
                -Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }

        private static void Place(Image image, Vector2 position)
        {
            RectTransform rect = image.rectTransform;
            Vector2 half = (rect.anchorMax - rect.anchorMin) * .5f;
            Vector2 center = new Vector2(.5f + position.x / W, .5f + position.y / H);
            rect.anchorMin = center - half;
            rect.anchorMax = center + half;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void UpdateQuestHUD()
        {
            string[] goals = { "ПОГОВОРИТЕ С РЕГИНОЙ", "НАЙДИТЕ БАТАРЕЙКИ В МОСКВИЧЕ",
                "НАЙДИТЕ ДНЕВНИК У ГАРАЖЕЙ", "ПОКАЖИТЕ ДНЕВНИК РЕГИНЕ", "УКРОЙТЕСЬ В ПОДЪЕЗДЕ", "ЭПИЗОД 01 ПРОЙДЕН" };
            objective.sprite = ReutovRuntimeArt.StoryPanel(goals[stage], false);
            battery.sprite = ReutovRuntimeArt.GetSprite(stage < 2 ? "Battery0" : "Battery2");
        }

        private void ShowDialogue(string message)
        {
            CancelInvoke(nameof(HideNotice));
            notice.gameObject.SetActive(false);
            dialog = true;
            dialoguePanel.sprite = ReutovRuntimeArt.StoryPanel(message + "\n\nE / ДЕЙСТВИЕ / НАЖАТИЕ - ДАЛЕЕ", true);
            dialoguePanel.gameObject.SetActive(true);
        }

        private void Interact()
        {
            if (dialog)
            {
                if (pendingStage > stage && ++dialoguePage < conversations[stage].Length)
                {
                    ShowDialogue(conversations[stage][dialoguePage]);
                    return;
                }
                dialog = false;
                detectionCooldown = Mathf.Max(detectionCooldown, 1f);
                dialoguePanel.gameObject.SetActive(false);
                stage = pendingStage;
                complete = stage == targets.Length;
                UpdateQuestHUD();
                if (complete)
                {
                    marker.gameObject.SetActive(false);
                    actionButton.interactable = false;
                    hint.sprite = ReutovRuntimeArt.StoryPanel("КОНЕЦ ЭПИЗОДА    НАЗАД - В МЕНЮ", false);
                }
                return;
            }
            if (complete) return;
            if (Vector2.Distance(pos, targets[stage]) < 95f)
            {
                dialoguePage = 0;
                pendingStage = stage + 1;
                ShowDialogue(conversations[stage][0]);
            }
            else
            {
                notice.sprite = ReutovRuntimeArt.GetSprite("GetCloser");
                notice.gameObject.SetActive(true);
                CancelInvoke(nameof(HideNotice));
                Invoke(nameof(HideNotice), 1.3f);
            }
        }

        private void HideNotice() { if (!dialog && notice != null) notice.gameObject.SetActive(false); }
        private void OnDisable() { CancelInvoke(); }
    }
}
