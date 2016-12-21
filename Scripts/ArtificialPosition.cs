using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ArtificialPosition : MonoBehaviour
{
    #region Private Fields

    [SerializeField]
    private int layer;

    [SerializeField]
    private Vector2 offset;

    [SerializeField]
    private Vector2 position;

    #endregion Private Fields

    #region Public Properties

    /// <summary>
    /// Layer the object inhabits.
    /// </summary>
    public int Layer
    {
        get { return layer; }
        set { layer = value; }
    }

    /// <summary>
    /// Offset of the object in contrast to world space.
    /// </summary>
    public Vector2 Offset
    {
        get { return offset; }
        set { offset = value; }
    }

    /// <summary>
    /// Modified position of the object.
    /// </summary>
    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    #endregion Public Properties

    #region Private Methods

    /// <summary>
    /// Called once per frame.
    /// </summary>
    private void Update()
    {
        if (Application.isPlaying)
            Destroy(this);
        else if (Selection.activeGameObject == this.gameObject)
        {
            position = offset + (Vector2)transform.position;
        }
    }

    #endregion Private Methods
}