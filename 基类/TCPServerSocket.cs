using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TCPTools{
	class TCPServerSocket : SocketModel {
		public TCPServerSocket() {
			DisplayName = "TCPServer";
			Name = "TCPServer";
			Icon = IconResources.ICON_SERVER;
		}

		public int CreateSocket(int port) {
			IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);

			Socket socket = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try {
				socket.Bind(localEP);
			} catch (Exception) { return -1; }

			TcpServerSocketObject socketObj = new TcpServerSocketObject();
			socketObj.socket = socket;
			socketObj.remoteIpEP = localEP;
			socketObj.Parent = this;
			socketObj.DisplayName = GetDisplayName(localEP);
			socketObjs.Add(socketObj);

			ThreadPool.QueueUserWorkItem(AsynchronousSocketListener.StartListening, socketObj);
			return 0;
		}

		protected override void DelAllRemoteSocketObj(int index2) {
			foreach (var c in ((SocketObject)socketObjs[index2]).remoteSocketObjs) {
				RemoteSocketObject obj = (RemoteSocketObject)c;
				obj.socket.Shutdown(SocketShutdown.Both);
				obj.socket.Disconnect(false);
				obj.socket.Close();
			}
			((SocketObject)socketObjs[index2]).remoteSocketObjs.Clear();
        }

		public void StartListening(int index2) {
			if (index2 >= 0 && index2 < socketObjs.Count) {
				((TcpServerSocketObject)socketObjs[index2]).socket.Listen(255);
				ThreadPool.QueueUserWorkItem(AsynchronousSocketListener.StartListening, socketObjs[index2]);
			}
		}

		public void StopListening(int index2) {
			if (index2 >= 0 && index2 < socketObjs.Count) {
				TcpServerSocketObject p2 = (TcpServerSocketObject)socketObjs[index2];
				p2.IsListening = false;
				DelAllRemoteSocketObj(index2);
				p2.socket.Close();
				Socket socket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.Bind(p2.remoteIpEP);
				p2.socket = socket;
			}
		}
	}
}
