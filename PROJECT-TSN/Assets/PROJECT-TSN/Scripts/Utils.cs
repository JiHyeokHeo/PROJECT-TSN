using UnityEngine;

public static class Utils
{
    public static Vector3 GetWorldPosToLeftFramePos(this Transform obj)
    {
        // LeftFrame center is treated as viewport (0.3, 0.5).
        // Returns the world point on the same Z plane as obj.
        Camera cam = Camera.main;
        if (cam == null || obj == null)
            return Vector3.zero;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.28125f, 0.5f, 0f));
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, obj.position.z));

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
}
