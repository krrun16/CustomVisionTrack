using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace CustomVision
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/MyTheme")]

    public class CustomMainActivity : AppCompatActivity, Android.Hardware.Camera.IPictureCallback, 
        Android.Hardware.Camera.IPreviewCallback, Android.Hardware.Camera.IShutterCallback, ISurfaceHolderCallback
    {
        public Android.Support.V7.Widget.Toolbar Toolbar { get; set; }

        public void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            throw new NotImplementedException();
        }
        

        public void OnPreviewFrame(byte[] data, Android.Hardware.Camera camera)
        {
            throw new NotImplementedException();
        }

        public void OnShutter()
        {
            throw new NotImplementedException();
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            throw new NotImplementedException();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            throw new NotImplementedException();
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            throw new NotImplementedException();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.custom_main);

            Toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            if (Toolbar != null)
                SetSupportActionBar(Toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);
            SurfaceView surface = (SurfaceView) FindViewById(Resource.Id.surface);

        }
    }
}