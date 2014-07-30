using System;
using Android.Graphics;
using System.Drawing;
using Android.Content.Res;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace cameraf2b
{
    public class ImageHelper
    {
        private static Size GetImageSizeFromArray(byte[] imgBuffer)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;

            BitmapFactory.DecodeByteArray(imgBuffer, 0, imgBuffer.Length, options);

            return new Size(options.OutWidth, options.OutHeight);
        }

        public static int CalculateSampleSizePower2(Size originalSize, int reqWidth, int reqHeight)
        {
            int height = originalSize.Height;
            int width = originalSize.Width;
            int IMAGE_MAX_SIZE = reqWidth >= reqHeight ? reqWidth : reqHeight;

            int inSampleSize = 1;

            if (height > IMAGE_MAX_SIZE || width > IMAGE_MAX_SIZE)
            {
                inSampleSize = (int)Math.Pow(2, (int)Math.Round(Math.Log(IMAGE_MAX_SIZE /
                (double)Math.Max(height, width)) / Math.Log(0.5)));
            }

            return inSampleSize;
        }

        private static int CalculateSampleSize(Size originalSize, int reqWidth, int reqHeight)
        {
            int sampleSize = 1;

            if (originalSize.Height > reqHeight || originalSize.Width > reqWidth)
                sampleSize = Convert.ToInt32(originalSize.Width > originalSize.Height ? 
                    (double)originalSize.Height / (double)reqHeight : (double)originalSize.Width / (double)reqWidth);

            return sampleSize;

        }
        //end static int CalculateSampleSize

        public static Bitmap CreateUserProfileImageForDisplay(byte[] userImage, int width, int height, Resources res)
        {
            if (userImage.Length > 0 && userImage.Length != 2)
            {
                Size imgSize = GetImageSizeFromArray(userImage);

                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InSampleSize = CalculateSampleSizePower2(imgSize, width, height);
                Bitmap scaledUserImage = BitmapFactory.DecodeByteArray(userImage, 0, userImage.Length, options);

                int scaledWidth = scaledUserImage.Width;
                int scaledHeight = scaledUserImage.Height;

                Bitmap resultImage = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeResource(res, Resource.Drawable.dummy), scaledWidth, scaledHeight, true);
                using (Canvas canvas = new Canvas(resultImage))
                {
                    using (Paint paint = new Paint(PaintFlags.AntiAlias))
                    {
                        paint.Dither = false;
                        paint.FilterBitmap = true;
                        paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.DstIn));
                        canvas.DrawBitmap(scaledUserImage, 0, 0, null);
                        scaledUserImage.Recycle();
								
                        using (Bitmap maskImage = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeResource(res, Resource.Drawable.emptybackground), scaledWidth, scaledHeight, true))
                        {
                            canvas.DrawBitmap(maskImage, 0, 0, paint);
                            maskImage.Recycle();
                        }
                    }
                }
                return resultImage;
            }
            else
            {
                return null;
            }

        }
        //end static Bitmap CreateUserProfileImageFordisplay

        public static void fontSizeInfo(Context context)
        {
            int imageSizeX = 0;
            using (BitmapFactory.Options opt = new BitmapFactory.Options())
            {
                opt.InJustDecodeBounds = true;
                BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.headerlogo, opt);
                imageSizeX = opt.OutWidth + (int)convertDpToPixel(12f, context);
            }
            float sizeLeft = (float)CameraTakePictureActivity.ScreenX - (float)imageSizeX;
            if (sizeLeft < 0)
                return;
            Paint paint = new Paint(PaintFlags.AntiAlias);
            paint.TextSize = convertDpToPixel((float)Header.fontsize, context);
            paint.SetTypeface(Typeface.DefaultBold);
            float width = paint.MeasureText(Header.headertext);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("width = {0}, sizeLeft = {1}, textSize = {2}", width, sizeLeft, paint.TextSize);
#endif
            while (width >= sizeLeft)
            {
                Header.fontsize -= convertDpToPixel(1f, context);
                paint.TextSize = convertDpToPixel((float)Header.fontsize, context);
                width = paint.MeasureText(Header.headertext);
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("new width = {0}", width);
                #endif
            }

#if DEBUG
            Console.WriteLine("Header.fontsize = {0}", Header.fontsize);
