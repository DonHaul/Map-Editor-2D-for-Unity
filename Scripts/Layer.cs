using UnityEngine;

public class Layer : MonoBehaviour
{
    #region Private Fields

    [SerializeField]
    private int id;

    [SerializeField]
    private int priority;

    #endregion Private Fields

    #region Public Constructors

    /// <summary>
    /// Constructor for the layer.
    /// </summary>
    /// <param name="priority">Priority of the layer</param>
    /// <param name="id">ID of the layer</param>
    public Layer(int priority, int id)
    {
        this.id = id;
        this.priority = priority;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <summary>
    /// The id of the layer.
    /// </summary>
    public int ID
    {
        get { return id; }
        set { id = value; }
    }

    /// <summary>
    /// The priority of the layer.
    /// </summary>
    public int Priority
    {
        get { return priority; }
        set { priority = value; }
    }

    #endregion Public Properties
}