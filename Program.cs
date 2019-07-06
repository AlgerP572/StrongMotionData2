using System;
using System.IO;
using System.Text.RegularExpressions;

namespace StrongMotionDataV2
{
    class Program
    {
        private static string _fileName;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Extract the data files from the zip file and provide the V2 file as innput");
                Console.WriteLine("Data file can be obtained from https://strongmotioncenter.org/");
                return;
            }

            // Check the first argument (fileName_
            _fileName = args[0];
            FileInfo fileInfo = new FileInfo(_fileName);
            if(fileInfo.Extension != ".V2")
            {
                Console.WriteLine("This program can only parse V2 string motion data.");
                return;
            }

            var strongMotionData = StrongMotionDataV2.Parse(fileInfo);

            StrongMotionDataV2ToCsvFile.WriteToCsvFile(strongMotionData);
        }
    }

    public class StrongMotionDataV2Header
    {
        public string Line1V2TitleLine;
        public string Line2V1TitleLine;
        public string Line3EarthquakeName;
        public string Line4EarthquakeDateTimeLocal;
        public string Line5AccelerogramNumber;
        public string Line6StationNumber;
        public string Line7StationName;
        public string Line8AccelerogramChannel;
        public string Line9EarthquakeTitle;
        public string Line10HypoCenter;
        public string Line11TrnsducerNaturalPeriod;
        public string Line12LengthInSeconds;
        public string Line13PeakAcceleration;
        public string Line14RMSV1Acceleration;
        public string Line15FrquncyLimits;
        public string Line16NumberV2DataPoints;
        public string Line17TimeStepV2Data;
        public string Line18PeakAccelerationValeAndTime;
        public string Line19PeakVelocityValeAndTime;
        public string Line20PeakDisplacmentValeAndTime;
        public string Line21InitialVelocityAndDisplacment;
        public string Line22EarthquakeNameV2;
        public string Line23Blank;
        public string Line24PlotTitle;
        public string Line25AdditionalPlotTitle;
        public string IntegerHeader26;
        public string IntegerHeader27;
        public string IntegerHeader28;
        public string IntegerHeader29;
        public string IntegerHeader30;
        public string IntegerHeader31;
        public string IntegerHeader32;
        public string FloatHeader33;
        public string FloatHeader34;
        public string FloatHeader35;
        public string FloatHeader36;
        public string FloatHeader37;
        public string FloatHeader38;
        public string FloatHeader39;
        public string FloatHeader40;
        public string FloatHeader41;
        public string FloatHeader42;
        public string FloatHeader43;
        public string FloatHeader44;
        public string FloatHeader45;
        public string NumberChannelPointsTypeTimeSpacing;
    }

    public class StrongMotionDataV2
    {
        public StrongMotionDataV2Header Header = new StrongMotionDataV2Header();

        public DateTime StartTime;

        public int NumberOfAccelPoints;
        public float TimeBetweenAccelPoints;
        public float[] AccelChannelData;

        public int NumberOfVelocPoints;
        public float TimeBetweenVelocPoints;
        public float[] VelocChannelData;

        public int NumberOfDisplPoints;
        public float TimeBetweenDisplPoints;
        public float[] DisplChannelData;

        public static StrongMotionDataV2 Parse(FileInfo file)
        {
            var result = new StrongMotionDataV2();

            FileStream fs = new FileStream(file.FullName,
                FileMode.Open,
                FileAccess.Read);
            TextReader reader = new StreamReader(fs);

            // Header is the first 25 lines of the file.
            result.Header.Line1V2TitleLine = reader.ReadLine();
            result.Header.Line2V1TitleLine = reader.ReadLine();
            result.Header.Line3EarthquakeName = reader.ReadLine();
            result.Header.Line4EarthquakeDateTimeLocal = reader.ReadLine();
            result.Header.Line5AccelerogramNumber = reader.ReadLine();
            result.Header.Line6StationNumber = reader.ReadLine();
            result.Header.Line7StationName = reader.ReadLine();
            result.Header.Line8AccelerogramChannel = reader.ReadLine();
            result.Header.Line9EarthquakeTitle = reader.ReadLine();
            result.Header.Line10HypoCenter = reader.ReadLine();
            result.Header.Line11TrnsducerNaturalPeriod = reader.ReadLine();
            result.Header.Line12LengthInSeconds = reader.ReadLine();
            result.Header.Line13PeakAcceleration = reader.ReadLine();
            result.Header.Line14RMSV1Acceleration = reader.ReadLine();
            result.Header.Line15FrquncyLimits = reader.ReadLine();
            result.Header.Line16NumberV2DataPoints = reader.ReadLine();
            result.Header.Line17TimeStepV2Data = reader.ReadLine();
            result.Header.Line18PeakAccelerationValeAndTime = reader.ReadLine();
            result.Header.Line19PeakVelocityValeAndTime = reader.ReadLine();
            result.Header.Line20PeakDisplacmentValeAndTime = reader.ReadLine();
            result.Header.Line21InitialVelocityAndDisplacment = reader.ReadLine();
            result.Header.Line22EarthquakeNameV2 = reader.ReadLine();
            result.Header.Line23Blank = reader.ReadLine();
            result.Header.Line24PlotTitle = reader.ReadLine();
            result.Header.Line25AdditionalPlotTitle = reader.ReadLine();

            // integerHeader
            result.Header.IntegerHeader26 = reader.ReadLine();
            result.Header.IntegerHeader27 = reader.ReadLine();
            result.Header.IntegerHeader28 = reader.ReadLine();
            result.Header.IntegerHeader29 = reader.ReadLine();
            result.Header.IntegerHeader30 = reader.ReadLine();
            result.Header.IntegerHeader31 = reader.ReadLine();
            result.Header.IntegerHeader32 = reader.ReadLine();

            // Float Header
            result.Header.FloatHeader33 = reader.ReadLine();
            result.Header.FloatHeader34 = reader.ReadLine();
            result.Header.FloatHeader35 = reader.ReadLine();
            result.Header.FloatHeader36 = reader.ReadLine();
            result.Header.FloatHeader37 = reader.ReadLine();
            result.Header.FloatHeader38 = reader.ReadLine();
            result.Header.FloatHeader39 = reader.ReadLine();
            result.Header.FloatHeader40 = reader.ReadLine();
            result.Header.FloatHeader41 = reader.ReadLine();
            result.Header.FloatHeader41 = reader.ReadLine();
            result.Header.FloatHeader43 = reader.ReadLine();
            result.Header.FloatHeader44 = reader.ReadLine();
            result.Header.FloatHeader45 = reader.ReadLine();

            ParseStartTime(result.Header.Line9EarthquakeTitle, ref result);
            ParseAccelerationData(ref reader, ref result);
            ParseVelocityData(ref reader, ref result);
            ParseDisplData(ref reader, ref result);

            // For now not supporting variable channel lengths...
            if(result.NumberOfAccelPoints != result.NumberOfVelocPoints ||
                result.NumberOfAccelPoints != result.NumberOfDisplPoints)
            {
                throw new NotSupportedException("Variable channel length currently not supported.");
            }


            return result;
        }

        private static void ParseStartTime(string headerLineWithTime, ref StrongMotionDataV2 result)
        {
            string pattern = @"Rcrd of (?<Day>[A-Za-z]{3}) (?<Month>[A-Za-z]{3})\s+(?<Date>[0-9]{1,2}),\s+(?<Year>[0-9]{4})" +
                @"\s+(?<Hour>[0-9]{1,2}):(?<Minute>[0-9]{2}):(?<Second>[0-9]{2})";

            Match m = Regex.Match(headerLineWithTime, pattern);

            string day = m.Groups["Day"].Value;
            string month = m.Groups["Month"].Value;
            int date = int.Parse(m.Groups["Date"].Value);
            int year = int.Parse(m.Groups["Year"].Value);
            int hour = int.Parse(m.Groups["Hour"].Value);
            int minute = int.Parse(m.Groups["Minute"].Value);
            int second = int.Parse(m.Groups["Second"].Value);
            
            int monthNumber = Convert.ToDateTime("01-" + month + "-2011").Month;
            result.StartTime = new DateTime(year, monthNumber, date, hour, minute, second);            
        }

        private static void ParseAccelerationData(ref TextReader reader, ref StrongMotionDataV2 result)
        {
            string accelDataInfo = reader.ReadLine();

            string pattern = @"\s*(?<points>[0-9]+) points of accel data equally spaced at\s*(?<time>[.0-9]+)";

            Match m = Regex.Match(accelDataInfo, pattern);

            result.NumberOfAccelPoints = int.Parse(m.Groups[1].Value);
            result.TimeBetweenAccelPoints = float.Parse(m.Groups[2].Value);

            result.AccelChannelData = new float[result.NumberOfAccelPoints];

            int parsedPoints = 0;
            while (parsedPoints < result.NumberOfAccelPoints)
            {
                var dataLine = reader.ReadLine();

                string dataPattern = @"[-]?[0-9]*[.]{1}[0-9]+";
                m = Regex.Match(dataLine, dataPattern);

                while (m.Success)
                {
                    result.AccelChannelData[parsedPoints] = float.Parse(m.Groups[0].Value);
                    parsedPoints++;
                    m = m.NextMatch();
                }
            }
        }

        private static void ParseVelocityData(ref TextReader reader, ref StrongMotionDataV2 result)
        {
            string accelDataInfo = reader.ReadLine();

            string pattern = @"\s*(?<points>[0-9]+) points of veloc data equally spaced at\s*(?<time>[.0-9]+)";

            Match m = Regex.Match(accelDataInfo, pattern);

            result.NumberOfVelocPoints = int.Parse(m.Groups[1].Value);
            result.TimeBetweenVelocPoints = float.Parse(m.Groups[2].Value);

            result.VelocChannelData = new float[result.NumberOfVelocPoints];

            int parsedPoints = 0;
            while (parsedPoints < result.NumberOfVelocPoints)
            {
                var dataLine = reader.ReadLine();

                string dataPattern = @"[-]?[0-9]*[.]{1}[0-9]+";
                m = Regex.Match(dataLine, dataPattern);

                while (m.Success)
                {
                    result.VelocChannelData[parsedPoints] = float.Parse(m.Groups[0].Value);
                    parsedPoints++;
                    m = m.NextMatch();
                }
            }
        }

        private static void ParseDisplData(ref TextReader reader, ref StrongMotionDataV2 result)
        {
            string accelDataInfo = reader.ReadLine();

            string pattern = @"\s*(?<points>[0-9]+) points of displ data equally spaced at\s*(?<time>[.0-9]+)";

            Match m = Regex.Match(accelDataInfo, pattern);

            result.NumberOfDisplPoints = int.Parse(m.Groups[1].Value);
            result.TimeBetweenDisplPoints = float.Parse(m.Groups[2].Value);

            result.DisplChannelData = new float[result.NumberOfDisplPoints];

            int parsedPoints = 0;
            while (parsedPoints < result.NumberOfDisplPoints)
            {
                var dataLine = reader.ReadLine();

                string dataPattern = @"[-]?[0-9]*[.]{1}[0-9]+";
                m = Regex.Match(dataLine, dataPattern);

                while (m.Success)
                {
                    result.DisplChannelData[parsedPoints] = float.Parse(m.Groups[0].Value);
                    parsedPoints++;
                    m = m.NextMatch();
                }
            }
        }
    }

    public static class StrongMotionDataV2ToCsvFile
    {
        public static void WriteToCsvFile(StrongMotionDataV2 strongMotionData)
        {
            string fileName = @"C:\EarthquakeData\EarthquakeSummary.csv";

            FileStream fs = new FileStream(fileName,
                FileMode.OpenOrCreate,
                FileAccess.Write);
            TextWriter writer = new StreamWriter(fs);

            using (fs)
            using (writer)
            {

                writer.WriteLine(strongMotionData.Header.Line7StationName);
                writer.WriteLine("Start Time: ," + strongMotionData.StartTime);
                writer.WriteLine("Time, Accel (cm/sec2), Velocity (cm / sec), Displacment (cm)");

                TimeSpan timeDelta = new TimeSpan(0, // days
                    0,
                    0,
                    0, // seconds
                    (int) (strongMotionData.TimeBetweenAccelPoints * 1000.0));

                for(int i = 0; i < strongMotionData.NumberOfAccelPoints; i++)
                {
                    DateTime time = strongMotionData.StartTime + i * timeDelta;
                    writer.WriteLine(time.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," +
                        strongMotionData.AccelChannelData[i] + "," +
                        strongMotionData.VelocChannelData[i] + "," +
                        strongMotionData.DisplChannelData[i]);
                }
            }
        }
    }
}
