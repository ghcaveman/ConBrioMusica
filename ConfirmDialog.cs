using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConBrioMusica
{
    public partial class ConfirmDialog : Form
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public string Title
        {
            set { this.Text = value; }
        }

        public string Artist
        {
            get { return artistTextBox.Text.Trim(); }
            set { artistTextBox.Text = value; }
        }

        public string Song
        {
            get { return songTextBox.Text.Trim(); }
            set { songTextBox.Text = value; }
        }
        
        public string Album
        {
            get { return albumTextBox.Text.Trim(); }
            set { albumTextBox.Text = value; }
        }

        public string Year
        {
            get { return yearTextBox.Text.Trim(); }
            set { yearTextBox.Text = value; }
        }

        public string Comment
        {
            get { return commentTextBox.Text.Trim(); }
            set { commentTextBox.Text = value; }
        }

        private void swapButton_Click(object sender, EventArgs e)
        {
            string temp = artistTextBox.Text.Trim();
            artistTextBox.Text = songTextBox.Text.Trim();
            songTextBox.Text = temp.Trim();
        }
    }
}