#endif
            /*if (sizeLeft < width) {
				width -= convertDpToPixel (.5f, context);
				paint.TextSize = width;
				width = convertDpToPixel (paint.MeasureText (Header.headertext), context);
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("new width = {0}", convertDpToPixel (width, context));
				#endif
			}*/
        }

        public static void setupTopPanel(ImageView buttons, TextView text, RelativeLayout layout, Context context)
        {
            layout.RemoveAllViewsInLayout();
            using (LinearLayout overall = new LinearLayout(context))
            {
                overall.Orientation = Android.Widget.Orientation.Horizontal;
                overall.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                {
                    lp.SetMargins((int)convertDpToPixel(6f, context), 0, (int)convertDpToPixel(6f, context), 0);
                    buttons.LayoutParameters = lp;
                }
                using (LinearLayout l = new LinearLayout(context))
                {
                    l.LayoutParameters = new LinearLayout.LayoutParams((int)convertDpToPixel(60f, context), LinearLayout.LayoutParams.MatchParent);
                    l.AddView(buttons);
                    overall.AddView(l);
                }
                using (LinearLayout m = new LinearLayout(context))
                {
                    float x = (float)CameraTakePictureActivity.ScreenX - convertDpToPixel(120f, context);
                    m.LayoutParameters = new LinearLayout.LayoutParams((int)x, LinearLayout.LayoutParams.MatchParent);
                    m.SetGravity(GravityFlags.CenterHorizontal);
                    m.AddView(text);
                    overall.AddView(m);
                }
                layout.AddView(overall);
            }
        }

        public static void setupButtonsPosition<T>(T[] buttons, LinearLayout layout, Context context) where T : View
        {
            layout.RemoveAllViewsInLayout();
            float eachSection = (float)CameraTakePictureActivity.ScreenX / buttons.Length;
            foreach (T btn in buttons)
            {
                int tag = (int)btn.Tag;
                using (LinearLayout l = new LinearLayout(context))
                {
                    l.LayoutParameters = new LinearLayout.LayoutParams((int)eachSection, LinearLayout.LayoutParams.MatchParent);
                    if (tag == 0)
                        l.SetGravity(GravityFlags.Left);
                    if (tag == buttons.Length - 1)
                        l.SetGravity(GravityFlags.Right);
                    if (tag != buttons.Length - 1 && tag != 0)
                        l.SetGravity(GravityFlags.Center);

                    l.AddView(btn);
                    layout.AddView(l);
                }
            }
        }

        public static void resizeLayout(LinearLayout layout, Context c)
        {
            int[] newSize = new int[2];
            using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams(newSize[0], newSize[1]))
            {
                layout.LayoutParameters = layParams;
            }
        }

        private static float resizeFont(string text, float size, float btnWidth, Context context)
        {
            Paint paint = new Paint(PaintFlags.AntiAlias);
            paint.TextSize = size;
            paint.SetTypeface(Typeface.DefaultBold);
            float width = convertDpToPixel(paint.MeasureText(text), context);
            float pxWidth = convertDpToPixel(btnWidth, context);
            while (width <= pxWidth)
            {
                size += .5f;
                paint.TextSize = size;
                width = convertDpToPixel(paint.MeasureText(text), context);
            }
			
            return size;
        }


        public static byte[] convColToByteArray(Android.Graphics.Color color)
        {
            byte[] toReturn = new byte[4];
            toReturn[0] = color.R;
            toReturn[1] = color.G;
            toReturn[2] = color.B;
            toReturn[3] = color.A;
            return toReturn;
        }


        public static float getNewFontSize(float size, Context c)
        {
            float fSize = convertDpToPixel(size, c);
            float calc = fSize + (fSize * .1f);
            return convertPixelToDp(calc, c);
        }

        public static float convertDpToPixel(float dp, Context context)
        {
            Android.Util.DisplayMetrics metrics = context.Resources.DisplayMetrics;
            return dp * ((float)metrics.DensityDpi / 160f);
        }

        public static float convertPixelToDp(float px, Context context)
        {
            Android.Util.DisplayMetrics metrics = context.Resources.DisplayMetrics;
            return (px * 160f) / (float)metrics.DensityDpi;
        }
    }
}

