using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Android;



public class ImageLoader : MonoBehaviour
{
    public Texture2D videoPlaceholder; //ÊÓÆµËõÂÔÍ¼ÁÙÊ±Õ¼Î»Í¼
    public RectTransform container; // ÄãµÄUIÈÝÆ÷£¬ÀýÈçÒ»¸öScrollRectµÄContent
    public GameObject imagePrefab;    // Ò»¸öRawImageÔ¤ÖÆÌå£¬ÓÃÓÚÏÔÊ¾Í¼Æ¬
    public bool ReadyToBuild = true;
    [Space]
    public Texture2D img_guidance;
    public Texture2D img_default;

    [SerializeField] private string folderPath;
    private VideoPlayer videoPlayer; // ÊÓÆµ²¥·ÅÆ÷
    private RenderTexture videoTexture;  // ÓÃÓÚ´æ´¢ÊÓÆµÖ¡µÄRenderTexture

    private Queue<string> videoPathsQueue = new Queue<string>();
    private bool isProcessing = false;


    private void Start()
    {
        if (string.IsNullOrEmpty(folderPath)) folderPath = "myfolder";
        videoTexture = new RenderTexture(1920, 1080, 0);
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.targetTexture = videoTexture;

        //if (ReadyToBuild) //°²×¿´ò°ü
        //{
        //    folderPath = "/sdcard/Pictures/3dMedia";

        //    RequestPermissions(); //ÇëÇóÐ´ÈëÈ¨ÏÞ
        //}
        //else //±à¼­Æ÷µ÷ÊÔ
        //{
        //    folderPath = Application.dataPath + "/Media/";

        //}
        Debug.Log("Pathfull : " + GetPlatformPath("myfolder"));
        Debug.Log("PathfullURI : " + GetPlatformURI("myfolder"));
        LoadAllImages();
    }

    //private void LoadAllImages()
    //{
    //    string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
    //                               .OrderBy(filePath => File.GetLastWriteTime(filePath))
    //                               .ToArray();

    //    StartCoroutine(LoadImagesInBatches(imageFiles, 2)); // ÒÔ2¸öÎÄ¼þÎªÒ»Åú½øÐÐ¼ÓÔØ

    //}
    //IEnumerator LoadImagesInBatches(string[] imageFiles, int batchSize)
    //{
    //    int currentBatch = 0;

    //    while (currentBatch * batchSize < imageFiles.Length)
    //    {
    //        int batchEnd = Mathf.Min((currentBatch + 1) * batchSize, imageFiles.Length);
    //        for (int i = currentBatch * batchSize; i < batchEnd; i++)
    //        {
    //            string imagePath = imageFiles[i];
    //            string extension = Path.GetExtension(imagePath).ToLower();

    //            if (extension == ".png" || extension == ".jpg")
    //            {
    //                yield return StartCoroutine(LoadImage(imagePath));
    //            }
    //            else if (extension == ".mp4")
    //            {
    //                AddVideoToQueue(imagePath);
    //                // ¿ÉÒÔÑ¡ÔñÔÚ´Ë´¦Ìí¼ÓÊÓÆµ¼ÓÔØÂß¼­
    //            }
    //        }
    //        currentBatch++;
    //        yield return null; // µÈ´ýÒ»Ö¡»ò¸ù¾ÝÐèÒª¸ü³¤Ê±¼ä
    //    }
    //}

    //×¥È¡Í¼Æ¬
    //private IEnumerator LoadImage(string imagePath)
    //{
    //    UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + imagePath);
    //    yield return uwr.SendWebRequest();

    //    if (uwr.result != UnityWebRequest.Result.ConnectionError && uwr.result != UnityWebRequest.Result.DataProcessingError)
    //    {
    //        Texture2D RawTexture = DownloadHandlerTexture.GetContent(uwr);
    //        Texture2D texture = TextureUtilities.ResizeTexture(RawTexture, 205f); //ËõÐ¡Í¼Æ¬³ß´ç
    //        Destroy(RawTexture);

    //        GameObject imageInstance = Instantiate<GameObject>(imagePrefab, container);
    //        imageInstance.GetComponent<MediaAttributes>().ImagePath = "file://" + imagePath;
    //        imageInstance.GetComponent<MediaAttributes>().IsVideo = false;


    //        imageInstance.transform.Find("Time").GetComponent<TextMeshProUGUI>().text = " "; //ÕÕÆ¬²»ÏÔÊ¾ÊÓÆµÊ±³¤

    //        RawImage childRawImage = imageInstance.transform.Find("Img").GetComponent<RawImage>();
    //        childRawImage.uvRect = ResizeTexUVRect(childRawImage, texture);
    //    }
    //    else
    //    {
    //        Debug.LogError("Error loading image: " + uwr.error);
    //    }
    //    uwr.Dispose(); // ÊÍ·ÅUnityWebRequestÊ¹ÓÃµÄ×ÊÔ´
    //}

