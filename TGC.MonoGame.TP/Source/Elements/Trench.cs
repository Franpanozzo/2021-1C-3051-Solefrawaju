﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
namespace TGC.MonoGame.TP
{
	public class Trench
	{
        public static float TrenchScale = 0.07f;
		Model[] Models;
		public Model SelectedModel;

		public float Rotation { get; set; }
		public Matrix SRT { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 Color { get; set; }
		public TrenchType Type { get; set; }
		public List<TrenchTurret> Turrets = new List<TrenchTurret>();
        public List<BoundingBox> boundingBoxes = new List<BoundingBox>();
        public List<OrientedBoundingBox> orientedBoundingBoxes = new List<OrientedBoundingBox>();
        public BoundingBox BB;
		public bool IsCurrent = false;
		
		public Trench(TrenchType t, float r)
		{
			Game = TGCGame.Instance;
			Type = t;
			Rotation = r;
		}
		
		public bool IsInTrench(BoundingSphere element)
        {
			var hit = false;
			foreach(var box in boundingBoxes)
            	hit |= box.Intersects(element);

			return hit;
        }
		public bool IsInTrench(OrientedBoundingBox element)
		{
			var hit = false;
			foreach (var box in orientedBoundingBoxes)
				hit |= box.Intersects(element);

			return hit;
        }
		public bool IsInTrench(BoundingBox element)
		{
			var hit = false;
			foreach (var box in boundingBoxes)
				hit |= box.Intersects(element);

			return hit;
        }
		TGCGame Game;
		public int SelectedIndex = 0; 
		public void Update(float elapsedTime)
        {
			var xwing = Game.Xwing;
			Turrets.ForEach(turret => turret.Update(xwing, elapsedTime));
			Turrets.RemoveAll(turret => turret.needsRemoval);

			var distance = Vector3.Distance(xwing.Position, Position);
			 
			SelectedIndex = distance < 400 ? 0 : 1;
			
			SelectedModel = Models[SelectedIndex];
		}
		/*Static elements*/
		private static Trench GetNextTrench(Trench input, float rotation)
		{
			//Si es bloque de estos y no esta alineado, entonces es un straight
			if (input.Type.Equals(TrenchType.Intersection) ||
				input.Type.Equals(TrenchType.Elbow) ||
				input.Type.Equals(TrenchType.Intersection)
				&& rotation != input.Rotation)
				return new Trench(TrenchType.Straight, rotation);
			//else
			//	return new Trench(TrenchType.Platform, rotation);
			

			Random rnd = new Random();
			int val = rnd.Next(0, 100);

			// 85% chance de que sea derecho, 15% repatirtido en CodoIzq, CodoDer, T, Interseccion.
			var straightChance = 85;
			var elbowLeftChance = 5;
			var elbowRightChance = 5;
			var tChance = 4;
			var intersectionChance = 1;
			
			if (val < straightChance)
				return new Trench(TrenchType.Straight, rotation);
			if (val < 100 - intersectionChance - tChance - elbowRightChance - elbowLeftChance)
				return new Trench(TrenchType.Elbow, LeftFromAngle(rotation));
			if (val < 100 - intersectionChance - tChance - elbowRightChance)
				return new Trench(TrenchType.Elbow, RightFromAngle(rotation));
			if (val < 100 - intersectionChance - tChance)
                return new Trench(TrenchType.T, rotation);
            if (val < 100 - intersectionChance)
                return new Trench(TrenchType.Intersection, rotation);

            return new Trench(TrenchType.Straight, 0f);
		}
		static float LeftFromAngle(float ang)
		{
			float res = ang - 90;
			if (res < 0)
				res += 360;
			return res;
        }
		static float RightFromAngle(float ang)
		{
			float res = ang + 90;
			res %= 360;
			return res;
		}
		static float OppositeFromAngle(float ang)
        {
			float res = ang + 180;
			res %= 360;
			return res;
        }

		static List<Vector3> GenLine(ref Trench[,] map, int xi, int yi, float rotation, int size)
        {
            int deltaX = 0;
			int deltaY = 0;
			int x = xi;
			int y = yi;
			int steps = 0;
			// X,Y,Rotation
			List<Vector3> pointsOfInterest = new List<Vector3>();
            // me muevo en la direccion en la que vengo, hasta el final
			switch (rotation)
            {
				case 0f:   deltaY = 1;  deltaX = 0; steps = size - yi-1;	break;
				case 90f:  deltaX = 1;  deltaY = 0; steps = size - xi-1;	break;
				case 180f: deltaY = -1; deltaX = 0; steps = yi;				break;
				case 270f: deltaX = -1; deltaY = 0; steps = xi;				break;
			}
			int prevX, prevY;
			for (int step = 0 ; step < steps; step++)
			{
				prevX = x;
				prevY = y;
				x += deltaX;
				y += deltaY;
				//Si hay algo ahi, freno (reemplazo por T? )
				
				Trench prevBlock = map[prevX, prevY];
				
				if (map[x, y] != null)
				{
					switch(map[x, y].Type)
                    {
						case TrenchType.Straight:
							map[x, y] = new Trench(TrenchType.T, rotation);
							//Debug.WriteLine("set T (from ST) at (" + x + "," + y + ") rot " + rotation);
							break;
						case TrenchType.T:
							map[x, y] = new Trench(TrenchType.Intersection, rotation);
							//Debug.WriteLine("set INT at (" + x + "," + y + ") rot " + rotation);
							break;
						case TrenchType.Elbow://check rotation when replacing

							float elbowRot = map[x, y].Rotation;
							Debug.WriteLine("set T (from E " + elbowRot + " at " + x + "," + y + " rot " + rotation + " prevR " + prevBlock.Rotation);
							
							//correctedRot = RightFromAngle(map[x, y].Rotation);
							map[x, y] = new Trench(TrenchType.T, OppositeFromAngle(elbowRot));
							break;

					}
					return pointsOfInterest;
				}

				//Obtengo el siguiente bloque (adelante en el mapa)
				Trench next = GetNextTrench(prevBlock, rotation);

				var rotDelta = next.Rotation - prevBlock.Rotation;
				var sameType = next.Type == prevBlock.Type;
				
				if (rotDelta != 0 && sameType && prevBlock.Type == TrenchType.Straight) //Rotacion incorrecta
				{
					Debug.WriteLine("fixing " + rotDelta + " at " + prevX + "," + prevY + " -> " + x + "," + y);

					next = new Trench(TrenchType.Straight, prevBlock.Rotation);
				}
				map[x, y] = next;
				//si es Interseccion o T, voy a tener dos direcciones en las que seguir dibujando (izq, der)
				if (next.Type.Equals(TrenchType.Intersection) || next.Type.Equals(TrenchType.T))
                {
					pointsOfInterest.Add(new Vector3(x, y, LeftFromAngle(rotation)));
					pointsOfInterest.Add(new Vector3(x, y, RightFromAngle(rotation)));
					poiCount++;
				}
				//si es un codo, voy a seguir en la direccion en la que este ubicado (izq O der)
				if(next.Type.Equals(TrenchType.Elbow))
				{
					//if (next.Rotation == rotation)
					//    pointsOfInterest.Add(new Vector3(x, y, LeftFromAngle(rotation)));
					//else
					//    pointsOfInterest.Add(new Vector3(x, y, RightFromAngle(rotation)));
					pointsOfInterest.Add(new Vector3(x, y, next.Rotation));
					poiCount++;
                }

				//Si no es una interseccion, no puedo seguir adelante y tengo que frenar la generacion en esta rama
				bool stop = !(next.Type.Equals(TrenchType.Straight) || next.Type.Equals(TrenchType.Intersection));
				if (stop)
					return pointsOfInterest;
				
			}
			return pointsOfInterest;

		}
		//static bool validateLine(List <Vector3> line)
  //      {

  //      }
		static void recursiveGen(List<Vector3> points, ref Trench[,] map, int size)
        {
			foreach (var point in points)
            {
				var otherPoints = GenLine(ref map, (int)point.X, (int)point.Y, (int) point.Z, size);
				recursiveGen(otherPoints, ref map, size);
			}
			//System.Diagnostics.Debug.Write(Trench.ShowMapInConsole(map, size));
		}
		static int poiCount = 0;
		public static Trench[,] GenerateMap(int size)
		{
			Trench[,] map = new Trench[size, size];

			int xi = size / 2;
			int yi = 0;

			//Cantidad de puntos de interes (codo, T, interseccion)
			var attempt = 0;
			poiCount = 0;
			var whiteSpace = 500;
			while (poiCount < 9 || whiteSpace > 300)
			{
				poiCount = 0;
				attempt++;
				ClearMap(ref map, size);
				Debug.WriteLine("----");
                map[xi, yi] = new Trench(TrenchType.Straight, 0f);
                //map[xi, yi] = new Trench(TrenchType.Elbow, 270f);
                var points = GenLine(ref map, xi, yi, 0f, size);

				recursiveGen(points, ref map, size);
				
				whiteSpace = SetPlatforms(ref map, size, false);
				
			}
			SetPlatforms(ref map, size, true);

			System.Diagnostics.Debug.WriteLine("attempts: "+ attempt+" ws: " + whiteSpace);
			return map;
		}
		static int SetPlatforms(ref Trench[,] map, int size, bool create)
        {
			int p = 0;
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					if (map[x, y] == null)
					{
						p++;
						if (create)
							map[x, y] = new Trench(TrenchType.Platform, 0f);
					}
			return p;
		}
		static void ClearMap(ref Trench[,] map, int size)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					map[x, y] = null;
		}
		public static String ShowMapInConsole(Trench[,] map, int size)
		{
			String str = "MAP\n";
			str += "  ";
			for (int i = size - 1; i >= 0; i--)
			{
				str += i + " ";
				if (i == 10)
					str += " ";
				if (i < 10)
					str += " ";
			}

			str += "\n";
			for (int y = size - 1; y >= 0; y--)
			{
				
				str += "" + y + " ";
				if (y < 10)
					str += " ";
				for (int x = size - 1; x >= 0; x--)
				{
					if(x < 10)
                    {
						//do something
                    }
					if (map[x, y] == null)
						str += "E ";
					else
						switch(map[x,y].Type)
						{
							case TrenchType.Platform: str += "█ ";break;
							case TrenchType.Intersection: str += "╬ "; break;
							case TrenchType.Straight:
								if(map[x,y].Rotation == 0f)
									str += "▲ "; 
								else if(map[x, y].Rotation == 90f)
									str += "► ";
								else if (map[x, y].Rotation == 180f)
									str += "▼ ";
								else if (map[x, y].Rotation == 270f)
									str += "◄ ";
								break;
							case TrenchType.T:
								if (map[x, y].Rotation == 0f)
									str += "┬ ";
								else if (map[x, y].Rotation == 90f)
									str += "├ ";
								else if (map[x, y].Rotation == 180f)
									str += "┴ ";
								else if (map[x, y].Rotation == 270f)
									str += "┤ ";
								break;
							case TrenchType.Elbow:
								if (map[x, y].Rotation == 0)
									str += "╝ ";
								else if (map[x, y].Rotation == 90)
									str += "╗ ";
								else if (map[x, y].Rotation == 180)
									str += "╔ ";
								else if (map[x, y].Rotation == 270)
									str += "╚ ";
								break;
							default: str += "e ";break;
						}
					str += " ";
				}
				str += "" + y+ "\n";
			}

			str += "  ";
			for (int i = size - 1; i >= 0; i--)
			{
				str += i + " ";

				if (i == 10)
					str += " ";
				if (i < 10)
					str += " ";
			}

			return str;
		}

