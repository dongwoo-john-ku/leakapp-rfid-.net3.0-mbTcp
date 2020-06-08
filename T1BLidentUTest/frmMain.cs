using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using T1UInterface;
using System.IO;

namespace T1BLidentUTest
{
    public partial class frmMain : Form
    {
        private UInterface ui = null;

        private BackgroundWorker bgService;

        private BackgroundWorker bgTrigger;
        private BackgroundWorker bgTeaching;
        private BackgroundWorker bgMonitoring;

        private DataTable dtData;
        private DataTable dtTeaching;
        private Hashtable htEPC;
        private Hashtable htErrEPC;

        public string currentStatus = "Idle";

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            ui = new UInterface((int) Enum_UIF.BL_MODEL.TBEN_S2_2RFID_4DXP);

            buttonState(false);

            //Table
            dtData = new DataTable("DATA");

            dtData.Columns.Add("NO");
            dtData.Columns.Add("HEAD");
            dtData.Columns.Add("EPC");
            dtData.Columns.Add("ACTIVE");
            dtData.Columns.Add("TIME");
            dtData.Columns.Add("T_AVG");
            dtData.Columns.Add("T_STDEV");
            dtData.Columns.Add("T_THRESHOLD");
            dtData.Columns.Add("T_CNT");
            dtData.Columns.Add("CURRENT_DATA");
            dtData.Columns.Add("M_CNT");
            dtData.Columns.Add("RSSI");
           
            
            //dgvData.DataSource = dtData;


            dtTeaching = new DataTable("TEACHING");
            dtTeaching.Columns.Add("NO");
            dtTeaching.Columns.Add("HEAD");
            dtTeaching.Columns.Add("EPC");
            dtTeaching.Columns.Add("LEAK_DATA");
            dtTeaching.Columns.Add("TIME");


            htEPC = new Hashtable();
            htErrEPC = new Hashtable();


            //dtData.AcceptChanges();

            //testAlarmGrid();
            //testMonitorGrid();

            dataGridView1.DataSource = dtData;

        }

        private void testMonitorGrid()
        {

            DataGridViewRow dgRow = new DataGridViewRow();
            dgRow.CreateCells(dgvData);

            dgRow.Cells[0].Value = "1";
            dgRow.Cells[1].Value = "1";
            dgRow.Cells[2].Value = "1";
            dgRow.Cells[3].Value = "";
            dgRow.Cells[4].Value = "";
            dgRow.Cells[5].Value = "";
            dgRow.Cells[6].Value = "";
            dgRow.Cells[7].Value = "";
            dgRow.Cells[8].Value = "";

            dgvData.Rows.Add(dgRow);

            dgRow = new DataGridViewRow();
            dgRow.CreateCells(dgvData);

            dgRow.Cells[0].Value = "10";
            dgRow.Cells[1].Value = "10";
            dgRow.Cells[2].Value = "";
            dgRow.Cells[3].Value = "";
            dgRow.Cells[4].Value = "";
            dgRow.Cells[5].Value = "";
            dgRow.Cells[6].Value = "";
            dgRow.Cells[7].Value = "";
            dgRow.Cells[8].Value = "";

            dgRow.DefaultCellStyle.BackColor = Color.Red;
            dgvData.Rows.Add(dgRow);

            dgRow = new DataGridViewRow();
            dgRow.CreateCells(dgvData);

            dgRow.Cells[0].Value = "2";
            dgRow.Cells[1].Value = "2";
            dgRow.Cells[2].Value = "2";
            dgRow.Cells[3].Value = "";
            dgRow.Cells[4].Value = "";
            dgRow.Cells[5].Value = "";
            dgRow.Cells[6].Value = "";
            dgRow.Cells[7].Value = "";
            dgRow.Cells[8].Value = "";

            dgvData.Rows.Add(dgRow);
        }

