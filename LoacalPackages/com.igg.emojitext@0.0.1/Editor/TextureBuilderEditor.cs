using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace igg.EmojiText.Editor
{

    public class TextureBuilderEditor : EditorWindow
    {
        #region Static Function

        [MenuItem("Tools/Texture Builder")]
        static void TextureBuilder()
        {
            TextureBuilderEditor window = (TextureBuilderEditor) EditorWindow.GetWindow(typeof(TextureBuilderEditor));
            window.Show();
        }

        #endregion

        private string m_texturePath = "ResourcesRex/EmojiRex/Emoji/";

        public enum ImageType
        {
            Null,
            Png,
            Jpg,
            Gif,
            Bmp
        }

        List<Texture2D> textures = new List<Texture2D>();

        private void OnGUI()
        {
            m_texturePath = EditorGUILayout.TextField(m_texturePath);

            if (GUILayout.Button("Build"))
            {
                textures.Clear();
                string dirPath = Path.Combine(Application.dataPath, m_texturePath);
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                FileInfo[] files = dirInfo.GetFiles("*.png", SearchOption.AllDirectories);

                foreach (FileInfo fileInfo in files)
                {
                    FileInfo(fileInfo.FullName, out byte[] bytes, out Vector2 size);

                    Texture2D tex = new Texture2D((int) size.x, (int) size.y, TextureFormat.RGBA32, false);
                    tex.LoadImage(bytes);
                    tex.Apply();
                    textures.Add(tex);
                }

                CreateAtlas(textures.ToArray(), Path.Combine("Assets/", "ResourcesRex/EmojiRex/Emoji.png"));
            }

            if (textures.Count > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.ObjectField(textures[0], typeof(Texture2D), false, GUILayout.Width(45), GUILayout.Height(45));
            }
        }

        static void CreateAtlas(Texture2D[] textures, string path)
        {
            int w = GenEmojiAtlas(textures);
            // make your new texture
            var atlas = new Texture2D(w, w, TextureFormat.RGBA32, false);
            // clear pixel
            Color32[] fillColor = atlas.GetPixels32();
            for (int i = 0; i < fillColor.Length; ++i)
            {
                fillColor[i] = Color.clear;
            }

            atlas.SetPixels32(fillColor);

            int textureWidthCounter = 0;
            int textureHeightCounter = 0;
            for (int i = 0; i < textures.Length; i++)
            {
                var frame = textures[i];
                // ?????????????????????????????? Atlas ???
                for (int k = 0; k < frame.width; k++)
                {
                    for (int l = 0; l < frame.height; l++)
                    {
                        atlas.SetPixel(k + textureWidthCounter, l + textureHeightCounter, frame.GetPixel(k, l));
                    }
                }

                textureWidthCounter += frame.width;
                if (textureWidthCounter > atlas.width - frame.width)
                {
                    textureWidthCounter = 0;
                    textureHeightCounter += frame.height;
                }
            }

            atlas.Apply();
            var tex = SaveSpriteToEditorPath(atlas, path);
        }

        static int GenEmojiAtlas(Texture2D[] textures)
        {
            // get all select textures
            int width = 0;
            int count = textures.Length;
            foreach (var texture in textures)
            {
                if (0 == width)
                {
                    width = texture.width;
                }
                else if (texture.width != width)
                {
                    Debug.LogError($"????????????????????????????????????????????????????????????: {width}, ???????????? {texture.name} ???????????????{texture.width}");
                }
            }

            int column = Mathf.CeilToInt(Mathf.Sqrt(count));
            int atlasWidth = column * width;
            return atlasWidth;
        }

        static Texture2D SaveSpriteToEditorPath(Texture2D sp, string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string dir = Path.GetDirectoryName(path);

            Directory.CreateDirectory(dir);

            File.WriteAllBytes(path, sp.EncodeToPNG());
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false;
            importer.mipmapEnabled = false;

            var settingsDefault = importer.GetDefaultPlatformTextureSettings();
            settingsDefault.textureCompression = TextureImporterCompression.Uncompressed;
            settingsDefault.maxTextureSize = 2048;
            settingsDefault.format = TextureImporterFormat.RGBA32;

            var settingsAndroid = importer.GetPlatformTextureSettings("Android");
            settingsAndroid.overridden = true;
            settingsAndroid.maxTextureSize = settingsDefault.maxTextureSize;
            settingsAndroid.format = TextureImporterFormat.ASTC_8x8;
            importer.SetPlatformTextureSettings(settingsAndroid);

            var settingsiOS = importer.GetPlatformTextureSettings("iPhone");
            settingsiOS.overridden = true;
            settingsiOS.maxTextureSize = settingsDefault.maxTextureSize;
            settingsiOS.format = TextureImporterFormat.ASTC_8x8;
            importer.SetPlatformTextureSettings(settingsiOS);

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
        }

        #region Image Reader to Bytes

        private static ImageType GetImageType(byte[] bytes)
        {
            byte[] header = new byte[8];
            Array.Copy(bytes, header, header.Length);
            ImageType type = ImageType.Null;
            // ???????????????8?????????
            //Png?????? 8?????????89 50 4E 47 0D 0A 1A 0A   =  [1]:P[2]:N[3]:G
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                type = ImageType.Png;
            }
            // Jpg??????2?????? FF D8
            else if (header[0] == 0xFF && header[1] == 0xD8)
            {
                type = ImageType.Jpg;
            }
            // Gif??????6????????? 47 49 46 38 39|37 61 
            else if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38 &&
                     (header[4] == 0x39 || header[4] == 0x37) && header[5] == 0x61)
            {
                type = ImageType.Gif;
            }
            // Bmp ??????2?????? 42 4D
            else if (header[0] == 0x42 && header[1] == 0x4D)
            {
                type = ImageType.Bmp;
            }

            return type;
        }


        private static byte[] _header = null;
        private static byte[] _buffer = null;

        public static void FileInfo(string filePath, out byte[] bytes, out Vector2 size)
        {
            size = Vector2.zero;
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int) stream.Length);

            ImageType imageType = GetImageType(bytes);
            switch (imageType)
            {
                case ImageType.Png:
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    _header = new byte[8];
                    stream.Read(_header, 0, 8);
                    stream.Seek(8, SeekOrigin.Current);

                    _buffer = new byte[8];
                    stream.Read(_buffer, 0, _buffer.Length);

                    Array.Reverse(_buffer, 0, 4);
                    Array.Reverse(_buffer, 4, 4);

                    size.x = BitConverter.ToInt32(_buffer, 0);
                    size.y = BitConverter.ToInt32(_buffer, 4);
                }
                    break;
                case ImageType.Jpg:
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    _header = new byte[2];
                    stream.Read(_header, 0, 2);
                    //?????????
                    int type = -1;
                    int ff = -1;
                    //???????????????????????????
                    long ps = 0;
                    //??????????????????????????????SOFO???
                    do
                    {
                        do
                        {
                            //??????????????????????????????oxff????????????????????????
                            ff = stream.ReadByte();
                            if (ff < 0) //????????????
                            {
                                return;
                            }
                        } while (ff != 0xff);

                        do
                        {
                            //?????????????????????????????????oxff?????????????????????oxff???????????????????????????
                            type = stream.ReadByte();
                        } while (type == 0xff);

                        //??????????????????
                        ps = stream.Position;
                        switch (type)
                        {
                            case 0x00:
                            case 0x01:
                            case 0xD0:
                            case 0xD1:
                            case 0xD2:
                            case 0xD3:
                            case 0xD4:
                            case 0xD5:
                            case 0xD6:
                            case 0xD7:
                                break;
                            case 0xc0: //SOF0???????????????????????????
                            case 0xc2: //JFIF????????? SOF0???
                            {
                                //??????SOFO?????????????????????????????????
                                //??????2????????????????????????1????????????????????????
                                stream.Seek(3, SeekOrigin.Current);

                                //?????? ???2?????? ??????????????????
                                size.y = stream.ReadByte() * 256;
                                size.y += stream.ReadByte();
                                //?????? ???2?????? ??????????????????
                                size.x = stream.ReadByte() * 256;
                                size.x += stream.ReadByte();
                                return;
                            }
                            default: //??????????????????
                                //??????????????????????????????
                                ps = stream.ReadByte() * 256;
                                ps = stream.Position + ps + stream.ReadByte() - 2;
                                break;
                        }

                        if (ps + 1 >= stream.Length) //????????????
                        {
                            return;
                        }

                        stream.Position = ps; //????????????
                    } while (type != 0xda); // ???????????????
                }
                    break;
                case ImageType.Gif:
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    _header = new byte[6];
                    stream.Read(_header, 0, 6);

                    _buffer = new byte[4];
                    stream.Read(_buffer, 0, _buffer.Length);

                    size.x = BitConverter.ToInt16(_buffer, 0);
                    size.y = BitConverter.ToInt16(_buffer, 2);
                }
                    break;
                case ImageType.Bmp:
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    _header = new byte[2];
                    stream.Read(_header, 0, 2);
                    //??????16?????????
                    stream.Seek(16, SeekOrigin.Current);
                    //bmp????????????????????????????????? 18-21??? 4??????
                    //bmp???????????????????????????????????? 22-25??? 4??????
                    _buffer = new byte[8];
                    stream.Read(_buffer, 0, _buffer.Length);

                    size.x = BitConverter.ToInt32(_buffer, 0);
                    size.y = BitConverter.ToInt32(_buffer, 4);
                }
                    break;
                default:
                    break;
            }

            stream.Close();
            stream.Dispose();
        }

        #endregion
    }
}