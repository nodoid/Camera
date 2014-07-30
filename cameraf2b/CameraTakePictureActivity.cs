using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.IO;
using Android.Util;

namespace cameraf2b
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Label = "cameraf2b", MainLauncher = true, Icon = "@drawable/icon")]
    public class CameraTakePictureActivity : Activity, Android.Hardware.Camera.IAutoFocusCallback, Android.Hardware.Camera.IPictureCallback,
        Android.Hardware.Camera.IPreviewCallback, Android.Hardware.Camera.IShutterCallback, ISurfaceHolderCallback, IDisposable
    {
        Android.Hardware.Camera camera;
        string PictureName = "userImage";
        int number, camID;
        bool isRunning;
        Context context;
        ISurfaceHolder holder;

        public static int ScreenX { get ; private set; }

        public static int ScreenY { get ; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(metrics);

            ScreenX = metrics.WidthPixels;
            ScreenY = metrics.HeightPixels;

            number = 0;
            SetContentView(Resource.Layout.CameraTakePicScreen);
            isRunning = false;
            var header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            var btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            var relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.photoTitle);
            Header.fontsize = 32f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            var surface = FindViewById<SurfaceView>(Resource.Id.surfaceCameraView);

            holder = surface.Holder;
            holder.AddCallback(this);
            context = surface.Context;

            var takePicture = FindViewById<ImageButton>(Resource.Id.btnCamera);
            takePicture.Tag = 1;
            var flipView = FindViewById<ImageButton>(Resource.Id.imgFlipView);
            flipView.Tag = 2;
            var returnBack = FindViewById<ImageButton>(Resource.Id.imgBack);
            returnBack.Tag = 0;
            var bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
			
            var btn = new ImageButton[3];
            btn[0] = returnBack;
            btn[1] = takePicture;
            btn[2] = flipView;
			
            ImageHelper.setupButtonsPosition(btn, bottom, context);
			
            var cameraInfo = new Android.Hardware.Camera.CameraInfo();
            camera = null;

            int cameraCount = Android.Hardware.Camera.NumberOfCameras;
            if (cameraCount == 1)
                flipView.Visibility = ViewStates.Invisible;
            else
            {
                flipView.Click += delegate
                {
                    if (isRunning)
                    {
                        camera.StopPreview();
                        isRunning = false;
                        camera.Unlock();
				
                        if (cameraCount > 1 && camID < cameraCount - 1)
                            camID++;
                        else
                            camID--;

                        camera = Android.Hardware.Camera.Open(camID);
                        isRunning = true;

                        camera.Lock();
                        camera.StartPreview();
                    }
                };
            }
            takePicture.Click += delegate
            {
                var p = camera.GetParameters();
                p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                camera.SetParameters(p);
                camera.TakePicture(this, this, this);
            };

            returnBack.Click += (object sender, EventArgs e) =>
            {
                var resultData = new Intent();
                resultData.PutExtra("filename", fullFilename);
                SetResult(!string.IsNullOrEmpty(fullFilename) ? Result.Ok : Result.Canceled, resultData);
                Finish();
            };
        }

        public void OnAutoFocus(bool focused, Android.Hardware.Camera camera)
        {
            if (focused)
            {
                Toast.MakeText(context, Application.Context.Resources.GetString(Resource.String.commonFocused), ToastLength.Short).Show();
            }
        }

        public void OnShutter()
        {
        }

        private void openFrontFacingCamera()
        {
            int cameraCount = 0;
            var cameraInfo = new Android.Hardware.Camera.CameraInfo();
            cameraCount = Android.Hardware.Camera.NumberOfCameras;
            int result = 0;
            for (int camIdx = 0; camIdx < cameraCount; ++camIdx)
            {
                Android.Hardware.Camera.GetCameraInfo(camIdx, cameraInfo);
                var rotation = WindowManager.DefaultDisplay.Rotation;
                int degrees = 0;
                camID = camIdx;
                switch (rotation)
                {
                    case SurfaceOrientation.Rotation0:
                        degrees = 0;
                        break;
                    case SurfaceOrientation.Rotation90:
                        degrees = 90;
                        break;
                    case SurfaceOrientation.Rotation180:
                        degrees = 180;
                        break;
                    case SurfaceOrientation.Rotation270:
                        degrees = 270;
                        break;
                }
				
                if (cameraInfo.Facing == Android.Hardware.CameraFacing.Front)
                {
                    result = (cameraInfo.Orientation + degrees) % 360;
                    result = (360 - result) % 360;
                    try
                    {
                        camera = Android.Hardware.Camera.Open(camIdx);
                    }
                    catch (Java.Lang.RuntimeException)
                    {
                        RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                            Application.Context.Resources.GetString(Resource.String.photoNoConnection)));
                    }
                }
                else
                    result = (cameraInfo.Orientation - degrees + 360) % 360;
            }
            if (camera != null)
                camera.SetDisplayOrientation(result);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                openFrontFacingCamera();
                if (camera != null)
                {
                    var p = camera.GetParameters();
                    p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                    camera.SetParameters(p);
                    camera.AutoFocus(this);
                    camera.SetPreviewCallback(this);
                    camera.Lock();
                    camera.SetPreviewDisplay(holder);
                    camera.StartPreview();
                    isRunning = true;
                }
                else
                {
                    var resultData = new Intent();
                    SetResult(Result.Canceled, resultData);
                    FinishActivity(1);
                }
            }
            catch (IOException)
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                        Application.Context.Resources.GetString(Resource.String.errorNoPreview));
                });
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            if (camera != null)
            {
                camera.Unlock();
                camera.Release();
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format fmt, int width, int height)
        {
        }

        public void OnPreviewFrame(byte[] data, Android.Hardware.Camera camera)
        {
            // TO DO
        }

        private string fullFilename;

        public void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            FileOutputStream output = null;
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Images");
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
			
            string filename = PictureName + number.ToString() + ".jpg";
            if (data != null)
            {
                fullFilename = System.IO.Path.Combine(path, filename);
                number++;
                try
                {
                    output = new FileOutputStream(fullFilename);
                    output.Write(data);
                    output.Close();
                }
                catch (FileNotFoundException)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                            Application.Context.Resources.GetString(Resource.String.errorFileTransfer));
                    });
                    return;
                }
                catch (IOException)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                            Application.Context.Resources.GetString(Resource.String.errorNoImagesTaken));
                    });
                    isRunning = true;
                    camera.StartPreview();
                    return;
                }
                catch (Exception e)
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Exception thrown - {0}", e.Message);
                    #endif
                    return;
                }
				
                var f = new File(fullFilename);
                try
                {
                    var exifInterface = new ExifInterface(f.CanonicalPath);
                    exifInterface.GetAttribute(ExifInterface.TagModel);
                    var latLong = new float[2];
                    exifInterface.GetLatLong(latLong);
                    exifInterface.SetAttribute(ExifInterface.TagMake, "Phone picture");
                    exifInterface.SetAttribute(ExifInterface.TagDatetime, DateTime.Now.ToString());
                }
                catch (IOException)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                            Application.Context.Resources.GetString(Resource.String.errorStoreEXIF));
                    });
                    isRunning = true;
                    camera.StartPreview();
                    return;
                }
            }
            RunOnUiThread(() => Toast.MakeText(context, Application.Context.Resources.GetString(Resource.String.commonPictureTaken), ToastLength.Short).Show());
            isRunning = true;
            camera.StartPreview();
            return;
        }
    }
}