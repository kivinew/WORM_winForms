using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Worm_WinForms;

namespace gameHost
{
    public partial class WormServer
    {
        public static TcpListener Listener; // Объект, принимающий TCP-клиентов
        // Запуск сервера (конструктор)
        public WormServer(int port)
        {
            // Создаем "слушателя" для указанного порта
            Listener = new TcpListener(IPAddress.Any, port);
            // Запускаем его 
            Listener.Start();
        }
        ~WormServer()
        {
            // Если "слушатель" был создан...
            if (Listener != null)
            {
                // ...oстановим его
                Listener.Stop();
            }
        }
        private void GenerateFood()         // поместим еду в случайном месте
        {
            var random = new Random();
            var food = new Square(random.Next(1, 14), random.Next(1, 13));
        }
        private void CreateWorm()           // новый червь из трёх элементов
        {                                   // в случайном месте игрового поля
            NewWorm worm;
        }
        private static void Main(string[] args)
        {
            // Создадим сервер
            var server = new WormServer(12345);

            TcpClient client;
            var num = 0;
            var readBuff = new byte[4];     // read buffer
            var writeBuff = new byte[4];    // write buffer
            while (true)
            {
                try
                {
                    // Принимаем новых клиентов
					client = Listener.AcceptTcpClient();
                    client.NoDelay = true;
                    num++;
                    Console.WriteLine($"Клиент №{num} подключён");
                    var stream = client.GetStream();
                    var response = new StringBuilder();
                    do
                    {
                        do
                        {
                            var bytes = stream.Read(readBuff, 0, readBuff.Length);
                            response.Append(Encoding.UTF8.GetString(readBuff, 0, bytes));
                        } while (stream.DataAvailable);
                        if(response != null)
                            Console.WriteLine($"Получено от клиента {num}: \t{response}");
                        response.Clear();
                    } while (!response.Equals("exit")); 
                    //clientStream.Write(rbuffer, 0, rbuffer.Length);
                    stream.Close();
                    client.Close();
                    Console.WriteLine($"Клиент {num} отключён.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: \n{ex}");
                }
                finally
                {
                    Listener?.Stop();
                }
            }
        }
    }
}