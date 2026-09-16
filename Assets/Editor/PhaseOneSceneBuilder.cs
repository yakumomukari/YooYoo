using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PhaseOneSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/PhaseOnePrototype.unity";
    private const string TuningPath = "Assets/Settings/YoyoTuning.asset";

    [InitializeOnLoadMethod]
    private static void BuildMissingPrototypeScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            return;

        EditorApplication.delayCall += TryBuildMissingPrototypeScene;
    }

    private static void TryBuildMissingPrototypeScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += TryBuildMissingPrototypeScene;
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            Build();
    }

    [MenuItem("Yooooooooo/Build Phase One Prototype Scene")]
    public static void Build()
    {
        YoyoTuning tuning = AssetDatabase.LoadAssetAtPath<YoyoTuning>(TuningPath);
        if (tuning == null)
        {
            tuning = ScriptableObject.CreateInstance<YoyoTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera();
        CreateLight();
        Transform world = new GameObject("Graybox World").transform;
        CreateBlock(world, "Ground Left", new Vector2(-13f, -4f), new Vector2(16f, 1f));
        CreateBlock(world, "Ground Right", new Vector2(12f, -4f), new Vector2(18f, 1f));
        CreateBlock(world, "High Wall", new Vector2(20.5f, 1f), new Vector2(1f, 11f));
        CreateBlock(world, "Ceiling", new Vector2(2f, 9f), new Vector2(20f, 1f));
        CreateBlock(world, "Platform Low", new Vector2(-2f, -0.5f), new Vector2(5f, 0.6f));
        CreateBlock(world, "Platform Mid", new Vector2(7f, 2.5f), new Vector2(5f, 0.6f));
        CreateBlock(world, "Platform High", new Vector2(15f, 6f), new Vector2(5f, 0.6f));

        GameObject player = new GameObject("Player");
        player.layer = 3;
        player.transform.position = new Vector3(-12f, -2.5f, 0f);
        SpriteRenderer playerRenderer = player.AddComponent<SpriteRenderer>();
        playerRenderer.sprite = BuiltinSprite();
        playerRenderer.color = new Color(0.2f, 0.75f, 1f);
        player.transform.localScale = new Vector3(0.9f, 1.4f, 1f);
        Rigidbody2D playerBody = player.AddComponent<Rigidbody2D>();
        playerBody.freezeRotation = true;
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<CapsuleCollider2D>();
        PlayerMovement movement = player.AddComponent<PlayerMovement>();
        BulletTimeController bulletTime = player.AddComponent<BulletTimeController>();
        DistanceJoint2D joint = player.AddComponent<DistanceJoint2D>();
        joint.enabled = false;
        YoyoController controller = player.AddComponent<YoyoController>();
        PrototypeReset reset = player.AddComponent<PrototypeReset>();

        GameObject projectileObject = new GameObject("Yoyo Projectile");
        SpriteRenderer projectileRenderer = projectileObject.AddComponent<SpriteRenderer>();
        projectileRenderer.sprite = BuiltinSprite();
        projectileRenderer.color = new Color(1f, 0.3f, 0.2f);
        projectileObject.transform.localScale = Vector3.one * 0.45f;
        Rigidbody2D projectileBody = projectileObject.AddComponent<Rigidbody2D>();
        projectileBody.gravityScale = 0f;
        CircleCollider2D projectileCollider = projectileObject.AddComponent<CircleCollider2D>();
        projectileCollider.radius = 0.5f;
        YoyoProjectile projectile = projectileObject.AddComponent<YoyoProjectile>();

        GameObject ropeObject = new GameObject("Rope");
        LineRenderer rope = ropeObject.AddComponent<LineRenderer>();
        rope.positionCount = 2;
        rope.startWidth = 0.06f;
        rope.endWidth = 0.06f;
        rope.material = new Material(Shader.Find("Sprites/Default"));
        rope.startColor = rope.endColor = new Color(1f, 0.85f, 0.25f);

        Set(movement, "tuning", tuning);
        Set(bulletTime, "tuning", tuning);
        Set(controller, "tuning", tuning);
        Set(controller, "player", movement);
        Set(controller, "projectile", projectile);
        Set(controller, "bulletTime", bulletTime);
        Set(controller, "launchOrigin", player.transform);
        Set(controller, "ropeLine", rope);
        Set(reset, "playerBody", playerBody);
        Set(reset, "yoyo", controller);

        CameraFollow2D follow = camera.gameObject.AddComponent<CameraFollow2D>();
        Set(follow, "target", player.transform);

        CreateHud(movement, controller);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Built phase one prototype scene: " + ScenePath);
    }

    private static Camera CreateCamera()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        Camera camera = go.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
        go.transform.position = new Vector3(-8f, 0f, -10f);
        return camera;
    }

    private static void CreateLight()
    {
        // Unlit sprite material keeps the graybox readable without a render-pipeline-specific light.
    }

    private static void CreateBlock(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject block = new GameObject(name);
        block.transform.SetParent(parent);
        block.transform.position = position;
        block.transform.localScale = size;
        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = BuiltinSprite();
        renderer.color = new Color(0.38f, 0.42f, 0.48f);
        block.AddComponent<BoxCollider2D>();
    }

    private static Sprite BuiltinSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static void CreateHud(PlayerMovement movement, YoyoController controller)
    {
        GameObject canvasObject = new GameObject("Prototype HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        PrototypeHUD hud = canvasObject.AddComponent<PrototypeHUD>();

        Text debug = CreateText(canvasObject.transform, "Debug Text", new Vector2(12f, -12f),
            new Vector2(620f, 100f), TextAnchor.UpperLeft, 18);
        debug.text = "Debug";

        GameObject sliderObject = new GameObject("Charge Slider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(canvasObject.transform, false);
        RectTransform sliderRect = (RectTransform)sliderObject.transform;
        sliderRect.anchorMin = sliderRect.anchorMax = new Vector2(0.5f, 0.12f);
        sliderRect.sizeDelta = new Vector2(280f, 20f);
        Slider slider = sliderObject.GetComponent<Slider>();
        Image background = CreateImage(sliderObject.transform, "Background", new Color(0f, 0f, 0f, 0.65f));
        background.rectTransform.anchorMin = Vector2.zero;
        background.rectTransform.anchorMax = Vector2.one;
        background.rectTransform.offsetMin = background.rectTransform.offsetMax = Vector2.zero;
        Image fill = CreateImage(sliderObject.transform, "Fill", new Color(1f, 0.55f, 0.1f));
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = new Vector2(3f, 3f);
        fill.rectTransform.offsetMax = new Vector2(-3f, -3f);
        slider.fillRect = fill.rectTransform;
        slider.targetGraphic = fill;

        GameObject arrowObject = new GameObject("Aim Arrow", typeof(RectTransform), typeof(Image));
        arrowObject.transform.SetParent(canvasObject.transform, false);
        RectTransform arrow = (RectTransform)arrowObject.transform;
        arrow.anchorMin = arrow.anchorMax = new Vector2(0.5f, 0.5f);
        arrow.pivot = new Vector2(0f, 0.5f);
        arrow.sizeDelta = new Vector2(100f, 5f);
        arrowObject.GetComponent<Image>().color = new Color(1f, 0.85f, 0.25f);

        Text controls = CreateText(canvasObject.transform, "Controls", new Vector2(12f, 12f),
            new Vector2(620f, 60f), TextAnchor.LowerLeft, 18);
        controls.text = "A/D or arrows: move    Hold/release LMB: aim, charge, throw    RMB/E: recall    R: reset";

        Set(hud, "player", movement);
        Set(hud, "yoyo", controller);
        Set(hud, "debugText", debug);
        Set(hud, "chargeSlider", slider);
        Set(hud, "aimArrow", arrow);
    }

    private static Text CreateText(Transform parent, string name, Vector2 position,
        Vector2 size, TextAnchor alignment, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = alignment.ToString().Contains("Lower") ? Vector2.zero : new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = rect.anchorMin;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static void Set(Object target, string property, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
