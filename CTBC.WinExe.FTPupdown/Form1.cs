using CTBC.FrameWork.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTBC.WinExe.FTPupdown
{
    public partial class Form1 : Form
    {
        //收文FTP
        private static string ftpserver;
        private static string port;
        private static string username;
        private static string password;
        private static string ftpReceiveDir;
        private static string loaclReceiveFilePath;

        //發文FTP
        private static string ftpserver2;
        private static string port2;
        private static string username2;
        private static string password2;
        private static string ftpSendDir;
        private static string loaclSendFilePath;

        private static string AttachFile;
        private static FtpClient ftpClient;
        private static FtpClient ftpClient2;
        public Form1()
        {
            //20150716收文和發文的FTP改為2組不同的FTP--Simon
            InitializeComponent();

            //收文的FTP參數
            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            port = ConfigurationManager.AppSettings["port"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];

            ftpReceiveDir = ConfigurationManager.AppSettings["ftpReceiveDir"];
            loaclReceiveFilePath = ConfigurationManager.AppSettings["loaclReceiveFilePath"];

            //發文的FTP參數
            ftpserver2 = ConfigurationManager.AppSettings["ftpserver2"];
            port2 = ConfigurationManager.AppSettings["port2"];
            username2 = ConfigurationManager.AppSettings["username2"];
            password2 = ConfigurationManager.AppSettings["password2"];

            ftpSendDir = ConfigurationManager.AppSettings["ftpSendDir"];
            loaclSendFilePath = ConfigurationManager.AppSettings["loaclSendFilePath"];

            AttachFile = ConfigurationManager.AppSettings["AttachFile"]; //added by Simon

             ftpClient = new FtpClient(ftpserver, username, password, port);
            ftpClient2 = new FtpClient(ftpserver2, username2, password2, port2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //獲取本地指定目錄下的文件清單
                string[] fileList = Directory.GetFiles(loaclReceiveFilePath);
                if (fileList.Length > 0)
                {
                    //將本地的指定文件上傳到FTP
                    ftpClient.SendFiles(ftpReceiveDir, loaclReceiveFilePath);
                    //刪除本地文件
                    foreach (var file in fileList)
                    {
                        File.Delete(file);
                    }
                    MessageBox.Show("已完成上傳" + fileList.Length + "個檔案");
                }
                else
                {
                    MessageBox.Show("沒有檔案可上傳");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("上傳失敗：" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //獲取FTP上指定目錄的文件清單
                ArrayList fileList = ftpClient2.GetFileList(ftpSendDir);
                if (fileList.Count > 0)
                {
                    //下載FTP指定目錄下的所有文件
                    int mCount = 0;
                    foreach (var file in fileList)
                    {
                        string remoteFile = ftpClient2.SetRemotePath(ftpSendDir) + "//" + file;
                        string localFile = loaclSendFilePath.TrimEnd('\\') + "\\" + file;
                        ftpClient2.GetFiles(remoteFile, localFile);
                        //讀取本機端獲得的檔案
                        if (File.Exists(localFile))
                        {
                            mCount += 1;
                            ftpClient2.DeleteFile(remoteFile);
                        }
                    }
                    ////刪除FTP指定目錄下的文件
                    //ftpClient2.DeleteFiles(ftpSendDir);
                    MessageBox.Show("已完成下載" + mCount + "個檔案");
                }
                else
                {
                    MessageBox.Show("沒有檔案可下載");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("下載失敗：" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(AttachFile))
            {
                MessageBox.Show("請先設定Config中正確的附件目錄!");
                return;
            }

            //以di檔名為主,變更pdf和txt的檔名,使3個檔名都相同
            try
            {
                int i_pdf = 0, i_txt = 0;
                //獲取本地指定目錄下的文件清單
                string[] fileList = Directory.GetFiles(loaclReceiveFilePath, "*.di");
                if (fileList.Length > 0)
                {
                    //搜尋文件
                    foreach (var file in fileList)
                    {
                        //變更PDF檔案名稱和di名稱相同
                        FileInfo mInfo = new FileInfo(file);
                        string mFileName = mInfo.Name.Replace(".di","");
                        string[] pdf = Directory.GetFiles(loaclReceiveFilePath, mFileName + "*.pdf");
                        foreach (var PDFfile in pdf)
                        {
                            //PDF檔更名
                            File.Move(PDFfile, loaclReceiveFilePath + "\\" + mFileName + ".pdf");

                            i_pdf += 1;
                        }


                        //在AttachFile尋找txt附件
                        string[] txt = Directory.GetFiles(AttachFile, mFileName + "*.txt");
                        foreach (var TXTfile in txt)
                        {
                            //將附件複製到loaclReceiveFilePath
                            File.Copy(TXTfile, loaclReceiveFilePath + "\\" + mFileName + ".txt");

                            i_txt += 1;
                        }

                       

                    }

                    string ms = "";
                    ms += "已完成處理" + fileList.Length + "個di檔案";
                    ms += "\n\r" + "已完成處理" + i_pdf + "個pdf檔案";
                    ms += "\n\r" + "已完成處理" + i_txt + "個txt檔案";
                    MessageBox.Show(ms);
                }
                else
                {
                    MessageBox.Show("沒有檔案需處理");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生異常：" + ex.Message);
            }
        }
    }
}
