using System.Text;

using Android.App;
using Android.Content;

namespace cameraf2b
{
    public static class GeneralUtils
    {
        public static void Alert(Context context, string title, string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
        }

        public static void Alert(Context context, int title, int message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(Application.Context.Resources.GetString(message));
            builder.SetTitle(Application.Context.Resources.GetString(title));
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
        }

        public static void Alert(Context c, string title, string message, Activity parent)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(c);
            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
        }

        public static bool AlertRV(Context context, string title, string Message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(Message);
            builder.SetTitle(title);
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
            return true;
        }
    }
}

