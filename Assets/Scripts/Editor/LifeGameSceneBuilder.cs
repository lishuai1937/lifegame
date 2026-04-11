#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LifeGameSceneBuilder : EditorWindow
{
    [MenuItem("LifeGame/Build All (2D Board + 3D World) #&b")]
    public static void BuildAll()
    {
        if (!EditorUtility.DisplayDialog("LifeGame",
            "Build 2D side-scroll board + 3D open world?\n\nGenerates:\n- 2D Board (Mario-style)\n- 2D Player Token\n- 3D Open World area\n- Full UI\n- All Managers", "Build", "Cancel"))
            return;

        // Clean up old objects first
        string[] oldNames = { "GameManager", "BoardManager", "AudioManager", "SystemManagers",
            "Board", "OpenWorld", "UICanvas", "EventSystem", "MainCamera" };
        foreach (var name in oldNames)
        {
            // Find all including inactive
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == name)
                    Undo.DestroyObjectImmediate(root);
            }
        }
        // Also clean DontDestroyOnLoad leftovers in editor
        var allGOs = Object.FindObjectsOfType<GameManager>();
        foreach (var gm in allGOs) Undo.DestroyObjectImmediate(gm.gameObject);

        BuildManagers();
        BuildBoard2D();
        BuildOpenWorldArea();
        BuildUICanvas();
        BuildEventSystem();
        WireReferences();
        Debug.Log("[LifeGame] Build complete! Press Play.");
        EditorUtility.DisplayDialog("Done", "Press Play to run!\n\nSpace = Roll dice\nWASD = Move (3D world)\nE = Interact\nESC = Exit world", "OK");
    }

    [MenuItem("LifeGame/Build 2D Board Only")]
    public static void BuildBoardOnly() { BuildBoard2D(); }

    [MenuItem("LifeGame/Build 3D World Only")]
    public static void BuildWorldOnly() { BuildOpenWorldArea(); }

    // ==================== Managers ====================
    static void BuildManagers()
    {
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
        gmObj.AddComponent<GameFlowController>();
        Undo.RegisterCreatedObjectUndo(gmObj, "GM");

        var bmObj = new GameObject("BoardManager");
        var bm = bmObj.AddComponent<BoardManager>();
        // Try load full grid data first, fallback to sample
        var gridJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/GridData/grids_full.json");
        if (gridJson == null) gridJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/GridData/grids_sample.json");
        if (gridJson != null) bm.GridDataJson = gridJson;
        bm.CellSpacing = 2.2f;
        bm.RowHeight = 3.2f;
        Undo.RegisterCreatedObjectUndo(bmObj, "BM");

        var amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();
        Undo.RegisterCreatedObjectUndo(amObj, "AM");

        // === ALL NEW SYSTEM MANAGERS ===
        var sysObj = new GameObject("SystemManagers");

        sysObj.AddComponent<SocialSystem>();
        sysObj.AddComponent<PhoneSystem>();
        sysObj.AddComponent<DialogueSystem>();
        sysObj.AddComponent<NPCEventManager>();
        sysObj.AddComponent<NPCInfluenceSystem>();
        sysObj.AddComponent<NPCSpawner>();
        sysObj.AddComponent<WorldEventSystem>();
        sysObj.AddComponent<WeatherSystem>();
        sysObj.AddComponent<EraSystem>();
        sysObj.AddComponent<AfterlifeManager>();
        sysObj.AddComponent<SceneGenerator>();
        sysObj.AddComponent<KarmaTracker>();
        sysObj.AddComponent<StatGrowth>();

        Undo.RegisterCreatedObjectUndo(sysObj, "Systems");
        Debug.Log("[LifeGame] All system managers created");
    }

    // ==================== 2D Board ====================
    static void BuildBoard2D()
    {
        // Delete old camera
        var oldCam = GameObject.FindObjectOfType<Camera>();
        if (oldCam != null) Undo.DestroyObjectImmediate(oldCam.gameObject);

        // 2D Orthographic Camera
        var camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.06f, 0.15f);
        cam.nearClipPlane = -10;
        cam.farClipPlane = 100;
        camObj.transform.position = new Vector3(5, 4, -10);
        camObj.AddComponent<AudioListener>();
        var camFollow = camObj.AddComponent<BoardCamera2D>();
        Undo.RegisterCreatedObjectUndo(camObj, "Cam");

        var board = new GameObject("Board");
        board.transform.position = Vector3.zero;

        float spacing = 2.2f;
        int perRow = 10;
        float rowH = 3.2f;
        int totalCells = 100;

        // === BACKGROUND LAYERS ===
        // Deep background
        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.SetParent(board.transform);
        bg.transform.position = new Vector3(perRow * spacing / 2f, totalCells / perRow * rowH / 2f, 8);
        bg.transform.localScale = new Vector3(perRow * spacing + 20, totalCells / perRow * rowH + 30, 1);
        var bgMr = bg.GetComponent<MeshRenderer>();
        var bgMat = new Material(Shader.Find("Unlit/Color"));
        bgMat.color = new Color(0.06f, 0.04f, 0.12f);
        bgMr.material = bgMat;
        Object.DestroyImmediate(bg.GetComponent<Collider>());

        // Decorative background stars/dots
        var starMat = new Material(Shader.Find("Unlit/Color"));
        starMat.color = new Color(0.25f, 0.22f, 0.35f);
        for (int s = 0; s < 60; s++)
        {
            var star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.name = "Star_" + s;
            star.transform.SetParent(board.transform);
            float sx = Random.Range(-3f, perRow * spacing + 3f);
            float sy = Random.Range(-2f, (totalCells / perRow) * rowH + 2f);
            star.transform.position = new Vector3(sx, sy, 6);
            float ss = Random.Range(0.05f, 0.15f);
            star.transform.localScale = new Vector3(ss, ss, 1);
            star.GetComponent<MeshRenderer>().material = starMat;
            Object.DestroyImmediate(star.GetComponent<Collider>());
        }

        // === PLATFORM GRID CELLS ===
        var platformMat = new Material(Shader.Find("Unlit/Color"));
        for (int i = 0; i < totalCells; i++)
        {
            int row = i / perRow;
            int col = i % perRow;
            bool goRight = (row % 2 == 0);
            float x = goRight ? col * spacing : (perRow - 1 - col) * spacing;
            float y = row * rowH;
            int age = i + 1;

            // Platform base (thicker, more solid)
            var cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cell.name = "Cell_" + age;
            cell.transform.SetParent(board.transform);
            cell.transform.position = new Vector3(x, y, 0);

            bool isMilestone = (age == 6 || age == 12 || age == 18 || age == 30 || age == 50 || age == 60 || age == 80 || age == 100);
            cell.transform.localScale = isMilestone ? new Vector3(2f, 1.2f, 1) : new Vector3(1.8f, 0.9f, 1);

            var mr = cell.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Unlit/Color"));
            Color baseColor = GetAgeColor(age);
            mat.color = isMilestone ? HighlightColor(baseColor) : baseColor;
            mr.material = mat;
            Object.DestroyImmediate(cell.GetComponent<Collider>());

            // Platform top highlight (thin bright line on top)
            var topLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
            topLine.name = "Top_" + age;
            topLine.transform.SetParent(cell.transform);
            float cellH = isMilestone ? 1.2f : 0.9f;
            topLine.transform.localPosition = new Vector3(0, 0.45f, -0.05f);
            topLine.transform.localScale = new Vector3(1f, 0.06f, 1);
            var tlMr = topLine.GetComponent<MeshRenderer>();
            var tlMat = new Material(Shader.Find("Unlit/Color"));
            tlMat.color = new Color(Mathf.Min(baseColor.r + 0.3f, 1), Mathf.Min(baseColor.g + 0.3f, 1), Mathf.Min(baseColor.b + 0.3f, 1));
            tlMr.material = tlMat;
            Object.DestroyImmediate(topLine.GetComponent<Collider>());

            // Platform bottom shadow
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadow.name = "Shadow_" + age;
            shadow.transform.SetParent(cell.transform);
            shadow.transform.localPosition = new Vector3(0, -0.45f, -0.05f);
            shadow.transform.localScale = new Vector3(1f, 0.06f, 1);
            var shMr = shadow.GetComponent<MeshRenderer>();
            var shMat = new Material(Shader.Find("Unlit/Color"));
            shMat.color = new Color(baseColor.r * 0.4f, baseColor.g * 0.4f, baseColor.b * 0.4f);
            shMr.material = shMat;
            Object.DestroyImmediate(shadow.GetComponent<Collider>());

            // Age label
            var label = new GameObject("Label_" + age);
            label.transform.SetParent(cell.transform);
            label.transform.localPosition = new Vector3(0, 0, -0.1f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = age.ToString();
            tm.fontSize = isMilestone ? 36 : 30;
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = isMilestone ? Color.white : new Color(1, 1, 1, 0.75f);
            if (isMilestone) tm.fontStyle = FontStyle.Bold;
        }

        // === CONNECTORS (ladder/vine style between rows) ===
        for (int row = 0; row < totalCells / perRow - 1; row++)
        {
            bool goRight = (row % 2 == 0);
            float x = goRight ? (perRow - 1) * spacing : 0;
            float y1 = row * rowH;
            float y2 = (row + 1) * rowH;

            // Main vine/ladder
            var conn = GameObject.CreatePrimitive(PrimitiveType.Quad);
            conn.name = "Connector_" + row;
            conn.transform.SetParent(board.transform);
            conn.transform.position = new Vector3(x, (y1 + y2) / 2f, 0.2f);
            conn.transform.localScale = new Vector3(0.2f, rowH - 0.8f, 1);
            var cMr = conn.GetComponent<MeshRenderer>();
            var cMat = new Material(Shader.Find("Unlit/Color"));
            cMat.color = new Color(0.35f, 0.55f, 0.35f); // green vine
            cMr.material = cMat;
            Object.DestroyImmediate(conn.GetComponent<Collider>());

            // Ladder rungs
            for (float ry = y1 + 0.8f; ry < y2 - 0.3f; ry += 0.7f)
            {
                var rung = GameObject.CreatePrimitive(PrimitiveType.Quad);
                rung.name = "Rung";
                rung.transform.SetParent(board.transform);
                rung.transform.position = new Vector3(x, ry, 0.15f);
                rung.transform.localScale = new Vector3(0.6f, 0.08f, 1);
                rung.GetComponent<MeshRenderer>().material = cMat;
                Object.DestroyImmediate(rung.GetComponent<Collider>());
            }
        }

        // === PLAYER CHARACTER (pixel person, not a dot) ===
        var tokenParent = new GameObject("PlayerToken");
        tokenParent.transform.SetParent(board.transform);
        tokenParent.transform.position = new Vector3(0, 1f, -0.5f);

        // Body
        var body = GameObject.CreatePrimitive(PrimitiveType.Quad);
        body.name = "Body";
        body.transform.SetParent(tokenParent.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.5f, 0.6f, 1);
        var bodyMat = new Material(Shader.Find("Unlit/Color"));
        bodyMat.color = new Color(0.2f, 0.6f, 1f); // blue shirt
        body.GetComponent<MeshRenderer>().material = bodyMat;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Quad);
        head.name = "Head";
        head.transform.SetParent(tokenParent.transform);
        head.transform.localPosition = new Vector3(0, 0.5f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 1);
        var headMat = new Material(Shader.Find("Unlit/Color"));
        headMat.color = new Color(1f, 0.85f, 0.7f); // skin
        head.GetComponent<MeshRenderer>().material = headMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Eyes
        var eyeL = GameObject.CreatePrimitive(PrimitiveType.Quad);
        eyeL.name = "EyeL";
        eyeL.transform.SetParent(tokenParent.transform);
        eyeL.transform.localPosition = new Vector3(-0.08f, 0.55f, -0.05f);
        eyeL.transform.localScale = new Vector3(0.08f, 0.08f, 1);
        var eyeMat = new Material(Shader.Find("Unlit/Color"));
        eyeMat.color = new Color(0.1f, 0.1f, 0.2f);
        eyeL.GetComponent<MeshRenderer>().material = eyeMat;
        Object.DestroyImmediate(eyeL.GetComponent<Collider>());

        var eyeR = GameObject.CreatePrimitive(PrimitiveType.Quad);
        eyeR.name = "EyeR";
        eyeR.transform.SetParent(tokenParent.transform);
        eyeR.transform.localPosition = new Vector3(0.08f, 0.55f, -0.05f);
        eyeR.transform.localScale = new Vector3(0.08f, 0.08f, 1);
        eyeR.GetComponent<MeshRenderer>().material = eyeMat;
        Object.DestroyImmediate(eyeR.GetComponent<Collider>());

        // Legs
        var legL = GameObject.CreatePrimitive(PrimitiveType.Quad);
        legL.name = "LegL";
        legL.transform.SetParent(tokenParent.transform);
        legL.transform.localPosition = new Vector3(-0.12f, -0.45f, 0);
        legL.transform.localScale = new Vector3(0.18f, 0.35f, 1);
        var legMat = new Material(Shader.Find("Unlit/Color"));
        legMat.color = new Color(0.25f, 0.25f, 0.4f); // dark pants
        legL.GetComponent<MeshRenderer>().material = legMat;
        Object.DestroyImmediate(legL.GetComponent<Collider>());

        var legR = GameObject.CreatePrimitive(PrimitiveType.Quad);
        legR.name = "LegR";
        legR.transform.SetParent(tokenParent.transform);
        legR.transform.localPosition = new Vector3(0.12f, -0.45f, 0);
        legR.transform.localScale = new Vector3(0.18f, 0.35f, 1);
        legR.GetComponent<MeshRenderer>().material = legMat;
        Object.DestroyImmediate(legR.GetComponent<Collider>());

        camFollow.Target = tokenParent.transform;

        // Dice visual (pixel dice)
        var dice = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dice.name = "DiceVisual";
        dice.transform.SetParent(board.transform);
        dice.transform.position = new Vector3(0, 2.5f, -0.5f);
        dice.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        var dMr = dice.GetComponent<MeshRenderer>();
        var dMat = new Material(Shader.Find("Unlit/Color"));
        dMat.color = new Color(0.95f, 0.95f, 0.85f);
        dMr.material = dMat;
        Object.DestroyImmediate(dice.GetComponent<Collider>());

        Undo.RegisterCreatedObjectUndo(board, "Board");
    }
    // ==================== 3D Open World Area ====================
    static void BuildOpenWorldArea()
    {
        var world = new GameObject("OpenWorld");
        world.transform.position = new Vector3(0, -50, 0); // below board, hidden

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "OW_Ground";
        ground.transform.SetParent(world.transform);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);
        var gMr = ground.GetComponent<MeshRenderer>();
        var gMat = new Material(Shader.Find("Standard"));
        gMat.color = new Color(0.25f, 0.45f, 0.25f);
        gMr.material = gMat;

        // Light for open world
        var owLight = new GameObject("OW_Light");
        owLight.transform.SetParent(world.transform);
        var lt = owLight.AddComponent<Light>();
        lt.type = LightType.Directional;
        lt.color = new Color(1, 0.95f, 0.85f);
        lt.intensity = 1.2f;
        owLight.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Player character (3D capsule)
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.SetParent(world.transform);
        player.transform.localPosition = new Vector3(0, 1, 0);
        var pMr = player.GetComponent<MeshRenderer>();
        var pMat = new Material(Shader.Find("Standard"));
        pMat.color = new Color(0.2f, 0.6f, 1f);
        pMr.material = pMat;
        Object.DestroyImmediate(player.GetComponent<Collider>());
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.5f; cc.center = new Vector3(0, 1, 0);
        player.AddComponent<PlayerController>();

        // 3D Camera for open world
        var owCam = new GameObject("OW_Camera");
        owCam.transform.SetParent(world.transform);
        owCam.transform.localPosition = new Vector3(0, 8, -6);
        owCam.transform.rotation = Quaternion.Euler(45, 0, 0);
        var owCamComp = owCam.AddComponent<Camera>();
        owCamComp.fieldOfView = 60;
        owCamComp.clearFlags = CameraClearFlags.SolidColor;
        owCamComp.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
        owCamComp.enabled = false; // disabled until entering open world

        // Sample buildings
        for (int i = 0; i < 5; i++)
        {
            var bld = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bld.name = "Building_" + i;
            bld.transform.SetParent(world.transform);
            float bx = Random.Range(-15f, 15f);
            float bz = Random.Range(-15f, 15f);
            float bh = Random.Range(2f, 8f);
            bld.transform.localPosition = new Vector3(bx, bh / 2f, bz);
            bld.transform.localScale = new Vector3(Random.Range(2f, 5f), bh, Random.Range(2f, 5f));
            var bMr = bld.GetComponent<MeshRenderer>();
            var bMat = new Material(Shader.Find("Standard"));
            bMat.color = new Color(Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f));
            bMr.material = bMat;
        }

        // Sample event trigger
        var trigger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trigger.name = "EventPoint";
        trigger.transform.SetParent(world.transform);
        trigger.transform.localPosition = new Vector3(5, 0.5f, 5);
        trigger.transform.localScale = new Vector3(1, 0.1f, 1);
        var eMr = trigger.GetComponent<MeshRenderer>();
        var eMat = new Material(Shader.Find("Standard"));
        eMat.color = Color.yellow;
        eMr.material = eMat;
        var et = trigger.AddComponent<EventTrigger>();
        et.EventTitle = "Sample Event";
        et.EventDescription = "Something happens here...";
        et.GoldReward = 50;
        et.IsExitPoint = true;
        var col = trigger.GetComponent<Collider>();
        if (col != null) { Object.DestroyImmediate(col); }
        var bc = trigger.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(3, 3, 3);

        world.SetActive(false); // hidden until player enters a grid world
        Undo.RegisterCreatedObjectUndo(world, "OpenWorld");
    }

    static Color GetAgeColor(int age)
    {
        if (age <= 12) return new Color(0.2f, 0.5f, 0.7f);    // blue-ish childhood
        if (age <= 17) return new Color(0.2f, 0.6f, 0.3f);    // green youth
        if (age <= 30) return new Color(0.7f, 0.6f, 0.15f);   // golden young
        if (age <= 50) return new Color(0.5f, 0.5f, 0.55f);   // gray-white prime
        if (age <= 65) return new Color(0.6f, 0.4f, 0.15f);   // brown middle
        return new Color(0.35f, 0.35f, 0.4f);                  // dark gray elder
    }

    static Color HighlightColor(Color c)
    {
        return new Color(Mathf.Min(c.r + 0.25f, 1), Mathf.Min(c.g + 0.25f, 1), Mathf.Min(c.b + 0.25f, 1));
    }
    // ==================== Wire References ====================
    static void WireReferences()
    {
        var gmObj = GameObject.Find("GameManager");
        if (gmObj == null) return;
        var flow = gmObj.GetComponent<GameFlowController>();
        if (flow == null) return;

        var bmObj = GameObject.Find("BoardManager");
        if (bmObj != null) flow.Board = bmObj.GetComponent<BoardManager>();

        flow.PlayerObj = GameObject.Find("Player");
        flow.BoardObj = GameObject.Find("Board");

        var token = GameObject.Find("PlayerToken");
        if (token != null)
        {
            flow.PlayerToken = token.transform;
            // Also wire to BoardManager
            if (flow.Board != null) flow.Board.PlayerToken = token.transform;
        }

        var dice = GameObject.Find("DiceVisual");
        if (dice != null) flow.DiceVisual = dice.transform;

        // Wire open world references (OpenWorld may be inactive, so search all root objects)
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "OpenWorld")
            {
                flow.OpenWorldRoot = root;
                var owCamT = root.transform.Find("OW_Camera");
                if (owCamT != null) flow.OpenWorldCamera = owCamT.GetComponent<Camera>();
                break;
            }
        }

        // Wire camera
        var camFollow = GameObject.FindObjectOfType<BoardCamera2D>();
        if (camFollow != null && token != null)
            camFollow.Target = token.transform;

        EditorUtility.SetDirty(flow);
        if (flow.Board != null) EditorUtility.SetDirty(flow.Board);
    }

    // ==================== UI Canvas ====================
    // ==================== PIXEL STYLE COLORS ====================
    static readonly Color PxBg = new Color(0.07f, 0.05f, 0.12f, 0.97f);       // deep purple-black
    static readonly Color PxAccent = new Color(0.4f, 0.85f, 0.4f);             // pixel green
    static readonly Color PxGold = new Color(1f, 0.84f, 0.2f);                 // gold/yellow
    static readonly Color PxCyan = new Color(0.3f, 0.9f, 0.95f);               // cyan
    static readonly Color PxRed = new Color(0.9f, 0.25f, 0.25f);               // red
    static readonly Color PxPurple = new Color(0.6f, 0.3f, 0.85f);             // purple
    static readonly Color PxOrange = new Color(0.95f, 0.55f, 0.15f);           // orange
    static readonly Color PxWhite = new Color(0.92f, 0.92f, 0.88f);            // warm white
    static readonly Color PxDim = new Color(0.45f, 0.42f, 0.5f);               // dim text
    static readonly Color PxPanel = new Color(0.1f, 0.08f, 0.16f, 0.95f);      // panel bg
    static readonly Color PxBtnGreen = new Color(0.15f, 0.55f, 0.2f);          // button green
    static readonly Color PxBtnRed = new Color(0.65f, 0.12f, 0.12f);           // button red
    static readonly Color PxBtnBlue = new Color(0.15f, 0.35f, 0.65f);          // button blue
    static readonly Color PxBtnOrange = new Color(0.7f, 0.4f, 0.1f);           // button orange
    static readonly Color PxBorder = new Color(0.3f, 0.25f, 0.4f);             // border/frame

    static void BuildUICanvas()
    {
        var canvasObj = new GameObject("UICanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        var uiMgr = canvasObj.AddComponent<UIManager>();

        // ===== MAIN MENU =====
        var mm = MkPanel(canvasObj.transform, "MainMenuPanel", new Color(0.07f, 0.05f, 0.12f, 1f));
        uiMgr.MainMenuPanel = mm;

        // Decorative top/bottom pixel bars
        PxBar(mm.transform, new Vector2(0, 440), new Vector2(1200, 4), PxAccent);
        PxBar(mm.transform, new Vector2(0, -440), new Vector2(1200, 4), PxAccent);

        // Title block
        MkText(mm.transform, "TitleText", "< 人 生 游 戏 >", 90, new Vector2(0, 160), PxAccent);
        MkText(mm.transform, "SubTitle", "~ 人生模拟器 ~", 28, new Vector2(0, 70), PxCyan);

        // Pixel art decorative dots
        MkText(mm.transform, "Dots1", ". . . . . . . . . . . . . . .", 18, new Vector2(0, 25), PxDim);

        // Start button - big and centered
        MkBtn(mm.transform, "StartButton", ">> 开始新人生 <<", new Vector2(0, -60), new Vector2(420, 70), PxBtnGreen);

        // Version + hint
        MkText(mm.transform, "VersionText", "v0.3 像素版", 18, new Vector2(0, -180), PxDim);
        MkText(mm.transform, "HintText", "空格=摇骰子 | WASD=移动 | E=交互 | ESC=菜单", 16, new Vector2(0, -220), PxWhite);

        // Bottom decorative
        PxBar(mm.transform, new Vector2(0, -280), new Vector2(600, 2), PxPurple);

        // ===== BOARD GAME PANEL =====
        var bp = MkPanel(canvasObj.transform, "BoardGamePanel", new Color(0, 0, 0, 0)); bp.SetActive(false);
        uiMgr.BoardGamePanel = bp;

        // ===== DICE PANEL =====
        var dp = MkPanel(canvasObj.transform, "DicePanel", new Color(0, 0, 0, 0)); dp.SetActive(false);
        uiMgr.DicePanel = dp;
        // Dice result - big pixel number
        MkText(dp.transform, "DiceResultText", "", 64, new Vector2(0, -320), PxGold);
        MkBtn(dp.transform, "RollDiceBtn", "[ 摇骰子 ] 空格", new Vector2(0, -420), new Vector2(280, 55), PxBtnRed);

        // ===== SPEED CHOICE =====
        var sp = MkPanel(canvasObj.transform, "SpeedChoicePanel", new Color(0.05f, 0.03f, 0.1f, 0.92f)); sp.SetActive(false);
        uiMgr.SpeedChoicePanel = sp;
        PxBar(sp.transform, new Vector2(0, 140), new Vector2(500, 3), PxCyan);
        MkText(sp.transform, "SpeedTitle", "选择人生节奏", 36, new Vector2(0, 100), PxCyan);
        MkText(sp.transform, "SpeedHint", "你想活得快还是慢？", 18, new Vector2(0, 55), PxDim);
        MkBtn(sp.transform, "SlowBtn", "慢速 [1-3]", new Vector2(-170, -20), new Vector2(240, 60), PxBtnBlue);
        MkBtn(sp.transform, "FastBtn", "快速 [3-6]", new Vector2(170, -20), new Vector2(240, 60), PxBtnOrange);
        PxBar(sp.transform, new Vector2(0, -100), new Vector2(500, 3), PxCyan);

        // ===== PLAYER INFO HUD =====
        var ip = new GameObject("PlayerInfoPanel");
        ip.transform.SetParent(canvasObj.transform, false);
        var ipR = ip.AddComponent<RectTransform>();
        ipR.anchorMin = new Vector2(0, 1); ipR.anchorMax = new Vector2(0, 1);
        ipR.pivot = new Vector2(0, 1);
        ipR.anchoredPosition = new Vector2(12, -12);
        ipR.sizeDelta = new Vector2(280, 120);
        ip.AddComponent<Image>().color = new Color(0.08f, 0.06f, 0.14f, 0.9f);
        // Border
        var ipBorder = new GameObject("Border"); ipBorder.transform.SetParent(ip.transform, false);
        var ibR = ipBorder.AddComponent<RectTransform>();
        ibR.anchorMin = Vector2.zero; ibR.anchorMax = Vector2.one; ibR.sizeDelta = Vector2.zero;
        var ibO = ipBorder.AddComponent<Outline>();
        ibO.effectColor = PxBorder; ibO.effectDistance = new Vector2(2, 2);
        ip.SetActive(false);
        uiMgr.PlayerInfoPanel = ip;
        MkText(ip.transform, "NameText", "> ---", 18, new Vector2(10, -18), PxAccent, TextAnchor.MiddleLeft);
        MkText(ip.transform, "AgeText", "AGE: 0", 18, new Vector2(10, -42), PxCyan, TextAnchor.MiddleLeft);
        MkText(ip.transform, "GoldText", "GOLD: 0", 18, new Vector2(10, -66), PxGold, TextAnchor.MiddleLeft);
        MkText(ip.transform, "KarmaText", "", 18, new Vector2(10, -90), PxAccent, TextAnchor.MiddleLeft);

        // ===== EVENT DIALOG =====
        var ep = MkPanel(canvasObj.transform, "EventDialogPanel", new Color(0.06f, 0.04f, 0.1f, 0.94f)); ep.SetActive(false);
        uiMgr.EventDialogPanel = ep;
        PxBar(ep.transform, new Vector2(0, 220), new Vector2(700, 3), PxGold);
        MkText(ep.transform, "EventTitle", "事件", 36, new Vector2(0, 180), PxGold);
        MkText(ep.transform, "EventDesc", "...", 22, new Vector2(0, 60), PxWhite);
        MkBtn(ep.transform, "Choice1Btn", "选项 A", new Vector2(0, -50), new Vector2(420, 50), PxBtnGreen);
        MkBtn(ep.transform, "Choice2Btn", "选项 B", new Vector2(0, -115), new Vector2(420, 50), PxBtnRed);
        MkBtn(ep.transform, "ContinueBtn", ">> 进入世界 <<", new Vector2(0, -200), new Vector2(300, 55), PxBtnBlue);
        PxBar(ep.transform, new Vector2(0, -260), new Vector2(700, 3), PxGold);

        // ===== DEATH PANEL =====
        var dth = MkPanel(canvasObj.transform, "DeathPanel", new Color(0.03f, 0.02f, 0.06f, 0.97f)); dth.SetActive(false);
        uiMgr.DeathPanel = dth;
        PxBar(dth.transform, new Vector2(0, 260), new Vector2(800, 4), PxRed);
        MkText(dth.transform, "DeathTitle", "人生终章", 60, new Vector2(0, 200), PxRed);
        PxBar(dth.transform, new Vector2(0, 160), new Vector2(400, 2), PxDim);
        MkText(dth.transform, "DeathAge", "享年: --", 26, new Vector2(0, 120), PxWhite);
        MkText(dth.transform, "DeathGold", "财富: --", 22, new Vector2(0, 80), PxGold);
        MkText(dth.transform, "DeathKarma", "...", 22, new Vector2(0, 45), PxAccent);
        MkText(dth.transform, "RealmText", "???", 30, new Vector2(0, -10), PxCyan);
        PxBar(dth.transform, new Vector2(0, -60), new Vector2(400, 2), PxDim);
        MkBtn(dth.transform, "ReincarnateBtn", "转世重生", new Vector2(-150, -120), new Vector2(240, 55), PxPurple);
        MkBtn(dth.transform, "MainMenuBtn", "返回主菜单", new Vector2(150, -120), new Vector2(240, 55), new Color(0.3f, 0.28f, 0.35f));
        PxBar(dth.transform, new Vector2(0, -200), new Vector2(800, 4), PxRed);

        // ===== CG PANEL =====
        var cg = MkPanel(canvasObj.transform, "CGPanel", new Color(0, 0, 0, 1)); cg.SetActive(false);
        uiMgr.CGPanel = cg;
        MkText(cg.transform, "CGText", "", 24, Vector2.zero, PxWhite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "UI");
    }

    /// <summary>Pixel-style decorative bar</summary>
    static void PxBar(Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        var bar = new GameObject("PxBar");
        bar.transform.SetParent(parent, false);
        var rt = bar.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        bar.AddComponent<Image>().color = color;
    }

    static void BuildEventSystem()
    {
        if (GameObject.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(es, "ES");
        }
    }

    // ==================== UI Helpers ====================
    static GameObject MkPanel(Transform p, string n, Color c)
    {
        var o = new GameObject(n); o.transform.SetParent(p, false);
        var r = o.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.sizeDelta = Vector2.zero;
        var img = o.AddComponent<Image>(); img.color = c;
        if (c.a < 0.01f) img.raycastTarget = false;
        return o;
    }

    static GameObject MkText(Transform p, string n, string txt, int sz, Vector2 pos, Color c, TextAnchor a = TextAnchor.MiddleCenter)
    {
        var o = new GameObject(n); o.transform.SetParent(p, false);
        o.AddComponent<RectTransform>().anchoredPosition = pos;
        o.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 60);
        var t = o.AddComponent<Text>();
        t.text = txt; t.fontSize = sz; t.color = c; t.alignment = a;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return o;
    }

    static GameObject MkBtn(Transform p, string n, string lbl, Vector2 pos, Vector2 sz, Color c)
    {
        var o = new GameObject(n); o.transform.SetParent(p, false);
        var r = o.AddComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = sz;
        o.AddComponent<Image>().color = c;
        var btn = o.AddComponent<Button>();
        var cols = btn.colors;
        cols.highlightedColor = new Color(Mathf.Min(c.r+0.15f,1), Mathf.Min(c.g+0.15f,1), Mathf.Min(c.b+0.15f,1));
        cols.pressedColor = new Color(Mathf.Max(c.r-0.1f,0), Mathf.Max(c.g-0.1f,0), Mathf.Max(c.b-0.1f,0));
        btn.colors = cols;
        var tO = new GameObject("Text"); tO.transform.SetParent(o.transform, false);
        var tR = tO.AddComponent<RectTransform>(); tR.anchorMin = Vector2.zero; tR.anchorMax = Vector2.one; tR.sizeDelta = Vector2.zero;
        var t = tO.AddComponent<Text>();
        t.text = lbl; t.fontSize = 22; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return o;
    }
}
#endif