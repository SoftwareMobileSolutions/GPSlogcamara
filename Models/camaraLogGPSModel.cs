namespace gpslogcamara.Models
{
    public class CamaraLogGPSModel: mensajesModel
    {
        public Int64 idlogcam { get; set; }
        public string imei { get; set; } = "";
        public float lat { get; set; } = 0;
        public float lng { get; set; } = 0;
        public double speed { get; set; } = 0;
        public string dategps { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public double heading { get; set; } = 0;
        public short evento { get; set; } = 1;
        public int numsat { get; set; } = 1;
        public bool accStatus { get; set; } = true;
        public short gpsSignal { get; set; } = 1;
        public int distance { get; set; } = 1;
        public double batteryPowerVal { get; set; } = 1;
        public string serverip { get; set; } = "";
    }
}
