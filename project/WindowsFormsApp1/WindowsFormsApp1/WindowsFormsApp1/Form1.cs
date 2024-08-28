using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(new Action(() => { 

                listView1.Items.Clear();

                var data = GetAllMacAddressesAndIppairs();
                int i = 0;


                foreach (var x in data)
                {
                    i++;

                    ListViewItem item = new ListViewItem(new string[] { i.ToString(), x.IPAddress, x.MacAddress, x.HostName });
                    listView1.BeginInvoke(new Action(() => {
                        listView1.Items.Add(item);
                    }));
                    
                }
            }));




        }

        public string getMacByIp(string ip)
        {
            var macIpPairs = GetAllMacAddressesAndIppairs();
            int index = macIpPairs.FindIndex(x => x.IPAddress == ip);
            if (index >= 0)
            {
                return macIpPairs[index].MacAddress.ToUpper();
            }
            else
            {
                return null;
            }
        }

        public List<IPInfo> GetAllMacAddressesAndIppairs()
        {
            List<IPInfo> mip = new List<IPInfo>();
            Process pProcess = new Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a ";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string cmdOutput = pProcess.StandardOutput.ReadToEnd();
            string pattern = @"(?<ip>([0-9]{1,3}\.?){4})\s*(?<mac>([a-f0-9]{2}-?){6})\s*(?<type>([a-zA-Z]){3})";


            foreach (Match m in Regex.Matches(cmdOutput, pattern, RegexOptions.IgnoreCase))
            {
                string type = m.Groups["type"].Value;
                if (type.ToLower().Equals("dyn"))
                {
                    string machine_name = GetMachineNameFromIPAddress(m.Groups["ip"].Value);
                    mip.Add(new IPInfo(m.Groups["mac"].Value, m.Groups["ip"].Value));

                }

            }

            return mip;
        }

        public static String ReverseIPLookup(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("IP address is not IPv4.", "ipAddress");
            var domain = String.Join(
              ".", ipAddress.GetAddressBytes().Reverse().Select(b => b.ToString())
            ) + ".in-addr.arpa";
            return DnsGetPtrRecord(domain);
        }

        static String DnsGetPtrRecord(String domain)
        {
            const Int16 DNS_TYPE_PTR = 0x000C;
            const Int32 DNS_QUERY_STANDARD = 0x00000000;
            const Int32 DNS_ERROR_RCODE_NAME_ERROR = 9003;
            IntPtr queryResultSet = IntPtr.Zero;
            try
            {
                var dnsStatus = DnsQuery(
                  domain,
                  DNS_TYPE_PTR,
                  DNS_QUERY_STANDARD,
                  IntPtr.Zero,
                  ref queryResultSet,
                  IntPtr.Zero
                );
                if (dnsStatus == DNS_ERROR_RCODE_NAME_ERROR)
                    return null;
                if (dnsStatus != 0)
                    throw new Win32Exception(dnsStatus);
                DnsRecordPtr dnsRecordPtr;
                for (var pointer = queryResultSet; pointer != IntPtr.Zero; pointer = dnsRecordPtr.pNext)
                {
                    dnsRecordPtr = (DnsRecordPtr)Marshal.PtrToStructure(pointer, typeof(DnsRecordPtr));
                    if (dnsRecordPtr.wType == DNS_TYPE_PTR)
                        return Marshal.PtrToStringUni(dnsRecordPtr.pNameHost);
                }
                return null;
            }
            finally
            {
                const Int32 DnsFreeRecordList = 1;
                if (queryResultSet != IntPtr.Zero)
                    DnsRecordListFree(queryResultSet, DnsFreeRecordList);
            }
        }

        [DllImport("Dnsapi.dll", EntryPoint = "DnsQuery_W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        static extern Int32 DnsQuery(String lpstrName, Int16 wType, Int32 options, IntPtr pExtra, ref IntPtr ppQueryResultsSet, IntPtr pReserved);

        [DllImport("Dnsapi.dll", SetLastError = true)]
        static extern void DnsRecordListFree(IntPtr pRecordList, Int32 freeType);

        [StructLayout(LayoutKind.Sequential)]
        struct DnsRecordPtr
        {
            public IntPtr pNext;
            public String pName;
            public Int16 wType;
            public Int16 wDataLength;
            public Int32 flags;
            public Int32 dwTtl;
            public Int32 dwReserved;
            public IntPtr pNameHost;
        }
        private string GetMachineNameFromIPAddress(string ipAdress)
        {
            string machineName = string.Empty;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAdress);

                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {

            }
            return machineName;
        }
        public class IPInfo
        {
            public IPInfo(string macAddress, string ipAddress)
            {
                this.MacAddress = macAddress;
                this.IPAddress = ipAddress;
            }

            public string MacAddress { get; private set; }
            public string IPAddress { get; private set; }

            private string _HostName = string.Empty;
            public string HostName
            {
                
                get
                {
                    Task t = Task.Factory.StartNew(new Action(() => {

                        if (string.IsNullOrEmpty(this._HostName))
                        {
                            try
                            {
                                
                                this._HostName = ReverseIPLookup(System.Net.IPAddress.Parse( this.IPAddress));
                                
                            }
                            catch
                            {
                                this._HostName = string.Empty;
                            }
                        }
                        
                    }));
                    t.Wait();
                    return this._HostName;

                }
            }

        }
    }
}
