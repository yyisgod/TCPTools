using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPTools{
	class UDPClientModel : SocketModel {
		public UDPClientModel() {
			DisplayName = "UDPClient";
			Name = "UDPClient";
			Icon = IconResources.ICON_CLIENT;
		}

		public int CreateSocket(IPEndPoint remoteIpEP, int localPort) {
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			try {
				socket.Bind(new IPEndPoint(IPAddress.Any, localPort));
			} catch (Exception) { return -1; }
			socket.EnableBroadcast = true;

			UdpClientSocketObject socketObj = new UdpClientSocketObject();
			socketObj.socket = socket;
			socketObj.Children = null; // 不显示隐藏的3级节点
			socketObj.remoteIpEP = remoteIpEP;
            socketObj.DisplayName = GetDisplayName(socketObj.remoteIpEP);
			socketObj.Parent = this;
			socketObjs.Add(socketObj);

			AsynchronousSocketListener.StartRead(socketObj);

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
	}
}
