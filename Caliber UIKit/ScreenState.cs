using UnityEngine;

namespace GameClient.Plugins.UIKit.Scripts
{
    public struct ScreenParams
    {
        public static Transform Camera;
        
        public static ScreenParams Create()
        {
            var state = new ScreenParams();
            state.Width = Screen.width;
            state.Height = Screen.height;
            if (Camera != null)
            {
                state.CameraPosition = Camera.position;
                state.CameraRotation = Camera.rotation;
            }
            return state;
        }

        public bool Equals(ScreenParams other)
        {
            return Width == other.Width && Height == other.Height && CameraPosition == other.CameraPosition && CameraRotation == other.CameraRotation;
        }

        public float Width;
        public float Height;
        public Vector3 CameraPosition;
        public Quaternion CameraRotation;
    }
}