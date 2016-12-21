using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class MapMaker2D : EditorWindow
{
    /*this code was made by WABBLE
	 * https://www.youtube.com/channel/UCuXkaW-PS6zmJ5zO4FbiiXQ
	 *
	 * Many thanks to pp3k07 for making his map editor code available
	 * without it I wouldn't have been able to make this tool
	 *
	 */

    #region Public Fields

    public static Layer activeLayer;
    public static int currentLayer;

    //?
    public static bool eraseTool, mouseDown;

    //onScreenGizmos
    public static GameObject gizmoCursor, gizmoTile;

    public static bool overWrite;

    //rotation
    public static float rotation;

    public static bool snapping;
    public static bool toolsActive;
    public bool alreadyRotated;
    public bool areaDeletion;
    public bool areaInsertion;

    //Pos on drag
    public Vector3 beginPos;

    public int controlID;

    //CurrentTile
    public GameObject currentPrefab;

    public Vector3 endPos;

    //Holding certain keys
    public bool holdingR, holdingEscape, holdingRightMouse;

    public bool holdingTab, holdingS, holdingA;
    public Tool lastTool;
    public float layerDepthMultiplier = 0.1f;
    public int layerId;

    //mousePosition at all times
    public Vector2 mousePos;

    public bool showConsole = false;

    #endregion Public Fields

    #region Private Fields

    //One of these should be deleted
    private static List<GameObject> allPrefabs;

    private static SpriteRenderer gizmoTileRenderer;
    private static MapMaker2D instance;

    private static int selectedGridID = 0;

    private static MapMaker2D window;

    //Aligner
    private Vector2 align;

    //int indexToDelete;
    private int alignId;

    private string foldoutStr = "Alignment";

    //drag
    private bool layerChanged;

    //List with all layers
    private List<Layer> layers;

    private bool playing;

    private Vector2 scrollPos;

    //Aligner GUI
    private bool showAlign = true;

    private GUIStyle style;

    private bool switchTool;

    private int updateCounter;

    #endregion Private Fields

    #region Public Properties

    /// <summary>
    /// The singleton instance of the editor.
    /// </summary>
    public static MapMaker2D Instance
    {
        get { return instance; }
    }

    #endregion Public Properties

    #region Private Methods

    /// <summary>
    /// Changes the tile that is displayed as a gizmo
    /// </summary>
    private static void ChangeGizmoTile()
    {
        if (gizmoTile != null)
            DestroyImmediate(gizmoTile);
        if (allPrefabs != null && allPrefabs.Count > selectedGridID && allPrefabs[selectedGridID] != null)
            gizmoTile = Instantiate(allPrefabs[selectedGridID]) as GameObject;
        else
            gizmoTile = new GameObject();

        rotation = allPrefabs[selectedGridID].transform.rotation.eulerAngles.z;
        gizmoTile.name = "gizmoTile";
        //	gizmoTile.hideFlags = HideFlags.HideInHierarchy;
        if (gizmoTileRenderer == null)
            gizmoTileRenderer = gizmoTile.GetComponent<SpriteRenderer>();
        //make it transparent
        MakeObjectSemiTransparent(gizmoTile);
    }

    /// <summary>
    /// Decrements the selected current layer
    /// </summary>
    [MenuItem("Window/2D MapEditor/Decrement Layer &#d", false, 36)]
    private static void DecrementLayer()
    {
        if (instance == null)
            return;

        currentLayer--;
        Undo.RecordObject(Instance, "Snapping");
    }

    /// <summary>
    /// Increments the selected current layer
    /// </summary>
    [MenuItem("Window/2D MapEditor/Increment Layer &d", false, 35)]
    private static void IncrementLayer()
    {
        if (instance == null)
            return;

        currentLayer++;
        Undo.RecordObject(Instance, "Snapping");
    }

    /// <summary>
    /// Add menu named "My Window" to the Window menu
    /// </summary>
    [MenuItem("Window/2D MapEditor/Open Map Editor %m", false, 1)]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        window = (MapMaker2D)EditorWindow.GetWindow(typeof(MapMaker2D));
        window.Show();

        window.minSize = new Vector2(200, 315);
        window.titleContent = new GUIContent("MapMaker 2D");
    }

    /// <summary>
    /// Sets the SpriteRenderer of an object and all its children to half transparent.
    /// </summary>
    /// <param name="gameObject">The GameObject of which the SpriteRenderer should be taken from</param>
    private static void MakeObjectSemiTransparent(GameObject gameObject)
    {
        if (gameObject.GetComponent<SpriteRenderer>() != null)
        {
            Color c = gameObject.GetComponent<SpriteRenderer>().color;
            c.a = 0.5f;
            gameObject.GetComponent<SpriteRenderer>().color = c;
        }

        foreach (Transform t in gameObject.transform)
        {
            MakeObjectSemiTransparent(t.gameObject);
        }
    }

    /// <summary>
    /// Rotates the gizmo tile counter clockwise
    /// </summary>
    [MenuItem("Window/2D MapEditor/Rotate CCW #&r", false, 12)]
    private static void RotateGizmoCCW()
    {
        if (instance == null)
            return;

        rotation += 90;
        Undo.RecordObject(gizmoTile.transform, "Rotation");
        gizmoTile.transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    /// <summary>
    /// Rotates the gizmo tile clockwise
    /// </summary>
    [MenuItem("Window/2D MapEditor/Rotate CW &r", false, 12)]
    private static void RotateGizmoCW()
    {
        if (instance == null)
            return;

        rotation -= 90;
        Undo.RecordObject(gizmoTile.transform, "Rotation");
        gizmoTile.transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    /// <summary>
    /// Selects the gameobject with the given value
    /// </summary>
    /// <param name="i">The id of the object</param>
    private static void SelectGameObject(int i)
    {
        if (Instance == null)
            return;

        if (allPrefabs.Count > i)
            selectedGridID = i;

        ChangeGizmoTile();
    }

    /// <summary>
    /// Select GameObject 1
    /// </summary>
    [MenuItem("Window/2D MapEditor/Select GameObject 1 _F1")]
    private static void SelectGameObject1()
    {
        SelectGameObject(0);
    }

    /// <summary>
    /// Select GameObject 2
    /// </summary>
    [MenuItem("Window/2D MapEditor/Select GameObject 2 _F2")]
    private static void SelectGameObject2()
    {
        SelectGameObject(1);
    }

    /// <summary>
    /// Select GameObject 3
    /// </summary>
    [MenuItem("Window/2D MapEditor/Select GameObject 3 _F3")]
    private static void SelectGameObject3()
    {
        SelectGameObject(2);
    }

    /// <summary>
    /// Select GameObject 4
    /// </summary>
    [MenuItem("Window/2D MapEditor/Select GameObject 4 _F4")]
    private static void SelectGameObject4()
    {
        SelectGameObject(3);
    }

    /// <summary>
    /// Select GameObject 5
    /// </summary>
    [MenuItem("Window/2D MapEditor/Select GameObject 5 _F5")]
    private static void SelectGameObject5()
    {
        SelectGameObject(4);
    }

    /// <summary>
    /// Toggles if overwrite is enabled
    /// </summary>
    [MenuItem("Window/2D MapEditor/OverWrite &a", false, 24)]
    private static void ToggleOverWrite()
    {
        if (instance == null)
            return;

        overWrite = !overWrite;
        Undo.RecordObject(Instance, "Snapping");
        //gizmoTile.transform.rotation = Quaternion.Euler(0,0,rotation);
    }

    /// <summary>
    /// Toggles if snapping is enabled
    /// </summary>
    [MenuItem("Window/2D MapEditor/Snap &s", false, 23)]
    private static void ToggleSnapping()
    {
        if (instance == null)
            return;

        snapping = !snapping;
        Undo.RecordObject(Instance, "Snapping");
        //gizmoTile.transform.rotation = Quaternion.Euler(0,0,rotation);
    }

    private void activateTools()
    {
        Tools.current = Tool.None;
        if (gizmoTile != null)
            gizmoTile.SetActive(true);
        if (gizmoCursor != null)
            gizmoCursor.SetActive(true);
    }

    /// <summary>
    /// Adds a tile to a given layer.
    /// </summary>
    /// <param name="position">The position of the tile</param>
    /// <param name="layer">The layer to which the tile shall be added</param>
    private void AddTile(Vector2 position, int layer)
    {
        GameObject gameObject = isObjectAt(position, layer);

        if (gameObject == null)
        {
            InstantiateTile(position, layer);
        }
        else if (overWrite)
        {
            Undo.DestroyObjectImmediate(gameObject);
            DestroyImmediate(gameObject);

            InstantiateTile(position, layer);
        }
    }

    /// <summary>
    /// Helpermethod that returns a vector to a certain alignmentsystem
    /// </summary>
    /// <param name="alignIndex">The given preset for the alignment</param>
    /// <returns>Aligned Vector2</returns>
    private Vector2 alignId2Vec(int alignIndex)
    {
        Vector2 aux;

        aux.x = alignIndex % 3 - 1;
        aux.y = alignIndex / 3 - 1;

        ShowLog(aux);
        return aux;
    }

    /// <summary>
    /// Removes an area from the currentlayer.
    /// </summary>
    private void AreaDeletion()
    {
        Vector2 topLeft;
        Vector2 downRight;

        endPos = gizmoTile.transform.position;

        topLeft.y = endPos.y > beginPos.y ? endPos.y : beginPos.y;

        topLeft.x = endPos.x < beginPos.x ? beginPos.x : endPos.x;

        downRight.y = endPos.y > beginPos.y ? beginPos.y : endPos.y;

        downRight.x = endPos.x < beginPos.x ? endPos.x : beginPos.x;

        ShowLog(downRight);
        ShowLog(topLeft);

        //Goes througt all units
        for (float y = downRight.y; y <= topLeft.y; y++)
        {
            for (float x = downRight.x; x <= topLeft.x; x++)
            {
                GameObject GOtoDelete = isObjectAt(new Vector3(x, y, currentLayer * layerDepthMultiplier), currentLayer);
                //If theres something then delete it
                if (GOtoDelete != null)
                {
                    Undo.DestroyObjectImmediate(GOtoDelete);
                    DestroyImmediate(GOtoDelete);
                }
            }
        }
        ShowLog("Area Deleted");
    }

    /// <summary>
    /// Adds an area to the current layer.
    /// </summary>
    private void AreaInsertion()
    {
        Vector2 topLeft;
        Vector2 downRight;

        endPos = gizmoTile.transform.position;

        topLeft.y = endPos.y > beginPos.y ? endPos.y : beginPos.y;

        topLeft.x = endPos.x < beginPos.x ? beginPos.x : endPos.x;

        downRight.y = endPos.y > beginPos.y ? beginPos.y : endPos.y;

        downRight.x = endPos.x < beginPos.x ? endPos.x : beginPos.x;

        ShowLog(downRight);
        ShowLog(topLeft);
        for (float y = downRight.y; y <= topLeft.y; y++)
        {
            for (float x = downRight.x; x <= topLeft.x; x++)
            {
                GameObject go = isObjectAt(new Vector3(x, y, currentLayer * layerDepthMultiplier), currentLayer);

                //If there no object than create it
                if (go == null)
                {
                    InstantiateTile(new Vector3(x, y, layerDepthMultiplier), currentLayer);
                }//in this case there is go in there
                else if (overWrite)
                {
                    Undo.DestroyObjectImmediate(go);
                    DestroyImmediate(go);

                    InstantiateTile(new Vector3(x, y), currentLayer);
                }
            }
        }
        ShowLog("Area Inserted");
    }

    /// <summary>
    /// Updates layerinfo by the real gameobjects
    /// </summary>
    private void ChangeLayerStuff()
    {
        ShowLog("Number of layers: " + layers.Count);

        layers.Sort(delegate (Layer x, Layer y)
        {
            return (x.transform.GetSiblingIndex() < y.transform.GetSiblingIndex()) ? -1 : 0;
        });

        //ORDER IN INSPECTOR BUBBLE SORT IMPROVE
        for (int j = 0; j < layers.Count; j++)
        {
            for (int k = 0; k < layers.Count - 1; k++)
            {
                if (layers[k].Priority > layers[k + 1].Priority)
                {
                    int aux = layers[k].Priority;
                    layers[k].Priority = layers[k + 1].Priority;
                    layers[k + 1].Priority = aux;
                }
            }
        }

        foreach (var item in layers)
        {
            item.transform.position = Vector3.forward * layerDepthMultiplier * item.Priority;
        }

        //CheckNameStuff
        //Keep layer number in name
        foreach (var item in layers)
        {
            if (item == null)
                continue;

            Regex regex = new Regex("([0-9])");

            if (regex.IsMatch(item.gameObject.name) == true)
            {
                if (item.gameObject.name.Contains("(" + item.Priority + ")") == false)
                {
                    item.gameObject.name = item.gameObject.name.Remove(item.gameObject.name.Length - 4);
                    item.transform.name += " (" + item.Priority + ")";
                }
            }
            else
            {
                item.transform.name += " (" + item.Priority + ")";
            }
        }
    }

    /// <summary>
    /// Updates the gizmos onscreen
    /// </summary>
    private void CursorUpdate()
    {
        //Creates the if they dont already exist
        if (gizmoCursor == null)
        {
            GameObject pointer = (GameObject)Resources.Load("TilePointerGizmo", typeof(GameObject));
            if (pointer != null)
                gizmoCursor = (GameObject)Instantiate(pointer);
            else
                gizmoCursor = new GameObject();

            gizmoCursor.name = "gizmoCursor";
            //	gizmoCursor.hideFlags = HideFlags.HideInHierarchy;
            ShowLog("Cursor Created");
        }

        if (gizmoTile == null)
        {
            if (allPrefabs != null && allPrefabs.Count > 0)
                ChangeGizmoTile();
            else
                gizmoTile = new GameObject();
        }
        //position cursor in correct place
        if (gizmoCursor != null)
        {
            //check if snaping is active
            if (snapping)
            {
                Vector2 gizmoPos = Vector2.zero;
                if (mousePos.x - Mathf.Floor(mousePos.x) < 0.5f)
                {
                    gizmoPos.x = Mathf.Floor(mousePos.x) + 0.5f;
                }
                else if (Mathf.Ceil(mousePos.x) - mousePos.x < 0.5f)
                {
                    gizmoPos.x = Mathf.Ceil(mousePos.x) - 0.5f;
                }
                if (mousePos.y - Mathf.Floor(mousePos.y) < 0.5f)
                {
                    gizmoPos.y = Mathf.Floor(mousePos.y) + 0.5f;
                }
                else if (Mathf.Ceil(mousePos.y) - mousePos.y < 0.5f)
                {
                    gizmoPos.y = Mathf.Ceil(mousePos.y) - 0.5f;
                }

                gizmoCursor.transform.position = gizmoPos;
                gizmoTile.transform.position = gizmoPos + (Vector2)gizmoTile.transform.InverseTransformVector(OffsetWeirdTiles());
            }
            else
            {
                gizmoCursor.transform.position = mousePos;
                gizmoTile.transform.position = mousePos;
            }
            //Scale the scale correctly
            if (currentPrefab != null)
                gizmoTile.transform.localScale = currentPrefab.transform.localScale;
        }
    }

    /// <summary>
    /// Prints every layer into the debug console.
    /// </summary>
    private void DebugLayers()
    {
        foreach (var item in layers)
        {
            Debug.Log(item.name);
        }
    }

    /// <summary>
    /// Draws the rectangle area
    /// </summary>
    private void DrawAreaRectangle()
    {
        Vector4 area = GetAreaBounds();
        //topline
        Handles.DrawLine(new Vector3(area[3] + 0.5f, area[0] + 0.5f, 0), new Vector3(area[1] - 0.5f, area[0] + 0.5f, 0));
        //downline
        Handles.DrawLine(new Vector3(area[3] + 0.5f, area[2] - 0.5f, 0), new Vector3(area[1] - 0.5f, area[2] - 0.5f, 0));
        //leftline
        Handles.DrawLine(new Vector3(area[3] + 0.5f, area[0] + 0.5f, 0), new Vector3(area[3] + 0.5f, area[2] - 0.5f, 0));
        //rightline
        Handles.DrawLine(new Vector3(area[1] - 0.5f, area[0] + 0.5f, 0), new Vector3(area[1] - 0.5f, area[2] - 0.5f, 0));
    }

    /// <summary>
    /// Returns The game object correnspondent to a layer, null if doesnt exits
    /// </summary>
    /// <param name="currentLayer">The id of the current layer</param>
    /// <returns>The object of the current layer</returns>
    private GameObject FindLayer(int currentLayer)
    {
        bool create = true;
        GameObject layer = null;

        if (layers.Count == 0)
        {
            LoadLayers();
        }

        foreach (Layer l in Object.FindObjectsOfType<Layer>())
        {
            if (l.Priority == currentLayer)
            {
                layer = l.gameObject;
                create = false;
                break;
            }
        }

        if (create)
        {
            ShowLog("Creating New Layer");
            layer = new GameObject("Layer" + currentLayer + " (" + currentLayer + ")");
            layer.AddComponent<Layer>();
            layer.GetComponent<Layer>().Priority = currentLayer;
            layer.GetComponent<Layer>().ID = layer.transform.GetSiblingIndex();//layerId++;
            layer.transform.position = Vector3.forward * layerDepthMultiplier * currentLayer;

            //do cleanup before acessing list

            //ORDERED INSERTION
            int i;
            for (i = 0; i < layers.Count && currentLayer > layers[i].Priority; i++)
            {
            }
            layers.Insert(i, layer.GetComponent<Layer>());

            //ORDER IN INSPECTOR BUBBLE SORT IMPROVE
            for (int j = 0; j < layers.Count; j++)
            {
                for (int k = 0; k < layers.Count - 1; k++)
                {
                    if (layers[k].transform.GetSiblingIndex() > layers[k + 1].transform.GetSiblingIndex())
                    {
                        int aux = layers[k].transform.GetSiblingIndex();
                        layers[k].transform.SetSiblingIndex(layers[k + 1].transform.GetSiblingIndex());
                        layers[k + 1].transform.SetSiblingIndex(aux);
                    }
                }
            }
            //DebugLayers();
        }
        return layer;
    }

    /// <summary>
    /// Corrects the area bounds and returns them as a Vector 4.
    /// </summary>
    /// <returns>Returns a Vector4 representing the boundary.</returns>
    private Vector4 GetAreaBounds()
    {
        Vector2 topLeft;
        Vector2 downRight;

        endPos = gizmoCursor.transform.position;

        topLeft.y = endPos.y > beginPos.y ? endPos.y : beginPos.y;

        topLeft.x = endPos.x < beginPos.x ? beginPos.x : endPos.x;

        downRight.y = endPos.y > beginPos.y ? beginPos.y : endPos.y;

        downRight.x = endPos.x < beginPos.x ? endPos.x : beginPos.x;

        return new Vector4(topLeft.y, downRight.x, downRight.y, topLeft.x);
    }

    /// <summary>
    /// Instantiates a tile at a given positon on a given layer.
    /// </summary>
    /// <param name="position">The position at which the new object shall be created</param>
    /// <param name="layer">The layer on which the new object shall be created</param>
    private void InstantiateTile(Vector2 position, int layer)
    {
        if (currentPrefab == null)
            return;

        GameObject metaTile = (GameObject)Instantiate(currentPrefab);
        metaTile.transform.rotation = Quaternion.Euler(0, 0, rotation);
        metaTile.transform.SetParent(FindLayer(layer).transform);
        metaTile.transform.localPosition = (Vector3)position + metaTile.transform.InverseTransformVector(OffsetWeirdTiles());

        //IF it is a weird shape
        if (metaTile.transform.localPosition != (Vector3)position)
        {
            ArtificialPosition artPos = metaTile.AddComponent<ArtificialPosition>();
            artPos.Position = position;
            artPos.Offset = artPos.Position - (Vector2)metaTile.transform.position;
            artPos.Layer = currentLayer;
        }

        Undo.RegisterCreatedObjectUndo(metaTile, "Created go");
    }

    /// <summary>
    /// Finds object in certain position
    /// returns null if not found
    /// </summary>
    /// <param name="titlePosition">The Location of the object</param>
    /// <param name="currentLayer">The ID of the current layer</param>
    /// <returns>Returns the object at the given position. null is returned if nothing could be found.</returns>
    private GameObject isObjectAt(Vector2 titlePosition, int currentLayer)
    {
        //Goes through all gameobjects in the scene
        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;

            //Checks if they are in this position
            ArtificialPosition artPos = g.GetComponent<ArtificialPosition>();

            if (artPos == null)
            {
                if (g.transform.localPosition == (Vector3)titlePosition && (g.name != "gizmoCursor" && g.name != "gizmoTile"))
                {
                    //fixes nested prefabs error
                    //only finds prefabs that are not nested
                    if (g.transform.parent != null && g.transform.parent.GetComponent<Layer>().Priority == currentLayer)
                    {
                        if (g.transform.parent.parent == null)
                            return g;
                    }
                }
            }
            else
            {
                if (artPos.Position == titlePosition && g.name != "gizmoCursor")
                    return g;
            }
        }
        //PerformChecks for artificial
        return null;
    }

    /// <summary>
    /// Loads the layers from the objects in the scenegraph.
    /// </summary>
    private void LoadLayers()
    {
        if (layers != null)
            layers.Clear();

        foreach (var item in Object.FindObjectsOfType<Layer>())
        {
            layers.Add(item);
        }

        ShowLog("Layers Updated");
    }

    /// <summary>
    /// Loads All prefabs
    /// HAS BUG IS SHOWING CHILDFABS as well
    /// </summary>
    private void LoadPrefabs()
    {
        if (allPrefabs == null)
            allPrefabs = new List<GameObject>();

        allPrefabs.Clear();

        var loadedObjects = Resources.LoadAll("");

        foreach (var loadedObject in loadedObjects)
        {
            if (loadedObject.GetType() == typeof(GameObject))
                allPrefabs.Add(loadedObject as GameObject);
        }

        //Can Now DIsplay prefabs on Window
        if (allPrefabs != null)
            ShowLog("Imported Prefabs:" + allPrefabs.Count);
    }

    /// <summary>
    /// Offsets tiles which are at weird locations.
    /// SHOULD BE LOOKED AT AGAIN
    /// </summary>
    /// <returns></returns>
    private Vector3 OffsetWeirdTiles()
    {
        //TODO ONLY WORKS FOR ONE BIG OBJECT, instead of parent of several objects
        if (gizmoTileRenderer != null && gizmoTileRenderer.sprite != null && (gizmoTileRenderer.sprite.bounds.extents.x != 0.5f || gizmoTileRenderer.sprite.bounds.extents.y != 0.5f))
            //the -0.5f is to center it correctly
            return new Vector3(-align.x * (gizmoTileRenderer.sprite.bounds.extents.x - 0.5f), align.y * (gizmoTileRenderer.sprite.bounds.extents.y - 0.5f), 0);

        return Vector3.zero;
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed
    /// </summary>
    private void OnDestroy()
    {
        Tools.current = lastTool;
        DestroyImmediate(GameObject.Find("gizmoTile"));
        DestroyImmediate(GameObject.Find("gizmoCursor"));
        SceneView.onSceneGUIDelegate -= SceneGUI;
    }

    /// <summary>
    /// Called when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        Tools.current = lastTool;
        DestroyImmediate(GameObject.Find("gizmoTile"));
        DestroyImmediate(GameObject.Find("gizmoCursor"));
        SceneView.onSceneGUIDelegate -= SceneGUI;
    }

    /// <summary>
    /// Called when the component is enabled
    /// </summary>
    private void OnEnable()
    {
        if (instance == null) instance = this;
        //is this evenright?

        alignId = 4;

        style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.onHover.textColor = Color.blue;
        style.normal.textColor = Color.blue;
        style = new GUIStyle(style);
        layerDepthMultiplier = 0.1f;
        lastTool = Tools.current;
        selectedGridID = 0;
        switchTool = false;

        align = Vector2.zero;
        alreadyRotated = false;
        rotation = 0;
        overWrite = true;
        snapping = true;
        toolsActive = true;

        layers = new List<Layer>();

        beginPos = Vector3.zero;
        endPos = Vector3.zero;

        LoadLayers();

        SceneView.onSceneGUIDelegate += SceneGUI;
    }

    /// <summary>
    /// Happens Everytime the window is focused (clicked)
    /// </summary>
    private void OnFocus()
    {
        //Check for new prefabs
        LoadPrefabs();
        ShowLog("MapMaker Activated");
        if (Tools.current != Tool.None)
            lastTool = Tools.current;

        activateTools();
        toolsActive = true;

        if (gizmoTileRenderer == null && gizmoTile != null)
            gizmoTileRenderer = gizmoTile.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Called every frame to update the UI.
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        EditorGUILayout.LabelField("Select a prefab:");

        //If prefabs have been loaded
        if (allPrefabs != null && allPrefabs.Count > 0)
        {
            GUIContent[] content = new GUIContent[allPrefabs.Count];

            for (int i = 0; i < allPrefabs.Count; i++)
            {
                if (allPrefabs[i] != null && allPrefabs[i].name != "")
                    content[i] = new GUIContent(allPrefabs[i].name, AssetPreview.GetAssetPreview(allPrefabs[i]));

                if (content[i] == null)
                    content[i] = GUIContent.none;
            }

            //creates selection grid
            EditorGUI.BeginChangeCheck();

            //prevents from error if object are deleted by user
            while (selectedGridID >= allPrefabs.Count)
                selectedGridID--;

            selectedGridID = GUILayout.SelectionGrid(selectedGridID, content, 5, GUILayout.Height(50 * (Mathf.Ceil(allPrefabs.Count / (float)5))), GUILayout.Width(this.position.width - 30));
            if (EditorGUI.EndChangeCheck())
            {
                ChangeGizmoTile();
            }

            currentPrefab = allPrefabs[selectedGridID];
        }

        EditorGUILayout.Space();
        //undoManager.CheckUndo(instance);
        //the layer
        EditorGUI.BeginChangeCheck();

        currentLayer = EditorGUILayout.IntField("Layer", currentLayer);

        //bools
        rotation = EditorGUILayout.FloatField("Rotation", rotation);

        //Undo.RecordObject (curPrefab.transform.position, "Undone SPnapping");
        snapping = EditorGUILayout.Toggle(new GUIContent("Snapping", "Should tiles snap to the grid"), snapping);

        overWrite = EditorGUILayout.Toggle(new GUIContent("Overwrite", "Do you want to overwrite tile in the same layer and position"), overWrite);

        showConsole = EditorGUILayout.Toggle(new GUIContent("Show in Console", "Show Whats happening on the console"), showConsole);

        if (EditorGUI.EndChangeCheck())
        {
            // Code to execute if GUI.changed
            Undo.RecordObject(Instance, "Name");
            /*snapping = asnapping;
			rotation = arotation;
			curLayer = acurLayer;
			overWrite = aoverWrite;
			showConsole = ashowConsole;*/
        }

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        showAlign = EditorGUILayout.Foldout(showAlign, foldoutStr);

        if (EditorGUI.EndChangeCheck())
        {
            // Code to execute if GUI.changed
            //NOT WORKING
        }

        if (showAlign)
        {
            EditorGUI.BeginChangeCheck();

            alignId = GUILayout.SelectionGrid(alignId, new string[9], 3, GUILayout.MaxHeight(100), GUILayout.MaxWidth(100));

            if (EditorGUI.EndChangeCheck())
            {
                // Code to execute if GUI.changed
                align = alignId2Vec(alignId);
            }
        }
        //Shows the prefab
        EditorGUI.BeginChangeCheck();

        currentPrefab = (GameObject)EditorGUILayout.ObjectField("Current Prefab", currentPrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
        {
            // Code to execute if GUI.changed
            if (allPrefabs != null)
            {
                //finds prefabs in the list
                int activePre = allPrefabs.IndexOf(currentPrefab);

                if (activePre > 0)
                {
                    selectedGridID = activePre;
                    Debug.Log("JUST DO IT");
                }
                //if its not on the list, then addit to it
            }
            else
            {
                //TODO ADD IF NOT ON RESOURCES
                //allPrefabs.Add (curPrefab);
                //selGridInt = allPrefabs.Count - 1;
            }
        }

        Texture2D previewImage = AssetPreview.GetAssetPreview(currentPrefab);
        GUILayout.Box(previewImage);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.Label("Made by ");

        if (GUILayout.Button("Wabble", style))
        {
            Application.OpenURL("http://www.youtube.com/channel/UCuXkaW-PS6zmJ5zO4FbiiXQ?sub_confirmation=1");
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Called when layers should be rearranged
    /// </summary>
    private void OnHierarchyChange()
    {
        RemoveDeleteItems();

        ChangeLayerStuff();
        //DebugLayers ();
    }

    /// <summary>
    /// Removes deleted layers from the layer list.
    /// </summary>
    private void RemoveDeleteItems()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] == null)
            {
                layers.Remove(layers[i]);
                i = 0;
            }
        }
    }

    /// <summary>
    /// Removes a tile at the cursor location.
    /// </summary>
    private void RemoveTile()
    {
        GameObject GOtoDelete = isObjectAt(new Vector3(gizmoCursor.transform.position.x, gizmoCursor.transform.position.y, currentLayer * layerDepthMultiplier), currentLayer);
        Undo.DestroyObjectImmediate(GOtoDelete);
        DestroyImmediate(GOtoDelete);
    }

    /// <summary>
    /// Updates with the scene
    /// </summary>
    /// <param name="sceneView">The used sceneview.</param>
    private void SceneGUI(SceneView sceneView)
    {
        if (Application.isPlaying)
        {
            DestroyImmediate(GameObject.Find("gizmoTile"));
            DestroyImmediate(GameObject.Find("gizmoCursor"));
            toolsActive = false;
        }
        else if (playing)
        {
            DestroyImmediate(GameObject.Find("gizmoTile"));
            DestroyImmediate(GameObject.Find("gizmoCursor"));
            toolsActive = true;
        }

        playing = Application.isPlaying;

        //Creates Event
        Event e = Event.current;

        //Reactivate tool if inactive
        if (e.type == EventType.keyDown && e.keyCode == KeyCode.M)
        {
            activateTools();
            toolsActive = true;
            Tools.current = Tool.None;
        };

        if (e.type == EventType.keyDown && e.keyCode == KeyCode.Escape)
        {
            holdingEscape = true;
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            holdingEscape = true;
        }

        if (e.type == EventType.MouseUp && e.button == 1)
        {
            holdingEscape = true;
            switchTool = false;
        }

        if (e.type == EventType.keyUp && e.keyCode == KeyCode.Escape)
        {
            holdingEscape = switchTool = false;
        }

        if (holdingEscape && !switchTool)
        {
            toolsActive = !toolsActive;

            if (toolsActive)
            {
                activateTools();
            }
            else
                Tools.current = lastTool;

            switchTool = true;
        }

        //if there is a too selected, then deactivate mapmaker
        if (Tools.current != Tool.None)
            toolsActive = false;

        //getsMousePosition
        mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

        if (!toolsActive)
        {
            if (gizmoTile != null)
                gizmoTile.SetActive(false);
            if (gizmoCursor != null)
                gizmoCursor.SetActive(false);
            return;
        }

        //Sets ControlID
        controlID = GUIUtility.GetControlID(FocusType.Passive);

        //Checks whats happening
        switch (e.type)
        {
            case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;

            case EventType.mouseDown:
                if (e.button == 0) //LEFT CLICK DOWN
                {
                    mouseDown = true;
                }

                if (e.button == 1) //RIGHT CLICK DOWN
                {
                }
                break;

            case EventType.mouseUp:
                if (e.button == 0) //LEFT CLICK UP
                {
                    mouseDown = false;
                }
                if (e.button == 1) //RIGHT CLICK UP
                {
                }
                break;

            case EventType.keyDown:
                if (e.keyCode == KeyCode.M)
                {
                    toolsActive = true;
                    Tools.current = Tool.None;
                }

                if (e.keyCode == KeyCode.T)
                {
                    //HandleUtility.AddDefaultControl ();
                }
                break;

            case EventType.keyUp:
                break;
        }
        /*if (e.control && e.keyCode == KeyCode.A)
        {
         Debug.Log("Ctrl + A");
        }  */

        //Code to rotate a piece
        if (e.shift && holdingR && !alreadyRotated)
        {
            Debug.Log("RotatePiece");
            //RotateGizmo ();
            alreadyRotated = true;
        }

        //Add Single tile
        if (mouseDown && !e.shift && !areaInsertion)
        {
            if (!snapping)
                mouseDown = false;

            AddTile(gizmoCursor.transform.position, currentLayer);
        }

        //Add Multiple tiles
        if (mouseDown && e.shift && !areaInsertion && !e.control)
        {
            areaInsertion = true;
            beginPos = gizmoCursor.transform.position;
            ShowLog("StartedArea");
        }

        //Draws Rectangle
        if (areaInsertion || areaDeletion)
        {
            DrawAreaRectangle();
            SceneView.RepaintAll();
        }

        //Cancel Area insertion if shift in released
        if (mouseDown && !e.shift && areaInsertion)
            areaInsertion = false;

        //Starts AreaDeletion
        if (mouseDown && e.shift && !areaDeletion && e.control)
        {
            areaDeletion = true;
            beginPos = gizmoTile.transform.position;
            ShowLog("StartedAreaDELETION");
        }

        //Deletes Elements in that area
        if (!mouseDown && areaDeletion && e.shift && e.control)
        {
            ShowLog("AreaDELETION");
            AreaDeletion();
            areaDeletion = false;
        }

        //Intantiates elements in that area
        if (!mouseDown && areaInsertion && e.shift && !e.control)
        {
            AreaInsertion();
            areaInsertion = false;
        }

        //Removes single tile
        if (mouseDown && e.control && !areaDeletion)
        {
            RemoveTile();
        }

        //SceneView.RepaintAll ();
        CursorUpdate();

        Repaint();
    }

    /// <summary>
    /// Pushs an object into the Debug-Console.
    /// </summary>
    /// <param name="sender">Object to push</param>
    private void ShowLog(object sender)
    {
        if (showConsole)
        {
            Debug.Log(sender);
        }
    }

    #endregion Private Methods
}