        static Vector3[] calculateTurretDelta(float rotation, Vector3 delta1, Vector3 delta2, TrenchType type)
        {
            Vector3[] deltas = new Vector3[] { Vector3.Zero, Vector3.Zero };
            switch (rotation)
            {
                case 0f:
                    deltas[0] = new Vector3(delta1.X, delta1.Y, delta1.Z);
                    deltas[1] = new Vector3(delta2.X, delta2.Y, delta2.Z);
                    break;
                case 90f:
                    deltas[0] = new Vector3(delta1.Z, delta1.Y, -delta1.X);
                    deltas[1] = new Vector3(delta2.Z, delta2.Y, -delta2.X);
                    break;
                case 180f:
                    deltas[0] = new Vector3(-delta1.X, delta1.Y, -delta1.Z);
                    deltas[1] = new Vector3(-delta2.X, delta2.Y, -delta2.Z);
                    break;
                case 270f:
                    switch (type)
                    {
                        case TrenchType.Straight:
                            deltas[0] = new Vector3(delta1.Z, delta1.Y, -delta1.X);
                            deltas[1] = new Vector3(delta2.Z, delta2.Y, -delta2.X);
                            break;
                        case TrenchType.T:
                            deltas[0] = new Vector3(-delta1.Z, delta1.Y, delta1.X);
                            deltas[1] = new Vector3(-delta2.Z, delta2.Y, delta2.X);
                            break;
                        case TrenchType.Elbow:
                            deltas[0] = new Vector3(-delta1.Z, delta1.Y, delta1.X);
                            deltas[1] = new Vector3(-delta2.Z, delta2.Y, delta2.X);
                            break;
                        default:
                            deltas[0] = new Vector3(delta1.Z, delta1.Y, -delta1.X);
                            deltas[1] = new Vector3(delta2.Z, delta2.Y, -delta2.X);
                            break;
                    }
                    break;

            }
            return deltas;

        }
        
