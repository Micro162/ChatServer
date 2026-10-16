
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        TcpListener server;
        List<TcpClient> clients = new List<TcpClient>();

        TextBox txtLog = new TextBox();
        Button btnStart = new Button();

        public Form1()
        {
            InitializeComponent();

            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.SetBounds(10, 10, 360, 240);

            btnStart.Text = "Запустить сервер";
            btnStart.SetBounds(10, 260, 160, 30);
            btnStart.Click += btnStart_Click;

            Controls.Add(txtLog);
            Controls.Add(btnStart);

            Text = "Сервер чата";
            Width = 400;
            Height = 350;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();

            btnStart.Enabled = false;
            txtLog.AppendText("Сервер запущен!\r\n");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                _ = HandleClient(client);
            }
        }

        async Task HandleClient(TcpClient client)
        {
            using (client)
            using (StreamReader reader =
                new StreamReader(client.GetStream()))
            {
                string name = await reader.ReadLineAsync();
                AddLog(name + " подключился");

                string message;

                while ((message = await reader.ReadLineAsync()) != null)
                {
                    AddLog(message);
                    Broadcast(message);
                }
            }

            clients.Remove(client);
        }

        void Broadcast(string message)
        {
            foreach (TcpClient client in clients.ToArray())
            {
                try
                {
                    StreamWriter writer =
                        new StreamWriter(client.GetStream());

                    writer.AutoFlush = true;
                    writer.WriteLine(message);
                }
                catch { }
            }
        }

        void AddLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddLog(message)));
                return;
            }

            txtLog.AppendText(message + "\r\n");
        }
    }
}
