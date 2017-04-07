using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPTools {
	class TCPClientModel : SocketModel{

		public TCPClientModel() {
			DisplayName = "TCPClient";
			Name = "TCPClient";
			Icon = IconResources.ICON_CLIENT;
		}

		public int CreateSocket(IPEndPoint ipEP) {

			TcpClientSocketObject socketObj = new TcpClientSocketObject();
			
			socketObj.remoteIpEP = ipEP;
			socketObj.DisplayName = GetDisplayName(ipEP);
			socketObj.Parent = this;
			socketObj.Children = null;
			socketObjs.Add(socketObj);

			//Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//socketObj.socket = socket;

			//Connect(socketObj);
			
			return 0;
		}

		public override void SendPacket(int index2, int index3, string text) {
			if (index3 == -1)
				index3 = 0;
			base.SendPacket(index2, index3, text);
		}

		public override string GetText(int index2, int index3) {
			if (index3 == -1)
				index3 = 0;
			return base.GetText(index2, index3);
		}

		public override void SetSendText(int index2, int index3, string sendText) {
			if (index3 == -1)
				index3 = 0;
			base.SetSendText(index2, index3, sendText);
		}

		public override string GetSendText(int index2, int index3) {
			if (index3 == -1)
				index3 = 0;
			return base.GetSendText(index2, index3);
		}

		protected override void DisconnectSocketObj(int index2) {
			try {
				SocketObject tObj = (SocketObject)socketObjs[index2];
				if (tObj.socket.Connected) {
					tObj.socket.Shutdown(SocketShutdown.Both);
					tObj.socket.Disconnect(true);
                }
				tObj.remoteSocketObjs.Clear();
				tObj.socket.Close();
			} catch (Exception) { }
		}

		private void Connect(TcpClientSocketObject socketObj) {
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socketObj.socket = socket;
			socketObj.connectDone.Reset();
			var ar = socketObj.socket.BeginConnect(socketObj.remoteIpEP, AsynchronousSocketListener.connectCallback, socketObj);

			try {
				if (socketObj.connectDone.WaitOne(400)) {
					if (socketObj.socket.Connected)
						AsynchronousSocketListener.StartRead(socketObj);
				} else {
					socketObj.socket.EndConnect(ar);
				}
			} catch (Exception) {

			}
		}

		public void Connect(int index2) {
			Connect((TcpClientSocketObject)socketObjs[index2]);
        }

		public void Disconnect(int index2) {
			DisconnectSocketObj(index2);
		}
	}
}
