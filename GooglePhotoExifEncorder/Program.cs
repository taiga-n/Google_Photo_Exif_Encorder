///
/// GooglePhotoExifEncorder
/// 
/// Googleフォトにアップロードした画像を，
/// 「Google データ　エクスポート」https://takeout.google.com/?continue=https://myaccount.google.com/dashboard&hl=ja
/// で一括ダウンロードした際に，Exif情報やファイル更新日時がずれてしまい，正しくアルバムに表示されない問題がある．
/// 
/// このプログラムは，エクスポートしたファイル内に存在するjsonファイルを読み込み，
/// 写真のExif情報やファイル更新日時を正しいものに一括で修正する．
/// 
/// masterReadFilePathに，googleからエクスポートしたフォトの親フォルダをのパスを選択し，
/// 実行すれば全自動で，masterSaveFilePathに書き出される．
/// 
/// 現在サポートするメディア形式はJPG, PNG, MP4, GIF, 3GP, WEBP, JPEGである．
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using static System.Console;


namespace GooglePhotoExifEncorder
{
    class Program
    {
        //読み込み画像のフォルダパス
        readonly static string masterReadFilePath = @"D:\Google フォト";//短くないとエラー
        //画像書き出し先のフォルダパス
        readonly static string masterSaveFilePath = @"D:\Google フォト_Revised\";//短くないとエラー


        static void Main(string[] args)
        {
            int c = 1;
            foreach (string f in JPGfiles)
            {
                JPGwrite(f);
                Console.WriteLine("JPG" + c++);
            }

            foreach (string f in PNGfiles)
            {
                PNGwrite(f);
                Console.WriteLine("PNG" + c++);
            }

            foreach (string f in MP4files)
            {
                MP4write(f);
                Console.WriteLine("MP4" + c++);
            }

            foreach (string f in GIFfiles)
            {
                GIFwrite(f);
                Console.WriteLine("GIF" + c++);
            }

            foreach (string f in _3GPfiles)
            {
                _3GPwrite(f);
                Console.WriteLine("3GP" + c++);
            }

            foreach (string f in WEBPfiles)
            {
                WEBPwrite(f);
                Console.WriteLine("WEBP" + c++);
            }

            foreach (string f in JPEGfiles)
            {
                JPEGwrite(f);
                Console.WriteLine("JPEG" + c++);
            }


        }

        static IEnumerable<string> JPGfiles =System.IO.Directory.EnumerateFiles(masterReadFilePath, "*.jpg", System.IO.SearchOption.AllDirectories);
        static IEnumerable<string> PNGfiles = System.IO.Directory.EnumerateFiles(masterReadFilePath, "*.png", System.IO.SearchOption.AllDirectories);
        static IEnumerable<string> MP4files = System.IO.Directory.EnumerateFiles(masterReadFilePath, "*.mp4", System.IO.SearchOption.AllDirectories);
        static IEnumerable<string> GIFfiles = System.IO.Directory.EnumerateFiles(masterReadFilePath, "*.gif", System.IO.SearchOption.AllDirectories);
        static IEnumerable<string> _3GPfiles = System.IO.Directory.EnumerateFiles(masterReadFilePath, "*.3gp", System.IO.SearchOption.AllDirectories);
        static IEnumerable<string> WEBPfiles = System.IO.Directory.EnumerateFiles(masterReadFilePath, "*.webp", System.IO.SearchOption.AllDirectories);
        static IEnumerable<string> JPEGfiles = System.IO.Directory.EnumerateFiles(masterReadFilePath, "*.jpeg", System.IO.SearchOption.AllDirectories);




