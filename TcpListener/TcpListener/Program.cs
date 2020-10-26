using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpListener
{
	internal class Program
	{
		private static readonly CancellationTokenSource _ctsOperation = new CancellationTokenSource();

		private static string Host { get; set; } = "localhost";

		private static int Port { get; set; } = 8080;

		private static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.GetEncoding(866);
			int num;
			do
			{
				num = FirstLayerMenu();
				if(num == 1) SecondLayerMenu();
			}
			while(num != 2 && num != 3);
			if(num != 2) return;

			StartReceive();
			do
				;
			while(Console.ReadKey().Key != ConsoleKey.Escape);
			Environment.Exit(0);
		}

		private static int FirstLayerMenu()
		{
			Console.Clear();
			Console.WriteLine("Enter the number of the menu item to select it...");
			Console.WriteLine("");
			Console.WriteLine("1. Enter a new address.");
			Console.WriteLine($"2. Start receiving messages from the address: {(object) Host}:{(object) Port} ");
			Console.WriteLine("3. Close the application.");
			string str;
			do
			{
				str = Console.ReadLine();
			}
			while(str != "1" && str != "2" && str != "3");
			Console.Clear();
			return Convert.ToInt32(str);
		}

		private static void SecondLayerMenu()
		{
			Console.Write("Host : ");
			Host = Console.ReadLine();
			Console.Write("Port : ");
			Port = Convert.ToInt32(Console.ReadLine());
			Console.Clear();
		}

		private static void Receive(CancellationToken cancelToken)
		{
			var hostEntry = Dns.GetHostEntry(Host);
			IPAddress address;
			try
			{
				address = IPAddress.Parse(Host);
			}
			catch
			{
				address = hostEntry.AddressList[0];
			}
			
			var ipEndPoint = new IPEndPoint(address, Port);

			Console.WriteLine("Press Esc to exit.");

			using(var socket1 = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
			{
				try
				{
					socket1.Bind(ipEndPoint);
					socket1.Listen(10);
					while(!cancelToken.IsCancellationRequested)
					{
						Console.WriteLine($"Waiting for a connection via the port {(object) ipEndPoint}");

						var socket2  = socket1.Accept();
						var empty    = string.Empty;
						var numArray = new byte[4096];
						var count    = socket2.Receive(numArray);
						var str      = empty + Encoding.GetEncoding(866).GetString(numArray, 0, count);

						Console.WriteLine("Message: ");
						Console.WriteLine("");
						Console.WriteLine(str);
						Console.WriteLine("");

						socket2.Shutdown(SocketShutdown.Both);
						socket2.Close();

						Console.WriteLine("");
						Console.WriteLine($"Connection {(object) ipEndPoint} closed.");
						Console.WriteLine("");
					}
					socket1.Shutdown(SocketShutdown.Both);
					socket1.Close();
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
					socket1.Shutdown(SocketShutdown.Both);
					socket1.Close();
					_ctsOperation.Cancel();
				}
			}
		}

		private static void StartReceive() => new Thread(() =>
		{
			try
			{
				Receive(_ctsOperation.Token);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}).Start();
	}
}

