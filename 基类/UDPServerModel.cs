using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPTools{
	class UDPServerModel : SocketModel {
		public UDPServerModel() {
			DisplayName = "UDPServer";
			Name = "UDPServer";
			Icon = IconResources.ICON_SERVER;
		}

		public int CreateSocket(int port) {
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			try {
				socket.Bind(new IPEndPoint(IPAddress.Any, port));
			} catch (Exception) { return -1; }

			UdpServerSocketObject socketObj = new UdpServerSocketObject();
			socketObj.socket = socket;
			socketObj.remoteIpEP = new IPEndPoint(IPAddress.Any, port);
			socketObj.DisplayName = GetDisplayName(socketObj.remoteIpEP);
			socketObj.Parent = this;
			socketObjs.Add(socketObj);

			StartRead(socketObj);

			return 0;
		}

		protected void StartRead(UdpServerSocketObject socketObj) {
			RemoteSocketObject listenSocketObj = new RemoteSocketObject();
			listenSocketObj.socket = socketObj.socket;
			listenSocketObj.Parent = socketObj;
			socketObj.listenSocketObj = listenSocketObj;
			EndPoint tempEP = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
			var ar = listenSocketObj.socket.BeginReceiveFrom(listenSocketObj.buffer, 0, RemoteSocketObject.BufferSize, 0,
			ref tempEP, new AsyncCallback(AsynchronousSocketListener.UDPServerReadCallback), listenSocketObj);
		}

		protected override bool DelRemoteSocketObj(int index2, int index3) {
			((SocketObject)socketObjs[index2]).remoteSocketObjs.RemoveAt(index3);

			return true;
		}
	}
}