        static void JPGwrite(string _imagePath) {
     
            //画像を読み込む
           System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(_imagePath);

            bool isFindExif = false;
            string jsonpath;

            if (_imagePath.Contains("(1)"))//例外処理
            {
                if (File.Exists(_imagePath.Replace("(1)", "") + "(1).json"))
                {
                    jsonpath = _imagePath.Replace("(1)", "") + "(1).json";
                } else
                {
                    jsonpath = _imagePath + ".json";
                }
            }
            else if (System.IO.Path.GetFileName(_imagePath).Length + 1 > 51)
            {
                jsonpath = _imagePath.Substring(0, _imagePath.LastIndexOf(@"\") + 1) + System.IO.Path.GetFileName(_imagePath).Substring(0, 46) + ".json";
            }
            else
            {
                jsonpath = _imagePath + ".json";
            }

            jsonpath = jsonpath.Replace("-編集済み","");//googleは編集済みにjsonないっぽい

            for (int i = 0; i < bmp.PropertyItems.Length; i++)
            {
                System.Drawing.Imaging.PropertyItem pi = bmp.PropertyItems[i];
                //Exif情報から撮影日時を探す
                if (pi.Id == 0x9003 && pi.Type == 2)
                {
                    isFindExif = true;
                    //値を変更する
                    pi.Value = System.Text.Encoding.ASCII.GetBytes(readJsonPhotoTakenTime(jsonpath));
                    pi.Len = pi.Value.Length;

                    Console.WriteLine(_imagePath);
                    Console.WriteLine(readJsonPhotoTakenTime(jsonpath));

                    //設定する
                    bmp.SetPropertyItem(pi);
                    break;
                }
            }

            if (!isFindExif) WriteLine("Exif発見不可");

            //保存する

            //string savePath = _imagePath.Replace("Google フォト", "Google フォト_Revised");
            string savePath = masterSaveFilePath + System.IO.Path.GetFileName(_imagePath);


            if (!Directory.Exists(savePath.Substring(0, savePath.LastIndexOf(@"\"))))
            {
                Directory.CreateDirectory(savePath.Substring(0, savePath.LastIndexOf(@"\")));
            }

            if (File.Exists(savePath))
            {
                int l = 1;
                while (File.Exists(savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").jpg"))
                {
                    l++;
                }
                savePath = savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").jpg";
            }

            bmp.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);

            bmp.Dispose();

            System.IO.FileInfo fi = new System.IO.FileInfo(savePath);//ファイル作成日時も変更する　アルバムによってはこれを参考にする場合もある
            fi.CreationTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC",""));
            fi.LastWriteTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));
        }

        static void PNGwrite(string _imagePath)
        {
            string jsonpath;

            if (_imagePath.Contains("(1)"))//例外処理
            {
                if (File.Exists(_imagePath.Replace("(1)", "") + "(1).json"))
                {
                    jsonpath = _imagePath.Replace("(1)", "") + "(1).json";
                }
                else
                {
                    jsonpath = _imagePath + ".json";
                }
            }
            else if (System.IO.Path.GetFileName(_imagePath).Length + 1 > 51)
            {
                jsonpath = _imagePath.Substring(0, _imagePath.LastIndexOf(@"\") + 1) + System.IO.Path.GetFileName(_imagePath).Substring(0, 46) + ".json";
            }
            else
            {
                jsonpath = _imagePath + ".json";
            }

            jsonpath = jsonpath.Replace("-編集済み", "");//googleは編集済みにjsonないっぽい

            //string savePath = _imagePath.Replace("Google フォト", "Google フォト_Revised");
            string savePath = masterSaveFilePath + System.IO.Path.GetFileName(_imagePath);

            if (File.Exists(savePath))
            {
                int l = 1;
                while (File.Exists(savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").png"))
                {
                    l++;
                }
                savePath = savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").png";
            }

            System.IO.File.Copy(_imagePath, savePath, true);

            System.IO.FileInfo fi = new System.IO.FileInfo(savePath);//ファイル作成日時も変更する　アルバムによってはこれを参考にする場合もある
            fi.CreationTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));
            fi.LastWriteTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));

        }

        static void MP4write(string _imagePath)
        {
            string jsonpath;

            if (_imagePath.Contains("(1)"))//例外処理
            {
                if (File.Exists(_imagePath.Replace("(1)", "") + "(1).json"))
                {
                    jsonpath = _imagePath.Replace("(1)", "") + "(1).json";
                }
                else
                {
                    jsonpath = _imagePath + ".json";
                }
            }
            else if (System.IO.Path.GetFileName(_imagePath).Length + 1 > 51)
            {
                jsonpath = _imagePath.Substring(0, _imagePath.LastIndexOf(@"\") + 1) + System.IO.Path.GetFileName(_imagePath).Substring(0, 46) + ".json";
            }
            else
            {
                jsonpath = _imagePath + ".json";
            }

            jsonpath = jsonpath.Replace("-編集済み", "");//googleは編集済みにjsonないっぽい

            //string savePath = _imagePath.Replace("Google フォト", "Google フォト_Revised");
            string savePath = masterSaveFilePath + System.IO.Path.GetFileName(_imagePath);

            if (File.Exists(savePath))
            {
                int l = 1;
                while (File.Exists(savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").mp4"))
                {
                    l++;
                }
                savePath = savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").mp4";
            }

            System.IO.File.Copy(_imagePath, savePath, true);

            System.IO.FileInfo fi = new System.IO.FileInfo(savePath);//ファイル作成日時も変更する　アルバムによってはこれを参考にする場合もある
            fi.CreationTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));
            fi.LastWriteTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));

        }

        static void GIFwrite(string _imagePath)
        {
            string jsonpath;

            if (_imagePath.Contains("(1)"))//例外処理
            {
                if (File.Exists(_imagePath.Replace("(1)", "") + "(1).json"))
                {
                    jsonpath = _imagePath.Replace("(1)", "") + "(1).json";
                }
                else
                {
                    jsonpath = _imagePath + ".json";
                }
            }
            else if (System.IO.Path.GetFileName(_imagePath).Length + 1 > 51)
            {
                jsonpath = _imagePath.Substring(0, _imagePath.LastIndexOf(@"\") + 1) + System.IO.Path.GetFileName(_imagePath).Substring(0, 46) + ".json";
            }
            else
            {
                jsonpath = _imagePath + ".json";
            }

            jsonpath = jsonpath.Replace("-編集済み", "");//googleは編集済みにjsonないっぽい

            //string savePath = _imagePath.Replace("Google フォト", "Google フォト_Revised");
            string savePath = masterSaveFilePath + System.IO.Path.GetFileName(_imagePath);

            if (File.Exists(savePath))
            {
                int l = 1;
                while (File.Exists(savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").gif"))
                {
                    l++;
                }
                savePath = savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").gif";
            }

            System.IO.File.Copy(_imagePath, savePath, true);

            System.IO.FileInfo fi = new System.IO.FileInfo(savePath);//ファイル作成日時も変更する　アルバムによってはこれを参考にする場合もある
            fi.CreationTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));
            fi.LastWriteTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));

        }

