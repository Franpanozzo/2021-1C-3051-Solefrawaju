using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    public class LightCamera : Camera
    {
  
        TGCGame Game;
        public LightCamera(float aspectRatio, Vector3 position) : base(aspectRatio)
        {
            Game = TGCGame.Instance;
            Position = position;

            //FrontDirection = Vector3.Normalize(Game.Xwing.Position - Position);
            FrontDirection = new Vector3(-0.86602545f, -0.5f, 0f);
            RightDirection = Vector3.Normalize(Vector3.Cross(FrontDirection, Vector3.Up));
            UpDirection = Vector3.Normalize(Vector3.Cross(RightDirection, FrontDirection));
            
            NearPlane = 250f;
            FarPlane = 900f;
            CalculateProjection();
        }

        private void CalculateView()
        {
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }
        public void CalculateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
        }

        public float Offset;
        
        public override void Update(GameTime gameTime)
        {
            Position = Game.Xwing.Position - Vector3.Left * Offset + Vector3.Up * (150 * MathF.Tan(MathHelper.ToRadians(30)));

            CalculateView();
            //CalculateProjection();
        }       
    }
}