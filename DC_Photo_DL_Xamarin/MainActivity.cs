using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;

namespace DC_Photo_DL_Xamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);


            EditText linkTextBox = FindViewById<EditText>(Resource.Id.linkTextBox);
            
            Button btnDownload = FindViewById<Button>(Resource.Id.btnDownload);
            btnDownload.Click += DownloadOnClick;

            bool permissionState = CheckPermissions();
            if (!permissionState)
            {
                RequestPermissions();
            }

            CrossDownloadManager.Current.PathNameForDownloadedFile = new System.Func<IDownloadFile, string>(file => {
                string fileName = Android.Net.Uri.Parse(file.Url).Path.Split('/')[Android.Net.Uri.Parse(file.Url).Path.Split('/').Length - 1];
                return Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Pictures/Dreamcatcher_App_Downloader/", fileName);
            });

        }

        private bool CheckPermissions()
        {
            bool gotReadPermission = false;
            bool gotWritePermission = false;

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted)
            {
                gotReadPermission = true;
            }
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                gotWritePermission = true;
            }

            return (gotReadPermission && gotWritePermission);
        }

        private void RequestPermissions()
        {
            int permissions_code = 42;
            String[] permissions = { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
            ActivityCompat.RequestPermissions(this, permissions, permissions_code);
        }

        private void DisplayPermissionAlertAndClose()
        {
            new Android.App.AlertDialog.Builder(this)
            .SetMessage("L'app ha bisogno dei permessi di accesso alla memoria per poter scaricare le foto.\nL'app verrà chiusa.\nI permessi saranno richiesti nuovamente al prossimo avvio dell'app.")
            .Show();

            System.Timers.Timer killTimer = new System.Timers.Timer();
            killTimer.Interval = 5000;
            killTimer.Enabled = true;
            killTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                killTimer.Stop();
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            };
        }

        public bool CheckInternetConnection()
        {
            string CheckUrl = "http://google.com";

            try
            {
                HttpWebRequest iNetRequest = (HttpWebRequest)WebRequest.Create(CheckUrl);
                iNetRequest.Timeout = 5000;
                WebResponse iNetResponse = iNetRequest.GetResponse();
                iNetResponse.Close();
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        private async void DownloadOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            EditText linkTextBox = FindViewById<EditText>(Resource.Id.linkTextBox);
            TextView debugTextView = FindViewById<TextView>(Resource.Id.debugTextView);

            PageParser parser = new PageParser();

            bool hasRequiredPermissions = CheckPermissions();

            if (hasRequiredPermissions)
            {
                if (CheckInternetConnection())
                {
                    if (linkTextBox.Text.Contains("https://dreamcatcher.candlemystar.com/post/"))
                    {
                        String[] links = await parser.ParsePage(linkTextBox.Text);

                        debugTextView.Text = "";
                        debugTextView.Append("Inizio download di " + links.Length + " foto!\n");

                        int i = 1;

                        foreach (string link in links)
                        {
                            var downloadManager = CrossDownloadManager.Current;
                            var file = downloadManager.CreateDownloadFile(link);
                            downloadManager.Start(file);

                            debugTextView.Append("Foto " + i + " in download!\n");
                            i++;
                        }

                        debugTextView.Append("Tutte le foto sono in fase di download, puoi seguire il progresso del download nelle barra delle notifiche!");
                        i = 0;
                    }
                    else
                    {
                        debugTextView.Text = "Il link che hai inserito non è uno ShareURL valido.";
                    }
                }
                else
                {
                    debugTextView.Text = "Controlla di essere connesso a internet!";
                }
            }
            else
            {
                DisplayPermissionAlertAndClose();
            }
        }

    }
}