        static void _3GPwrite(string _imagePath)
        {
            string jsonpath;

            if (_imagePath.Contains("(1)"))//例外処理
            {
                if (File.Exists(_imagePath.Replace("(1)", "") + "(1).json"))
                {
                    jsonpath = _imagePath.Replace("(1)", "") + "(1).json";
                }
                else
                {
                    jsonpath = _imagePath + ".json";
                }
            }
            else if (System.IO.Path.GetFileName(_imagePath).Length + 1 > 51)
            {
                jsonpath = _imagePath.Substring(0, _imagePath.LastIndexOf(@"\") + 1) + System.IO.Path.GetFileName(_imagePath).Substring(0, 46) + ".json";
            }
            else
            {
                jsonpath = _imagePath + ".json";
            }

            jsonpath = jsonpath.Replace("-編集済み", "");//googleは編集済みにjsonないっぽい

            //string savePath = _imagePath.Replace("Google フォト", "Google フォト_Revised");
            string savePath = masterSaveFilePath + System.IO.Path.GetFileName(_imagePath);

            if (File.Exists(savePath))
            {
                int l = 1;
                while (File.Exists(savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").3gp"))
                {
                    l++;
                }
                savePath = savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 7 : 4)) + "(" + l + ").3gp";
            }

            System.IO.File.Copy(_imagePath, savePath, true);

            System.IO.FileInfo fi = new System.IO.FileInfo(savePath);//ファイル作成日時も変更する　アルバムによってはこれを参考にする場合もある
            fi.CreationTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));
            fi.LastWriteTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));

        }

        static void WEBPwrite(string _imagePath)
        {
            string jsonpath;

            if (_imagePath.Contains("(1)"))//例外処理
            {
                if (File.Exists(_imagePath.Replace("(1)", "") + "(1).json"))
                {
                    jsonpath = _imagePath.Replace("(1)", "") + "(1).json";
                }
                else
                {
                    jsonpath = _imagePath + ".json";
                }
            }
            else if (System.IO.Path.GetFileName(_imagePath).Length > 51)
            {
                jsonpath = _imagePath.Substring(0, _imagePath.LastIndexOf(@"\") + 1) + System.IO.Path.GetFileName(_imagePath).Substring(0, 46) + ".json";
            }
            else
            {
                jsonpath = _imagePath + ".json";
            }

            jsonpath = jsonpath.Replace("-編集済み", "");//googleは編集済みにjsonないっぽい

            //string savePath = _imagePath.Replace("Google フォト", "Google フォト_Revised");
            string savePath = masterSaveFilePath + System.IO.Path.GetFileName(_imagePath);

            if (File.Exists(savePath))
            {
                int l = 1;
                while (File.Exists(savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 8 : 5)) + "(" + l + ").webp"))
                {
                    l++;
                }
                savePath = savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 8 : 5)) + "(" + l + ").webp";
            }

            System.IO.File.Copy(_imagePath, savePath, true);

            System.IO.FileInfo fi = new System.IO.FileInfo(savePath);//ファイル作成日時も変更する　アルバムによってはこれを参考にする場合もある
            fi.CreationTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));
            fi.LastWriteTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));

        }

        static void JPEGwrite(string _imagePath)
        {
            string jsonpath;

            if (_imagePath.Contains("(1)"))//例外処理
            {
                if (File.Exists(_imagePath.Replace("(1)", "") + "(1).json"))
                {
                    jsonpath = _imagePath.Replace("(1)", "") + "(1).json";
                }
                else
                {
                    jsonpath = _imagePath + ".json";
                }
            }
            else if (System.IO.Path.GetFileName(_imagePath).Length > 51)
            {
                jsonpath = _imagePath.Substring(0, _imagePath.LastIndexOf(@"\") + 1) + System.IO.Path.GetFileName(_imagePath).Substring(0, 46) + ".json";
            }
            else
            {
                jsonpath = _imagePath + ".json";
            }

            jsonpath = jsonpath.Replace("-編集済み", "");//googleは編集済みにjsonないっぽい

            //string savePath = _imagePath.Replace("Google フォト", "Google フォト_Revised");
            string savePath = masterSaveFilePath + System.IO.Path.GetFileName(_imagePath);

            if (File.Exists(savePath))
            {
                int l = 1;
                while (File.Exists(savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 8 : 5)) + "(" + l + ").jpeg"))
                {
                    l++;
                }
                savePath = savePath.Substring(0, savePath.Length - (savePath.Contains("(") ? 8 : 5)) + "(" + l + ").jpeg";
            }

            System.IO.File.Copy(_imagePath, savePath, true);

            System.IO.FileInfo fi = new System.IO.FileInfo(savePath);//ファイル作成日時も変更する　アルバムによってはこれを参考にする場合もある
            fi.CreationTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));
            fi.LastWriteTime = DateTime.Parse(readJsonPhotoTakenTime(jsonpath).Replace("UTC", ""));

        }




        static string readJsonPhotoTakenTime(string _jsonPath)
        {

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(_jsonPath))))
            {
                var serializer = new DataContractJsonSerializer(typeof(JsonClass));
                ms.Position = 0;
                var deserialized = (JsonClass)serializer.ReadObject(ms);

                return deserialized.photoTakenTime.formatted;
            }
        }