    //×¥È¡ÊÓÆµ
    public void AddVideoToQueue(string videoPath)
    {
        videoPathsQueue.Enqueue(videoPath);

        if (!isProcessing)
        {
            StartCoroutine(ProcessVideos());
        }
    }
    private IEnumerator ProcessVideos() //ÊÓÆµ¼ÓÔØÁ÷
    {
        isProcessing = true;

        while (videoPathsQueue.Count > 0)
        {
            string currentVideoPath = videoPathsQueue.Dequeue();
            yield return LoadThumbnailFromVideo(currentVideoPath);
        }

        isProcessing = false;
    }
    private IEnumerator LoadThumbnailFromVideo(string videoPath) //ÊÓÆµËõÂÔÍ¼
    {
        //´´½¨ÊµÀý
        GameObject imageInstance = Instantiate<GameObject>(imagePrefab, container);
        imageInstance.GetComponent<MediaAttributes>().ImagePath = "file://" + videoPath;
        imageInstance.GetComponent<MediaAttributes>().IsVideo = true;

        RawImage childRawImage = imageInstance.transform.Find("Img").GetComponent<RawImage>();

        if (childRawImage != null)
        {
            childRawImage.texture = videoPlaceholder;
        }

        //¼ÓÔØÊÓÆµ
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.playOnAwake = false;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null; // µÈ´ýÊÓÆµ×¼±¸Íê±Ï
        }

        double videoLength = videoPlayer.length;// »ñÈ¡ÊÓÆµÊ±³¤
        string minutes = Mathf.Floor((float)(videoLength / 60)).ToString("00");
        string seconds = Mathf.Floor((float)(videoLength % 60)).ToString("00");
        imageInstance.transform.Find("Time").GetComponent<TextMeshProUGUI>().text = minutes + ":" + seconds; //ÏÔÊ¾ÊÓÆµÊ±³¤

        RenderTexture tempRenderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
        videoPlayer.targetTexture = tempRenderTexture; // ÉèÖÃVideoPlayerµÄÄ¿±êÎÆÀíÎªÐÂ´´½¨µÄRenderTexture

        Texture2D videoFrame = new Texture2D((int)videoPlayer.width, (int)videoPlayer.height);
        videoPlayer.frame = 0; // »ñÈ¡ÊÓÆµµÄµÚÒ»Ö¡
        videoPlayer.SetDirectAudioMute(0, true); //¾²Òô

        videoPlayer.Play();
        yield return new WaitForSeconds(0.5f); //µÈ´ý0.1ÃëÒÔÈ·±£µÚÒ»Ö¡±»äÖÈ¾
        videoPlayer.Stop();

