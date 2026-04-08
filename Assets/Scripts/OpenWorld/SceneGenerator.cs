using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural scene generator for open world grid cells
/// Generates different scene templates based on SceneId/age phase
/// 
/// Templates: city, school, countryside, hospital, beach, home, park,
///            heaven, hell, dream
/// 
/// Each template places ground, buildings, props, NPCs, event triggers
/// All using primitive shapes (cubes, cylinders, spheres) as placeholders
/// Replace with real models later
/// </summary>
public class SceneGenerator : MonoBehaviour
{
    public static SceneGenerator Instance { get; private set; }

    [Header("NPC Shadow Material")]
    public Material ShadowMaterial; // shared black material for shadow NPCs

    private GameObject currentScene;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Generate a scene for the given sceneId and age
    /// </summary>
    public GameObject Generate(string sceneId, int age)
    {
        // Destroy previous scene
        if (currentScene != null) Destroy(currentScene);

        currentScene = new GameObject("GeneratedScene_" + sceneId);

        switch (sceneId)
        {
            case "home_baby":
            case "home_child":
            case "home_teen":
            case "home_parents":
            case "city_home":
            case "old_home":
                BuildHome(currentScene.transform, age);
                break;
            case "kindergarten":
            case "school_primary":
            case "school_middle":
            case "school_high":
            case "university":
                BuildSchool(currentScene.transform, age);
                break;
            case "city_office":
            case "city_startup":
            case "city_apartment":
            case "city_suburb":
            case "city_park":
                BuildCity(currentScene.transform, age);
                break;
            case "hospital":
                BuildHospital(currentScene.transform, age);
                break;
            case "countryside":
                BuildCountryside(currentScene.transform, age);
                break;
            case "beach_town":
            case "travel":
                BuildBeach(currentScene.transform, age);
                break;
            case "restaurant":
            case "tea_house":
            case "market":
            case "community_center":
            case "town_shop":
            case "driving_school":
                BuildTown(currentScene.transform, age);
                break;
            case "wedding_hall":
                BuildWeddingHall(currentScene.transform, age);
                break;
            case "park_garden":
                BuildPark(currentScene.transform, age);
                break;
            case "dream":
                BuildDream(currentScene.transform, age);
                break;
            default:
                BuildCity(currentScene.transform, age);
                break;
        }

        // Add lighting based on age phase
        AddSceneLighting(currentScene.transform, age);

        Debug.Log($"[SceneGen] Generated: {sceneId} (age {age})");
        return currentScene;
    }

    /// <summary>
    /// Generate Heaven world
    /// </summary>
    public GameObject GenerateHeaven()
    {
        var scene = new GameObject("Heaven");
        BuildHeaven(scene.transform);
        return scene;
    }

    /// <summary>
    /// Generate Hell world
    /// </summary>
    public GameObject GenerateHell()
    {
        var scene = new GameObject("Hell");
        BuildHell(scene.transform);
        return scene;
    }

    public void ClearScene()
    {
        if (currentScene != null) Destroy(currentScene);
        currentScene = null;
    }
    // ==================== Scene Templates ====================