		public static void UpdateTrenches()
        {
            var Game = TGCGame.Instance;
            //Inicializo valores importantes del mapa
            float tx = 0;
            float tz = 0;
            Matrix S = Matrix.CreateScale(TrenchScale);
            Matrix R = Matrix.Identity;
            Matrix T = Matrix.CreateTranslation(new Vector3(0, -35, 0));
            float delta = 395.5f;

			Random rnd = new Random();
            for (int x = 0; x < TGCGame.MapSize; x++)
            {

                tz = 0;
				for (int z = 0; z < TGCGame.MapSize; z++)
				{
					var r = rnd.Next(0, 100);

					Trench block = Game.Map[x, z];


					block.Models = Drawer.GetModelsFromType(Game.Map[x, z].Type);

					block.Position = new Vector3(tx, 0, tz);

					var boxWidth = 175f;
					var boxDepth = 50f;
					var deltaOver2 = delta * 0.5f;

					var BBmin = block.Position - new Vector3(deltaOver2, boxDepth, deltaOver2);
					var BBmax = block.Position + new Vector3(deltaOver2, 0f, deltaOver2);

					block.BB = new BoundingBox(BBmin, BBmax);
					
					var verticalLeftBoxMin = block.Position + new Vector3(-deltaOver2, -boxDepth, -deltaOver2);
					var verticalLeftBoxMax = verticalLeftBoxMin + new Vector3(boxWidth, boxDepth, delta);
					var VerticalLeftBox = new BoundingBox(verticalLeftBoxMin, verticalLeftBoxMax);
					

					var verticalRightBoxMin = block.Position + new Vector3(deltaOver2 - boxWidth, -boxDepth, -deltaOver2);
					var verticalRightBoxMax = verticalRightBoxMin + new Vector3(boxWidth, boxDepth, delta);
					var VerticalRightBox = new BoundingBox(verticalRightBoxMin, verticalRightBoxMax);
					
					var horizontalTopBoxMin = block.Position + new Vector3(-deltaOver2, -boxDepth, deltaOver2 - boxWidth);
					var horizontalTopBoxMax = horizontalTopBoxMin + new Vector3(delta, boxDepth, boxWidth);
					var HorizontalTopBox = new BoundingBox(horizontalTopBoxMin, horizontalTopBoxMax);
					
					var horizontalBottomBoxMin = block.Position + new Vector3(-deltaOver2, -boxDepth, -deltaOver2);
					var horizontalBottomBoxMax = horizontalBottomBoxMin + new Vector3(delta, boxDepth, boxWidth);
					var HorizontalBottomBox = new BoundingBox(horizontalBottomBoxMin, horizontalBottomBoxMax);
					
					var BottomLeftBox = new BoundingBox(verticalLeftBoxMin, verticalLeftBoxMin + new Vector3(boxWidth, boxDepth, boxWidth));
					
					var TopLeftBox = new BoundingBox(horizontalTopBoxMin, horizontalTopBoxMin + new Vector3(boxWidth, boxDepth, boxWidth));
					
					var topRightMin = verticalRightBoxMin + new Vector3(0, 0, delta - boxWidth);
					var TopRightBox = new BoundingBox(topRightMin, topRightMin + new Vector3(boxWidth, boxDepth, boxWidth));
					
					var BottomRightBox = new BoundingBox(verticalRightBoxMin, verticalRightBoxMin + new Vector3(boxWidth, boxDepth, boxWidth));
					
					var VLOOB = OrientedBoundingBox.FromAABB(VerticalLeftBox);
					var VROOB = OrientedBoundingBox.FromAABB(VerticalRightBox);
					var HTOOB = OrientedBoundingBox.FromAABB(HorizontalTopBox);
					var HBOOB = OrientedBoundingBox.FromAABB(HorizontalBottomBox);
					var TLOOB = OrientedBoundingBox.FromAABB(TopLeftBox);
					var BLOOB = OrientedBoundingBox.FromAABB(BottomLeftBox);
					var TROOB = OrientedBoundingBox.FromAABB(TopRightBox);
					var BROOB = OrientedBoundingBox.FromAABB(BottomRightBox);

					Vector3[] turretDelta = new Vector3[] { Vector3.Zero, Vector3.Zero };


					//var turretDeltaY = 8f;

					switch (block.Type)
					{
						case TrenchType.Platform:
							R = Matrix.Identity;

							turretDelta = calculateTurretDelta(
								block.Rotation,
								new Vector3(77.3f, 8f, -82.5f),
								new Vector3(-76.9f, 8f, 82.6f),
								TrenchType.Platform);

							break;
						case TrenchType.Straight:
							R = Matrix.CreateRotationY(-MathHelper.PiOver2);

							turretDelta = calculateTurretDelta(
								block.Rotation,
								new Vector3(50.5f, 8f, -159f),
								new Vector3(-83.49f, 8f, -77.5f),
								TrenchType.Straight);

							if (block.Rotation == 0f || block.Rotation == 180f)
							{
                                block.boundingBoxes.Add(VerticalLeftBox);
                                block.boundingBoxes.Add(VerticalRightBox);
                                block.orientedBoundingBoxes.Add(VLOOB);
                                block.orientedBoundingBoxes.Add(VROOB);

                            }
							else
                            {
                                block.boundingBoxes.Add(HorizontalTopBox);
                                block.boundingBoxes.Add(HorizontalBottomBox);
                                block.orientedBoundingBoxes.Add(HTOOB);
                                block.orientedBoundingBoxes.Add(HBOOB);
                            }

							break;

                        case TrenchType.T:
                            R = Matrix.CreateRotationY(MathHelper.PiOver2);

                            turretDelta = calculateTurretDelta(
                               block.Rotation,
                               new Vector3(22.4f, 8f, 83f),
                               new Vector3(50.6f, 8f, -159.3f),
                               TrenchType.T);
                            switch (block.Rotation)
                            {
                                case 0f:
                                    block.boundingBoxes.Add(HorizontalTopBox);
                                    block.boundingBoxes.Add(BottomLeftBox);
                                    block.boundingBoxes.Add(BottomRightBox);
                                    block.orientedBoundingBoxes.Add(HTOOB);
                                    block.orientedBoundingBoxes.Add(BLOOB);
                                    block.orientedBoundingBoxes.Add(BROOB);

                                    break;
                                case 90f:
                                    block.boundingBoxes.Add(VerticalRightBox);
                                    block.boundingBoxes.Add(TopLeftBox);
                                    block.boundingBoxes.Add(BottomLeftBox);
                                    block.orientedBoundingBoxes.Add(VROOB);
                                    block.orientedBoundingBoxes.Add(TLOOB);
                                    block.orientedBoundingBoxes.Add(BLOOB);
                                    break;
                                case 180f:
                                    block.boundingBoxes.Add(HorizontalBottomBox);
                                    block.boundingBoxes.Add(TopLeftBox);
                                    block.boundingBoxes.Add(TopRightBox);
                                    block.orientedBoundingBoxes.Add(HBOOB);
                                    block.orientedBoundingBoxes.Add(TLOOB);
                                    block.orientedBoundingBoxes.Add(TROOB);
                                    break;
                                case 270f:
                                    block.boundingBoxes.Add(VerticalLeftBox);
                                    block.boundingBoxes.Add(TopRightBox);
                                    block.boundingBoxes.Add(BottomRightBox);
                                    block.orientedBoundingBoxes.Add(VLOOB);
                                    block.orientedBoundingBoxes.Add(TROOB);
                                    block.orientedBoundingBoxes.Add(BROOB);
                                    break;
                            }
                            break;
                        case TrenchType.Elbow:
                            R = Matrix.Identity;

                            turretDelta = calculateTurretDelta(
                                block.Rotation,
                                new Vector3(83f, 8f, 77f),
                                new Vector3(-22f, 8f, -82.3f),
                                TrenchType.Elbow);

                            switch (block.Rotation)
                            {
                                case 0f:
                                    block.boundingBoxes.Add(HorizontalTopBox);
                                    block.boundingBoxes.Add(VerticalRightBox);
                                    block.boundingBoxes.Add(BottomLeftBox);
									block.orientedBoundingBoxes.Add(HTOOB);
									block.orientedBoundingBoxes.Add(VROOB);
									block.orientedBoundingBoxes.Add(BLOOB);
									break;
                                case 90f:
                                    block.boundingBoxes.Add(HorizontalBottomBox);
                                    block.boundingBoxes.Add(VerticalLeftBox);
                                    block.boundingBoxes.Add(TopRightBox);
									block.orientedBoundingBoxes.Add(HBOOB);
									block.orientedBoundingBoxes.Add(VLOOB);
									block.orientedBoundingBoxes.Add(TROOB);
									break;
                                case 180f:
                                    block.boundingBoxes.Add(HorizontalTopBox);
                                    block.boundingBoxes.Add(VerticalLeftBox);
                                    block.boundingBoxes.Add(BottomRightBox);
									block.orientedBoundingBoxes.Add(HTOOB);
									block.orientedBoundingBoxes.Add(VLOOB);
									block.orientedBoundingBoxes.Add(BROOB);
									break;
                                case 270f:
                                    block.boundingBoxes.Add(HorizontalBottomBox);
                                    block.boundingBoxes.Add(VerticalRightBox);
                                    block.boundingBoxes.Add(TopLeftBox);
									block.orientedBoundingBoxes.Add(HBOOB);
									block.orientedBoundingBoxes.Add(VROOB);
									block.orientedBoundingBoxes.Add(TLOOB);
									break;
                            }
                            break;
                        case TrenchType.Intersection:
                            R = Matrix.Identity;

                            turretDelta = calculateTurretDelta(
                                0f,
                                new Vector3(82.4f, 8f, -145.7f),
                                new Vector3(-63.3f, 8f, 50.7f),
                                TrenchType.Intersection);

                            block.boundingBoxes.Add(TopLeftBox);
                            block.boundingBoxes.Add(BottomLeftBox);
                            block.boundingBoxes.Add(TopRightBox);
                            block.boundingBoxes.Add(BottomRightBox);
                            block.orientedBoundingBoxes.Add(TLOOB);
                            block.orientedBoundingBoxes.Add(BLOOB);
                            block.orientedBoundingBoxes.Add(TROOB);
                            block.orientedBoundingBoxes.Add(BROOB);
                            
							break;
                    }

					if(block.Type.Equals(TrenchType.Intersection))
                    	block.SRT = S * R * Matrix.CreateTranslation(block.Position) * T;
					else
						block.SRT = S * R * Matrix.CreateRotationY(MathHelper.ToRadians(block.Rotation)) *
									Matrix.CreateTranslation(block.Position) * T;

                    

                    if (r < 30) // %30 chance de tener una torre
                        block.Turrets.Add(new TrenchTurret());
                    if (r < 10) // %10 chance de tener dos
                        block.Turrets.Add(new TrenchTurret());

					var tsize = 4f;

                    int index = 0;
                    foreach (var turret in block.Turrets)
                    {
						turret.Position = block.Position + turretDelta[index];
                        turret.S = S;
                        turret.SRT = S * R * Matrix.CreateTranslation(turret.Position);
                        turret.BoundingBox = new BoundingBox(turret.Position - new Vector3(tsize), turret.Position + new Vector3(tsize));
                        //turret.BoundingBox = BoundingVolumesExtensions.CreateAABBFrom(TGCGame.TrenchTurret);
                        //turret.BoundingBox
                        index++;                        
                    }

                    tz += delta;
                }
                tx += delta;
            }
            Game.MapLimit = tz;

        }
		
		public static void UpdateCurrent()
        {
			var Game = TGCGame.Instance;
			var current = Game.Xwing.CurrentBlock;

			for (var x = 0; x < TGCGame.MapSize; x++)
				for (var z = 0; z < TGCGame.MapSize; z++)
					Game.Map[x, z].IsCurrent = false;

			Game.Map[(int)current.X, (int)current.Y].IsCurrent = true;

		}
	}

    public enum TrenchType
	{
		Platform,
		Straight,
		T,
		Intersection,
		Elbow
	}
}