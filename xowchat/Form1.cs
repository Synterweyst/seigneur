using Bunifu.Framework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Transitions;
using xowchat.Properties;

namespace xowchat
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        // debug option
        private Boolean debug = false;
        private string channel = "chat";


        // last message received
        private String lastMessage = "";
        // user token
        private String token = "";
        // check for messages every x milliseconds
        private int delay = 100;

        private void button1_Click(object sender, EventArgs e)
        {
            // send button

            // check if textbox contents are valid
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                // set the token
                token = textBox2.Text;
                // start a new thread to prevent freezing
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    // initiate the web client
                    System.Net.WebClient wc = new System.Net.WebClient();

                    // send api request with message and token
                    // also url encoded to prevent bugs, also a generally good practice
                    byte[] raw = wc.DownloadData("https://xowbuk.xyz/api/say?key="+Uri.EscapeDataString(token)+"&message="+ Uri.EscapeDataString(textBox1.Text) + "&channel="+channel);
                    
                    // invoke, as it is a new thread
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        // checks if the token is invalid
                        if(System.Text.Encoding.UTF8.GetString(raw).Equals("Invalid key."))
                        {
                            // the token is invalid
                            listBox1.Items.Add("Invalid token.");
                            addMessage("Error", "Invalid token.", "");
                            label3.Visible = true;
                            textBox2.Visible = true;
                            linkLabel1.Visible = true;
                            label1.Text = "Connected as (                 )";
                            label2.Text = "Guest";

                            // reset token from save
                            Settings.Default.token = "";
                            Settings.Default.Save();
                        } else
                        {
                            // the token is valid
                            label3.Visible = false;
                            textBox2.Visible = false;
                            linkLabel1.Visible = false;
                            label1.Text = "Connected as";
                            label2.Text = "Member";

                            // save the token for next launch
                            Settings.Default.token = token;
                            Settings.Default.Save();
                        }

                        // make the textbox available again
                        textBox1.ReadOnly = false; 
                        textBox1.Text = "";
                    }));
                }).Start();
                // make the textbox readonly to prevent multiple tasks
                textBox1.ReadOnly = true;
            } else if(textBox1.Text == "")
            {
                // the message textbox is empty
                listBox1.Items.Add("Message is required.");
                addMessage("Error", "Message is required.", "");
            }
            else if(textBox2.Text == "")
            {
                // the token textbox is empty
                textBox2.Visible = true;
                label3.Visible = true;
                listBox1.Items.Add("Token is required.");
                addMessage("Error", "Token is required.", "");
            }
        }
        private void addMessage(string name, string content, string avatar)
        {
            // generate the required elements for a message
            PictureBox pfp = new PictureBox();
            Label username = new Label();
            Label message = new Label();

            // resize the pictureBox to the needed
            pfp.Left = -549;
            pfp.Top = yOffset;
            pfp.Size = new Size(30, 30);

            // start the image off as default (question mark)
            pfp.Image = Resources.unknownentity;

            // start new thread to not freeze while loading
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                // check if avatar is set
                if (avatar != "")
                {
                    // invoke since it's a different thread
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        // get avatar from uid and load it onto the pictureBox
                        pfp.LoadAsync(avatar);
                    }));
                }
            }).Start();

            // customize the username label
            username.Top = yOffset + 1;
            username.AutoSize = true;
            username.Left = -541;
            username.ForeColor = Color.WhiteSmoke;
            username.Font = new Font(new Font(new FontFamily("Segoe UI"), 8.25f), FontStyle.Bold);

            // customize the message label
            message.Font = new Font(new Font(new FontFamily("Segoe UI"), 8.25f), FontStyle.Regular);
            message.Top = yOffset + 16;
            message.Left = -541;
            message.AutoSize = true;
            message.MaximumSize = new Size(300, 500);
            message.ForeColor = Color.WhiteSmoke;

            // set the username
            username.Text = name;

            // set the message
            message.Text = content;
            // use the default image (question mark)
            pfp.Image = Resources.unknownentity;
            

            // fit the image
            pfp.SizeMode = PictureBoxSizeMode.Zoom;

            // round the profile picture
            BunifuElipse roundPfp = new BunifuElipse();
            roundPfp.TargetControl = pfp;
            roundPfp.ElipseRadius = 50;

            // this is to stop gifs from animating
            pfp.Enabled = false;

            Boolean hasEmbed = false;


            // add the elements onto the messages panel
            panel2.Controls.Add(pfp);
            panel2.Controls.Add(username);
            panel2.Controls.Add(message);

            // auto scroll to the bottom of the message list
            panel2.ScrollControlIntoView(message);

            // add an animation for 1000ms
            Transition t = new Transition(new TransitionType_EaseInEaseOut(1000));
            t.add(pfp, "Left", 8);
            t.add(username, "Left", 41);
            t.add(message, "Left", 41);


            // add to the offset so the next message is below
            yOffset += 36 + (message.Height > 16 ? message.Height - 13 : 0);

            string theMessage = content;
            if ((theMessage.StartsWith("http://") || theMessage.StartsWith("https://")) && !theMessage.Contains(" ") && (theMessage.EndsWith(".png") || theMessage.EndsWith(".jpg") || theMessage.EndsWith(".webm") || theMessage.EndsWith(".jpeg")))
            {
                // might need for later
                hasEmbed = true;
            }

            if (hasEmbed)
            {
                // create the embed
                PictureBox embed = new PictureBox();

                // set the name to the url to load
                embed.Name = content;

                // customize the embed
                embed.Top = yOffset;
                embed.Left = -541;
                embed.Width = 200;
                embed.Height = 100;
                embed.SizeMode = PictureBoxSizeMode.Zoom;
                embed.MaximumSize = new Size(200, 100);
                embed.BackColor = Color.Black;

                // set the unloaded image to the question mark
                embed.Image = Resources.unknownentity;

                // transition it
                t.add(embed, "Left", 41);

                // add click event to load
                embed.Click += new EventHandler(loadImage_Click);

                // round the embed
                BunifuElipse roundEmbed = new BunifuElipse();
                roundEmbed.TargetControl = embed;
                roundEmbed.ElipseRadius = 15;

                // add the embed to the messages panel
                panel2.Controls.Add(embed);

                // add to the offset because of the embed
                yOffset += embed.Height + 10;
                // scroll again
                panel2.ScrollControlIntoView(embed);
            }

            // run the animation for everything at once
            t.run();
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // start the web client
            System.Net.WebClient wc = new System.Net.WebClient();

            // gets the last message
            byte[] raw = wc.DownloadData("https://xowbuk.xyz/api/chats/"+ (channel=="chat" ? "lastmessage" : "lastmessage-"+channel));

            string webData = System.Text.Encoding.UTF8.GetString(raw);

            // check if the last message is the same to not repeat
            if (webData != lastMessage)
            {
                // set the last message so it doesn't repeat
                lastMessage = webData;
                this.Invoke(new MethodInvoker(delegate ()
                {
                    // for debugging
                    listBox1.Items.Add(webData.Contains("| ") ? webData.Split(new[] { "| " }, StringSplitOptions.None)[1] : webData);

                    // remove timestamp from message
                    noTimestamp = webData.Split(new[] { "| " }, StringSplitOptions.None)[1];
                    string avatar = "";
                    new Thread(() =>
                    {
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            Thread.CurrentThread.IsBackground = true;
                            // get the uid from the sender username
                            int UID = noTimestamp.StartsWith("[external] ")?-1:getUIDFromUsername(noTimestamp.Split(new[] { " > " }, StringSplitOptions.None)[0]);
                            // check if user exists (uid > 0) and if it's a message
                            if (UID > 0 && webData.Contains("| ") && !noTimestamp.StartsWith("[external] "))
                            {
                                avatar = getAvatarFromUID(UID); 
                                addMessage(noTimestamp.StartsWith("[external] ") ? (noTimestamp.Split(new[] { " > " }, StringSplitOptions.None)[0]).Split(new[] { "[external] " }, StringSplitOptions.None)[1] : noTimestamp.Split(new[] { " > " }, StringSplitOptions.None)[0], webData.Contains("| ") ? (noTimestamp.Split(new[] { " > " }, StringSplitOptions.None)[1]) : webData, avatar);
                            }
                            else if(noTimestamp.StartsWith("[external] "))
                            {
                                addMessage(noTimestamp.StartsWith("[external] ") ? (noTimestamp.Split(new[] { " > " }, StringSplitOptions.None)[0]).Split(new[] { "[external] " }, StringSplitOptions.None)[1] : noTimestamp.Split(new[] { " > " }, StringSplitOptions.None)[0], webData.Contains("| ") ? (noTimestamp.Split(new[] { " > " }, StringSplitOptions.None)[1]) : webData, avatar);
                            }
                        }));
                    }).Start();
                    }));
            }
        }


        private void loadImage_Click(object sender, EventArgs e)
        {
            // the same embed
            PictureBox embed = sender as PictureBox;

            // invoke (idk why)
            try
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                        // start web client
                        WebClient wc = new WebClient();

                        // get image from url
                        byte[] bytes = wc.DownloadData(embed.Name);
                        // stream bytes to actual image
                        MemoryStream ms = new MemoryStream(bytes);
                        // convert to image
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

                        // set image
                        embed.Image = img;
                    
                }));
            }
            catch (Exception)
            {
                addMessage("Error", "Could not load image", "");
            }
            
        }


        // initiate
        private String noTimestamp = "";

        // starting offset
        private int yOffset = 8;

        private void timer1_Tick(object sender, EventArgs e)
        {
            // loop for message checking
            if(!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }
        private String getAvatarFromUID(int uid)
        {
            System.Net.WebClient wc = new System.Net.WebClient();

            // api request
            byte[] raw3 = wc.DownloadData("https://xowbuk.xyz/" + uid + "/pfp.txt");

            string webData3 = System.Text.Encoding.UTF8.GetString(raw3);
            return webData3;
        }

        private int getUIDFromUsername(string username)
        {
            System.Net.WebClient wc = new System.Net.WebClient();

            // api request
            byte[] raw2 = wc.DownloadData("https://xowbuk.xyz/api/usernametoid?username=" + username);

            string webData2 = System.Text.Encoding.UTF8.GetString(raw2);
            if(webData2 == "false : nothing found")
            {
                return -1;
            }
            
            return Int16.Parse(webData2);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // change button colour when the textbox is empty
            if(textBox1.Text == "")
            {
                button1.BackColor = Color.DimGray;
            } else
            {
                button1.BackColor = Color.DodgerBlue;
            }
        }

        // <comment>
        // this is to make the form draggable
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        // </comment>

        private void button3_Click(object sender, EventArgs e)
        {
            // minimize button
            this.WindowState = FormWindowState.Minimized;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // exit button
            Environment.Exit(0);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // set the token
            if (e.KeyChar.Equals(13))
            {
                textBox2.Visible = false;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // show inputs to start connection
            label3.Visible = true;
            textBox2.Visible = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = delay;
            // load token if it exists
            if (Settings.Default.token != "")
            {
                textBox2.Text = Settings.Default.token;
                token = Settings.Default.token;
                label3.Visible = false;
                textBox2.Visible = false;
                linkLabel1.Visible = false;
                label1.Text = "Connected as";
                label2.Text = "Member";
            }
            // load/unload debug elements
            if (!debug)
            {
                panel2.Height = 323;
                panel2.Top = 53;
                listBox2.Height = 323;
                listBox2.Top = 53;
                listBox1.Visible = false;
            }

            // initiate web client
            System.Net.WebClient wc = new System.Net.WebClient();

            // api request to get the channels
            byte[] raw = wc.DownloadData("https://xowbuk.xyz/api/channels");

            string webData = System.Text.Encoding.UTF8.GetString(raw);

            // convert the channels to a list
            channels = webData.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            // add the channels to the listbox
            foreach(string channel in channels)
            {
                listBox2.Items.Add(channel);
            }

            // set the default channel to "chat" (the first)
            listBox2.SelectedIndex = 0;
        }

        private List<String> channels = new System.Collections.Generic.List<String>();

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // change channel
            channel = listBox2.SelectedItem.ToString();
            // clear message panel
            panel2.Controls.Clear();
            yOffset = 8;
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            // prevent clicking in empty area
            Point pt = new Point(e.X, e.Y);
            int index = listBox1.IndexFromPoint(pt);

            if (index <= -1)
            {
                listBox1.SelectedIndex = 0;
                return;
            }
        }

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            // change list selection colour
            var combo = sender as System.Windows.Forms.ComboBox;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.DimGray), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);
            }

            e.Graphics.DrawString(combo.Items[e.Index].ToString(),
                                          e.Font,
                                          new SolidBrush(Color.Black),
                                          new Point(e.Bounds.X, e.Bounds.Y));
        }
    }
}
