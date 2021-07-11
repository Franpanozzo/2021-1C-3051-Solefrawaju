using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP
{
    public class Drawer
    {
        TGCGame Game;
        GraphicsDevice GraphicsDevice;
        ContentManager Content;
        String ContentFolder3D;
        String ContentFolderEffects;
        String ContentFolderTextures;
        //TGCGame.GmState GameState;
        #region models effects textures targets
        public Model TieModel;
        public Model XwingModel;
        private Model XwingEnginesModel;

        public static Model[] TrenchPlatform;
        public static Model[] TrenchStraight;
        public static Model[] TrenchT;
        public static Model[] TrenchIntersection;
        public static Model[] TrenchElbow;
        public static Model TrenchTurret;
        SkyBox SkyBox;
        Model skyboxModel;
        public Model LaserModel;
        
        public Effect EffectBloom;
        public Effect EffectBlur;
        public Effect MasterMRT;

        private Texture TieTexture;
        private Texture TieNormalTex;
        //private Texture TrenchTexture;
        private Texture[] XwingTextures;
        private Texture[] XwingNormalTex;
        private TextureCube skyBoxTexture;

        private FullScreenQuad FullScreenQuad;
        private RenderTarget2D BlurHRenderTarget;
        private RenderTarget2D BlurVRenderTarget;
        private RenderTarget2D ShadowMapRenderTarget;
        private RenderTarget2D ColorTarget;
        private RenderTarget2D NormalTarget;
        private RenderTarget2D DirToCamTarget;
        private RenderTarget2D BloomFilterTarget;
        private RenderTarget2D LightTarget;
        private RenderTarget2D SceneTarget;
        #endregion

        Vector2 ShadowMapSize;

        public List<Trench> trenchesToDraw = new List<Trench>();
        public List<TieFighter> tiesToDraw = new List<TieFighter>();
        public List<Laser> lasersToDraw = new List<Laser>();
        public List<Ship> shipsToDraw = new List<Ship>();
        public bool showXwing;

        SpriteBatch SpriteBatch;
        enum DrawType
        {
            Regular,
            DepthMap
        }

        public Drawer()
        {
           
        }
        
        public void Init()
        {
            Game = TGCGame.Instance;
            GraphicsDevice = Game.GraphicsDevice;
            Content = Game.Content;

            ContentFolder3D = TGCGame.ContentFolder3D;
            ContentFolderEffects = TGCGame.ContentFolderEffects;
            ContentFolderTextures = TGCGame.ContentFolderTextures;

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            LoadContent();
        }
        Model[] loadNumberedModels(String source, int start, int end, int inc)
        {
            List<Model> models = new List<Model>();
            for (int n = start; n <= end; n += inc)
                models.Add(Content.Load<Model>(ContentFolder3D + source + n));

            return models.ToArray();
        }
        void LoadContent()
        {
            TieModel = Content.Load<Model>(ContentFolder3D + "TIE/TIE");
            XwingModel = Content.Load<Model>(ContentFolder3D + "XWing/model");
            XwingEnginesModel = Content.Load<Model>(ContentFolder3D + "XWing/xwing-engines");


            TrenchPlatform = loadNumberedModels("Trench/Platform/", 0, 3, 1);
            TrenchStraight = loadNumberedModels("Trench/Straight/", 0, 3, 1);
            //TrenchT = loadNumberedModels("Trench/T/", 0, 3, 1);
            //TrenchElbow = loadNumberedModels("Trench/Elbow/", 0, 3, 1);
            //TrenchIntersection = loadNumberedModels("Trench/Intersection/", 0, 3, 1);

            //TrenchPlatform = new Model[] { Content.Load<Model>(ContentFolder3D + "Trench/Trench-Platform-Block") };
            //TrenchStraight = new Model[] { Content.Load<Model>(ContentFolder3D + "Trench/Trench-Straight-Block") };
            TrenchT = new Model[] { Content.Load<Model>(ContentFolder3D + "Trench/T/0") };
            TrenchElbow = new Model[] { Content.Load<Model>(ContentFolder3D + "Trench/Elbow/0") };
            TrenchIntersection = new Model[] { Content.Load<Model>(ContentFolder3D + "Trench/Intersection/0") };

            //TrenchPlatform = Content.Load<Model>(ContentFolder3D + "Trench/Trench-Platform-Block");
            //TrenchStraight = Content.Load<Model>(ContentFolder3D + "Trench/Trench-Straight-Block");
            //TrenchT = Content.Load<Model>(ContentFolder3D + "Trench/Trench-T-Block");
            //TrenchElbow = Content.Load<Model>(ContentFolder3D + "Trench/Trench-Elbow-Block");
            //TrenchIntersection = Content.Load<Model>(ContentFolder3D + "Trench/Trench-Intersection");
            TrenchTurret = Content.Load<Model>(ContentFolder3D + "Trench/Trench-Turret");

            //Trench2 = Content.Load<Model>(ContentFolder3D + "Trench2/Trench");
            LaserModel = Content.Load<Model>(ContentFolder3D + "Laser/Laser");

            EffectBloom = Content.Load<Effect>(ContentFolderEffects + "Bloom");
            EffectBlur = Content.Load<Effect>(ContentFolderEffects + "GaussianBlur");

            MasterMRT = Content.Load<Effect>(ContentFolderEffects + "MasterMRT");
            XwingTextures = new Texture[] { Content.Load<Texture2D>(ContentFolderTextures + "xWing/lambert6_Base_Color"),
                                            Content.Load<Texture2D>(ContentFolderTextures + "xWing/lambert5_Base_Color") };
            XwingNormalTex = new Texture[] { Content.Load<Texture2D>(ContentFolderTextures + "xWing/lambert6_Normal_DirectX"),
                                            Content.Load<Texture2D>(ContentFolderTextures + "xWing/lambert5_Normal_DirectX") };

            TieTexture = Content.Load<Texture2D>(ContentFolderTextures + "TIE/TIE_IN_Diff");
            TieNormalTex = Content.Load<Texture2D>(ContentFolderTextures + "TIE/TIE_IN_Normal");
            //TrenchTexture = Content.Load<Texture2D>(ContentFolderTextures + "Trench/MetalSurface");

            skyboxModel = Content.Load<Model>(ContentFolder3D + "skybox/cube");

            skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skybox/space_earth_small_skybox");
            
            SkyBox = new SkyBox(skyboxModel, skyBoxTexture, MasterMRT);

            assignEffectToModels(new Model[] { TieModel, XwingModel, XwingEnginesModel, TrenchTurret, LaserModel, SkyBox.Model }, MasterMRT);
            
            assignEffectToModels(TrenchElbow, MasterMRT);
            assignEffectToModels(TrenchIntersection, MasterMRT);
            assignEffectToModels(TrenchPlatform, MasterMRT);
            assignEffectToModels(TrenchStraight, MasterMRT);
            assignEffectToModels(TrenchT, MasterMRT);

            manageEffectParameters();
            
            InitRTs();
           
            
        }
        public void InitRTs()
        {
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;

            BlurHRenderTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            BlurVRenderTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            ColorTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            NormalTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, width * 2, height * 2, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            ShadowMapSize = new Vector2(width * 2, height * 2); 
            DirToCamTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            BloomFilterTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            LightTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            SceneTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }
        public void Unload()
        {
            FullScreenQuad.Dispose();
            BlurHRenderTarget.Dispose();
            BlurVRenderTarget.Dispose();
            ColorTarget.Dispose();
            NormalTarget.Dispose();
            DirToCamTarget.Dispose();
            BloomFilterTarget.Dispose();
            LightTarget.Dispose();
            SceneTarget.Dispose();
        }

        #region drawers

        public int ShowTarget = 0;
        public float SpecularPower = 2.55f;
        public float SpecularIntensity = 0.5f;

        void DrawSceneMRT(DrawType dt)
        {
            var CameraView = Game.SelectedCamera.View;
            var CameraProjection = Game.SelectedCamera.Projection;
            var CameraPosition = Game.SelectedCamera.Position;

            //MRTapplyLightEffect.SetValue(0f);
            //MRTinvertViewProjection.SetValue(Matrix.Invert(CameraView * CameraProjection));
            MRTcameraPosition.SetValue(CameraPosition);
            MRTlightDirection.SetValue(Game.LightCamera.FrontDirection);
            MRTspecularIntensity.SetValue(SpecularIntensity);
            MRTspecularPower.SetValue(SpecularPower);

            if (dt == DrawType.Regular)
            {

                MasterMRT.CurrentTechnique = MRTskybox;
                SkyBox.Draw(CameraView, CameraProjection, CameraPosition);
            }
            if(dt == DrawType.DepthMap)
                MasterMRT.CurrentTechnique = MRTshadowMap;
            
            //MasterMRT.Parameters["ApplyLightEffect"]?.SetValue(1f);
            if (Game.GameState.Equals(TGCGame.GmState.Running) ||
                Game.GameState.Equals(TGCGame.GmState.Paused) ||
                Game.GameState.Equals(TGCGame.GmState.Defeat))
            {
                
                
                if (showXwing)
                    DrawXWingMRT(Game.Xwing, dt);
                foreach (var enemy in tiesToDraw)
                    DrawTieMRT(enemy);
                foreach (var ship in shipsToDraw)
                {
                    if (ship.Allied)
                        DrawXwingMRT(ship);
                    else
                        DrawTieMRT(ship);
                }
                if (dt == DrawType.Regular)
                {
                    MasterMRT.CurrentTechnique = MRTbasicColor;
                    MRTaddToBloomFilter.SetValue(1f);
                    //MasterMRT.Parameters["ApplyLightEffect"]?.SetValue(0f);
                    //MRTapplyLightEffect.SetValue(0f);

                    foreach (var laser in lasersToDraw)
                        DrawModelMRT(LaserModel, laser.SRT, laser.Color);
                    
                    MRTaddToBloomFilter.SetValue(0f);
                    //MasterMRT.Parameters["ApplyLightEffect"]?.SetValue(1f);
                    //MRTapplyLightEffect.SetValue(1f);

                    MRTcolor.SetValue(new Vector3(0.5f, 0.5f, 0.5f));

                    MasterMRT.CurrentTechnique = MasterMRT.Techniques["TrenchDraw"];
                }
                
                DrawMapMRT();
            }
            
            
        }
        public float modEpsilon = 0.000041200182749889791011810302734375f;
        public float maxEpsilon = 0.02f;
        public bool debugShadowMap = false;
        public void DrawMRT()
        {
            /* Draw Shadow Map*/
            MasterMRT.Parameters["modulatedEpsilon"].SetValue(modEpsilon);
            MasterMRT.Parameters["maxEpsilon"].SetValue(maxEpsilon);

            GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            DrawSceneMRT(DrawType.DepthMap);

            /* Draw Scene with MRT and apply shadows*/
            GraphicsDevice.SetRenderTargets(ColorTarget, NormalTarget, DirToCamTarget, BloomFilterTarget);

            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.Opaque;


            MRTshadowMapTex.SetValue(ShadowMapRenderTarget);
            //MasterMRT.Parameters["ShadowMapSize"]?.SetValue(ShadowMapSize);
            MRTlightViewProjection.SetValue(Game.LightCamera.View * Game.LightCamera.Projection);

            DrawSceneMRT(DrawType.Regular);
            
            if (ShowTarget == 0)
            {

                /* Calculate and integrate lights, also blur the filtered bloom*/
                GraphicsDevice.SetRenderTargets(SceneTarget, BlurHRenderTarget, BlurVRenderTarget);

                MasterMRT.CurrentTechnique = MRTCalculateIntegrateLightBlur;
                MRTcolorMap.SetValue(ColorTarget);
                MRTdirToCamMap.SetValue(DirToCamTarget);
                MRTnormalMap.SetValue(NormalTarget);
                MRTbloomFilter.SetValue(BloomFilterTarget);
                MRTlightColor.SetValue(new Vector3(1f, 1f, 1f));
                MRTambientLightColor.SetValue(new Vector3(0.98f, 0.9f, 1f));
                MRTambientLightIntensity.SetValue(0.35f);
                MRTscreenSize.SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

                
                FullScreenQuad.Draw(MasterMRT);
                
                /* integrate */
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);

                EffectBloom.CurrentTechnique = EffectBloom.Techniques["Integrate"];
                EPbloomTexture.SetValue(SceneTarget);
                EPbloomBlurHTexture.SetValue(BlurHRenderTarget);
                EPbloomBlurVTexture.SetValue(BlurVRenderTarget);

                FullScreenQuad.Draw(EffectBloom);

                if (debugShadowMap)
                {
                    SpriteBatch.Begin();
                    SpriteBatch.Draw(ShadowMapRenderTarget,
                               new Vector2(0, 250), null, Color.White, 0f, Vector2.Zero, 0.10f, SpriteEffects.None, 0);
                    SpriteBatch.End();
                }
            }
            else if (ShowTarget >= 2)
            {

                GraphicsDevice.SetRenderTarget(null);

                SpriteBatch.Begin();

                if (ShowTarget == 2)
                    SpriteBatch.Draw(ColorTarget, Vector2.Zero, Color.White);
                else if (ShowTarget == 3)
                    SpriteBatch.Draw(NormalTarget, Vector2.Zero, Color.White);
                else if (ShowTarget == 4)
                    SpriteBatch.Draw(LightTarget, Vector2.Zero, Color.White);
                else if (ShowTarget == 5)
                    SpriteBatch.Draw(BloomFilterTarget, Vector2.Zero, Color.White);
                else if (ShowTarget == 6)
                    SpriteBatch.Draw(DirToCamTarget, Vector2.Zero, Color.White);

                //else if (ShowTarget == 6)
                //    SpriteBatch.Draw(DepthTarget,
                //           Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
                else if (ShowTarget == 7)
                    SpriteBatch.Draw(ShadowMapRenderTarget,
                           Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);


                SpriteBatch.End();
            }
            //DrawScene(DrawType.Regular);

        }
        
        #region MRTelements
        void DrawMapMRT()
        {
            foreach (var t in trenchesToDraw)
                DrawTrenchMRT(t);
        }
        void DrawTrenchMRT(Trench t)
        {
            
            Matrix world;
            
            foreach (var mesh in t.SelectedModel.Meshes)
            {
                world = mesh.ParentBone.Transform * t.SRT;
                //if(t.SelectedIndex == 0)
                    MRTcolor.SetValue(new Vector3(0.5f, 0.5f, 0.5f));
                //if (t.SelectedIndex == 1)
                //    MRTcolor.SetValue(new Vector3(0.5f, 0f, 0f));
                //if (t.SelectedIndex == 2)
                //    MRTcolor.SetValue(new Vector3(0f, 0.5f, 0f));
                //if (t.SelectedIndex == 3)
                //    MRTcolor.SetValue(new Vector3(0f, 0f, 0.5f));

                MRTworld.SetValue(world);
                MRTworldViewProjection.SetValue(world * Game.SelectedCamera.View * Game.SelectedCamera.Projection);
                MRTinverseTransposeWorld.SetValue(Matrix.Transpose(Matrix.Invert(world)));
                mesh.Draw();

            }
            foreach (var turret in t.Turrets)
            {
                foreach (var mesh in TrenchTurret.Meshes)
                {
                    world = mesh.ParentBone.Transform * turret.SRT;
                    MRTcolor.SetValue(new Vector3(0.5f, 0.5f, 0.5f));
                    MRTworld.SetValue(world);
                    MRTworldViewProjection.SetValue(world * Game.SelectedCamera.View * Game.SelectedCamera.Projection);
                    MRTinverseTransposeWorld.SetValue(Matrix.Transpose(Matrix.Invert(world)));
                    mesh.Draw();
                }
            }

        }
        
        void DrawXwingMRT(Ship xwing)
        {
            Matrix world;
            int meshCount = 0;
            foreach (var mesh in XwingModel.Meshes)
            {
                world = mesh.ParentBone.Transform * xwing.SRT;

                var wvp = world * Game.SelectedCamera.View * Game.SelectedCamera.Projection;
                //var itw = Matrix.Transpose(Matrix.Invert(xwing.World));

                MRTtexture.SetValue(XwingTextures[meshCount]);
                MRTmodelNormal.SetValue(XwingNormalTex[meshCount]);
                MRTworld.SetValue(world);
                MRTworldViewProjection.SetValue(wvp);
                //EPMRTlightViewProjection.SetValue(world * Game.LightCamera.View * Game.LightCamera.Projection);
                MRTinverseTransposeWorld.SetValue(Matrix.Transpose(Matrix.Invert(world)));
                meshCount++;

                mesh.Draw();
            }
        }

        void DrawXWingMRT(Xwing xwing, DrawType dt)
        {
            
            int meshCount = 0;
            if (dt == DrawType.Regular)
                MasterMRT.CurrentTechnique = MRTbasicColor; // remove for light post proc.
            
            //MRTapplyLightEffect.SetValue(0f);

            MRTaddToBloomFilter.SetValue(1f);
            DrawModelMRT(XwingEnginesModel, xwing.EnginesSRT, xwing.EnginesColor);
            MRTaddToBloomFilter.SetValue(0f);
            
            //MRTapplyLightEffect.SetValue(1f);
            
            if (dt == DrawType.Regular)
                MasterMRT.CurrentTechnique = MRTtextured;

            foreach (var mesh in XwingModel.Meshes)
            {
                xwing.World = mesh.ParentBone.Transform * xwing.SRT;

                var wvp = xwing.World * Game.SelectedCamera.View * Game.SelectedCamera.Projection;
                
                MRTtexture.SetValue(XwingTextures[meshCount]);
                MRTmodelNormal.SetValue(XwingNormalTex[meshCount]);
                MRTworld.SetValue(xwing.World);
                MRTworldViewProjection.SetValue(wvp);
                MRTinverseTransposeWorld.SetValue(Matrix.Transpose(Matrix.Invert(xwing.World)));
                meshCount++;

                mesh.Draw();
            }
        }

        void DrawTieMRT(Ship tie)
        {
            Matrix world;
            //MasterMRT.Parameters["ApplyShieldEffect"]?.SetValue(1f);
            //MasterMRT.Parameters["ShieldColor"]?.SetValue(new Vector3(0.8f, 0f, 0f));

            MRTmodelNormal.SetValue(TieNormalTex);
            foreach (var mesh in TieModel.Meshes)
            {
                world = mesh.ParentBone.Transform * tie.SRT;

                var wvp = world * Game.SelectedCamera.View * Game.SelectedCamera.Projection;
                MRTworld.SetValue(world);
                MRTtexture.SetValue(TieTexture);
                MRTworldViewProjection.SetValue(wvp);
                MRTinverseTransposeWorld.SetValue(Matrix.Transpose(Matrix.Invert(world)));
                mesh.Draw();
            }
            //MasterMRT.Parameters["ApplyShieldEffect"]?.SetValue(0f);
        }
        void DrawTieMRT(TieFighter tie)
        {
            Matrix world;
            MRTmodelNormal.SetValue(TieNormalTex);
            foreach (var mesh in TieModel.Meshes)
            {
                world = mesh.ParentBone.Transform * tie.SRT;

                var wvp = world * Game.SelectedCamera.View * Game.SelectedCamera.Projection;
                MRTworld.SetValue(world);
                MRTtexture.SetValue(TieTexture);
                MRTworldViewProjection.SetValue(wvp);
                MRTinverseTransposeWorld.SetValue(Matrix.Transpose(Matrix.Invert(world)));
                mesh.Draw();
            }
        }
        void DrawModelMRT(Model model, Matrix SRT, Vector3 color)
        {
            
            foreach (var mesh in model.Meshes)
            {
                var world = mesh.ParentBone.Transform * SRT;
                var wvp = world * Game.SelectedCamera.View * Game.SelectedCamera.Projection;

                MRTworld.SetValue(world);
                MRTcolor.SetValue(color);
                MRTworldViewProjection.SetValue(wvp);
                MRTinverseTransposeWorld.SetValue(Matrix.Transpose(Matrix.Invert(world)));
                mesh.Draw();
            }

        }

        #endregion
        #endregion

        void assignEffectToModel(Model model, Effect effect)
        {
            foreach (var mesh in model.Meshes)
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = effect;
        }
        void assignEffectToModels(Model[] models, Effect effect)
        {
            foreach (Model model in models)
                assignEffectToModel(model, effect);
        }

        #region effectParameters
        EffectParameter EPbloomTexture;
        EffectParameter EPbloomBlurHTexture;
        EffectParameter EPbloomBlurVTexture;

        EffectParameter MRTworld;
        EffectParameter MRTworldViewProjection;
        EffectParameter MRTinverseTransposeWorld;
        EffectParameter MRTaddToBloomFilter;
        EffectParameter MRTcolor;
        EffectParameter MRTtexture;
        EffectParameter MRTlightViewProjection;
        EffectParameter MRTcolorMap;
        EffectParameter MRTdirToCamMap;
        EffectParameter MRTnormalMap;
        EffectParameter MRTmodelNormal;
        EffectParameter MRTbloomFilter;
        EffectParameter MRTlightColor;
        EffectParameter MRTambientLightColor;
        EffectParameter MRTambientLightIntensity;
        EffectParameter MRTscreenSize;
        EffectParameter MRTshadowMapTex;
        EffectParameter MRTshadowMapSize;
        EffectParameter MRTapplyLightEffect;
        EffectParameter MRTinvertViewProjection;
        EffectParameter MRTcameraPosition;
        EffectParameter MRTlightDirection;
        EffectParameter MRTspecularIntensity;
        EffectParameter MRTspecularPower;
        EffectParameter MRTskyboxTex;
        
        EffectTechnique MRTtextured;
        EffectTechnique MRTbasicColor;
        EffectTechnique MRTskybox;
        EffectTechnique MRTshadowMap;
        EffectTechnique MRTCalculateIntegrateLightBlur;
        void manageEffectParameters()
        {
            
            
            
            EPbloomTexture =        EffectBloom.Parameters["baseTexture"];
            EPbloomBlurHTexture =   EffectBloom.Parameters["blurHTexture"];
            EPbloomBlurVTexture =   EffectBloom.Parameters["blurVTexture"];

            MRTworld =                  MasterMRT.Parameters["World"];
            MRTworldViewProjection =    MasterMRT.Parameters["WorldViewProjection"];
            MRTtexture =                MasterMRT.Parameters["Texture"];
            MRTaddToBloomFilter =       MasterMRT.Parameters["AddToFilter"];
            MRTcolor =                  MasterMRT.Parameters["Color"];
            MRTlightViewProjection =    MasterMRT.Parameters["LightViewProjection"];
            MRTinverseTransposeWorld =  MasterMRT.Parameters["InverseTransposeWorld"];
            MRTcolorMap =               MasterMRT.Parameters["ColorMap"];
            MRTdirToCamMap =            MasterMRT.Parameters["DirToCamMap"];
            MRTnormalMap =              MasterMRT.Parameters["NormalMap"];
            MRTmodelNormal =            MasterMRT.Parameters["ModelNormal"];
            MRTapplyLightEffect =       MasterMRT.Parameters["ApplyLightEffect"];
            MRTinvertViewProjection =   MasterMRT.Parameters["InvertViewProjection"];
            MRTcameraPosition =         MasterMRT.Parameters["CameraPosition"];
            MRTlightDirection =         MasterMRT.Parameters["LightDirection"];
            MRTspecularIntensity =      MasterMRT.Parameters["SpecularIntensity"];
            MRTspecularPower =          MasterMRT.Parameters["SpecularPower"];
            MRTskyboxTex =              MasterMRT.Parameters["SkyBoxTexture"];
            MRTbloomFilter =            MasterMRT.Parameters["BloomFilter"];
            MRTlightColor =             MasterMRT.Parameters["LightColor"];
            MRTambientLightColor =      MasterMRT.Parameters["AmbientLightColor"];
            MRTambientLightIntensity =  MasterMRT.Parameters["AmbientLightIntensity"];
            MRTscreenSize =             MasterMRT.Parameters["screenSize"];
            MRTshadowMapTex =           MasterMRT.Parameters["ShadowMap"];
            MRTshadowMapSize =          MasterMRT.Parameters["ShadowMapSize"];


            MRTbasicColor =                 MasterMRT.Techniques["BasicColorDraw"];
            MRTtextured =                   MasterMRT.Techniques["TexturedDraw"];
            MRTskybox =                     MasterMRT.Techniques["Skybox"];
            MRTshadowMap =                  MasterMRT.Techniques["DepthPass"];
            MRTCalculateIntegrateLightBlur= MasterMRT.Techniques["CalcIntLightBlur"];
        }
        #endregion
        public static Model[] GetModelsFromType(TrenchType type)
        {
            switch (type)
            {
                case TrenchType.Platform: return TrenchPlatform;
                case TrenchType.Straight: return TrenchStraight;
                case TrenchType.T: return TrenchT;
                case TrenchType.Intersection: return TrenchIntersection;
                case TrenchType.Elbow: return TrenchElbow;
                default: return TrenchPlatform;
            }
        }
    }
}