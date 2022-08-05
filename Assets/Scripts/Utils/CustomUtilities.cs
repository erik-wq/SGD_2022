using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Utils
{
    public static class CustomUtilities
    {
        public static Vector2 GetMouseDirection(Camera mainCamera, Transform locationFrom)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            var worldPosition = mainCamera.ScreenPointToRay(mousePos);
            Plane plane = new Plane(-Vector3.forward, Vector3.zero);

            float dist;
            if (plane.Raycast(worldPosition, out dist))
            {
                Vector3 P = worldPosition.GetPoint(dist);
                return ((Vector2)P - (Vector2)locationFrom.position).normalized;
            }

            return Vector2.zero;
        }

        public static Vector2 GetMousePossition(Camera mainCamera)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            var worldPosition = mainCamera.ScreenPointToRay(mousePos);
            Plane plane = new Plane(-Vector3.forward, Vector3.zero);

            float dist;
            if (plane.Raycast(worldPosition, out dist))
            {
                Vector3 P = worldPosition.GetPoint(dist);
                return P;
            }

            return Vector2.zero;
        }
    }
}