        private void testAlarmGrid()
        {
            DataGridViewRow dgRow = new DataGridViewRow();
            dgRow.CreateCells(dgAlarms);

            dgRow.Cells[0].Value = (dgAlarms.Rows.Count + 1).ToString();
            dgRow.Cells[1].Value = "1";
            dgRow.Cells[2].Value = "1";
            dgRow.Cells[3].Value = "1";
            dgRow.Cells[4].Value = "1";
            dgRow.Cells[5].Value = "1";

            dgAlarms.Rows.Add(dgRow);

            dgRow = new DataGridViewRow();
            dgRow.CreateCells(dgAlarms);

            dgRow.Cells[0].Value = (dgAlarms.Rows.Count + 1).ToString();
            dgRow.Cells[1].Value = "10";
            dgRow.Cells[2].Value = "10";
            dgRow.Cells[3].Value = "10";
            dgRow.Cells[4].Value = "10";
            dgRow.Cells[5].Value = "10";

            dgAlarms.Rows.Add(dgRow);

            dgRow = new DataGridViewRow();
            dgRow.CreateCells(dgAlarms);

            dgRow.Cells[0].Value = (dgAlarms.Rows.Count + 1).ToString();
            dgRow.Cells[1].Value = "2";
            dgRow.Cells[2].Value = "2";
            dgRow.Cells[3].Value = "2";
            dgRow.Cells[4].Value = "2";
            dgRow.Cells[5].Value = "2";

            dgAlarms.Rows.Add(dgRow);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ui.ConnectDevice(txtIpaddr.Text);
                if (ui.isConnected)
                {
                    tssDeviceInfo.Text = "Connected";
                    currentStatusLabel.Text = "Connected!";
                    ui.startReadInputParameter();
                    startReadInputParameter();
                    buttonState(true);

                    changeIdleMode(0);
                    changeIdleMode(1);
                    
                    currentStatus = "readyInventory";
                    //btnExit.Enabled = false;

                }
                else
                {
                    stopReadInputParameter();
                    tssDeviceInfo.Text = "Disconnected";
                    currentStatusLabel.Text = "Disconnected!!";
                    buttonState(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    

        private void startReadInputParameter()
        {
            try
            {
                //firstFlag = true;
                int interval = 50;
                bgService = new BackgroundWorker();
                bgService.WorkerSupportsCancellation = true;
                bgService.DoWork += new DoWorkEventHandler(backgroundWorkerService_DoWork);
                bgService.RunWorkerCompleted +=
                    new RunWorkerCompletedEventHandler(backgroundWorkerService_RunWorkerCompleted);
                bgService.RunWorkerAsync(interval);

                //eventLog1.WriteEntry("Start bgMakeAlarm Service");
            }
            catch (Exception ex)
            {
                throw new Exception("Error Start : " + ex.Message);
            }
        }

        private void stopReadInputParameter()
        {
            if (bgService != null)
            {
                if (bgService.IsBusy)
                {
                    bgService.CancelAsync();
                }
            }
        }


        private void backgroundWorkerService_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            int arg = (int)e.Argument;
            string ret = "";
            try
            {
                // Start the time-consuming operation.
                ret = ReadInputProcess(bw, arg);
            }
            catch (Exception ex)
            {

                ret = "ERROR :" + ex.Message;
            }

            e.Result = ret;

            // If the operation was canceled by the user, 
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }

        }

        private void backgroundWorkerService_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                // The user canceled the operation.
                string msg = String.Format("Stop Read Parameter Service");
                Console.WriteLine(@"Making data RunWorkerCompleted:" + msg, "INFO");
                //Logging.LogInfo(msg);
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                Console.WriteLine(@"Making data RunWorkerCompleted:Error" + msg, "ERROR");
                //Logging.LogInfo(msg);
            }
            else
            {
                // 초기화 정상 완료.
                if (e.Result.ToString().IndexOf("ERROR :") == 0)
                {
                    string msg = String.Format("{0}", "Read Input Parameter " + e.Result);

                    Console.WriteLine(@"Making data " + msg, "ERROR");

                    //Logging.LogInfo(msg);
                }
                else
                {
                    string msg = String.Format("Result = {0}", e.Result);
                    Console.WriteLine(@"Making data  RunWorkerCompleted:" + msg, "INFO");
                    //Logging.LogInfo(msg);
                }

            }

        }

        private string ReadInputProcess(BackgroundWorker bw, int sleepPeriod)
        {
            PerformanceCounter performanceCounter1 = new PerformanceCounter();
            bool bgTeachingCancelled = false;
            bool bgMonitoringCancelled = false;

            while (!bw.CancellationPending)
            {
                performanceCounter1.Start();

                for (int i = 0; i < 2; i++)
                {

                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        if (ui.inputParameters[i].HeadNotConnected == false)
                        {
                            if (i == 0)
                            {
                                tssChannel1.Text = "Channel 1";
                                if (ui.inputParameters[i].Error)
                                    tssChannel1.BackColor = Color.Magenta;
                                else
                                    tssChannel1.BackColor = Color.LawnGreen;
                            }
                            else
                            {

                                tssChannel2.Text = "Channel 2";
                                if (ui.inputParameters[i].Error)
                                    tssChannel2.BackColor = Color.Magenta;
                                else
                                    tssChannel2.BackColor = Color.LawnGreen;
                            }

                        }
                        else
                        {
                            if (i == 0)
                            {
                                tssChannel1.Text = "Channel 1";
                                tssChannel1.BackColor = Color.Gray;

                                
                            }
                            else
                            {

                                tssChannel2.Text = "Channel 2";
                                tssChannel2.BackColor = Color.Gray;
                            }
                        }
                        try
                        {
                            if (i == 0)
                            {
                                tssStatus1.Text = Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), ui.inputParameters[i].ResponseCode);

                            }
                            else
                            {
                                tssStatus2.Text = Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), ui.inputParameters[i].ResponseCode);
                            }


                            if (bgTeaching.CancellationPending) {
                                bgTeachingCancelled = true;
                            }

                            else
                            {
                                if (bgTeachingCancelled)
                                {
                                    userInputValueEnable(true);          

                                    
                                    btnTeaching.Enabled = true;
                                    btnTrigger.Enabled = true;
                                    btnReadSensor.Enabled = true;
                                    currentStatusLabel.Text = "Stopped Teaching";
                                    bgTeachingCancelled = false;
                                }
                            }


                            if (bgMonitoring.CancellationPending)
                            {
                                bgMonitoringCancelled = true;
                            }

                            else
                            {
                                if (bgMonitoringCancelled)
                                {
                                    userInputValueEnable(true);          

                                    btnTeaching.Enabled = true;
                                    btnTrigger.Enabled = true;
                                    btnReadSensor.Enabled = true;
                                    currentStatusLabel.Text = "Stopped Monitoring";
                                    bgMonitoringCancelled = false;
                                }
                            }

                                 
                        }
                        catch (Exception ex)
                        {

                        }



                    }
                    ));

                }

                performanceCounter1.Stop();

                //int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000) - 500);
                int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000));
                if (interval < 0)
                    interval = 0;

                Thread.Sleep(interval);

            }

            return "Done";
        }
    

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnTrigger.FlatStyle != FlatStyle.Standard)
                {
                    //MessageBox.Show("Trigger가 동작 중입니다. 정지 후 해제 하시기 바랍니다.");
                    MessageBox.Show("After stopping searching.");
                    return;
                }

                if (btnTeaching.FlatStyle != FlatStyle.Standard)
                {
                   // MessageBox.Show("Teaching 중입니다. 정지 후 해제 하시기 바랍니다.");
                    MessageBox.Show("After stopping Teaching.");
                    return;
                }

                if (btnReadSensor.FlatStyle != FlatStyle.Standard)
                {
                    // MessageBox.Show("Monitoring 중입니다. 정지 후 해제 하시기 바랍니다.");
                    MessageBox.Show("After stopping Monitoring.");
                    return;
                }

                if (ui.isConnected)
                {
                    ui.stopReadInputParameter();
                    Thread.Sleep(500);
                    ui.disconnectDevice();
                    tssDeviceInfo.Text = "Disconnected";
                    buttonState(false);

                    //btnExit.Enabled = true;
                }
                currentStatusLabel.Text = "Disconnected!";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        
        private void buttonState(bool ena)
        {
            btnConnect.Enabled = !ena;
            btnDisconnect.Enabled = ena;
            panCmd.Enabled = ena;
            btnAlarmClear.Enabled = ena;
            
        }

        private byte getSensorCode(byte[] data)
        {
            byte ret = 0;
            try
            {
                byte[] bytes = new byte[1];
                bytes[0] = data[1];
                var bits = new BitArray(bytes);

                byte[] b = new byte[1];
                b[0] = 31;
                var cal = new BitArray(b);
                bits.And(cal);

                bits.CopyTo(bytes, 0);

                ret = bytes[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
       
            }

            return ret;

        }

        private void userInputValueEnable(bool operatingStatus)
        {
            txtIpaddr.Enabled = operatingStatus;
            chkFilter.Enabled = operatingStatus;
            txtFilter.Enabled = operatingStatus;
            numAlarmCnt.Enabled = operatingStatus;
            numMissAlarm.Enabled = operatingStatus;
            numResetCnt.Enabled = operatingStatus;
            numChnInterval.Enabled = operatingStatus;
            numInterval.Enabled = operatingStatus;
            numMissTimeout.Enabled = operatingStatus;

        }

        private void btnTrigger_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnTrigger.FlatStyle == FlatStyle.Standard)
                {
                    if(Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), 0x0000) != tssStatus1.Text
                        || Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), 0x0000) != tssStatus2.Text)
                    {

                        return;
                    }

                    btnTrigger.FlatStyle = FlatStyle.Flat;
                    btnTrigger.Text = "Searching Off";
                    startTrigger();
                    currentStatusLabel.Text = "Searching Process";

                    userInputValueEnable(false);
                    btnTeaching.Enabled = false;
                    btnReadSensor.Enabled = false;

                }
                else
                {
                    btnTrigger.FlatStyle = FlatStyle.Standard;
                    btnTrigger.Text = "Searching On";
                    stopTrigger();
                    currentStatusLabel.Text = "Stopped Searching";
                    userInputValueEnable(true);          
                    btnTeaching.Enabled = true;
//                    btnReadSensor.Enabled = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Trigger Error :" + ex.Message);
            }
        }
        private void startTrigger()
        {
            try
            {
                htEPC.Clear();

                //firstFlag = true;
                dtData.Rows.Clear();
                dgvData.Rows.Clear();
                int interval = Convert.ToInt16(numInterval.Value);
                bgTrigger = new BackgroundWorker();
                bgTrigger.WorkerSupportsCancellation = true;
                bgTrigger.DoWork += new DoWorkEventHandler(backgroundWorkerbgTrigger_DoWork);
                bgTrigger.RunWorkerCompleted +=
                    new RunWorkerCompletedEventHandler(backgroundWorkerbgTrigger_RunWorkerCompleted);
                bgTrigger.RunWorkerAsync(interval);

                //eventLog1.WriteEntry("Start bgMakeAlarm Service");
            }
            catch (Exception ex)
            {
                throw new Exception("Error Start : " + ex.Message);
            }
        }

        private void stopTrigger()
        {
            if (bgTrigger != null)
            {
                if (bgTrigger.IsBusy)
                {
                    bgTrigger.CancelAsync();
                    currentStatus = "readyMonitoring";
                }
            }
        }

        private void startTeaching()
        {
            try
            {
                //firstFlag = true;
                
                int interval = Convert.ToInt16(1000);
                bgTeaching = new BackgroundWorker();
                bgTeaching.WorkerSupportsCancellation = true;
                bgTeaching.DoWork += new DoWorkEventHandler(backgroundWorkerbgTeaching_DoWork);
                bgTeaching.RunWorkerCompleted +=
                    new RunWorkerCompletedEventHandler(backgroundWorkerbgTeaching_RunWorkerCompleted);
                bgTeaching.RunWorkerAsync(interval);

                //eventLog1.WriteEntry("Start bgMakeAlarm Service");
            }
            catch (Exception ex)
            {
                throw new Exception("Error Start : " + ex.Message);
            }
        }

        private void stopTeaching()
        {
            if (bgTeaching != null)
            {
                if (bgTeaching.IsBusy)
                {
                    bgTeaching.CancelAsync();
                }
            }

            checkThresold();
        }

        private void checkThresold()
        {
            for (int i = 0; i < dgvData.Rows.Count; i++)
            {
                if (double.Parse(dgvData.Rows[i].Cells[3].Value.ToString()) <= 5)
                {
                    dgvData.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    currentStatusLabel.Text = "Check! the Red tag(s)'s position";
                }
                else
                {
                    dgvData.Rows[i].DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        private void startMonitoring()
        {
            try
            {
                //firstFlag = true;
                htErrEPC.Clear();

                int interval = Convert.ToInt16(1000);
                bgMonitoring = new BackgroundWorker();
                bgMonitoring.WorkerSupportsCancellation = true;
                bgMonitoring.DoWork += new DoWorkEventHandler(backgroundWorkerbgMnitoring_DoWork);
                bgMonitoring.RunWorkerCompleted +=
                    new RunWorkerCompletedEventHandler(backgroundWorkerbgMonitoring_RunWorkerCompleted);
                bgMonitoring.RunWorkerAsync(interval);

                //eventLog1.WriteEntry("Start bgMakeAlarm Service");
            }
            catch (Exception ex)
            {
                throw new Exception("Error Start : " + ex.Message);
            }
        }


        private void stopMonitoring()
        {
            if (bgMonitoring != null)
            {
                if (bgMonitoring.IsBusy)
                {
                    bgMonitoring.CancelAsync();
                }
            }
        }


        private void backgroundWorkerbgTrigger_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            int arg = (int)e.Argument;
            string ret = "";
            try
            {
                // Start the time-consuming operation.
                ret = TriggerOn(bw, arg);
            }
            catch (Exception ex)
            {

                ret = "ERROR :" + ex.Message;
            }

            e.Result = ret;

            // If the operation was canceled by the user, 
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }

        }

        private void backgroundWorkerbgTeaching_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Do not access the form's BackgroundWorker reference directly.
                // Instead, use the reference provided by the sender parameter.
                BackgroundWorker bw = sender as BackgroundWorker;

                // Extract the argument.
                int arg = (int)e.Argument;
                string ret = "";
                try
                {
                    // Start the time-consuming operation.
                    ret = TeachingOn(bw, arg);
                }
                catch (Exception ex)
                {

                    ret = "ERROR :" + ex.Message;
                }

                e.Result = ret;

                // If the operation was canceled by the user, 
                // set the DoWorkEventArgs.Cancel property to true.
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
            catch(Exception)
            {

            }
        }

        private void backgroundWorkerbgMnitoring_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Do not access the form's BackgroundWorker reference directly.
                // Instead, use the reference provided by the sender parameter.
                BackgroundWorker bw = sender as BackgroundWorker;

                // Extract the argument.
                int arg = (int)e.Argument;
                string ret = "";
                try
                {
                    // Start the time-consuming operation.
                    ret = MonitoringOn(bw, arg);
                }
                catch (Exception ex)
                {

                    ret = "ERROR :" + ex.Message;
                }

                e.Result = ret;

                // If the operation was canceled by the user, 
                // set the DoWorkEventArgs.Cancel property to true.
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
            catch(Exception)
            {

            }
        }

        private string getAVG(string head, string epc)
        {
            double v1 = 0;
            int nCnt = 0;
            double ret = 0;
            try
            {
                foreach (DataRow row in dtTeaching.Rows)
                {
                    if (row["HEAD"].ToString().Equals(head) && row["EPC"].ToString().Equals(epc))
                    {
                        nCnt++;
                        v1 = v1 + double.Parse(row["LEAK_DATA"].ToString());
                    }
                }

                ret = Math.Round((v1 / nCnt), 2);
            }
            catch(Exception)
            {
                
            }
            return ret.ToString();
        }

        private string getAVGTime(string head, string epc)
        {
            int v1 = 0;
            int nCnt = 0;
            int ret = 0;

            try
            {
                foreach (DataRow row in dtTeaching.Rows)
                {
                    if (row["HEAD"].ToString().Equals(head) && row["EPC"].ToString().Equals(epc))
                    {
                        nCnt++;
                        v1 = v1 + Int32.Parse(row["TIME"].ToString());
                    }
                }

                ret = (v1 / nCnt);
            }
            catch (Exception)
            {

            }
            return ret.ToString();
        }

        private string getSTDEV(string head, string epc)
        {
            int nCnt = 0;
            double ret = 0;
            List<double> leakValues = null;
            try
            {
                leakValues = new List<double> { };
                foreach (DataRow row in dtTeaching.Rows)
                {
                    if (row["HEAD"].ToString().Equals(head) && row["EPC"].ToString().Equals(epc))
                    {
                        nCnt++;
                        double v1 = double.Parse(row["LEAK_DATA"].ToString());
                        leakValues.Add(v1);

                    }
                }
                ret = leakValues.StandardDeviation();

                if (ret == double.NaN)
                    ret = 0;
            }
            catch(Exception)
            {

            }

            ret = Math.Round(ret, 2);
            return ret.ToString().Equals("NaN") ? "0" : ret.ToString();
        }


        public string ParseHex(string hex)
        {
            string ret = "";

            for (int i = 0; i < hex.Length / 2; i++)
            {
                if (i == 1)
                {
                    ret += (Convert.ToInt32(hex.Substring(i * 2, 2), 16)).ToString();
                    break;
                }
            }
            return ret;
        }

        private string getThreshold(double avg, double stdev)
        {
            double ret = 0;
            try
            {
                if (avg == 0)
                {
                    ret = 0;
                }
                else if (avg > 0 & avg <= 5)
                {
                    ret = avg - 1;
                }
                else if (avg > 5 & avg <= 10)
                {
                    if (stdev <= 1)
                    {
                        ret = avg - 3;
                    }
                    else if (stdev > 1 & stdev < 2)
                    {
                        ret = avg - 4;
                    }
                    else
                    {
                        ret = avg - 5;
                    }
                }
                else if (avg > 10 & avg <= 25)
                {
                    if (stdev <= 1)
                    {
                        ret = avg - 5;
                    }
                    else if (stdev > 1 & stdev < 2)
                    {
                        ret = avg - stdev * 5;
                    }
                    else
                    {
                        ret = avg - 10;
                    }
                }
                else
                {
                    if (stdev <= 1)
                    {
                        ret = avg - 10;
                    }
                    else if (stdev > 1 & stdev < 2)
                    {
                        ret = avg - stdev * 6;
                    }
                    else
                    {
                        ret = avg - 15;
                    }
                }     

            }
            catch(Exception)
            {

            }

            if (ret == double.NaN || ret < 0)
                ret = 0;

            return Math.Round(ret, 0).ToString();
        }
        
        private string TeachingOn(BackgroundWorker bw, int sleepPeriod)
        {
            PerformanceCounter performanceCounter1 = new PerformanceCounter();

            while (!bw.CancellationPending)
            {
                performanceCounter1.Start();

                try
                {
                    if (ui != null && ui.isConnected)
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtData.Rows)
                            {
                                int channel = Convert.ToInt32(row["HEAD"].ToString());
                                byte[] epcdata = Util.StringToByteArray(htEPC[row["EPC"].ToString()].ToString());
                                ushort errorCode = 0;
                                string msg = "";

                                DateTime t1 = DateTime.Now;

                                byte[] rdata = null;
                                // Read Sensor Data
                                bool flag = ui.readDataUHF(channel, (byte)Enum_UIF.UHF_MEMORY.KILL_PASSWORD, 22,
                                    2, 500, 12, epcdata, true, out rdata, out errorCode, out msg);
                                if (flag)
                                {
                                    TimeSpan span = DateTime.Now.Subtract(t1);

                                    string time = span.Milliseconds.ToString();

                                    byte sensor = getSensorCode(rdata);

                                    DataRow rowTeaching = dtTeaching.NewRow();
                                    rowTeaching["NO"] = row["NO"].ToString();
                                    rowTeaching["HEAD"] = row["HEAD"].ToString();
                                    rowTeaching["EPC"] = row["EPC"].ToString();
                                    rowTeaching["LEAK_DATA"] = sensor.ToString();
                                    rowTeaching["TIME"] = time;

                                    dtTeaching.Rows.Add(rowTeaching);
                                        
                                    row["T_AVG"] = getAVG(row["HEAD"].ToString(), row["EPC"].ToString());
                                    row["T_STDEV"] = getSTDEV(row["HEAD"].ToString(), row["EPC"].ToString());
                                    row["T_THRESHOLD"] = getThreshold(double.Parse(row["T_AVG"].ToString()), double.Parse(row["T_STDEV"].ToString()));
                                    row["TIME"] = getAVGTime(row["HEAD"].ToString(), row["EPC"].ToString());

                                    if (row["T_CNT"].ToString().Length == 0)
                                    {
                                        row["T_CNT"] = "1";
                                    }
                                    else
                                    {
                                        int iCnt = Int32.Parse(row["T_CNT"].ToString()) + 1;

                                        if (iCnt > 1000) iCnt = 1;

                                        row["T_CNT"] = iCnt.ToString();
                                    }

                                    this.Invoke(new MethodInvoker(delegate ()
                                    {
                                        activeTeaching(row["EPC"].ToString());

                                        //for (int i = 0; i < dgvData.Rows.Count; i++)
                                        //{
                                        //    if (dgvData.Rows[i].Cells[2].Value.ToString().Equals(row["EPC"].ToString()))
                                        //    {
                                        //        dgvData.Rows[i].Cells[1].Value = row["HEAD"].ToString();
                                        //        dgvData.Rows[i].Cells[3].Value = row["T_AVG"].ToString();
                                        //        dgvData.Rows[i].Cells[4].Value = row["T_STDEV"].ToString();
                                        //        dgvData.Rows[i].Cells[5].Value = row["T_THRESHOLD"].ToString();
                                        //        dgvData.Rows[i].Cells[6].Value = row["T_CNT"].ToString();
                                        //        break;
                                        //    }
                                        //}
                                    }));
                                }
                                else
                                {
                                    changeIdleMode(channel);
                                    Thread.Sleep(50);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }


                performanceCounter1.Stop();

                //int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000) - 500);
                int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000));
                if (interval < 0)
                    interval = 0;

                Thread.Sleep(interval);

            }

            return "Done";
        }

        private void activeTeaching(string epc)
        {
            DataRow rowA = null;
            DataRow rowB = null;

            foreach(DataRow rowActive in dtData.Rows)
            {
                if(rowActive["EPC"].ToString().Equals(epc))
                {
                    if(rowA == null)
                    {
                        rowA = rowActive;
                    }
                    else
                    {
                        rowB = rowActive;

                        Int32 rowACount = Int32.Parse(rowA["T_CNT"].ToString());
                        Int32 rowBCount = Int32.Parse(rowB["T_CNT"].ToString());
 
                        if (rowACount * 0.95 < rowBCount & rowACount > rowBCount * 0.95)
                        {
                            /*
                            Int32 rowATime = Int32.Parse(rowA["TIME"].ToString());
                            Int32 rowBTime = Int32.Parse(rowB["TIME"].ToString());
                            */
                            if (Double.Parse(rowA["TIME"].ToString()) - 200 < Double.Parse(rowB["TIME"].ToString()) & Double.Parse(rowB["TIME"].ToString()) - 200 < Double.Parse(rowA["TIME"].ToString()))
                            {
                                /*
                                Int32 rowAStd = Int32.Parse(rowA["T_STDEV"].ToString());
                                Int32 rowBStd = Int32.Parse(rowB["T_STDEV"].ToString());
                                */
                                if (Double.Parse(rowA["T_STDEV"].ToString()) > Double.Parse(rowB["T_STDEV"].ToString()))
                                {
                                    rowA["ACTIVE"] = "0";
                                    rowB["ACTIVE"] = "1";
                                }
                                else
                                {
                                    rowA["ACTIVE"] = "1";
                                    rowB["ACTIVE"] = "0";
                                }
                            }
                        }

                        else if ((rowACount * 0.95 >= rowBCount & rowACount * 0.90 < rowBCount) 
                            & (rowBCount * 0.95 >= rowACount & rowBCount * 0.90 < rowACount))
                        {
                            if (Double.Parse(rowA["TIME"].ToString()) > Double.Parse(rowB["TIME"].ToString()))
                            {
                                rowA["ACTIVE"] = "0";
                                rowB["ACTIVE"] = "1";
                            }
                            else
                            {
                                rowA["ACTIVE"] = "1";
                                rowB["ACTIVE"] = "0";
                            }
                        }
                        else
                        {
                            if (rowACount > rowBCount)
                            {
                                rowA["ACTIVE"] = "1";
                                rowB["ACTIVE"] = "0";
                            }
                            else
                            {
                                rowA["ACTIVE"] = "0";
                                rowB["ACTIVE"] = "1";
                            }
                        }
/*
                        if (Int32.Parse(rowA["T_CNT"].ToString()) == Int32.Parse(rowB["T_CNT"].ToString()))
                        {
                            if (Int32.Parse(rowA["TIME"].ToString()) > Int32.Parse(rowB["TIME"].ToString()))
                            {
                                String A = rowA["TIME"].ToString();
                                String B = rowB["TIME"].ToString();

                                rowA["ACTIVE"] = "0";
                                rowB["ACTIVE"] = "1";
                            }
                            else
                            {
                                rowA["ACTIVE"] = "1";
                                rowB["ACTIVE"] = "0";
                            }

                        }
                        else if (Int32.Parse(rowA["T_CNT"].ToString()) < Int32.Parse(rowActive["T_CNT"].ToString()))
                        {
                            rowA["ACTIVE"] = "0";
                            rowB["ACTIVE"] = "1";
                        }
                        else
                        {
                            rowA["ACTIVE"] = "1";
                            rowB["ACTIVE"] = "0";
                        }
 */
                    }
                }
            }

            if (rowB != null)
            {
                if (rowA["ACTIVE"].Equals("1"))
                {
                    for (int i = 0; i < dgvData.Rows.Count; i++)
                    {
                        if (dgvData.Rows[i].Cells[2].Value.ToString().Equals(rowA["EPC"].ToString()))
                        {
                            dgvData.Rows[i].Cells[1].Value = rowA["HEAD"].ToString();
                            dgvData.Rows[i].Cells[3].Value = rowA["T_AVG"].ToString();
                            dgvData.Rows[i].Cells[4].Value = rowA["T_STDEV"].ToString();
                            dgvData.Rows[i].Cells[5].Value = rowA["T_THRESHOLD"].ToString();
                            dgvData.Rows[i].Cells[6].Value = rowA["T_CNT"].ToString();
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dgvData.Rows.Count; i++)
                    {
                        if (dgvData.Rows[i].Cells[2].Value.ToString().Equals(rowB["EPC"].ToString()))
                        {
                            dgvData.Rows[i].Cells[1].Value = rowB["HEAD"].ToString();
                            dgvData.Rows[i].Cells[3].Value = rowB["T_AVG"].ToString();
                            dgvData.Rows[i].Cells[4].Value = rowB["T_STDEV"].ToString();
                            dgvData.Rows[i].Cells[5].Value = rowB["T_THRESHOLD"].ToString();
                            dgvData.Rows[i].Cells[6].Value = rowB["T_CNT"].ToString();
                            break;
                        }
                    }
                }
            }
            else
            {
                rowA["ACTIVE"] = "1";

                for (int i = 0; i < dgvData.Rows.Count; i++)
                {
                    if (dgvData.Rows[i].Cells[2].Value.ToString().Equals(rowA["EPC"].ToString()))
                    {
                        dgvData.Rows[i].Cells[1].Value = rowA["HEAD"].ToString();
                        dgvData.Rows[i].Cells[3].Value = rowA["T_AVG"].ToString();
                        dgvData.Rows[i].Cells[4].Value = rowA["T_STDEV"].ToString();
                        dgvData.Rows[i].Cells[5].Value = rowA["T_THRESHOLD"].ToString();
                        dgvData.Rows[i].Cells[6].Value = rowA["T_CNT"].ToString();
                        break;
                    }
                }
            }
        }

        

        private void resetMissingCnt()
        {
            foreach (DataRow row in dtData.Rows)
            {
                row["M_CNT"] = "0";
            }
        }

        private bool bChangeChannel(string head, string epc)
        {
            bool bRet = false;

            foreach (DataRow row in dtData.Rows)
            {
                //다른 채널로 리딩 시도
                if (row["EPC"].ToString().Equals(epc) && row["HEAD"].ToString().Equals(head) == false)
                {
                    int channel = Convert.ToInt32(row["HEAD"].ToString());
                    byte[] epcdata = Util.StringToByteArray(htEPC[row["EPC"].ToString()].ToString());
                    string msg = "";
                    ushort errorCode = 0;
                    byte[] rdata = null;

                    bool flag = ui.readDataUHF(channel, (byte)Enum_UIF.UHF_MEMORY.KILL_PASSWORD, 22,
                                    2, ushort.Parse(numMissTimeout.Value.ToString()), 12, epcdata, true, out rdata, out errorCode, out msg);
                    if (flag)
                    {
                        bRet = true;
                        
                        //리딩 성공 시 티칭 채널 전환
                        for (int i = 0; i < dgvData.Rows.Count; i++)
                        {
                            if (dgvData.Rows[i].Cells[2].Value.ToString().Equals(row["EPC"].ToString()))
                            {
                                foreach(DataRow rowTrigger in dtData.Rows)
                                {
                                    if(rowTrigger["EPC"].ToString().Equals(row["EPC"].ToString()))
                                    {
                                        if(rowTrigger["HEAD"].ToString().Equals(row["HEAD"].ToString()))
                                        {
                                            rowTrigger["ACTIVE"] = "1";
                                            rowTrigger["M_CNT"] = "0";

                                            dgvData.Rows[i].Cells[1].Value = row["HEAD"].ToString();
                                            dgvData.Rows[i].Cells[3].Value = row["T_AVG"].ToString();
                                            dgvData.Rows[i].Cells[4].Value = row["T_STDEV"].ToString();
                                            dgvData.Rows[i].Cells[5].Value = row["T_THRESHOLD"].ToString();
                                            dgvData.Rows[i].Cells[6].Value = row["T_CNT"].ToString();

                                            dgvData.Rows[i].Cells[8].Value = rowTrigger["M_CNT"].ToString();
                                        }
                                        else
                                        {
                                            rowTrigger["ACTIVE"] = "0";
                                            rowTrigger["M_CNT"] = "0";
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        int iMonitoring = 0;

        private string MonitoringOn(BackgroundWorker bw, int sleepPeriod)
        {
            PerformanceCounter performanceCounter1 = new PerformanceCounter();

            iMonitoring = 0;

            resetMissingCnt();

            while (!bw.CancellationPending)
            {
                performanceCounter1.Start();

                iMonitoring++;

                if(iMonitoring > Int32.Parse(numResetCnt.Value.ToString()))
                {
                    iMonitoring = 1;

                    resetMissingCnt();
                }

                this.Invoke(new MethodInvoker(delegate ()
                {
                    label11.Text = "Leak Data (" + iMonitoring + ")";
                }));

                try
                {
                    if (ui != null && ui.isConnected)
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtData.Rows)
                            {
                                if (row["ACTIVE"].Equals("0")) continue;

                                int channel = Convert.ToInt32(row["HEAD"].ToString());
                                byte[] epcdata = Util.StringToByteArray(htEPC[row["EPC"].ToString()].ToString());
                                ushort errorCode = 0;
                                string msg = "";

                                byte[] rdata = null;
                                // Read Sensor Data
                                bool flag = ui.readDataUHF(channel, (byte)Enum_UIF.UHF_MEMORY.KILL_PASSWORD, 22,
                                    2, ushort.Parse(numMissTimeout.Value.ToString()), 12, epcdata, true, out rdata, out errorCode, out msg);
                                if (flag)
                                {

                                    //// byte oSensor = 0;
                                    ////if (row["SENSOR"].ToString().Length > 0)
                                    ////    oSensor = Convert.ToByte(row["SENSOR"].ToString());

                                    byte sensor = getSensorCode(rdata);
                                    ////int diff = sensor - oSensor;
                                    row["CURRENT_DATA"] = sensor.ToString();
                                    
                                    if (chkRead.Checked)
                                    {
                                        string writeStr = row["HEAD"].ToString() + "," + row["EPC"].ToString() + "," + row["CURRENT_DATA"].ToString() + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        writeWorkLog(writeStr);
                                    }

                                    double v1 = double.Parse(row["T_THRESHOLD"].ToString());
                                    double v2 = double.Parse(row["CURRENT_DATA"].ToString());

                                    if (v1 >= v2)
                                    {
                                        if (htErrEPC.ContainsKey(row["EPC"].ToString()))
                                        {
                                            int iErrCnt = Int32.Parse(htErrEPC[row["EPC"].ToString()].ToString());

                                            if (iErrCnt >= Int32.Parse(numAlarmCnt.Value.ToString()))
                                            {
                                                bool bExist = false;
                                                for (int i = 0; i < dgAlarms.Rows.Count; i++)
                                                {
                                                    if (dgAlarms.Rows[i].Cells[2].Value.ToString().Equals(row["EPC"].ToString())
                                                    && dgAlarms.Rows[i].Cells[5].Value.ToString().Length == 0)
                                                    {
                                                        bExist = true;
                                                    }
                                                }

                                                if (bExist == false)
                                                {
                                                    ushort addr = 0x0898;
                                                    ushort data = 48;
                                                    ui.writeSingleRegister(addr, data);

                                                    DataGridViewRow dgRow = new DataGridViewRow();
                                                    dgRow.CreateCells(dgAlarms);

                                                    dgRow.Cells[0].Value = (dgAlarms.Rows.Count + 1).ToString();
                                                    dgRow.Cells[1].Value = row["HEAD"].ToString();
                                                    dgRow.Cells[2].Value = row["EPC"].ToString();
                                                    dgRow.Cells[3].Value = row["CURRENT_DATA"].ToString();
                                                    dgRow.Cells[4].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    dgRow.Cells[5].Value = "";

                                                    dgRow.DefaultCellStyle.BackColor = Color.Red;

                                                    this.Invoke(new MethodInvoker(delegate ()
                                                    {
                                                        dgAlarms.Rows.Add(dgRow);
                                                        dgAlarms.CurrentCell = dgAlarms.Rows[dgAlarms.Rows.Count - 1].Cells[0];
                                                    }));
                                                    
                                                }
                                            }
                                            else
                                            {
                                                htErrEPC[row["EPC"].ToString()] = (iErrCnt + 1).ToString();
                                            }
                                        }
                                        else
                                        {
                                            htErrEPC.Add(row["EPC"].ToString(), "1");
                                        }
                                    }
                                    else
                                    {
                                        if (htErrEPC.ContainsKey(row["EPC"].ToString()))
                                        {
                                            htErrEPC.Remove(row["EPC"].ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    if (row["M_CNT"].ToString().Length == 0)
                                    {
                                        row["M_CNT"] = "1";
                                    }
                                    else
                                    {
                                        int iCnt = Int32.Parse(row["M_CNT"].ToString()) + 1;
                                        row["M_CNT"] = iCnt.ToString();

                                        if(iCnt == Int32.Parse(numMissAlarm.Value.ToString()))
                                        {
                                            if(bChangeChannel(row["HEAD"].ToString(), row["EPC"].ToString()))
                                            {
                                                continue;
                                            }

                                            ushort addr = 0x0898;
                                            ushort data = 48;
                                            ui.writeSingleRegister(addr, data);

                                            DataGridViewRow dgRow = new DataGridViewRow();
                                            dgRow.CreateCells(dgAlarms);

                                            dgRow.Cells[0].Value = (dgAlarms.Rows.Count + 1).ToString();
                                            dgRow.Cells[1].Value = row["HEAD"].ToString();
                                            dgRow.Cells[2].Value = row["EPC"].ToString();
                                            dgRow.Cells[3].Value = row["CURRENT_DATA"].ToString();
                                            dgRow.Cells[4].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            dgRow.Cells[5].Value = "";

                                            dgRow.DefaultCellStyle.BackColor = Color.Yellow;

                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                dgAlarms.Rows.Add(dgRow);
                                                dgAlarms.CurrentCell = dgAlarms.Rows[dgAlarms.Rows.Count - 1].Cells[0];
                                            }));

                                            
                                        }
                                    }

                                    changeIdleMode(Int32.Parse(row["HEAD"].ToString()));

                                    Thread.Sleep(50);
                                }

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    for (int i = 0; i < dgvData.Rows.Count; i++)
                                    {
                                        if (dgvData.Rows[i].Cells[2].Value.ToString().Equals(row["EPC"].ToString()))
                                        {
                                            dgvData.Rows[i].Cells[7].Value = row["CURRENT_DATA"].ToString();
                                            dgvData.Rows[i].Cells[8].Value = row["M_CNT"].ToString();

                                            if (flag)
                                            {
                                                dgvData.Rows[i].DefaultCellStyle.BackColor = Color.White;
                                            }
                                            else
                                            {
                                                dgvData.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                                            }
                                            break;
                                        }
                                    }
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                performanceCounter1.Stop();

                //int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000) - 500);
                int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000));
                if (interval < 0)
                    interval = 0;

                Thread.Sleep(interval);

            }

            return "Done";
        }

        


        private void backgroundWorkerbgTrigger_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                // The user canceled the operation.
                string msg = String.Format("Stop Read Parameter Service");
                Console.WriteLine(@"Making data RunWorkerCompleted:" + msg, "INFO");
                //Logging.LogInfo(msg);
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                Console.WriteLine(@"Making data RunWorkerCompleted:Error" + msg, "ERROR");
                //Logging.LogInfo(msg);
            }
            else
            {
                // 초기화 정상 완료.
                if (e.Result.ToString().IndexOf("ERROR :") == 0)
                {
                    string msg = String.Format("{0}", "Read Input Parameter " + e.Result);

                    Console.WriteLine(@"Making data " + msg, "ERROR");

                    //Logging.LogInfo(msg);
                }
                else
                {
                    string msg = String.Format("Result = {0}", e.Result);
                    Console.WriteLine(@"Making data  RunWorkerCompleted:" + msg, "INFO");
                    //Logging.LogInfo(msg);
                }

            }

            // Complete

            btnTrigger.FlatStyle = FlatStyle.Standard;
            btnTrigger.Text = "Searching On";
            
        }

        private void backgroundWorkerbgMonitoring_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                // The user canceled the operation.
                string msg = String.Format("Stop Read Parameter Service");
                Console.WriteLine(@"Making data RunWorkerCompleted:" + msg, "INFO");
                //Logging.LogInfo(msg);
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                Console.WriteLine(@"Making data RunWorkerCompleted:Error" + msg, "ERROR");
                //Logging.LogInfo(msg);
            }
            else
            {
                // 초기화 정상 완료.
                if (e.Result.ToString().IndexOf("ERROR :") == 0)
                {
                    string msg = String.Format("{0}", "Read Input Parameter " + e.Result);

                    Console.WriteLine(@"Making data " + msg, "ERROR");

                    //Logging.LogInfo(msg);
                }
                else
                {
                    string msg = String.Format("Result = {0}", e.Result);
                    Console.WriteLine(@"Making data  RunWorkerCompleted:" + msg, "INFO");
                    //Logging.LogInfo(msg);
                }

            }

            // Complete

            btnReadSensor.FlatStyle = FlatStyle.Standard;
            btnReadSensor.Text = "Monitoring On";

        }

        private void backgroundWorkerbgTeaching_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                // The user canceled the operation.
                string msg = String.Format("Stop Read Parameter Service");
                Console.WriteLine(@"Making data RunWorkerCompleted:" + msg, "INFO");
                //Logging.LogInfo(msg);
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                Console.WriteLine(@"Making data RunWorkerCompleted:Error" + msg, "ERROR");
                //Logging.LogInfo(msg);
            }
            else
            {
                // 초기화 정상 완료.
                if (e.Result.ToString().IndexOf("ERROR :") == 0)
                {
                    string msg = String.Format("{0}", "Read Input Parameter " + e.Result);

                    Console.WriteLine(@"Making data " + msg, "ERROR");

                    //Logging.LogInfo(msg);
                }
                else
                {
                    string msg = String.Format("Result = {0}", e.Result);
                    Console.WriteLine(@"Making data  RunWorkerCompleted:" + msg, "INFO");
                    //Logging.LogInfo(msg);
                }

            }

            // Complete

            btnTeaching.FlatStyle = FlatStyle.Standard;
            btnTeaching.Text = "Teaching On";

        }

        private string TriggerOn(BackgroundWorker bw, int sleepPeriod)
        {
            PerformanceCounter performanceCounter1 = new PerformanceCounter();
            int idx = 0;
            while (!bw.CancellationPending)
            {
                performanceCounter1.Start();

                idx++;
                if (idx > 2)
                    idx = 1;

                if (ui != null && ui.isConnected)
                {
                    List<inventoryResult> data = null;

                    ushort errorCode = 0;
                    string msg = "";
                    bool flag = ui.inventoryUHF(idx, Convert.ToUInt16(numInterval.Value), true, true, out data,
                        out errorCode, out msg);

                    if (flag)
                    {
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            if (data.Count > 0)
                            {
                                foreach (inventoryResult ir in data)
                                {
                                    string epcstr = Util.ByteArrayToString(ir.epcData).ToUpper();
                                    if (chkFilter.Checked)
                                    {
                                        if (epcstr.IndexOf(txtFilter.Text.ToUpper().PadLeft(2,'0')) != 0)
                                        {
                                            continue;
                                        }
                                    }

                                    //if (htEPC.ContainsValue(epcstr))
                                    if(htEPC.ContainsKey(ParseHex(epcstr)))
                                    {
                                        bool bExist = false;
                                        string no = "";

                                        foreach(DataRow rowRssi in dtData.Rows)
                                        {
                                            if (rowRssi["EPC"].ToString().Equals(ParseHex(epcstr)))
                                            {
                                                if(rowRssi["HEAD"].ToString().Equals(idx.ToString()) == false)
                                                {
                                                    no = rowRssi["NO"].ToString();
                                                }
                                                else
                                                {
                                                    bExist = true;
                                                }
                                            }
                                        }

                                        if(bExist == false)
                                        {
                                            DataRow row = dtData.NewRow();

                                            row["NO"] = no;
                                            row["HEAD"] = idx.ToString();
                                            row["EPC"] = ParseHex(epcstr);
                                            row["RSSI"] = ir.rssi.ToString();
                                            row["ACTIVE"] = "0";
                                            row["T_CNT"] = "0";

                                            dtData.Rows.Add(row);
                                        }

                                        continue;
                                    }
                                    else
                                    {
                                        DataRow row = dtData.NewRow();

                                        row["NO"] = (dgvData.RowCount + 1).ToString();
                                        row["HEAD"] = idx.ToString();
                                        row["EPC"] = ParseHex(epcstr);
                                        row["RSSI"] = ir.rssi.ToString();
                                        row["ACTIVE"] = "1";
                                        row["T_CNT"] = "0";

                                        dtData.Rows.Add(row);

                                        htEPC.Add(row["EPC"], epcstr);

                                        DataGridViewRow dgRow = new DataGridViewRow();
                                        dgRow.CreateCells(dgvData);

                                        dgRow.Cells[0].Value = row["NO"].ToString();
                                       // dgRow.Cells[1].Value = row["HEAD"].ToString() + " (" + ir.rssi.ToString() + ")";
                                        dgRow.Cells[2].Value = row["EPC"].ToString();
                                        dgRow.Cells[3].Value = "";
                                        dgRow.Cells[4].Value = "";
                                        dgRow.Cells[5].Value = "";
                                        dgRow.Cells[6].Value = "";
                                        dgRow.Cells[7].Value = "";
                                        dgRow.Cells[8].Value = "";

                                        dgvData.Rows.Add(dgRow);
                                    }
                                }
                            }
                        }));
                    }
                }
                else
                {
                    break;
                }


                performanceCounter1.Stop();

                //int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000) - 500);
                int interval = (int)(sleepPeriod - (performanceCounter1.Duration * 1000));
                if (interval < 0)
                    interval = 0;

                Thread.Sleep(Int32.Parse(numChnInterval.Value.ToString()));

            }

            return "Done";
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (btnTrigger.FlatStyle != FlatStyle.Standard)
                {
                    //MessageBox.Show("Trigger가 동작 중입니다. 정지 후 종료 하시기 바랍니다.");
                    MessageBox.Show("After stopping searching."); 
                    e.Cancel = true;
                    //this.OnFormClosing(e);
                    return;
                }

                if (btnTeaching.FlatStyle != FlatStyle.Standard)
                {
                   // MessageBox.Show("Teaching 중입니다. 정지 후 종료 하시기 바랍니다.");
                    MessageBox.Show("After stopping teaching.");
                    e.Cancel = true;
                    return;
                }

                if (btnReadSensor.FlatStyle != FlatStyle.Standard)
                {
                    MessageBox.Show("After stopping monitoring.");
                    e.Cancel = true;
                    return;
                }

                if(MessageBox.Show("Do you want to quit?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                if (ui.isConnected)
                {
                    ui.stopReadInputParameter();
                    Thread.Sleep(500);
                    ui.disconnectDevice();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
             
            }
        }

        private void btnReadSensor_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnReadSensor.FlatStyle == FlatStyle.Standard)
                {
                    if (Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), 0x0000) != tssStatus1.Text
                        || Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), 0x0000) != tssStatus2.Text)
                    {
                        return;
                    }

                    btnReadSensor.FlatStyle = FlatStyle.Flat;
                    btnReadSensor.Text = "Monitoring Off";
                    startMonitoring();
                    currentStatusLabel.Text = "Monitoring Process";
                    userInputValueEnable(false);          
                    btnTrigger.Enabled = false;
                    btnTeaching.Enabled = false;

                }
                else
                {
                    btnReadSensor.FlatStyle = FlatStyle.Standard;
                    btnReadSensor.Text = "Monitoring On";
                    stopMonitoring();
                    currentStatusLabel.Text = "Please Wait Remained Process";
                    userInputValueEnable(false);  
                    btnReadSensor.Enabled = false;
                    btnTrigger.Enabled = false;
                    btnTeaching.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void btnTeaching_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnTeaching.FlatStyle == FlatStyle.Standard)
                {
                    if (Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), 0x0000) != tssStatus1.Text
                        || Enum.GetName(typeof(Enum_UIF.COMMAND_CODE), 0x0000) != tssStatus2.Text)
                    {
                        return;
                    }

                    dtTeaching.Rows.Clear();

                    btnTeaching.FlatStyle = FlatStyle.Flat;
                    btnTeaching.Text = "Teaching Off";
                    currentStatusLabel.Text = "Start Teaching";
                    startTeaching();
                    userInputValueEnable(false);          
                    btnTrigger.Enabled = false;
                    btnReadSensor.Enabled = false;
                }
                else
                {
                    btnTeaching.FlatStyle = FlatStyle.Standard;
                    btnTeaching.Text = "Teaching On";
                    stopTeaching();
                    currentStatusLabel.Text = "Please Wait Remained Process";
                    userInputValueEnable(false);          
                    btnTeaching.Enabled = false;

                    foreach(DataRow rowEnd in dtData.Rows)
                    {
                        activeTeaching(rowEnd["EPC"].ToString());
                    }


//                    btnTrigger.Enabled = true;
//                    btnReadSensor.Enabled = true;

                    TeachingSave();


                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Trigger Error :" + ex.Message);
            }
        }

        private void TeachingSave()
        {
            if (chkSave.Checked)
            {
                string saveFileName = "Teaching_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                StreamWriter sw = new StreamWriter(saveFileName);

                try
                {
                    foreach (DataRow row in dtTeaching.Rows)
                    {
                        string writeStr = row["NO"].ToString() + "," + row["HEAD"].ToString() + "," + row["EPC"].ToString() + "," + row["LEAK_DATA"].ToString();
                        sw.WriteLine(writeStr);
                    }
                }
                catch(Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                finally
                {
                    sw.Close();
                }
            }
        }

        public void writeWorkLog(String text1)
        {
            try
            {
                String logFileName = String.Empty;

                logFileName = "ReadData_" + DateTime.Now.ToString("yyyyMMddHH") + ".csv";

                if (File.Exists(logFileName) == false)
                {
                    FileStream fs = new FileStream(logFileName, FileMode.Create);
                    fs.Close();
                }

                StreamWriter objStreamWriter = new StreamWriter(logFileName, true);

                objStreamWriter.WriteLine(text1);

                objStreamWriter.Close();
            }
            catch (Exception)
            {
            }
        }


        private void btnAlarmClear_Click(object sender, EventArgs e)
        {
            ushort addr = 0x0898;
            ushort data = 0;
            ui.writeSingleRegister(addr, data);

            DateTime now = DateTime.Now;

            for(int i = 0; i < dgAlarms.Rows.Count; i++)
            {
                if (dgAlarms.Rows[i].DefaultCellStyle.BackColor == Color.Red || dgAlarms.Rows[i].DefaultCellStyle.BackColor == Color.Yellow)
                {
                    dgAlarms.Rows[i].Cells[5].Value = now.ToString("yyyy-MM-dd HH:mm:ss");
                    dgAlarms.Rows[i].DefaultCellStyle.BackColor = Color.White;

                    if(htErrEPC.ContainsKey(dgAlarms.Rows[i].Cells[2].Value.ToString()))
                    {
                        htErrEPC.Remove(dgAlarms.Rows[i].Cells[2].Value.ToString());
                    }
                }
            }

            foreach(DataRow row in dtData.Rows)
            {
                row["M_CNT"] = 0;
            }
        }

        private void btnListClear_Click(object sender, EventArgs e)
        {
            for(int i = dgAlarms.Rows.Count-1; i >= 0; i--)
            {
                if (dgAlarms.Rows[i].DefaultCellStyle.BackColor == Color.White)
                {
                    dgAlarms.Rows.RemoveAt(i);
                }
            }

            for (int i = 0; i < dgAlarms.Rows.Count - 1; i++)
            {
                dgAlarms.Rows[i].Cells[0].Value = (i + 1).ToString();
            }
        }

        private void dgvData_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            string s1 = "0";
            string s2 = "0";

            try
            {
                if (e.CellValue1.ToString().Length > 0) s1 = e.CellValue1.ToString();
                if (e.CellValue2.ToString().Length > 0) s2 = e.CellValue2.ToString();
            }
            catch(Exception)
            {

            }

            double a = double.Parse(s1), b = double.Parse(s2);
            e.SortResult = a.CompareTo(b);
            e.Handled = true;

            for(int i = 0; i < dgvData.Rows.Count; i++)
            {
                dgvData.Rows[i].Cells[0].Value = (i + 1).ToString();
            }
            //checkThresold();
        }

        private void dgAlarms_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            try
            {
                double a = double.Parse(e.CellValue1.ToString()), b = double.Parse(e.CellValue2.ToString());
                e.SortResult = a.CompareTo(b);
                
            }
            catch(Exception)
            {

            }

            e.Handled = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (btnTeaching.FlatStyle == FlatStyle.Standard && e.ColumnIndex == 5)
                {
                    frmNewThreshold frm = new frmNewThreshold(dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());

                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = frm.THRESHOLD;

                        foreach (DataRow row in dtData.Rows)
                        {
                            if (row["EPC"].ToString().Equals(dgvData.Rows[e.RowIndex].Cells[2].Value.ToString()))
                            {
                                row["T_THRESHOLD"] = frm.THRESHOLD;
                                break;
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {

            }
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                e.Cancel = true;
                return;
            }
            catch(Exception)
            {

            }
        }

        private void changeIdleMode(int channel)
        {
            try
            {
                if (ui != null && ui.isConnected)
                {
                    ushort errorCode = 0;
                    string msg = "";
                    bool flag = ui.changeIdleMode(channel, out errorCode, out msg);

                    if (!flag)
                    {
                        //MessageBox.Show(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }

        private void btnChangeIdle_Click(object sender, EventArgs e)
        {
            try
            {

                if (ui != null && ui.isConnected)
                {
//                    byte[] data = null;
//                    float rssi = 0;
                    ushort errorCode = 0;
                    string msg = "";
                    bool flag = ui.changeIdleMode(Convert.ToInt32(0),
                        out errorCode, out msg);

                    if (!flag)
                    {
                        MessageBox.Show(msg);
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                if (ui != null && ui.isConnected)
                {
//                    byte[] data = null;
//                    float rssi = 0;
                    ushort errorCode = 0;
                    string msg = "";
                    bool flag = ui.changeReadMode(Convert.ToInt32(0),
                        out errorCode, out msg);

                    if (!flag)
                    {
                        MessageBox.Show(msg);
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(dataGridView1.Visible == true)
            {
                dataGridView1.Visible = false;
            }
            else
            {
                dataGridView1.Visible = true;
            }
        }

        private void numChnInterval_ValueChanged(object sender, EventArgs e)
        {

        }

        private void panTop_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void numAlarmCnt_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void chkFilter_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label2_Click_2(object sender, EventArgs e)
        {

        }

        private void numInterval_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtIpaddr_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void dgAlarms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void numMissAlarm_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {

        }

        private void chkSave_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkRead_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

    public static class MyListExtensions
    {
        public static double Mean(this List<double> values)
        {
            return values.Count == 0 ? 0 : values.Mean(0, values.Count);
        }

        public static double Mean(this List<double> values, int start, int end)
        {
            double s = 0;

            for (int i = start; i < end; i++)
            {
                s += values[i];
            }

            return s / (end - start);
        }

        public static double Variance(this List<double> values)
        {
            return values.Variance(values.Mean(), 0, values.Count);
        }

        public static double Variance(this List<double> values, double mean)
        {
            return values.Variance(mean, 0, values.Count);
        }

        public static double Variance(this List<double> values, double mean, int start, int end)
        {
            double variance = 0;

            for (int i = start; i < end; i++)
            {
                variance += Math.Pow((values[i] - mean), 2);
            }

            int n = end - start;
            if (start > 0) n -= 1;

            return variance / (n);
        }

        public static double StandardDeviation(this List<double> values)
        {
            return values.Count == 0 ? 0 : values.StandardDeviation(0, values.Count);
        }

        public static double StandardDeviation(this List<double> values, int start, int end)
        {
            double mean = values.Mean(start, end);
            double variance = values.Variance(mean, start, end);

            return Math.Sqrt(variance);
        }
    }
}