    void BuildHome(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.6f, 0.5f, 0.4f), 20);

        // House
        MakeBox(parent, "House", new Vector3(0, 2, 5), new Vector3(8, 4, 6), new Color(0.8f, 0.75f, 0.65f));
        MakeBox(parent, "Roof", new Vector3(0, 4.5f, 5), new Vector3(9, 1, 7), new Color(0.6f, 0.2f, 0.15f));
        MakeBox(parent, "Door", new Vector3(0, 1, 2.1f), new Vector3(1.2f, 2.2f, 0.1f), new Color(0.4f, 0.25f, 0.1f));

        // Furniture inside
        MakeBox(parent, "Table", new Vector3(2, 0.5f, 5), new Vector3(2, 1, 1.5f), new Color(0.5f, 0.35f, 0.2f));
        MakeBox(parent, "Bed", new Vector3(-2, 0.4f, 7), new Vector3(2, 0.8f, 3), new Color(0.9f, 0.9f, 0.95f));

        // Yard
        MakeCylinder(parent, "Tree", new Vector3(6, 2, 3), new Vector3(0.5f, 2, 0.5f), new Color(0.3f, 0.5f, 0.2f));
        MakeSphere(parent, "TreeTop", new Vector3(6, 4.5f, 3), 1.5f, new Color(0.2f, 0.6f, 0.15f));

        // NPCs
        SpawnNPC(parent, "Family Member", new Vector3(2, 0, 4), age <= 12);
        SpawnNPC(parent, "Neighbor", new Vector3(8, 0, 0), false);

        // Exit point
        SpawnExit(parent, new Vector3(-8, 0, 0), "Leave home");
    }

    void BuildSchool(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.5f, 0.55f, 0.5f), 30);

        // Main building
        MakeBox(parent, "SchoolBuilding", new Vector3(0, 3, 8), new Vector3(16, 6, 10), new Color(0.85f, 0.85f, 0.8f));
        MakeBox(parent, "SchoolRoof", new Vector3(0, 6.5f, 8), new Vector3(17, 1, 11), new Color(0.3f, 0.3f, 0.5f));

        // Playground
        MakeBox(parent, "Court", new Vector3(0, 0.05f, -5), new Vector3(10, 0.1f, 8), new Color(0.7f, 0.3f, 0.2f));
        MakeCylinder(parent, "Flagpole", new Vector3(-8, 3, 0), new Vector3(0.1f, 3, 0.1f), Color.gray);

        // Gate
        MakeBox(parent, "Gate", new Vector3(0, 1.5f, -10), new Vector3(4, 3, 0.3f), new Color(0.4f, 0.4f, 0.45f));

        // NPCs
        SpawnNPC(parent, "Teacher", new Vector3(3, 0, 6), true);
        SpawnNPC(parent, "Classmate", new Vector3(-3, 0, -3), false);
        SpawnNPC(parent, "Classmate", new Vector3(5, 0, -4), false);

        SpawnExit(parent, new Vector3(0, 0, -14), "Leave school");
    }

    void BuildCity(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.35f, 0.35f, 0.38f), 40);

        // Road
        MakeBox(parent, "Road", new Vector3(0, 0.02f, 0), new Vector3(6, 0.05f, 40), new Color(0.25f, 0.25f, 0.28f));

        // Buildings along road
        for (int i = 0; i < 6; i++)
        {
            float side = (i % 2 == 0) ? -8 : 8;
            float z = -15 + i * 6;
            float h = Random.Range(4f, 12f);
            float w = Random.Range(3f, 6f);
            Color c = new Color(Random.Range(0.5f, 0.85f), Random.Range(0.5f, 0.85f), Random.Range(0.5f, 0.85f));
            MakeBox(parent, "Building_" + i, new Vector3(side, h/2, z), new Vector3(w, h, w), c);

            // Windows
            for (int wy = 1; wy < (int)(h/2); wy++)
            {
                MakeBox(parent, "Window", new Vector3(side + (side > 0 ? -w/2-0.05f : w/2+0.05f), wy*2, z),
                    new Vector3(0.1f, 0.8f, 0.8f), new Color(0.7f, 0.85f, 1f));
            }
        }

        // Street lamp
        MakeCylinder(parent, "Lamp", new Vector3(3.5f, 2, 0), new Vector3(0.1f, 2, 0.1f), Color.gray);
        MakeSphere(parent, "LampLight", new Vector3(3.5f, 4.2f, 0), 0.3f, new Color(1f, 0.95f, 0.7f));

        // NPCs
        SpawnNPC(parent, "Colleague", new Vector3(2, 0, 5), false);
        SpawnNPC(parent, "Stranger", new Vector3(-2, 0, -8), false);
        SpawnNPC(parent, "Boss", new Vector3(7, 0, 10), age >= 24);

        SpawnExit(parent, new Vector3(0, 0, -20), "Leave the city");
    }

    void BuildHospital(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.7f, 0.7f, 0.72f), 25);

        // Hospital building
        MakeBox(parent, "Hospital", new Vector3(0, 4, 5), new Vector3(14, 8, 10), new Color(0.95f, 0.95f, 0.95f));
        MakeBox(parent, "RedCross", new Vector3(0, 7, 0.1f), new Vector3(2, 2, 0.1f), new Color(0.9f, 0.15f, 0.15f));

        // Ambulance
        MakeBox(parent, "Ambulance", new Vector3(-6, 1, -2), new Vector3(4, 2, 2), Color.white);
        MakeBox(parent, "AmbLight", new Vector3(-6, 2.2f, -2), new Vector3(0.5f, 0.3f, 0.5f), Color.red);

        // Bench
        MakeBox(parent, "Bench", new Vector3(5, 0.3f, -3), new Vector3(2, 0.6f, 0.8f), new Color(0.4f, 0.3f, 0.2f));

        SpawnNPC(parent, "Doctor", new Vector3(2, 0, 3), true);
        SpawnNPC(parent, "Nurse", new Vector3(-2, 0, 3), false);
        SpawnNPC(parent, "Patient", new Vector3(5, 0, -3), false);

        SpawnExit(parent, new Vector3(0, 0, -12), "Leave hospital");
    }

    void BuildCountryside(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.35f, 0.55f, 0.25f), 50);

        // Fields
        MakeBox(parent, "Field1", new Vector3(-10, 0.1f, 5), new Vector3(12, 0.2f, 10), new Color(0.6f, 0.7f, 0.2f));
        MakeBox(parent, "Field2", new Vector3(10, 0.1f, -5), new Vector3(10, 0.2f, 8), new Color(0.5f, 0.65f, 0.15f));

        // Trees
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(-20f, 20f);
            float z = Random.Range(-20f, 20f);
            float h = Random.Range(2f, 4f);
            MakeCylinder(parent, "Tree_" + i, new Vector3(x, h/2, z), new Vector3(0.3f, h/2, 0.3f), new Color(0.4f, 0.3f, 0.15f));
            MakeSphere(parent, "TreeTop_" + i, new Vector3(x, h+1, z), Random.Range(1f, 2f), new Color(0.15f, Random.Range(0.4f, 0.7f), 0.1f));
        }

        // River
        MakeBox(parent, "River", new Vector3(0, 0.05f, -15), new Vector3(4, 0.1f, 30), new Color(0.2f, 0.4f, 0.7f));

        // Small house
        MakeBox(parent, "Cottage", new Vector3(-5, 1.5f, -5), new Vector3(4, 3, 4), new Color(0.7f, 0.6f, 0.45f));

        SpawnNPC(parent, "Farmer", new Vector3(-8, 0, 5), false);
        SpawnNPC(parent, "Old Friend", new Vector3(5, 0, 0), false);

        SpawnExit(parent, new Vector3(20, 0, 0), "Leave countryside");
    }

    void BuildBeach(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.85f, 0.8f, 0.6f), 40); // sand

        // Ocean
        MakeBox(parent, "Ocean", new Vector3(0, -0.3f, 15), new Vector3(60, 0.5f, 30), new Color(0.15f, 0.35f, 0.7f));

        // Palm trees
        for (int i = 0; i < 4; i++)
        {
            float x = Random.Range(-12f, 12f);
            MakeCylinder(parent, "Palm_" + i, new Vector3(x, 2.5f, Random.Range(-5f, 5f)), new Vector3(0.3f, 2.5f, 0.3f), new Color(0.5f, 0.35f, 0.15f));
            MakeSphere(parent, "PalmTop_" + i, new Vector3(x, 5.5f, Random.Range(-5f, 5f)), 1.5f, new Color(0.1f, 0.5f, 0.1f));
        }

        // Beach umbrella
        MakeCylinder(parent, "Umbrella", new Vector3(3, 1.5f, -3), new Vector3(0.1f, 1.5f, 0.1f), Color.gray);
        MakeSphere(parent, "UmbrellaTop", new Vector3(3, 3.2f, -3), 1.5f, new Color(0.9f, 0.3f, 0.2f));

        SpawnNPC(parent, "Traveler", new Vector3(-5, 0, 0), false);
        SpawnExit(parent, new Vector3(-15, 0, -5), "Leave beach");
    }
    void BuildTown(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.45f, 0.42f, 0.4f), 25);
        MakeBox(parent, "Shop", new Vector3(-4, 2, 3), new Vector3(6, 4, 5), new Color(0.8f, 0.7f, 0.5f));
        MakeBox(parent, "ShopSign", new Vector3(-4, 4.3f, 0.6f), new Vector3(4, 0.8f, 0.1f), new Color(0.9f, 0.2f, 0.15f));
        MakeBox(parent, "Shop2", new Vector3(5, 1.5f, 3), new Vector3(5, 3, 4), new Color(0.7f, 0.75f, 0.8f));
        MakeBox(parent, "Road", new Vector3(0, 0.02f, -2), new Vector3(20, 0.05f, 4), new Color(0.3f, 0.3f, 0.32f));
        SpawnNPC(parent, "Shopkeeper", new Vector3(-3, 0, 1), false);
        SpawnNPC(parent, "Local", new Vector3(4, 0, -1), false);
        SpawnExit(parent, new Vector3(12, 0, -2), "Leave town");
    }

    void BuildWeddingHall(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.8f, 0.75f, 0.7f), 20);
        MakeBox(parent, "Hall", new Vector3(0, 3, 5), new Vector3(12, 6, 8), new Color(0.95f, 0.9f, 0.85f));
        MakeBox(parent, "Arch", new Vector3(0, 3, 0), new Vector3(4, 4, 0.3f), new Color(0.9f, 0.85f, 0.8f));
        // Red carpet
        MakeBox(parent, "Carpet", new Vector3(0, 0.05f, 2), new Vector3(2, 0.1f, 8), new Color(0.8f, 0.1f, 0.1f));
        SpawnNPC(parent, "Partner", new Vector3(0, 0, 4), true);
        SpawnNPC(parent, "Guest", new Vector3(3, 0, 2), false);
        SpawnNPC(parent, "Guest", new Vector3(-3, 0, 2), false);
        SpawnExit(parent, new Vector3(0, 0, -8), "Leave");
    }

    void BuildPark(Transform parent, int age)
    {
        MakeGround(parent, new Color(0.3f, 0.55f, 0.25f), 30);
        // Pond
        MakeSphere(parent, "Pond", new Vector3(5, -0.2f, 5), 4, new Color(0.2f, 0.4f, 0.65f));
        // Benches
        MakeBox(parent, "Bench1", new Vector3(-3, 0.3f, 0), new Vector3(2, 0.6f, 0.8f), new Color(0.45f, 0.3f, 0.15f));
        MakeBox(parent, "Bench2", new Vector3(3, 0.3f, -5), new Vector3(2, 0.6f, 0.8f), new Color(0.45f, 0.3f, 0.15f));
        // Trees
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(-12f, 12f); float z = Random.Range(-12f, 12f);
            MakeCylinder(parent, "Tree_" + i, new Vector3(x, 1.5f, z), new Vector3(0.25f, 1.5f, 0.25f), new Color(0.4f, 0.3f, 0.15f));
            MakeSphere(parent, "Top_" + i, new Vector3(x, 3.5f, z), 1.2f, new Color(0.15f, 0.55f, 0.1f));
        }
        SpawnNPC(parent, "Elder", new Vector3(-3, 0, 0), false);
        SpawnNPC(parent, "Child", new Vector3(8, 0, 3), false);
        SpawnExit(parent, new Vector3(-14, 0, 0), "Leave park");
    }

    void BuildDream(Transform parent, int age)
    {
        // Surreal dreamscape
        MakeGround(parent, new Color(0.3f, 0.2f, 0.4f), 40);
        // Floating objects
        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(-15f, 15f); float y = Random.Range(2f, 8f); float z = Random.Range(-15f, 15f);
            float s = Random.Range(0.5f, 2f);
            Color c = new Color(Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f), 0.7f);
            if (i % 3 == 0) MakeSphere(parent, "DreamObj_" + i, new Vector3(x, y, z), s, c);
            else MakeBox(parent, "DreamObj_" + i, new Vector3(x, y, z), new Vector3(s, s, s), c);
        }
        // Childhood river
        MakeBox(parent, "River", new Vector3(0, 0.1f, 0), new Vector3(3, 0.2f, 30), new Color(0.3f, 0.5f, 0.8f));
        SpawnNPC(parent, "Memory", new Vector3(5, 0, 0), true);
        SpawnExit(parent, new Vector3(0, 0, 15), "Wake up");
    }

    // ==================== Afterlife Scenes ====================

    void BuildHeaven(Transform parent)
    {
        // White/gold ground, clouds, bright
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "HeavenGround";
        ground.transform.SetParent(parent);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(8, 1, 8);
        SetColor(ground, new Color(0.95f, 0.95f, 1f));

        // Cloud platforms
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(-25f, 25f); float z = Random.Range(-25f, 25f); float y = Random.Range(0.5f, 3f);
            var cloud = MakeSphere(parent, "Cloud_" + i, new Vector3(x, y, z), Random.Range(2f, 5f), new Color(1, 1, 1, 0.8f));
        }

        // Golden gate
        MakeBox(parent, "GateL", new Vector3(-3, 3, 15), new Vector3(0.5f, 6, 0.5f), new Color(0.9f, 0.8f, 0.3f));
        MakeBox(parent, "GateR", new Vector3(3, 3, 15), new Vector3(0.5f, 6, 0.5f), new Color(0.9f, 0.8f, 0.3f));
        MakeBox(parent, "GateTop", new Vector3(0, 6.2f, 15), new Vector3(7, 0.5f, 0.5f), new Color(0.9f, 0.8f, 0.3f));

        // Light pillar
        MakeCylinder(parent, "LightPillar", new Vector3(0, 10, 0), new Vector3(1, 10, 1), new Color(1, 1, 0.8f, 0.3f));

        // Heaven NPCs (angels/spirits - important, shown as true form)
        SpawnAfterlifeNPC(parent, "Guardian Angel", new Vector3(0, 0, 12), WorldRealm.Heaven);
        SpawnAfterlifeNPC(parent, "Peaceful Spirit", new Vector3(8, 0, 5), WorldRealm.Heaven);

        // Reincarnation portal
        SpawnAfterlifeExit(parent, new Vector3(0, 0, -15), "Step into the light of rebirth", WorldRealm.Heaven);

        // Light
        var lt = new GameObject("HeavenLight"); lt.transform.SetParent(parent);
        var l = lt.AddComponent<Light>(); l.type = LightType.Directional;
        l.color = new Color(1, 0.98f, 0.9f); l.intensity = 1.5f;
        lt.transform.rotation = Quaternion.Euler(30, 0, 0);
    }

    void BuildHell(Transform parent)
    {
        // Dark red ground, fire pillars
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "HellGround";
        ground.transform.SetParent(parent);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(8, 1, 8);
        SetColor(ground, new Color(0.15f, 0.05f, 0.05f));

        // Lava pools
        for (int i = 0; i < 5; i++)
        {
            float x = Random.Range(-20f, 20f); float z = Random.Range(-20f, 20f);
            MakeSphere(parent, "Lava_" + i, new Vector3(x, -0.1f, z), Random.Range(2f, 4f), new Color(0.9f, 0.3f, 0.05f));
        }

        // Fire pillars
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(-15f, 15f); float z = Random.Range(-15f, 15f);
            MakeCylinder(parent, "FirePillar_" + i, new Vector3(x, 3, z), new Vector3(0.5f, 3, 0.5f), new Color(0.8f, 0.2f, 0.05f));
        }

        // Broken gate
        MakeBox(parent, "BrokenGateL", new Vector3(-4, 2, 15), new Vector3(0.8f, 5, 0.8f), new Color(0.2f, 0.15f, 0.1f));
        MakeBox(parent, "BrokenGateR", new Vector3(3, 1.5f, 15), new Vector3(0.8f, 3, 0.8f), new Color(0.2f, 0.15f, 0.1f));

        // Hell NPCs
        SpawnAfterlifeNPC(parent, "Tormented Soul", new Vector3(5, 0, 5), WorldRealm.Hell);
        SpawnAfterlifeNPC(parent, "Gatekeeper", new Vector3(0, 0, 12), WorldRealm.Hell);

        // Reincarnation portal
        SpawnAfterlifeExit(parent, new Vector3(0, 0, -15), "Crawl toward the faint light", WorldRealm.Hell);

        // Dim red light
        var lt = new GameObject("HellLight"); lt.transform.SetParent(parent);
        var l = lt.AddComponent<Light>(); l.type = LightType.Directional;
        l.color = new Color(0.8f, 0.2f, 0.1f); l.intensity = 0.6f;
        lt.transform.rotation = Quaternion.Euler(60, 30, 0);
    }
    // ==================== Lighting by Age ====================

    void AddSceneLighting(Transform parent, int age)
    {
        var ltObj = new GameObject("SceneLight");
        ltObj.transform.SetParent(parent);
        var lt = ltObj.AddComponent<Light>();
        lt.type = LightType.Directional;
        lt.shadows = LightShadows.Soft;

        if (age <= 12)
        {
            // Childhood: warm, bright, golden hour
            lt.color = new Color(1f, 0.9f, 0.7f);
            lt.intensity = 1.3f;
            ltObj.transform.rotation = Quaternion.Euler(35, -20, 0);
        }
        else if (age <= 17)
        {
            // Youth: clear, slightly cool
            lt.color = new Color(0.9f, 0.95f, 1f);
            lt.intensity = 1.2f;
            ltObj.transform.rotation = Quaternion.Euler(45, 10, 0);
        }
        else if (age <= 30)
        {
            // Young adult: neutral, bright
            lt.color = new Color(1f, 0.98f, 0.95f);
            lt.intensity = 1.1f;
            ltObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
        else if (age <= 50)
        {
            // Prime: slightly harsh
            lt.color = new Color(1f, 0.95f, 0.9f);
            lt.intensity = 1.0f;
            ltObj.transform.rotation = Quaternion.Euler(55, -40, 0);
        }
        else if (age <= 65)
        {
            // Middle: warm sunset tones
            lt.color = new Color(1f, 0.85f, 0.65f);
            lt.intensity = 0.9f;
            ltObj.transform.rotation = Quaternion.Euler(25, -50, 0);
        }
        else
        {
            // Elder: dim, soft, twilight
            lt.color = new Color(0.8f, 0.75f, 0.85f);
            lt.intensity = 0.6f;
            ltObj.transform.rotation = Quaternion.Euler(15, -60, 0);
        }
    }

    // ==================== Primitive Helpers ====================

    GameObject MakeGround(Transform parent, Color color, float size)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
        g.name = "Ground";
        g.transform.SetParent(parent);
        g.transform.localPosition = Vector3.zero;
        g.transform.localScale = new Vector3(size / 10f, 1, size / 10f);
        SetColor(g, color);
        return g;
    }

    GameObject MakeBox(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
    {
        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = name;
        b.transform.SetParent(parent);
        b.transform.localPosition = pos;
        b.transform.localScale = scale;
        SetColor(b, color);
        return b;
    }

    GameObject MakeSphere(Transform parent, string name, Vector3 pos, float radius, Color color)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.name = name;
        s.transform.SetParent(parent);
        s.transform.localPosition = pos;
        s.transform.localScale = Vector3.one * radius;
        SetColor(s, color);
        return s;
    }

    GameObject MakeCylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
    {
        var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        c.name = name;
        c.transform.SetParent(parent);
        c.transform.localPosition = pos;
        c.transform.localScale = scale;
        SetColor(c, color);
        return c;
    }

    void SetColor(GameObject obj, Color color)
    {
        var r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            r.material = mat;
        }
    }

    // ==================== NPC Spawning ====================

    void SpawnNPC(Transform parent, string npcName, Vector3 pos, bool isImportant)
    {
        var npcObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcObj.name = "NPC_" + npcName;
        npcObj.transform.SetParent(parent);
        npcObj.transform.localPosition = pos;
        npcObj.transform.localScale = new Vector3(0.5f, 1, 0.5f);

        // Shadow appearance by default (black)
        SetColor(npcObj, Color.black);

        // Add components
        var npc = npcObj.AddComponent<NPC>();
        npc.NpcName = npcName;
        npc.GreetingText = "...";

        var reveal = npcObj.AddComponent<NPCReveal>();
        reveal.IsImportantNPC = isImportant;
        reveal.ShadowColor = Color.black;

        // Add trigger collider for interaction
        var col = npcObj.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        var bc = npcObj.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(3, 3, 3);
    }

    void SpawnExit(Transform parent, Vector3 pos, string exitText)
    {
        var exitObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        exitObj.name = "Exit";
        exitObj.transform.SetParent(parent);
        exitObj.transform.localPosition = pos;
        exitObj.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
        SetColor(exitObj, new Color(0.2f, 0.8f, 0.2f));

        var et = exitObj.AddComponent<EventTrigger>();
        et.EventTitle = exitText;
        et.EventDescription = "Leave this place and return to the board.";
        et.IsExitPoint = true;

        var col = exitObj.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        var bc = exitObj.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(3, 3, 3);
    }

    void SpawnAfterlifeNPC(Transform parent, string name, Vector3 pos, WorldRealm realm)
    {
        var npcObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcObj.name = "AfterlifeNPC_" + name;
        npcObj.transform.SetParent(parent);
        npcObj.transform.localPosition = pos;

        // Afterlife NPCs are always visible (no shadow)
        Color c = realm == WorldRealm.Heaven ? new Color(0.9f, 0.9f, 1f) : new Color(0.5f, 0.15f, 0.1f);
        SetColor(npcObj, c);

        var ae = npcObj.AddComponent<AfterlifeEvent>();
        ae.EventTitle = name;
        ae.Realm = realm;

        if (realm == WorldRealm.Heaven)
        {
            ae.EventDescription = "A peaceful spirit greets you.";
            ae.Choices = new AfterlifeChoice[]
            {
                new AfterlifeChoice { Text = "Share your life story", Result = "They listen with warmth.", KarmaBonus = 2 },
                new AfterlifeChoice { Text = "Ask about this place", Result = "They smile knowingly.", KarmaBonus = 1 }
            };
        }
        else
        {
            ae.EventDescription = "A tormented figure reaches out.";
            ae.Choices = new AfterlifeChoice[]
            {
                new AfterlifeChoice { Text = "Offer comfort", Result = "They seem slightly relieved.", KarmaBonus = 3 },
                new AfterlifeChoice { Text = "Walk away", Result = "Their cries echo behind you.", KarmaBonus = -1 }
            };
        }

        var col = npcObj.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        var bc = npcObj.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(3, 3, 3);
    }

    void SpawnAfterlifeExit(Transform parent, Vector3 pos, string text, WorldRealm realm)
    {
        var exitObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        exitObj.name = "ReincarnationPortal";
        exitObj.transform.SetParent(parent);
        exitObj.transform.localPosition = pos;
        exitObj.transform.localScale = new Vector3(2, 0.1f, 2);

        Color c = realm == WorldRealm.Heaven ? new Color(1, 1, 0.8f) : new Color(0.5f, 0.1f, 0.05f);
        SetColor(exitObj, c);

        var ae = exitObj.AddComponent<AfterlifeEvent>();
        ae.EventTitle = text;
        ae.EventDescription = "A portal to a new life.";
        ae.IsExitPoint = true;
        ae.Realm = realm;

        var col = exitObj.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        var bc = exitObj.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(4, 4, 4);
    }
}