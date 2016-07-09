using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            runSP("NAccountCentura");
            runSP("NAccountCHS");
            runSP("NAccountSCLEpic"); 
            runSP("NAccountWellstar"); 
            runSP("NAccountSCL"); 
            runSP("NAccountCapeCodHC"); 
            runSP("NAccountBJC");
            runSP("NAccountSteward");
            runSP("NAccountGwinnett");
            //runSP("NAccountGeisinger"); 
            callSQL("C:\\Users\\axiong\\OneDrive for Business\\3.0 Payment Plan option\\SSIS package\\02 totalSourceDataPrep.sql");
            callSQL("C:\\Users\\axiong\\OneDrive for Business\\3.0 Payment Plan option\\SSIS package\\03 PCAscore.sql");
            callSQL("C:\\Users\\axiong\\OneDrive for Business\\3.0 Payment Plan option\\SSIS package\\01 trainDataPrep.sql");

            CreateInputFileData("PaymentPlanSandbox", "PaymentPlanTrain", "C:\\Users\\axiong\\Desktop\\Release_PPEventModelpp_1_Std_3_1_2016\\InputFiles\\mockupInputData2.csv");
            Directory.SetCurrentDirectory("C:\\Users\\axiong\\Desktop\\Release_PPEventModelpp_1_Std_3_1_2016");
            runCommand("C:\\Users\\axiong\\Desktop\\Release_PPEventModelpp_1_Std_3_1_2016\\quote_app.bat", "");
            runCommand("C:\\Users\\axiong\\Desktop\\Release_PPEventModelpp_1_Std_3_1_2016\\prod.bat", "");
            Console.ReadLine();
            }

        //Run SP available in the database
        public static void runSP(string clientName)
        {
            string sqlConnectionString = "server=10.0.0.27;Integrated Security=SSPI;app=Microsoft® Visual Studio® 2010;MultipleActiveResultSets=True;Asynchronous Processing=true";
          
            using (var conn = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand("paymentPlanSandbox..uspPaymentPlanData", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                //command.Parameters.Add("@client", SqlDbType.VarChar).Value = "NAccountCentura";
                command.Parameters.Add("@client", SqlDbType.VarChar).Value = clientName;
                conn.Open();
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        //Call SQL command to run
        public static void callSQL(string filePath)
        {
            string sqlConnectionString = "server=10.0.0.27;Integrated Security=SSPI;app=Microsoft® Visual Studio® 2010;MultipleActiveResultSets=True;Asynchronous Processing=true";
            FileInfo file = new FileInfo(filePath);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open(); 
            Server server = new Server(new ServerConnection(conn));
            server.ConnectionContext.ExecuteNonQuery(script);
        }


        public static void CreateInputFileData(string DatabaseName, string TableName, string InputFileLocation)
        {
            string connectionString = "server=10.0.0.27;Integrated Security=SSPI;app=Microsoft® Visual Studio® 2010;wsid=VM8SJUMP31;MultipleActiveResultSets=True;Asynchronous Processing=true";
            //string connectionString = "server=10.0.0.27;uid=jknoll;Database=" + DatabaseName + ";Integrated Security=SSPI";
            // In a using statement, acquire the SqlConnection as a resource.
            System.Console.WriteLine(connectionString);
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                //
                // Open the SqlConnection.
                //

                con.Open();
                //
                // The following code uses an SqlCommand based on the SqlConnection.
                //
                SqlCommand use = new SqlCommand("use " + DatabaseName, con);
                use.BeginExecuteNonQuery();

                using (SqlCommand command = new SqlCommand("select * from " + TableName, con))
                using (SqlCommand command2 = new SqlCommand("select col.name from sys.columns col join sys.tables tab on col.object_id=tab.object_id where tab.name = '" + TableName + "' order by col.column_id", con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    using (System.IO.StreamWriter DTfile = new System.IO.StreamWriter(InputFileLocation))
                    {
                        using (SqlDataReader reader2 = command2.ExecuteReader())
                        {
                            reader2.Read();
                            DTfile.Write(reader2.GetValue(0).ToString());



                            while (reader2.Read())
                            {
                                DTfile.Write(",");
                                DTfile.Write(reader2.GetValue(0).ToString());



                            }
                        }
                        DTfile.WriteLine();


                        while (reader.Read())
                        {

                            DTfile.Write(reader.GetValue(0).ToString());
                            DTfile.Write("," + reader.GetValue(1).ToString());
                            DTfile.Write("," + reader.GetValue(2).ToString());
                            DTfile.Write("," + reader.GetDateTime(3).ToString().Replace(" 12:00:00 AM", "").Replace("/", "-"));
                            DTfile.Write("," + reader.GetValue(4).ToString());
                            DTfile.Write("," + reader.GetValue(5).ToString());
                            for (int x = 6; x < reader.FieldCount; x++)
                                DTfile.Write("," + reader.GetValue(x).ToString());

                            DTfile.WriteLine();
                        }
                    }
                }
            }
        }


        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
        }

        static void runCommand(string ExeLocation, string argument)
        {
            //* Create your Process
            Process process = new Process();
            process.StartInfo.FileName = ExeLocation;
            process.StartInfo.Arguments = argument;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            //* Set your output and error (asynchronous) handlers
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            //* Start process and handlers
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }

    }
}