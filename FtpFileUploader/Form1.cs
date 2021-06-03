using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FtpFileUploader
{
    public partial class Form1 : Form
    {
        string filePathToUpload = "";
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "http://www.btso.org.tr/linkfiles";
            textBox2.Text = "Gönderilecek dosya yolunu seçiniz.";
        }
        private string FileNameFixer(string name)
        {
            const string Punctuation = "* <>$#;,./&%?!'^+&()=";
            string fixedName = "";

            foreach (char c in name)
            {
                if (Punctuation.Contains(c)) {                    
                    fixedName += "_";
                }
                else if (c.Equals('Ö')) { fixedName += "O"; }
                else if (c.Equals('ö')) { fixedName += "o"; }
                else if (c.Equals('Ç')) { fixedName += "C"; }
                else if (c.Equals('ç')) { fixedName += "c"; }
                else if (c.Equals('Ş')) { fixedName += "S"; }
                else if (c.Equals('ş')) { fixedName += "s"; }
                else if (c.Equals('Ğ')) { fixedName += "G"; }
                else if (c.Equals('ğ')) { fixedName += "g"; }
                else if (c.Equals('Ü')) { fixedName += "U"; }
                else if (c.Equals('ü')) { fixedName += "u"; }
                else if (c.Equals('İ') || c.Equals('ı')) { fixedName += "i"; }
                else
                {
                    fixedName += c;
                }
            }
            progressBar1.Value = 40;
            return fixedName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 20;

            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Dosya Seçin";
            openFileDialog1.Filter = "Pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            DialogResult dr = this.openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
                
                string name = FileNameFixer(System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName));
                string uzanti = System.IO.Path.GetExtension(openFileDialog1.FileName);             
                string message = "Dosya Adı: " + name + uzanti + " olarak gönderilecektir.";
                const string caption = "Dosya adı onayı ";
                var result = MessageBox.Show(message, caption,
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Information);
                progressBar1.Value = 60;

                if (result == DialogResult.No)
                {
                    MessageBox.Show("Lüften dosya adını yeniden düzenleyiniz. Ardından yükle butonuna basınız.", "Dosya adı uyarısı",
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Information);
                    progressBar1.Value = 80;

                }

                textBox3.Text = name+uzanti;
                button3.Visible = true;
                progressBar1.Value = 100;

            }

            progressBar1.Value = 100; 

        }

        private void button3_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;

            filePathToUpload = Path.GetDirectoryName(openFileDialog1.FileName) + "\\" + textBox3.Text;
            File.Move(textBox2.Text, filePathToUpload);
            
            
            progressBar1.Value = 10;

            linkLabel1.Text = textBox1.Text + "/" + textBox3.Text;
            linkLabel1.Visible = true;

            progressBar1.Value = 20;


            string FTPDosyaYolu = "ftp://88.255.87.108:21/www.btso.org.tr/linkfiles/"+textBox3.Text;//change your ftp url information as yours
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(FTPDosyaYolu);
            progressBar1.Value = 30;
            string username = "btso_org_tr"; //change username information as yours
            string password = "Bts@2020.f0Y_M"; //change pasword information as yours
            
            request.Credentials = new NetworkCredential(username, password);

            request.UsePassive = true; // pasif olarak kullanabilme
            request.UseBinary = true; // aktarım binary ile olacak
            request.KeepAlive = false; // sürekli açık tutma

            request.Method = WebRequestMethods.Ftp.UploadFile; // Dosya yüklemek için bu request metodu gerekiyor
            progressBar1.Value = 40;

            Console.WriteLine(filePathToUpload);
            FileStream stream = File.OpenRead(filePathToUpload);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            Stream reqStream = request.GetRequestStream(); // yükleme işini yapan kodlar
            reqStream.Write(buffer, 0, buffer.Length);
            reqStream.Close();

            Clipboard.SetText(linkLabel1.Text); // link yolu kopyalanıyor

            MessageBox.Show("Link panoya koplandı. \n " +textBox1.Text + "/" + textBox3.Text, "Dosyanın linki",
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Information);

            progressBar1.Value = 100;


        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(textBox1.Text + "/" + textBox3.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "ftp://www.btso.org.tr/linkfiles";

            DialogResult result = dialog.ShowDialog(this);

            if (result != DialogResult.OK)
            {
                //return false;
            }

            if (result == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
    

            }        



        }
    }
}
