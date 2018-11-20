using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController2D : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Struct
    // --------------------------------------------------------------------------------

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }



    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    public LayerMask _collisionMask;
    public BoxCollider2D _collider;
    public RaycastOrigins _raycastOrigins;

    public const float _padding = .015f;
    const  float _dstBetweenRays = .2f;
    //[HideInInspector]
    public int _horizontalRayCount;
    //[HideInInspector]
    public int _verticalRayCount;
    //[HideInInspector]
    public float _horizontalRaySpacing;
    //[HideInInspector]
    public float _verticalRaySpacing;



    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    public virtual void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    public virtual void Start()
    {
        CalculateRaySpacing();
    }

    public void CalculateRaySpacing()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(_padding * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        _horizontalRayCount = Mathf.RoundToInt(boundsHeight / _dstBetweenRays);
        _verticalRayCount = Mathf.RoundToInt(boundsWidth / _dstBetweenRays);

        _horizontalRaySpacing = boundsHeight / (_horizontalRayCount - 1);
        _verticalRaySpacing = boundsWidth / (_verticalRayCount - 1);
    }

    // To be used by child class (Controller2D)
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(_padding * -2);

        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }
}