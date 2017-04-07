using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TCPTools {
	public class UDPGroupSocket : SocketModel {
		public UDPGroupSocket() {
			DisplayName = "UDPGroup";
			Name = "UDPGroup";
			Icon = IconResources.ICON_CLIENT;
		}

		public int CreateSocket(IPEndPoint ipEP, int index1, int index2) {
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			UnicastIPAddressInformationCollection ipInf = interfaces[index1].GetIPProperties().UnicastAddresses;
			IPAddress localAddress = ipInf[index2].Address;

			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			try {
				socket.Bind(new IPEndPoint(localAddress, ipEP.Port));
			} catch (Exception) { return -1; }

			MulticastOption mcastOpt = new MulticastOption(ipEP.Address, localAddress);
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOpt);
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, 0);
			UdpGroupSocketObject socketObj = new UdpGroupSocketObject();
			socketObj.socket = socket;
			socketObj.remoteIpEP = ipEP;
			socketObj.DisplayName = GetDisplayName(ipEP);
			socketObj.Parent = this;
			socketObj.Children = null;
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
