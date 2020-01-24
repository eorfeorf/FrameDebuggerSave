﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.FrameDebugSave
{
    public class TextureUtility
    {
        public class SaveTextureInfo
        {
            public string path;
        }

        public static RenderTexture GetGameViewRT()
        {
            var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();
            foreach (var rt in renderTextures)
            {
                if (rt.name == "GameView RT")
                {
                    return rt;
                }
            }
            return null;
        }

        public static RenderTexture GetTargetRenderTexture(FrameInfoCrawler.FrameDebuggerEventData info)
        {
            RenderTexture target = null;
            var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();
            foreach (var rt in renderTextures)
            {
                if (rt.width == info.rtWidth && rt.height == info.rtHeight &&
                     rt.name == info.rtName)
                {
                    if (target != null) { Debug.LogWarning("Already find renderTarget."); }
                    target = rt;
                }
            }
            return target;
        }

        public static SaveTextureInfo SaveRenderTexture(RenderTexture renderTexture, string file)
        {
            SaveTextureInfo saveInfo = new SaveTextureInfo();
            try
            {
                Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                RenderTexture.active = renderTexture;
                tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                tex.Apply();
                byte[] bytes = tex.EncodeToPNG();
                file += ".png";
                System.IO.File.WriteAllBytes(file, bytes);
                Object.DestroyImmediate(tex);
                return saveInfo;
            }catch(System.Exception e){
                Debug.LogError(e);
            }
            return null;
        }

        public static SaveTextureInfo SaveTexture(Texture2D tex, string file)
        {
            SaveTextureInfo saveInfo = new SaveTextureInfo();
            try
            {
                Texture2D writeTexture = null;
                if (tex.isReadable)
                {
                    writeTexture = tex;
                }
                else
                {
                    writeTexture = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount, false);
                    Graphics.CopyTexture(tex, writeTexture);
                }
                byte[] bytes = writeTexture.EncodeToPNG();
                file += ".png";
                System.IO.File.WriteAllBytes(file, bytes);
                if (tex != writeTexture)
                {
                    Object.DestroyImmediate(writeTexture);
                }
                return saveInfo;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }

        private bool ShouldSaveEXR(RenderTexture tex)
        {
            switch (tex.format)
            {
            }

            return false;
        }

        private bool ShouldSaveEXR(Texture2D tex)
        {
            switch(tex.format)
            {
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGBAHalf:
                    return true;
            }
            return false;
        }
        private bool IsCompress(Texture2D tex)
        {
            return false;
        }
    }
}