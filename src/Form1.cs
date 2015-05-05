using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpotifyAPI.SpotifyLocalAPI;
using System.Threading;
using SpotifyEventHandler = SpotifyAPI.SpotifyLocalAPI.SpotifyEventHandler;


namespace Spotify_NowPlaying
{
    public partial class Form1 : Form
    {
        SpotifyLocalAPIClass spotify;
        SpotifyMusicHandler mh;
        SpotifyEventHandler eh;
        String textFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public Form1()
        {
           
            InitializeComponent();
            spotify = new SpotifyLocalAPIClass();
            if (!SpotifyLocalAPIClass.IsSpotifyRunning())
            {
                spotify.RunSpotify();
                Thread.Sleep(5000);
            }

            if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
            {
                spotify.RunSpotifyWebHelper();
                Thread.Sleep(4000);
            }

            if(!spotify.Connect())
            {
                Boolean retry = true;
                while(retry)
                {
                    if (MessageBox.Show("SpotifyLocalAPIClass could'nt load!", "Error", MessageBoxButtons.RetryCancel) == System.Windows.Forms.DialogResult.Retry)
                    {
                        if(spotify.Connect())
                            retry = false;
                        else
                            retry = true;
                    }
                    else
                    {
                        this.Close();
                        return;
                    }
                }
            }
            mh = spotify.GetMusicHandler();
            eh = spotify.GetEventHandler();
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            spotify.Update();
            pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);

            linkLabel1.Text = mh.GetCurrentTrack().GetTrackName();
            linkLabel1.LinkClicked += (senderTwo, args) => Process.Start(mh.GetCurrentTrack().GetTrackURI());
            linkLabel2.Text = mh.GetCurrentTrack().GetArtistName();
            linkLabel2.LinkClicked += (senderTwo, args) => Process.Start(mh.GetCurrentTrack().GetArtistURI());
            linkLabel3.Text = mh.GetCurrentTrack().GetAlbumName();
            linkLabel3.LinkClicked += (senderTwo, args) => Process.Start(mh.GetCurrentTrack().GetAlbumURI());
            printSong(mh.GetCurrentTrack());
            eh.OnTrackChange += new SpotifyEventHandler.TrackChangeEventHandler(trackchange);
            eh.SetSynchronizingObject(this);
            eh.ListenForEvents(true);
        }
        private async void trackchange(TrackChangeEventArgs e)
        {
            linkLabel1.Text = e.new_track.GetTrackName();
            linkLabel2.Text = e.new_track.GetArtistName();
            linkLabel3.Text = e.new_track.GetAlbumName();
            pictureBox1.Image = await e.new_track.GetAlbumArtAsync(AlbumArtSize.SIZE_160);
            printSong(e.new_track);

        }

        private void printSong(Track t)
        {
            String spaceChar;
            if (checkBox1.Checked) { spaceChar = "    "; }
            else { spaceChar = ""; }

            string nowPlaying = t.GetTrackName() + tbContext.Text + t.GetArtistName() + spaceChar;
            System.IO.File.WriteAllText(textFile + @"\NowPlaying.txt", nowPlaying);
        }
        private String formatTime(double sec)
        {
            TimeSpan span = TimeSpan.FromSeconds(sec);
            String secs = span.Seconds.ToString(), mins = span.Minutes.ToString();
            if (secs.Length < 2)
                secs = "0" + secs;
            return mins + ":" + secs;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mh.Play();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mh.Pause();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            mh.Previous();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            mh.Skip();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
        private void menuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
