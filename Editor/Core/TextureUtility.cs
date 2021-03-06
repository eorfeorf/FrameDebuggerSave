﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.FrameDebugSave
{
    public class TextureUtility
    {
        public class SaveTextureInfo
        {
            public const int TYPE_PNG = 0;
            public const int TYPE_EXR = 1;
            public const int TYPE_RAWDATA = 2;

            public string path;
            public int type;
            public int width;
            public int height;

            public TextureFormat rawFormat;
            public int mipCount;

            public SaveTextureInfo(string p, int t, int w, int h, TextureFormat format, int mip)
            {
                this.path = p;
                this.type = t;
                this.width = w;
                this.height = h;
                this.rawFormat = format;
                this.mipCount = mip;

            }

            public SaveTextureInfo(string p,Texture tex,int t)
            {
                this.width = tex.width;
                this.height = tex.height;
                this.mipCount = GetMipMapCount(tex);
                this.type = t;
                p = p.Replace('\\', '/');
                int fileNameIdx = p.LastIndexOf('/');
                int lastDirIdx = 0;
                if (fileNameIdx > 0)
                {
                    lastDirIdx = p.LastIndexOf('/', fileNameIdx - 1);
                }
                lastDirIdx += 1;

                this.path = p.Substring(lastDirIdx);
                if( t == TYPE_RAWDATA && tex.GetType() == typeof(Texture2D))
                {
                    this.rawFormat = ((Texture2D)tex).format;
                }
            }
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
                     rt.name == info.rtName )
                {
                    if (target != null) { Debug.LogWarning("Already find renderTarget. " + info.rtName); }
                    target = rt;
                }
            }
            return target;
        }

        public static SaveTextureInfo SaveRenderTexture(RenderTexture renderTexture, string file)
        {
            SaveTextureInfo saveInfo = null;
            try
            {
                Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, GetTextureFormat(renderTexture), false);
                RenderTexture.active = renderTexture;
                tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                tex.Apply();

                if (ShoudSaveRawData(tex))
                {
                    byte[] bytes = tex.GetRawTextureData();
                    file += ".raw";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new SaveTextureInfo(file, renderTexture, SaveTextureInfo.TYPE_RAWDATA);
                }
                else
                {
                    byte[] bytes = tex.EncodeToPNG();
                    file += ".png";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new SaveTextureInfo(file, renderTexture, SaveTextureInfo.TYPE_PNG);
                }
                Object.DestroyImmediate(tex);
                return saveInfo;
            }catch(System.Exception e){
                Debug.LogError(e);
            }
            return null;
        }

        public static SaveTextureInfo SaveTexture(Texture2D tex, string file)
        {
            SaveTextureInfo saveInfo = null;
            try
            {
                Texture2D writeTexture = null;
                if (tex.isReadable)
                {
                    writeTexture = tex;
                }
                else
                {
#if UNITY_2019_2_OR_NEWER
                    writeTexture = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount, false);
#else
                    writeTexture = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount > 0, false);
#endif
                    Graphics.CopyTexture(tex, writeTexture);
                }
                if (ShoudSaveRawData(tex))
                {
                    byte[] bytes = writeTexture.GetRawTextureData();
                    file += ".raw";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new SaveTextureInfo(file, tex, SaveTextureInfo.TYPE_RAWDATA);
                }
                else
                {
                    byte[] bytes = writeTexture.EncodeToPNG();
                    file += ".png";
                    System.IO.File.WriteAllBytes(file, bytes);
                    saveInfo = new SaveTextureInfo(file, tex, SaveTextureInfo.TYPE_PNG);
                }
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

        private static TextureFormat GetTextureFormat(RenderTexture tex)
        {
            switch (tex.format)
            {
                case RenderTextureFormat.ARGB2101010:
                case RenderTextureFormat.ARGB64:
                case RenderTextureFormat.ARGBFloat:
                    return TextureFormat.RGBAFloat;
                case RenderTextureFormat.ARGBHalf:
                case RenderTextureFormat.DefaultHDR:
                case RenderTextureFormat.RGB111110Float:
                    return TextureFormat.RGBAHalf;
            }
            return TextureFormat.RGBA32;
        }

        private static bool ShouldSaveAsDepth(RenderTexture tex)
        {
            switch (tex.format)
            {
                case RenderTextureFormat.Depth:
                    return true;
                case RenderTextureFormat.Shadowmap:
                    return true;
            }
            return false;
        }

 
        private static bool ShoudSaveRawData(Texture2D tex)
        {
            switch (tex.format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5:
                case TextureFormat.DXT5Crunched:
                    return true;
#if UNITY_2019_1_OR_NEWER || UNITY_2019_OR_NEWER
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;
#endif
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                    return true;
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC2_RGBA8Crunched:
                    return true;
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC_RGB4Crunched:
                    return true;

                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGB9e5Float:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                    return true;
            }
            return false;
        }

        public static Texture LoadTexture(string basePath , SaveTextureInfo info)
        {
            if( info == null || info.path == null) {
                return null;
            }
            string path = System.IO.Path.Combine(basePath,info.path);
            byte[] data = System.IO.File.ReadAllBytes(path);
            Texture2D tex = null;
#if UNITY_2019_2_OR_NEWER
            tex = new Texture2D(info.width, info.height, info.rawFormat, info.mipCount, false);
#else
            tex = new Texture2D(info.width, info.height, info.rawFormat, (info.mipCount > 0), false);
#endif
            switch (info.type)
            {
                case SaveTextureInfo.TYPE_PNG:
                    ImageConversion.LoadImage(tex, data);
                    break;
                case SaveTextureInfo.TYPE_EXR:
                    ImageConversion.LoadImage(tex, data);
                    break;
                case SaveTextureInfo.TYPE_RAWDATA:
                    tex.LoadRawTextureData(data);
                    tex.Apply();
                    break;
            }

            return tex;
        }

        public static int GetMipMapCount(Texture tex)
        {
#if UNITY_2019_2_OR_NEWER
            return tex.mipmapCount;
#else
            Texture2D tex2d = tex as Texture2D;
            if(tex2d == null){return 0;}
            return tex2d.mipmapCount;
#endif
        }
    }
}