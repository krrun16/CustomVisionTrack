using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
//using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Util;
using Plugin.TextToSpeech;
using System.IO;

namespace CustomVision
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/MyTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity_custom : AppCompatActivity, TextureView.ISurfaceTextureListener, Camera.IPreviewCallback
    {
        /*private CameraDevice camera;
        private ISurfaceHolder mHolder;*/

        private Camera cameraMain;
        private TextureView textureViewCamera;
        protected int previewWidth = 0;
        protected int previewHeight = 0;
        private bool isProcessingFrame = false;
        private byte[] lastPreviewFrame;
        private byte[][] yuvBytes = new byte[3][];
        private int[] rgbBytes = null;
        private int yRowStride;
        private Android.Graphics.Bitmap rgbFrameBitmap = null;
        private static int CAMERA_PORTRAIT_ROTATION = 90;

        readonly ImageClassifier imageClassifier = new ImageClassifier();



        public Android.Support.V7.Widget.Toolbar Toolbar { get; set; }

        /*Button takePhotoButton;
        private Button TakePhotoButton => takePhotoButton ?? (takePhotoButton = FindViewById<Button>(Resource.Id.take_photo_button));

        ImageView photoView;
        private ImageView PhotoView => photoView ?? (photoView = FindViewById<ImageView>(Resource.Id.photo));

        TextView resultLabel;
        private TextView ResultLabel => resultLabel ?? (resultLabel = FindViewById<TextView>(Resource.Id.result_label));

        ProgressBar progressBar;
        private ProgressBar ProgressBar => progressBar ?? (progressBar = FindViewById<ProgressBar>(Resource.Id.progressbar));

        readonly ImageClassifier imageClassifier = new ImageClassifier();

        private TextureView textureView;
        private String cameraId;*/
       


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            textureViewCamera = new TextureView(this);
            textureViewCamera.SurfaceTextureListener = this;

            SetContentView(textureViewCamera);
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            
        }

        public void startCamera(Android.Graphics.SurfaceTexture surface,int w, int h)
        {
            int cameraNumber = 0;
            Camera.CameraInfo camInfo = new Camera.CameraInfo();
            for (int i = 0; i < Camera.NumberOfCameras; i++)
            {
                Camera.GetCameraInfo(i, camInfo);
                if (camInfo.Facing == CameraFacing.Back)
                {
                    cameraNumber = i;
                    break;
                }
            }
            cameraMain = Camera.Open(cameraNumber);

            textureViewCamera.LayoutParameters = new FrameLayout.LayoutParams(w, h);

            try
            {
                cameraMain.SetPreviewTexture(surface);

                var p = cameraMain.GetParameters();
                p.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
                cameraMain.SetParameters(p);
                cameraMain.SetDisplayOrientation(CAMERA_PORTRAIT_ROTATION);
                cameraMain.SetPreviewCallback(this);
                //cameraMain.SetOneShotPreviewCallback(this);
                cameraMain.StartPreview();

            }
            catch (Java.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h)
        {
            startCamera(surface,w,h);
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            cameraMain.StopPreview();
            cameraMain.Release();

            return true;
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            // camera takes care of this

        }

        public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {



        }


        /*        async void TakePhotoButton_Click(object sender, EventArgs e)
                {
                    TakePhotoButton.Enabled = false;
                    ProgressBar.Visibility = ViewStates.Visible;

                    try
                    {
                        var image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { PhotoSize = PhotoSize.Medium });
                        var bitmap = await BitmapFactory.DecodeStreamAsync(image.GetStreamWithImageRotatedForExternalStorage());

                        PhotoView.SetImageBitmap(bitmap);
                        var result = await Task.Run(() => imageClassifier.RecognizeImage(bitmap));
        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        CrossTextToSpeech.Current.Speak($"I think it is {result}");
        #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        ResultLabel.Text = result;
                    }
                    finally
                    {
                        TakePhotoButton.Enabled = true;
                        ProgressBar.Visibility = ViewStates.Invisible;
                    }
                }*/

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) => PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        int count = 0;
        public void OnPreviewFrame(byte[] data, Camera camera)
        {
            Log.Error("Rumi","Preview frame received");
            if (isProcessingFrame == true)
            {
                count++;
                if(count == 30)
                {
                    isProcessingFrame = false;
                    count = 0;
                }
                Log.Error("OnPreviewFrame", "Dropping frame!");
                return;
            }
           

            try
            {
                // Initialize the storage bitmaps once when the resolution is known.
                if (rgbBytes == null)
                {
                    Camera.Size previewSize = cameraMain.GetParameters().PreviewSize;
                    previewHeight = previewSize.Height;
                    previewWidth = previewSize.Width;
                    rgbBytes = new int[previewWidth * previewHeight];
                    //onPreviewSizeChosen(new Size(previewSize.width, previewSize.height), 90);
                }
            }
            catch (Exception e) {
                Log.Error("OnPreviewFrame", "Exception!");
                return;
            }

            //if(count == 20)
            isProcessingFrame = true;
            lastPreviewFrame = data;
            yuvBytes[0] = data;
            yRowStride = previewWidth;
            System.Threading.Tasks.Task.Run(() =>
            {
                //Add your code here.
                convertYUV420SPToARGB8888(data, previewWidth, previewHeight, rgbBytes);
                rgbFrameBitmap = Android.Graphics.Bitmap.CreateBitmap(previewWidth, previewHeight, Android.Graphics.Bitmap.Config.Argb8888);
                rgbFrameBitmap.SetPixels(rgbBytes, 0, previewWidth, 0, 0, previewWidth, previewHeight);
                //cameraMain.AddCallbackBuffer(data);
                //isProcessingFrame = false;

                Android.Graphics.Bitmap image = Android.Graphics.Bitmap.CreateBitmap(previewWidth, previewHeight, Android.Graphics.Bitmap.Config.Argb8888);
                byte[] bytes = null;
                using (var stream = new MemoryStream())
                {
                    rgbFrameBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 0, stream);
                    bytes = stream.ToArray();
                }

                recognizeImage(rgbFrameBitmap, bytes);
            }).ConfigureAwait(false);
            /*convertYUV420SPToARGB8888(data, previewWidth, previewHeight, rgbBytes);
            rgbFrameBitmap = Android.Graphics.Bitmap.CreateBitmap(previewWidth, previewHeight, Android.Graphics.Bitmap.Config.Argb8888);
            rgbFrameBitmap.SetPixels(rgbBytes, 0, previewWidth, 0, 0, previewWidth, previewHeight);
            //cameraMain.AddCallbackBuffer(data);
            //isProcessingFrame = false;

            Android.Graphics.Bitmap image = Android.Graphics.Bitmap.CreateBitmap(previewWidth, previewHeight, Android.Graphics.Bitmap.Config.Argb8888);
            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {
                rgbFrameBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 0, stream);
                bytes = stream.ToArray();
            }

            recognizeImage(rgbFrameBitmap, bytes);*/
        }
        
        async void recognizeImage(Android.Graphics.Bitmap rgbBitmap, byte[] data)
        {
            Log.Error("Rumi","Recognize image");
            var sdcardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
            var FilePath = System.IO.Path.Combine(sdcardPath, "rumi"+ SystemClock.CurrentThreadTimeMillis()+".png");//Φάκελος και εικόνα
            if (!System.IO.File.Exists(FilePath))
            {
                File.WriteAllBytes(FilePath, data);
                /*using (System.IO.StreamWriter writer = new System.IO.StreamWriter(FilePath, true))
                {
                    writer.WriteLine("blablabla");
                }*/
            }

            var result = await Task.Run(() => imageClassifier.RecognizeImage(rgbBitmap));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CrossTextToSpeech.Current.Speak($"{result}");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //ResultLabel.Text = result;
            Log.Error("Rumi","Result= "+result);
        }


        private static long YUVToRGB(int y, int u, int v)
        {
            int kMaxChannelValue = 262143;
            // Adjust and check YUV values
            y = (y - 16) < 0 ? 0 : (y - 16);
            u -= 128;
            v -= 128;

            // This is the floating point equivalent. We do the conversion in integer
            // because some Android devices do not have floating point in hardware.
            // nR = (int)(1.164 * nY + 2.018 * nU);
            // nG = (int)(1.164 * nY - 0.813 * nV - 0.391 * nU);
            // nB = (int)(1.164 * nY + 1.596 * nV);
            int y1192 = 1192 * y;
            int r = (y1192 + 1634 * v);
            int g = (y1192 - 833 * v - 400 * u);
            int b = (y1192 + 2066 * u);

            // Clipping RGB values to be inside boundaries [ 0 , kMaxChannelValue ]
            r = r > kMaxChannelValue ? kMaxChannelValue : (r < 0 ? 0 : r);
            g = g > kMaxChannelValue ? kMaxChannelValue : (g < 0 ? 0 : g);
            b = b > kMaxChannelValue ? kMaxChannelValue : (b < 0 ? 0 : b);

            
            return 0xff000000 | ((r << 6) & 0xff0000) | ((g >> 2) & 0xff00) | ((b >> 10) & 0xff);
        }


        public static void convertYUV420SPToARGB8888(
            byte[] input,
            int width,
            int height,
            int[] output)
        {

            int frameSize = width * height;
            for (int j = 0, yp = 0; j < height; j++)
            {
                int uvp = frameSize + (j >> 1) * width;
                int u = 0;
                int v = 0;

                for (int i = 0; i < width; i++, yp++)
                {
                    int y = 0xff & input[yp];
                    if ((i & 1) == 0)
                    {
                        v = 0xff & input[uvp++];
                        u = 0xff & input[uvp++];
                    }

                    output[yp] = (int)YUVToRGB(y, u, v);
                }
            }
        }
    

    }



}


