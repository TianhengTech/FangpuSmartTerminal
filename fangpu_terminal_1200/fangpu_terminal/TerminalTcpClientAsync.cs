using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace fangpu_terminal
{
    public class StateObject
    {
        // Client socket.     
        public Socket workSocket = null;
        // Size of receive buffer.     
        public const int BufferSize = 256;
        // Receive buffer.     
        public byte[] buffer = new byte[BufferSize];
        // Received data string.     
        public StringBuilder sb = new StringBuilder();
    }

    class TerminalTcpClientAsync
    {
             //委托
        private delegate void delSocketDataArrival(byte[] data);
        static delSocketDataArrival socketDataArrival = socketDataArrivalHandler;

        private delegate void delSocketDisconnected();
        static delSocketDisconnected socketDisconnected = socketDisconnectedHandler;

        public static Socket theSocket = null;
        private static string remoteHost = "192.168.1.71";
        private static int remotePort = 6666;

        private static String SockErrorStr = null;
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        private static Boolean IsconnectSuccess = false;   //异步连接情况，由异步连接回调函数置位
        private static object lockObj_IsConnectSuccess = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strIp"></param>
        /// <param name="iPort"></param>
        public TerminalTcpClientAsync()
        {
            remoteHost = Properties.TerminalParameters.Default.terminal_server_ip;
            remotePort = Properties.TerminalParameters.Default.terminal_server_port;
        }

        /// <summary>
        /// 设置心跳
        /// </summary>
        private static void SetXinTiao()
        {
            //byte[] inValue = new byte[] { 1, 0, 0, 0, 0x20, 0x4e, 0, 0, 0xd0, 0x07, 0, 0 };// 首次探测时间20 秒, 间隔侦测时间2 秒
            byte[] inValue = new byte[] { 1, 0, 0, 0, 0x88, 0x13, 0, 0, 0xd0, 0x07, 0, 0 };// 首次探测时间5 秒, 间隔侦测时间2 秒
            theSocket.IOControl(IOControlCode.KeepAliveValues, inValue, null);
        }

        /// <summary>
        /// 创建套接字+异步连接函数
        /// </summary>
        /// <returns></returns>
        private static bool socket_create_connect()
        {
            IPAddress ipAddress = IPAddress.Parse(remoteHost);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, remotePort);
            theSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            theSocket.SendTimeout = 1000;

            SetXinTiao();//设置心跳参数

            #region 异步连接代码

            TimeoutObject.Reset();  //复位timeout事件
            try
            {
                theSocket.BeginConnect(remoteEP, connectedCallback, theSocket);
            }
            catch (Exception err)
            {
                SockErrorStr = err.ToString();
                return false;
            }
            if (TimeoutObject.WaitOne(10000, false))//直到timeout，或者TimeoutObject.set()
            {
                if (IsconnectSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                SockErrorStr = "Time Out";
                return false;
            }
            #endregion
        }

        /// <summary>
        /// 同步receive函数
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <returns></returns>
        public string socket_receive(byte[] readBuffer)
        {
            try
            {
                if (theSocket == null)
                {
                    socket_create_connect();
                }
                else if (!theSocket.Connected)
                {
                    if (!IsSocketConnected())
                        Reconnect();
                }

                int bytesRec = theSocket.Receive(readBuffer);

                if (bytesRec == 0)
                {
                    //warning 0 bytes received
                }
                return Encoding.ASCII.GetString(readBuffer, 0, bytesRec);
            }
            catch (SocketException se)
            {
                //print se.ErrorCode
                throw;
            }
        }

        /// <summary>
        /// 同步send函数
        /// </summary>
        /// <param name="sendMessage"></param>
        /// <returns></returns>
        public bool socket_send(string sendMessage)
        {
            if (checkSocketState())
            {
                return SendData(sendMessage);
            }
            return false;
        }

        /// <summary>
        /// 断线重连函数
        /// </summary>
        /// <returns></returns>
        private static bool Reconnect()
        {
            //关闭socket
            theSocket.Shutdown(SocketShutdown.Both);

            theSocket.Disconnect(true);
            IsconnectSuccess = false;

            theSocket.Close();

            //创建socket
            return socket_create_connect();
        }

        /// <summary>
        /// 当socket.connected为false时，进一步确定下当前连接状态
        /// </summary>
        /// <returns></returns>
        private bool IsSocketConnected()
        {
            #region remarks
            /********************************************************************************************
             * 当Socket.Conneted为false时， 如果您需要确定连接的当前状态，请进行非阻塞、零字节的 Send 调用。
             * 如果该调用成功返回或引发 WAEWOULDBLOCK 错误代码 (10035)，则该套接字仍然处于连接状态；        
             * 否则，该套接字不再处于连接状态。
             * Depending on http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.connected.aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-2
            ********************************************************************************************/
            #endregion

            #region 过程
            // This is how you can determine whether a socket is still connected.
            bool connectState = true;
            bool blockingState = theSocket.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                theSocket.Blocking = false;
                theSocket.Send(tmp, 0, 0);
                //Console.WriteLine("Connected!");
                connectState = true;    //若Send错误会跳去执行catch体，而不会执行其try体里其之后的代码
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    //Console.WriteLine("Still Connected, but the Send would block");
                    connectState = true;
                }

                else
                {
                    //Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    connectState = false;
                }
            }
            finally
            {
                theSocket.Blocking = blockingState;
            }

            //Console.WriteLine("Connected: {0}", client.Connected);
            return connectState;
            #endregion
        }

        /// <summary>
        /// 另一种判断connected的方法，但未检测对端网线断开或ungraceful的情况
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsSocketConnected(Socket s)
        {
            #region remarks
            /* As zendar wrote, it is nice to use the Socket.Poll and Socket.Available, but you need to take into consideration 
             * that the socket might not have been initialized in the first place. 
             * This is the last (I believe) piece of information and it is supplied by the Socket.Connected property. 
             * The revised version of the method would looks something like this:   
             * from：http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c       */
            #endregion

            #region 过程

            if (s == null)
                return false;
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);

            /* The long, but simpler-to-understand version:

                    bool part1 = s.Poll(1000, SelectMode.SelectRead);
                    bool part2 = (s.Available == 0);
                    if ((part1 && part2 ) || !s.Connected)
                        return false;
                    else
                        return true;

            */
            #endregion
        }

        /// <summary>
        /// 异步连接回调函数
        /// </summary>
        /// <param name="iar"></param>
        static void connectedCallback(IAsyncResult iar)
        {
            #region <remarks>
            /// 1、置位IsconnectSuccess
            #endregion </remarks>

            lock (lockObj_IsConnectSuccess)
            {
                Socket client = (Socket)iar.AsyncState;
                try
                {
                    client.EndConnect(iar);
                    IsconnectSuccess = true;
                    StartKeepAlive();                     //开始KeppAlive检测
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    SockErrorStr = e.ToString();
                    IsconnectSuccess = false;
                }
                finally
                {
                    TimeoutObject.Set();
                }
            }
        }

        /// <summary>
        /// 开始KeepAlive检测函数
        /// </summary>
        private static void StartKeepAlive()
        {
            theSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallback), theSocket);
        }

        /// <summary>
        /// BeginReceive回调函数
        /// </summary>
        static byte[] buffer = new byte[1024];
        private static void OnReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket peerSock = (Socket)ar.AsyncState;
                int BytesRead = peerSock.EndReceive(ar);
                if (BytesRead > 0)
                {
                    byte[] tmp = new byte[BytesRead];
                    Array.ConstrainedCopy(buffer, 0, tmp, 0, BytesRead);
                    if (socketDataArrival != null)
                    {
                        socketDataArrival(tmp);
                    }
                }
                else//对端gracefully关闭一个连接
                {
                    if (theSocket.Connected)//上次socket的状态
                    {
                        if (socketDisconnected != null)
                        {
                            //1-重连
                            socketDisconnected();
                            //2-退出，不再执行BeginReceive
                            return;
                        }
                    }
                }
                //此处buffer似乎要清空--待实现 zq
                theSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallback), theSocket);
            }
            catch (Exception ex)
            {
                if (socketDisconnected != null)
                {
                    socketDisconnected();  //Keepalive检测网线断开引发的异常在这里捕获
                    return;
                }
            }
        }

        /// <summary>
        /// 异步收到消息处理器
        /// </summary>
        /// <param name="data"></param>
        private static void socketDataArrivalHandler(byte[] data)
        {
            TerminalQueues.tcpdownlinkqueue.Enqueue(Encoding.ASCII.GetString(data, 0, data.Length));
        }

        /// <summary>
        /// socket由于连接中断(软/硬中断)的后续工作处理器
        /// </summary>
        private static void socketDisconnectedHandler()
        {
            Reconnect();
        }

        /// <summary>
        /// 检测socket的状态
        /// </summary>
        /// <returns></returns>
        public static bool checkSocketState()
        {
            try
            {
                if (theSocket == null)
                {
                    return socket_create_connect();
                }
                else if (IsconnectSuccess)
                {
                    return true;
                }
                else//已创建套接字，但未connected
                {
                    #region 异步连接代码

                    TimeoutObject.Reset();  //复位timeout事件
                    try
                    {
                        IPAddress ipAddress = IPAddress.Parse(remoteHost);
                        IPEndPoint remoteEP = new IPEndPoint(ipAddress, remotePort);
                        theSocket.BeginConnect(remoteEP, connectedCallback, theSocket);

                        SetXinTiao();//设置心跳参数
                    }
                    catch (Exception err)
                    {
                        SockErrorStr = err.ToString();
                        return false;
                    }
                    if (TimeoutObject.WaitOne(2000, false))//直到timeout，或者TimeoutObject.set()
                    {
                        if (IsconnectSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        SockErrorStr = "Time Out";
                        return false;
                    }

                    #endregion
                }

            }
            catch (SocketException se)
            {
                SockErrorStr = se.ToString();
                return false;
            }
        }


        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="dataStr"></param>
        /// <returns></returns>
        public static bool SendData(string dataStr)
        {
            bool result = false;
            if (dataStr == null || dataStr.Length < 0)
                return result;
            try
            {
                byte[] cmd = Encoding.Default.GetBytes(dataStr);
                int n = theSocket.Send(cmd);
                if (n < 1)
                    result = false;
            }
            catch (Exception ee)
            {
                SockErrorStr = ee.ToString();
                result = false;
            }
            return result;
        }

        public void  socket_stop_connect()
        {
            if (IsconnectSuccess == true)
            {
                theSocket.Shutdown(SocketShutdown.Both);

                theSocket.Disconnect(true);
                IsconnectSuccess = false;

                theSocket.Close();
                socketDisconnected = null;
            }
        }
    
    }
}
