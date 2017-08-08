using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.DirectPlay;
using Shared;
using System.IO;
using Checkers;
using BoardGame;

namespace GameServer
{

    public partial class _Server : Form
    {
        private Server connection = null;
        private bool connected = false;
        public delegate void StatusConnection(bool statusConnection);
        public delegate void StatusCreated(bool statusCreated);
        public delegate void LeftMouseUp(Square locatin);
        public delegate void LeftMouseDown(Square location);

        private StatusConnection statusConnection;
        private StatusCreated statusCreated;
        public LeftMouseUp leftMouseUp;
        public LeftMouseDown leftMouseDown;

        private int playerID;
        public _Server()
        {
            InitializeComponent();
        }

        public void InitializeServer()
        {
            connection = new Server();
            connection.PlayerCreated += new
            PlayerCreatedEventHandler(OnPlayerCreated);
            connection.PlayerDestroyed += new
            PlayerDestroyedEventHandler(OnPlayerDestroyed);
            connection.Receive += new ReceiveEventHandler(OnDataReceive);
            connection.Disposing += new EventHandler(OnDisposing);


            if (!IServiceProviderValid(Address.ServiceProviderTcpIp))
            {
                MessageBox.Show("Невозможно создать TCP/IP службу поддержки",
                    "Выход", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            Address deviceAddress = null;
            deviceAddress = new Address();
            deviceAddress.ServiceProvider = Address.ServiceProviderTcpIp;
            deviceAddress.AddComponent(Address.KeyPort, SharedCode.DataPort);
            
            ApplicationDescription desc = new ApplicationDescription();
            desc.SessionName = textBox1.Text;
            desc.MaxPlayers = 2;
            desc.GuidApplication = SharedCode.ApplicationGuid;
            desc.Flags = SessionFlags.ClientServer | SessionFlags.NoDpnServer;
            try
            {
                connection.Host(desc, deviceAddress);
                statusCreated(true);
                MessageBox.Show("Сессия создана",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Невозможно создать сессию",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        
        }


        private bool IServiceProviderValid(Guid provider)
        {
            ServiceProviderInformation[] providers =
                connection.GetServiceProviders(true);

            foreach (ServiceProviderInformation info in providers)
            {
                if (info.Guid == provider)
                    return true;
            }
            return false;
        }


        private void OnPlayerCreated(object sender, PlayerCreatedEventArgs e)
        {
            try
            {
                Shared.SharedCode.EnemyPlayer.Name = ((Server)sender).GetClientInformation(e.Message.PlayerID).Name;
                MessageBox.Show(string.Format("Установлено новое соединение с {0}",  Shared.SharedCode.EnemyPlayer.Name),"", MessageBoxButtons.OK, MessageBoxIcon.Information);
                connected = true;
                playerID = e.Message.PlayerID;
                if (statusConnection != null)
                    statusConnection(connected);
            }
            catch
            {
            }

        }

        private void OnPlayerDestroyed(object sender, PlayerDestroyedEventArgs e)
        {
            MessageBox.Show(string.Format
                ("Игрок {0} покинул сессию",
                Shared.SharedCode.EnemyPlayer.Name),
    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            connected = false;
            if (statusConnection != null)
                statusConnection(connected);

        }

        private void OnDataReceive(object sender, ReceiveEventArgs e)
        {
            SharedCode.NetworkMessages msg = (SharedCode.NetworkMessages)e.Message.ReceiveData.Read(typeof(SharedCode.NetworkMessages));
            NetworkPacket returnedPacked = new NetworkPacket();
            switch (msg)
            {
                case SharedCode.NetworkMessages.Image:
                    byte[] reciveLen = new byte[4];
                    reciveLen = (byte[])e.Message.ReceiveData.Read(typeof(byte), 4);
                    int lenght = BitConverter.ToInt32(reciveLen, 0);
                    byte[] recive = new byte[lenght];
                    recive = (byte[])e.Message.ReceiveData.Read(typeof(byte), lenght);
                    MemoryStream ms = new MemoryStream();
                    ms.Write(recive, 0, lenght);
                    Shared.SharedCode.EnemyPlayer.UserImage = Image.FromStream(ms);
                    break;
                case SharedCode.NetworkMessages.LeftMouseDown:
                    leftMouseDown((Square)e.Message.ReceiveData.Read(typeof(Square)));
                    break;
                case SharedCode.NetworkMessages.LeftMouseUp:
                    leftMouseUp((Square)e.Message.ReceiveData.Read(typeof(Square)));
                    break;
            }
        }

        private void OnDisposing(object sender, EventArgs e)
        {
            statusCreated(false);
        }

        public void SendImage(Image userImage)
        {
            if (connected)
            {
                NetworkPacket packet = new NetworkPacket();
                packet.Write(Shared.SharedCode.NetworkMessages.Image);
                MemoryStream ms = new MemoryStream();
                // Сохранили картинку в MemStream
                userImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                // Картинка в массиве
                byte[] arrImage = ms.GetBuffer();
                // Получили длину массива с картинкой
                int lenght = arrImage.Length;
                byte[] len = BitConverter.GetBytes(lenght);
                // Записали длину в поток
                packet.Write(len);
                packet.Write(arrImage);
                connection.SendTo(playerID, packet, 0, SendFlags.Guaranteed);
            }
        }

        public void SendMainPlayer(Player mainPlayer)
        {
            if (connected)
            {
                NetworkPacket packet = new NetworkPacket();
                packet.Write(Shared.SharedCode.NetworkMessages.MainPlayer);
                packet.Write(mainPlayer);
                connection.SendTo(playerID, packet, 0, SendFlags.Guaranteed);
            }
        }

        public void SendLeftMouseDown(Square location)
        {
            if (connected)
            {
                NetworkPacket packet = new NetworkPacket();
                packet.Write(Shared.SharedCode.NetworkMessages.LeftMouseDown);
                packet.Write(location);
                connection.SendTo(playerID, packet, 0, SendFlags.Guaranteed);
            }
        }

        public void SendLeftMouseUp(Square location)
        {
            if (connected)
            {
                NetworkPacket packet = new NetworkPacket();
                packet.Write(Shared.SharedCode.NetworkMessages.LeftMouseUp);
                packet.Write(location);
                connection.SendTo(playerID, packet, 0, SendFlags.Guaranteed);
            }
        }

        public void SetStatusCreated(StatusCreated statusCreated)
        {
            this.statusCreated = statusCreated;
        }

        public void SetStatusConnection(StatusConnection statusConnection)
        {
            this.statusConnection = statusConnection;
        }

        public void DisposeConnection()
        {
            connected = false;
            if (statusConnection != null)
                statusConnection(connected);
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                InitializeServer();
                PlayerInformation info = new PlayerInformation();
                info.Name = Shared.SharedCode.CurrentPlayer.Name;
                connection.SetServerInformation(info, SyncFlags.ClientInformation);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Введите название сессии",
"", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }


    }

}