        RenderTexture.active = tempRenderTexture;  // ÉèÖÃtempRenderTextureÎªµ±Ç°»îÔ¾µÄRenderTexture
        videoFrame.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);

        videoFrame.Apply();

        //ÓÃÊµ¼ÊµÄËõÂÔÍ¼Ìæ´úÕ¼Î»Í¼
        childRawImage.texture = videoFrame;
        childRawImage.uvRect = ResizeTexUVRect(childRawImage, videoFrame);

        // ÊÍ·Å×ÊÔ´
        videoPlayer.targetTexture = null;
        RenderTexture.active = null;
        Destroy(tempRenderTexture);
    }

    private Rect ResizeTexUVRect(RawImage childRawImage, Texture2D texture)
    { //µ÷ÕûÍ¼Æ¬³ß´ç&Î»ÖÃ
        if (childRawImage != null)
        {
            childRawImage.texture = texture;

            float aspectRatio = (float)texture.width / texture.height;
            Rect newUVRect = childRawImage.uvRect;

            if (aspectRatio > 2f) //ºáÏòÍ¼Ïñ
            {
                newUVRect.x = 0.25f - (newUVRect.width / 2.0f);
                newUVRect.y = 0f;
                newUVRect.width = (float)texture.height / texture.width;
                newUVRect.height = 1f;

            }
            else if (aspectRatio == 2f) //·½ÐÎÍ¼Ïñ
            {
                newUVRect.x = 0f;
                newUVRect.y = 0f;
                newUVRect.width = 0.5f;
                newUVRect.height = 1f;
            }
            else //ÊúÏòÍ¼Ïñ
            {
                newUVRect.x = 0f;
                newUVRect.y = 0.33f;
                newUVRect.width = 0.5f;
                newUVRect.height = 0.66f;
            }

            return newUVRect;
        }
        return new Rect(0, 0, 1, 1);
    }

    void RequestPermissions() //¼ì²éÐ´ÈëÍâ²¿´æ´¢µÄÈ¨ÏÞ
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        else
        {
            OnPermissionGranted();
        }
    }

    void OnApplicationFocus(bool hasFocus) //ÖØÐÂ»ñµÃ½¹µãÊ±
    {
        if (hasFocus && ReadyToBuild)
        {
            RequestPermissions(); //ÔÙ¼ì²éÈ¨ÏÞ
        }
    }

    void OnPermissionGranted() //ÒÑ»ñµÃÈ¨ÏÞ£¬¿ªÊ¼Ð´Èë
    {
        if (!Directory.Exists(folderPath)) //ÎÄ¼þ¼ÐÂ·¾¶²»´æÔÚ
        {
            Directory.CreateDirectory(folderPath); //´´½¨ÎÄ¼þ¼Ð
            SaveTextureToDisk(img_guidance, "/sdcard/Pictures/3dMedia/img_guidance.png");
            SaveTextureToDisk(img_default, "/sdcard/Pictures/3dMedia/img_default.png");
            LoadAllImages();
        }
        else
        {
            string[] entries = Directory.GetFileSystemEntries(folderPath);
            if (entries.Length == 0) //ÎÄ¼þ¼ÐÎª¿Õ
            {
                SaveTextureToDisk(img_guidance, "/sdcard/Pictures/3dMedia/img_guidance.png");
                SaveTextureToDisk(img_default, "/sdcard/Pictures/3dMedia/img_default.png");
                LoadAllImages();
            }
            else
            {
                LoadAllImages();
            }
        }

    }

    void SaveTextureToDisk(Texture2D texture, string filePath) //±£´æÍ¼Æ¬µ½±¾»ú
    {
        try
        {
            // »ñÈ¡Ô­Ê¼ÎÆÀíµÄÏñËØÊý¾Ý
            Color[] pixels = texture.GetPixels();

            // ´´½¨ÐÂµÄ Texture2D ¶ÔÏó
            Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

            // ½«ÏñËØÊý¾ÝÉèÖÃµ½ÐÂµÄ Texture2D ¶ÔÏóÖÐ
            readableTexture.SetPixels(pixels);
            readableTexture.Apply();


            Graphics.CopyTexture(texture, readableTexture);
            byte[] textureBytes = readableTexture.EncodeToPNG();
            if (textureBytes != null)
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllBytes(filePath, textureBytes);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception while saving texture: " + e.Message);
        }
    }

    #region my code 
    private void LoadAllImages()
    {
        // ✅ Cross-platform path check
        string fullPath = GetPlatformPath(folderPath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError("❌ Folder does not exist: " + fullPath);
            return;
        }

        string[] imageFiles = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
                                    .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".mp4"))
                                    .OrderBy(f => File.GetLastWriteTime(f))
                                    .ToArray();

        StartCoroutine(LoadImagesInBatches(imageFiles, 2)); // 2 files per batch
    }

    private IEnumerator LoadImagesInBatches(string[] imageFiles, int batchSize)
    {
        int currentBatch = 0;

        while (currentBatch * batchSize < imageFiles.Length)
        {
            int batchEnd = Mathf.Min((currentBatch + 1) * batchSize, imageFiles.Length);

            for (int i = currentBatch * batchSize; i < batchEnd; i++)
            {
                string path = imageFiles[i];
                string extension = Path.GetExtension(path).ToLower();

                if (extension == ".png" || extension == ".jpg")
                    yield return StartCoroutine(LoadImage(path));
                else if (extension == ".mp4")
                    AddVideoToQueue(path);
            }

            currentBatch++;
            yield return null;
        }
    }

    private IEnumerator LoadImage(string imagePath)
    {
        string uri = GetPlatformURI(imagePath);

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D rawTex = DownloadHandlerTexture.GetContent(uwr);
                Texture2D texture = TextureUtilities.ResizeTexture(rawTex, 205f);
                Destroy(rawTex);

                GameObject imageInstance = Instantiate(imagePrefab, container);
                imageInstance.GetComponent<MediaAttributes>().ImagePath = uri;
                imageInstance.GetComponent<MediaAttributes>().IsVideo = false;

                imageInstance.transform.Find("Time").GetComponent<TextMeshProUGUI>().text = " ";

                RawImage childRawImage = imageInstance.transform.Find("Img").GetComponent<RawImage>();
                childRawImage.texture = texture;
                childRawImage.uvRect = ResizeTexUVRect(childRawImage, texture);
            }
            else
            {
                Debug.LogError("❌ Error loading image: " + uwr.error + " at " + uri);
            }
        }
    }

    // 🔧 Platform helpers
    private string GetPlatformPath(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return Path.Combine(Application.persistentDataPath, path);
#elif UNITY_IOS && !UNITY_EDITOR
        return Path.Combine(Application.persistentDataPath, path);
#else
        return Path.Combine(Application.dataPath, path);
#endif
    }

    private string GetPlatformURI(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return "file://" + path;
#elif UNITY_IOS && !UNITY_EDITOR
        return "file://" + path;
#else
        return "file://" + path.Replace("\\", "/");
#endif
    }

    
    #endregion
}
 