#if UNITY_EDITOR
using UnityEngine;
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
        Undo.RegisterCreatedObjectUndo(bmObj, "BM");

        var amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();
        Undo.RegisterCreatedObjectUndo(amObj, "AM");
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
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.12f);
        cam.nearClipPlane = -10;
        cam.farClipPlane = 100;
        camObj.transform.position = new Vector3(5, 4, -10);
        camObj.AddComponent<AudioListener>();
        var camFollow = camObj.AddComponent<BoardCamera2D>();
        Undo.RegisterCreatedObjectUndo(camObj, "Cam");

        // Board parent
        var board = new GameObject("Board");
        board.transform.position = Vector3.zero;

        float spacing = 2.5f;
        int perRow = 10;
        float rowH = 3f;
        int totalCells = 100;

        // Background gradient (simple sprite-less colored quads)
        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.SetParent(board.transform);
        bg.transform.position = new Vector3(perRow * spacing / 2f, totalCells / perRow * rowH / 2f, 5);
        bg.transform.localScale = new Vector3(perRow * spacing + 10, totalCells / perRow * rowH + 20, 1);
        var bgMr = bg.GetComponent<MeshRenderer>();
        var bgMat = new Material(Shader.Find("Unlit/Color"));
        bgMat.color = new Color(0.05f, 0.05f, 0.1f);
        bgMr.material = bgMat;
        Object.DestroyImmediate(bg.GetComponent<Collider>());

        // Grid cells
        for (int i = 0; i < totalCells; i++)
        {
            int row = i / perRow;
            int col = i % perRow;
            bool goRight = (row % 2 == 0);
            float x = goRight ? col * spacing : (perRow - 1 - col) * spacing;
            float y = row * rowH;

            var cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cell.name = "Cell_" + (i + 1);
            cell.transform.SetParent(board.transform);
            cell.transform.position = new Vector3(x, y, 0);
            cell.transform.localScale = new Vector3(2f, 0.6f, 1);

            var mr = cell.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = GetAgeColor(i + 1);
            mr.material = mat;
            Object.DestroyImmediate(cell.GetComponent<Collider>());

            // Age label (using a child quad is placeholder - real version uses TextMesh)
            var label = new GameObject("Label_" + (i + 1));
            label.transform.SetParent(cell.transform);
            label.transform.localPosition = new Vector3(0, 0, -0.1f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = (i + 1).ToString();
            tm.fontSize = 32;
            tm.characterSize = 0.15f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1, 1, 1, 0.8f);

            // Milestone markers (bigger cells for key ages)
            int age = i + 1;
            if (age == 6 || age == 12 || age == 18 || age == 30 || age == 50 || age == 60 || age == 80 || age == 100)
            {
                cell.transform.localScale = new Vector3(2.2f, 0.8f, 1);
                mat.color = HighlightColor(mat.color);
                tm.fontStyle = FontStyle.Bold;
                tm.color = Color.white;
            }
        }

        // Connectors between rows (vertical lines at zigzag turns)
        for (int row = 0; row < totalCells / perRow - 1; row++)
        {
            bool goRight = (row % 2 == 0);
            float x = goRight ? (perRow - 1) * spacing : 0;
            float y1 = row * rowH;
            float y2 = (row + 1) * rowH;

            var conn = GameObject.CreatePrimitive(PrimitiveType.Quad);
            conn.name = "Connector_" + row;
            conn.transform.SetParent(board.transform);
            conn.transform.position = new Vector3(x, (y1 + y2) / 2f, 0.1f);
            conn.transform.localScale = new Vector3(0.15f, rowH - 0.6f, 1);
            var cMr = conn.GetComponent<MeshRenderer>();
            var cMat = new Material(Shader.Find("Unlit/Color"));
            cMat.color = new Color(0.3f, 0.3f, 0.4f);
            cMr.material = cMat;
            Object.DestroyImmediate(conn.GetComponent<Collider>());
        }

        // Player Token (2D circle)
        var token = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        token.name = "PlayerToken";
        token.transform.SetParent(board.transform);
        token.transform.position = new Vector3(0, 1f, -0.5f);
        token.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        var tMr = token.GetComponent<MeshRenderer>();
        var tMat = new Material(Shader.Find("Unlit/Color"));
        tMat.color = new Color(1f, 0.25f, 0.25f);
        tMr.material = tMat;
        Object.DestroyImmediate(token.GetComponent<Collider>());

        // Wire camera to follow token
        camFollow.Target = token.transform;

        // Dice visual (small cube near token)
        var dice = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dice.name = "DiceVisual";
        dice.transform.SetParent(board.transform);
        dice.transform.position = new Vector3(0, 2.5f, -0.5f);
        dice.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        var dMr = dice.GetComponent<MeshRenderer>();
        var dMat = new Material(Shader.Find("Unlit/Color"));
        dMat.color = Color.white;
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

        // Wire camera
        var camFollow = GameObject.FindObjectOfType<BoardCamera2D>();
        if (camFollow != null && token != null)
            camFollow.Target = token.transform;

        EditorUtility.SetDirty(flow);
        if (flow.Board != null) EditorUtility.SetDirty(flow.Board);
    }

    // ==================== UI Canvas ====================
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

        // Main Menu
        var mm = MkPanel(canvasObj.transform, "MainMenuPanel", new Color(0.05f,0.05f,0.1f,0.95f));
        uiMgr.MainMenuPanel = mm;
        MkText(mm.transform, "TitleText", "LIFE GAME", 72, new Vector2(0,100), Color.cyan);
        MkText(mm.transform, "SubTitle", "Ren Sheng Mo Ni Qi", 28, new Vector2(0,30), new Color(0.7f,0.7f,0.8f));
        MkBtn(mm.transform, "StartButton", "Start New Life", new Vector2(0,-80), new Vector2(300,60), new Color(0.2f,0.7f,0.3f));
        MkText(mm.transform, "VersionText", "v0.2 - 2D Board Edition", 16, new Vector2(0,-200), new Color(0.4f,0.4f,0.4f));

        var bp = MkPanel(canvasObj.transform, "BoardGamePanel", new Color(0,0,0,0)); bp.SetActive(false);
        uiMgr.BoardGamePanel = bp;

        var dp = MkPanel(canvasObj.transform, "DicePanel", new Color(0,0,0,0)); dp.SetActive(false);
        uiMgr.DicePanel = dp;
        MkBtn(dp.transform, "RollDiceBtn", "Roll [Space]", new Vector2(0,-420), new Vector2(240,60), new Color(0.8f,0.2f,0.2f));
        MkText(dp.transform, "DiceResultText", "", 52, new Vector2(0,-330), Color.yellow);

        var sp = MkPanel(canvasObj.transform, "SpeedChoicePanel", new Color(0,0,0,0.85f)); sp.SetActive(false);
        uiMgr.SpeedChoicePanel = sp;
        MkText(sp.transform, "SpeedTitle", "Choose Life Pace", 40, new Vector2(0,80), Color.white);
        MkBtn(sp.transform, "SlowBtn", "Slow (1-3)", new Vector2(-160,-20), new Vector2(220,55), new Color(0.3f,0.6f,0.9f));
        MkBtn(sp.transform, "FastBtn", "Fast (3-6)", new Vector2(160,-20), new Vector2(220,55), new Color(0.9f,0.5f,0.2f));

        var ip = new GameObject("PlayerInfoPanel");
        ip.transform.SetParent(canvasObj.transform, false);
        var ipR = ip.AddComponent<RectTransform>();
        ipR.anchorMin = new Vector2(0,1); ipR.anchorMax = new Vector2(0.3f,1);
        ipR.pivot = new Vector2(0,1); ipR.anchoredPosition = new Vector2(10,-10); ipR.sizeDelta = new Vector2(0,130);
        ip.AddComponent<Image>().color = new Color(0,0,0,0.7f);
        ip.SetActive(false);
        uiMgr.PlayerInfoPanel = ip;
        MkText(ip.transform, "NameText", "Name: ---", 20, new Vector2(100,-15), Color.white, TextAnchor.MiddleLeft);
        MkText(ip.transform, "AgeText", "Age: 0", 20, new Vector2(100,-40), Color.cyan, TextAnchor.MiddleLeft);
        MkText(ip.transform, "GoldText", "Gold: 0", 20, new Vector2(100,-65), Color.yellow, TextAnchor.MiddleLeft);
        MkText(ip.transform, "KarmaText", "Karma: 0", 20, new Vector2(100,-90), Color.green, TextAnchor.MiddleLeft);

        var ep = MkPanel(canvasObj.transform, "EventDialogPanel", new Color(0,0,0,0.88f)); ep.SetActive(false);
        uiMgr.EventDialogPanel = ep;
        MkText(ep.transform, "EventTitle", "Event", 38, new Vector2(0,150), Color.yellow);
        MkText(ep.transform, "EventDesc", "...", 24, new Vector2(0,40), Color.white);
        MkBtn(ep.transform, "Choice1Btn", "Choice 1", new Vector2(0,-60), new Vector2(400,50), new Color(0.3f,0.6f,0.3f));
        MkBtn(ep.transform, "Choice2Btn", "Choice 2", new Vector2(0,-125), new Vector2(400,50), new Color(0.6f,0.3f,0.3f));
        MkBtn(ep.transform, "ContinueBtn", "Continue", new Vector2(0,-200), new Vector2(220,50), new Color(0.4f,0.4f,0.5f));

        var dth = MkPanel(canvasObj.transform, "DeathPanel", new Color(0,0,0,0.95f)); dth.SetActive(false);
        uiMgr.DeathPanel = dth;
        MkText(dth.transform, "DeathTitle", "End of Life", 52, new Vector2(0,180), new Color(0.8f,0.2f,0.2f));
        MkText(dth.transform, "DeathAge", "Age: --", 28, new Vector2(0,100), Color.white);
        MkText(dth.transform, "DeathGold", "Gold: --", 24, new Vector2(0,60), Color.yellow);
        MkText(dth.transform, "DeathKarma", "Karma: --", 24, new Vector2(0,25), Color.green);
        MkText(dth.transform, "RealmText", "Destination: ???", 32, new Vector2(0,-40), Color.cyan);
        MkBtn(dth.transform, "ReincarnateBtn", "Reincarnate", new Vector2(-130,-140), new Vector2(220,55), new Color(0.6f,0.3f,0.8f));
        MkBtn(dth.transform, "MainMenuBtn", "Main Menu", new Vector2(130,-140), new Vector2(220,55), new Color(0.4f,0.4f,0.4f));

        var cg = MkPanel(canvasObj.transform, "CGPanel", new Color(0,0,0,1)); cg.SetActive(false);
        uiMgr.CGPanel = cg;
        MkText(cg.transform, "CGText", "", 28, Vector2.zero, Color.white);

        Undo.RegisterCreatedObjectUndo(canvasObj, "UI");
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