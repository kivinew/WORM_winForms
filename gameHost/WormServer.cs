using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Worm_WinForms;

namespace gameHost
{
    public class Server
    {
        public class NewWorm
        {
            private int Direction;          // направление движения

            private NewWorm()               // конструктор червя
            {
                var worm = new List<Square>();
                var randX = new Random();
                var randY = new Random();
                for (var i = 0; i < 3; i++)
                {
                    var part = new Square(randX.Next(), randY.Next());
                    worm.Add(part);
                }
                Direction = new Random().Next(0, 3); // случайное направление движения
            }

        }
        public static TcpListener Listener; // Объект, принимающий TCP-клиентов
        // Запуск сервера (конструктор)
        public Server(int port)
        {
            // Создаем "слушателя" для указанного порта
            Listener = new TcpListener(IPAddress.Any, port);
            // Запускаем его 
            Listener.Start();
        }
        ~Server()
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
            var server = new Server(8889);

            TcpClient client;
            var num = 0;
            var rbuffer = new byte[4];       // read buffer
            var wbuffer = new byte[4];       // write buffer
            while (true)
            {
                try
                {
                    // Принимаем новых клиентов
					client = Listener.AcceptTcpClient();
                    client.NoDelay = true;
                    num++;
                    Console.WriteLine("Клиент №{0} подключён", num);
                    var stream = client.GetStream();
                    var response = new StringBuilder();
                    do
                    {
                        do
                        {
                            var bytes = stream.Read(rbuffer, 0, rbuffer.Length);
                            response.Append(Encoding.UTF8.GetString(rbuffer, 0, bytes));
                        } while (stream.DataAvailable);
                        if(response != null)
                            Console.WriteLine("Получено от клиента {0}: \t{1}", num, response);
                        response.Clear();
                    } while (!response.Equals("exit")); 
                    //clientStream.Write(rbuffer, 0, rbuffer.Length);
                    stream.Close();
                    client.Close();
                    Console.WriteLine("Клиент {0} отключён.", num);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: \n{0}", ex);
                }
                finally
                {
                    Listener?.Stop();
                }
            }
        }
    }
}