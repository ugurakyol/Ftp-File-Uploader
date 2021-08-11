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
            textBox5.Text = "http://www.btso.org.tr/documents/othernotice";
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


            string FTPDosyaYolu = "ftp://  /"+textBox3.Text;//change your ftp url information as yours
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(FTPDosyaYolu);
            progressBar1.Value = 30;
            string username = ""; //change username information as yours
            string password = ""; //change pasword information as yours
            
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



        private void label4_Click(object sender, EventArgs e)
        {
            if(label4.Text == "link oluşturma için tıklayınız.")
            {
                panel1.Visible = true;
                panel2.Visible = false;
                label4.Text = "TIFF yükleme için tıklayınız.";
            }
            else
            {
                panel1.Visible = false;
                panel2.Visible = true;
                label4.Text = "link oluşturma için tıklayınız.";
            }
            
        }

        private void TIFFConvert(string path)
        {
            string fileName = System.IO.Path.GetDirectoryName(path) +@"\"+ System.IO.Path.GetFileNameWithoutExtension(path) + ".pdf";            
            // load the TIFF file in an instance of Image
            using (var image = Aspose.Imaging.Image.Load(path))
            {
                // create an instance of PdfOptions
                var options = new Aspose.Imaging.ImageOptions.PdfOptions();
                // save TIFF as a PDF
                image.Save(fileName, options);
            }
            listBox2.Items.Add((listBox1.Items.Count + 1) + " " + System.IO.Path.GetFileNameWithoutExtension(path) + ".pdf");
            label8.Text = "Yüklenen Dosyaların Listesi - (" + listBox2.Items.Count + ")";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            progressBar2.Value = 20;

            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Dosya Seçin";
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            DialogResult dr = this.openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                textBox4.Text = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);

                ProcessDirectory(textBox4.Text);  
            }

            progressBar2.Value = 100;
        }


        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);

            button6.Visible = true;
        }

        // Insert logic for processing found files here.
        public void ProcessFile(string path)
        {
            // Console.WriteLine("Processed file '{0}'.", path);
            if(System.IO.Path.GetExtension(path) == ".tiff" )
            {
                TIFFConvert(path);
                listBox1.Items.Add((listBox1.Items.Count + 1) + " " + System.IO.Path.GetFileName(path) + ".");
                label7.Text = "Yüklenecek Dosyaların Listesi - (" + listBox1.Items.Count + ")";

            }else if(System.IO.Path.GetExtension(path) == ".TIFF")
            {
                TIFFConvert(path);
                listBox1.Items.Add((listBox1.Items.Count + 1) + " " + System.IO.Path.GetFileName(path) + ".");
                label7.Text = "Yüklenecek Dosyaların Listesi - (" + listBox1.Items.Count + ")";

            }
            else if (System.IO.Path.GetExtension(path) == ".TIF")
            {
                TIFFConvert(path);
                listBox1.Items.Add((listBox1.Items.Count + 1) + " " + System.IO.Path.GetFileName(path) + ".");
                label7.Text = "Yüklenecek Dosyaların Listesi - (" + listBox1.Items.Count + ")";

            }



        }

        public void UploadFiles(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                if (System.IO.Path.GetExtension(fileName) == ".pdf")
                {
                    filePathToUpload = System.IO.Path.GetFullPath(fileName);
                    string FTPDosyaYolu = "ftp://88.255.87.108:21/www.btso.org.tr/documents/othernotice/" + System.IO.Path.GetFileName(fileName); //change your ftp url information as yours
                    FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(FTPDosyaYolu);
                    progressBar2.Value = 25;
                    string username = "btso_org_tr"; //change username information as yours
                    string password = "Bts@2020.f0Y_M"; //change pasword information as yours

                    request.Credentials = new NetworkCredential(username, password);

                    request.UsePassive = true; // pasif olarak kullanabilme
                    request.UseBinary = true; // aktarım binary ile olacak
                    request.KeepAlive = false; // sürekli açık tutma

                    request.Method = WebRequestMethods.Ftp.UploadFile; // Dosya yüklemek için bu request metodu gerekiyor
                    progressBar2.Value = 50;

                    Console.WriteLine(filePathToUpload);
                    FileStream stream = File.OpenRead(filePathToUpload);
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    stream.Close();
                    progressBar2.Value = 75;
                    Stream reqStream = request.GetRequestStream(); // yükleme işini yapan kodlar
                    reqStream.Write(buffer, 0, buffer.Length);
                    reqStream.Close();
                    progressBar2.Value = 100;
                    
                }
            MessageBox.Show("Dosya gönderimi tamamlanmıştır.", "Dosya Transfer",
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Information);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            UploadFiles(textBox4.Text);
        }
    }
}