#region jsonclass
        [System.Runtime.Serialization.DataContractAttribute()]
        public partial class JsonClass
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string title;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string description;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string imageViews;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public CreationTime creationTime;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public ModificationTime modificationTime;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public GeoData geoData;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public GeoDataExif geoDataExif;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public PhotoTakenTime photoTakenTime;
        }

        // Type created for JSON at <<root>> --> creationTime
        [System.Runtime.Serialization.DataContractAttribute(Name = "creationTime")]
        public partial class CreationTime
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string timestamp;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string formatted;
        }

        // Type created for JSON at <<root>> --> modificationTime
        [System.Runtime.Serialization.DataContractAttribute(Name = "modificationTime")]
        public partial class ModificationTime
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string timestamp;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string formatted;
        }

        // Type created for JSON at <<root>> --> geoData
        [System.Runtime.Serialization.DataContractAttribute(Name = "geoData")]
        public partial class GeoData
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double latitude;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double longitude;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double altitude;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double latitudeSpan;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double longitudeSpan;
        }

        // Type created for JSON at <<root>> --> geoDataExif
        [System.Runtime.Serialization.DataContractAttribute(Name = "geoDataExif")]
        public partial class GeoDataExif
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double latitude;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double longitude;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double altitude;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double latitudeSpan;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double longitudeSpan;
        }

        // Type created for JSON at <<root>> --> photoTakenTime
        [System.Runtime.Serialization.DataContractAttribute(Name = "photoTakenTime")]
        public partial class PhotoTakenTime
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string timestamp;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string formatted;
        }
#endregion

    }
}
